using System.Diagnostics;
using System.Text;
using Unite.Data.Source.Web.Configuration.Options;
using Unite.Data.Source.Web.Handlers.Constants;
using Unite.Data.Source.Web.Handlers.Contract;
using Unite.Essentials.Tsv;

namespace Unite.Data.Source.Web.Handlers;

public class ExploringHandler
{
    private readonly WorkerOptions _workerOptions;
    private readonly ConfigOptions _configOptions;
    private readonly FeedOptions _feedOptions;
    private readonly ILogger _logger;
    
    private readonly string _configPath;
    private readonly FoundFilesCache _foundFilesCache;
    private readonly FoundFilesCache _errorFilesCache;
    private readonly HostFilesCache _hostFilesCache;

    private record ResourceFile(string Key, string Path, Resource Resource);


    public ExploringHandler(
        WorkerOptions workerOptions,
        ConfigOptions confOptions,
        FeedOptions feedOptions,
        ILogger<ExploringHandler> logger)
    {
        _workerOptions = workerOptions;
        _configOptions = confOptions;
        _feedOptions = feedOptions;
        _logger = logger;

        _configPath = Path.Combine(_configOptions.ConfigPath, "config.tsv");
        _foundFilesCache = new FoundFilesCache(Path.Combine(_configOptions.CachePath, "found-files.tsv"));
        _errorFilesCache = new FoundFilesCache(Path.Combine(_configOptions.CachePath, "error-files.tsv"));
        _hostFilesCache = new HostFilesCache(Path.Combine(_configOptions.CachePath, "host-files.tsv"));
    }


    public async Task Prepare()
    {
        await Task.CompletedTask;
    }

    public async Task Handle()
    {
        var folderConfigs = ConfigEntry.Read(_configPath);

        foreach (var folderConfig in folderConfigs)
        {
            // _logger.LogInformation("Exploring folder '{path}'", folderConfig.Path);

            foreach (var type in folderConfig.Types)
            {
                var crawlerPath = Path.Combine(_configOptions.ConfigPath, folderConfig.Crawler, "crawler");
                var crawlerExists = File.Exists(crawlerPath);

                var sheetFilePath = Path.Combine(_configOptions.DataPath, folderConfig.Path, $"{type}.tsv");
                var sheetExists = File.Exists(sheetFilePath);

                if (!crawlerExists && !sheetExists)
                {
                    _logger.LogWarning("Neither crawler '{crawler}' nor sheet file '{sheet}' exists for type '{type}' in folder '{path}'",
                        crawlerPath, sheetFilePath, type, folderConfig.Path);

                    continue;
                }

                var tsv = sheetExists ? await File.ReadAllTextAsync(sheetFilePath) : await ReadContent(crawlerPath, type, folderConfig.Path);

                if (string.IsNullOrWhiteSpace(tsv))
                {
                    _logger.LogWarning("Sheet content is empty for type '{type}' in folder '{path}'",
                        type, folderConfig.Path);

                    continue;
                }

                if (type.Contains('-')) // Result file
                {
                    var files = TsvReader.Read<ResultFile>(tsv).ToArray();

                    foreach (var file in files)
                    {
                        var path = Path.Combine(folderConfig.Path, file.Path);

                        if (_foundFilesCache.Contains(path) || _errorFilesCache.Contains(path))
                            continue;

                        if (file.Reader.StartsWith("cmd/")) // File has custom reader
                        {
                            var readerPath = Path.Combine(_configOptions.ConfigPath, folderConfig.Crawler, "readers", file.Reader[4..]);

                            var content = await ReadContent(readerPath, file.Path);
                            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

                            var form = file.AsForm();
                            form.Add(new StreamContent(stream), "entries", "entries.tsv");

                            try
                            {
                                UploadResult(type, form, file.Reader);

                                _foundFilesCache.Add(path);
                            }
                            catch (Exception ex)
                            {
                                _errorFilesCache.Add(path);
                                _logger.LogError("Failed to upload file '{path}'", path);
                                _logger.LogError("{message}", ex.Message);

                                continue;
                            }
                        }
                        else
                        {
                            if (IsResourceType(type)) // File is a resource
                            {
                                var key = Guid.NewGuid().ToString();
                                var url = $"{_workerOptions.Host}/api/file/{key}";
                                var resource = file.AsResource(type, url);

                                var content = TsvWriter.Write([resource]);
                                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

                                var form = file.AsForm();
                                form.Add(new StreamContent(stream), "resources", "resources.tsv");

                                try
                                {
                                    UploadSample(type, form);

                                    _hostFilesCache.Add(key, path);
                                    _foundFilesCache.Add(path);
                                }
                                catch (Exception ex)
                                {
                                    _errorFilesCache.Add(path);
                                    _logger.LogError("Failed to upload file '{path}'", path);
                                    _logger.LogError("{message}", ex.Message);

                                    continue;
                                }
                            }
                            else // File has standard reader
                            {
                                var form = file.AsForm();
                                using var stream = File.OpenRead(file.Path);
                                form.Add(new StreamContent(stream), "entries", "entries.tsv");

                                try
                                {
                                    UploadResult(type, form, file.Reader);

                                    _foundFilesCache.Add(path);
                                }
                                catch (Exception ex)
                                {
                                    _errorFilesCache.Add(path);
                                    _logger.LogError("Failed to upload file '{path}'", path);
                                    _logger.LogError("{message}", ex.Message);

                                    continue;
                                }
                            }
                        }
                    }
                }
                else // Sample file
                {
                    var files = TsvReader.Read<SampleFile>(tsv).ToArray();

                    foreach (var file in files)
                    {
                        var path = Path.Combine(folderConfig.Path, file.Path);

                        if (_foundFilesCache.Contains(path) || _errorFilesCache.Contains(path))
                            continue;

                        var key = Guid.NewGuid().ToString();
                        var url = $"{_workerOptions.Host}/api/file/{key}";
                        var resource = file.AsResource(type, url);

                        var content = TsvWriter.Write([resource]);
                        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

                        var form = file.AsForm();
                        form.Add(new StreamContent(stream), "resources", "resources.tsv");

                        try
                        {
                            UploadSample(type, form);

                            _hostFilesCache.Add(key, path);
                            _foundFilesCache.Add(path);
                        }
                        catch (Exception ex)
                        {
                            _errorFilesCache.Add(path);
                            _logger.LogError("Failed to upload file '{path}'", path);
                            _logger.LogError("{message}", ex.Message);
                        }
                    }
                }

                continue;


                // var crawlerPath = Path.Combine(_configOptions.ConfigPath, folderConfig.Crawler, "crawler");

                if (!File.Exists(crawlerPath))
                {
                    _logger.LogWarning("Crawler '{path}' not found", crawlerPath);

                    continue;
                }

                var crawlerProcess = PrepareProcess(crawlerPath, type, Path.Combine(_configOptions.DataPath, folderConfig.Path));

                var crawlerOutput = await RunProcess(crawlerProcess);

                var filesMetadata = TsvReader.Read<FileMetadata>(crawlerOutput).ToArray();

                // _logger.LogInformation("Found {count} files of type '{type}'", filesMetadata.Length, type);

                foreach (var fileMetadata in filesMetadata)
                {
                    var path = GetPath(_configOptions.DataPath, fileMetadata.Path);

                    var content = string.Empty;

                    if (_foundFilesCache.Contains(path) || _errorFilesCache.Contains(path))
                    {
                        continue;
                    }

                    _logger.LogInformation("New '{type}' file '.../{path}'", type, path);

                    if (fileMetadata.Reader.StartsWith("cmd/"))
                    {
                        var readerPath = Path.Combine(_configOptions.ConfigPath, folderConfig.Crawler, "readers", fileMetadata.Reader[4..]);

                        if (!File.Exists(readerPath))
                        {
                            _errorFilesCache.Add(path);
                            _logger.LogWarning("Reader '{path}' not found", readerPath);

                            continue;
                        }

                        var readerProcess = PrepareProcess(readerPath, $"\"{fileMetadata.Path}\"");

                        content += await RunProcess(readerProcess);

                        if (string.IsNullOrWhiteSpace(content))
                        {
                            _errorFilesCache.Add(path);
                            _logger.LogError("Failed to read file '{path}'", path);

                            continue;
                        }
                    }
                    else
                    {
                        content += await File.ReadAllTextAsync(fileMetadata.Path);

                        if (string.IsNullOrWhiteSpace(content))
                        {
                            _errorFilesCache.Add(path);
                            _logger.LogError("Failed to read file '{path}'", path);

                            continue;
                        }
                    }

                    if (IsResourceType(type))
                    {
                        var key = Guid.NewGuid().ToString();

                        var url = $"{_workerOptions.Host}/api/file/{key}";

                        var resource = new Resource(fileMetadata.Name, type, fileMetadata.Format, fileMetadata.Archive, url);

                        content = Merge(content, TsvWriter.Write([resource]));

                        try
                        {
                            UploadFileContent(type, content);

                            _hostFilesCache.Add(key, path);
                            _foundFilesCache.Add(path);
                        }
                        catch (Exception ex)
                        {
                            _errorFilesCache.Add(path);
                            _logger.LogError(ex, "Failed to upload and host file '{path}'", path);
                            _logger.LogError("{content}", content);
                        }
                    }
                    else
                    {
                        try
                        {
                            UploadFileContent(type, content);

                            _foundFilesCache.Add(path);
                        }
                        catch (Exception ex)
                        {
                            _errorFilesCache.Add(path);
                            _logger.LogError(ex, "Failed to upload file '{path}'", path);
                            _logger.LogError("{content}", content);
                        }
                    }
                }
            }
        }

        await Task.CompletedTask;
    }

    private void UploadSample(string type, MultipartFormDataContent form)
    {
        using var handler = new HttpClientHandler { UseProxy = false };
        using var client = new HttpClient(handler);

        var url = type switch
        {
            DataTypes.Omics.Dna.Sample => $"{_feedOptions.OmicsHost}/{UrlsFile.Omics.Dna.Sample}?review=false",
            DataTypes.Omics.Meth.Sample => $"{_feedOptions.OmicsHost}/{UrlsFile.Omics.Meth.Sample}?review=false",
            DataTypes.Omics.Rna.Sample => $"{_feedOptions.OmicsHost}/{UrlsFile.Omics.Rna.Sample}?review=false",
            DataTypes.Omics.Rnasc.Sample => $"{_feedOptions.OmicsHost}/{UrlsFile.Omics.Rnasc.Sample}?review=false",
            _ => throw new ArgumentException($"Unknown data type '{type}'")
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("Authorization", $"Bearer {_workerOptions.Token}");

        request.Content = form;

        var result = client.SendAsync(request).Result;

        if (!result.IsSuccessStatusCode)
        {
            var error = $"Uploading to '{url}' resulted in '{result.StatusCode}'\n{result.Content.ReadAsStringAsync().Result}";

            throw new Exception(error);
        }
    }

    public void UploadResult(string type, MultipartFormDataContent form, string format)
    {
        using var handler = new HttpClientHandler { UseProxy = false };
        using var client = new HttpClient(handler);

        var url = type switch
        {
            DataTypes.Omics.Dna.Sm => $"{_feedOptions.OmicsHost}/{UrlsFile.Omics.Dna.Sm}?review=false",
            DataTypes.Omics.Dna.Cnv => $"{_feedOptions.OmicsHost}/{UrlsFile.Omics.Dna.Cnv}?review=false",
            DataTypes.Omics.Dna.Sv => $"{_feedOptions.OmicsHost}/{UrlsFile.Omics.Dna.Sv}?review=false",
            DataTypes.Omics.Meth.Level => $"{_feedOptions.OmicsHost}/{UrlsFile.Omics.Meth.Level}?review=false",
            DataTypes.Omics.Rna.Exp => $"{_feedOptions.OmicsHost}/{UrlsFile.Omics.Rna.Exp}?review=false",
            DataTypes.Omics.Rnasc.Exp => $"{_feedOptions.OmicsHost}/{UrlsFile.Omics.Rnasc.Exp}?review=false",
            _ => throw new ArgumentException($"Unknown data type '{type}'")
        };

        if (!string.IsNullOrWhiteSpace(format))
            url += $"&format={format}";

        var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("Authorization", $"Bearer {_workerOptions.Token}");

        request.Content = form;

        var result = client.SendAsync(request).Result;

        if (!result.IsSuccessStatusCode)
        {
            var error = $"Uploading to '{url}' resulted in '{result.StatusCode}'\n{result.Content.ReadAsStringAsync().Result}";

            throw new Exception(error);
        }
    }

    private void UploadFileContent(string type, string content)
    {
        var handler = new HttpClientHandler
        {
            UseProxy = false
        };

        using var client = new HttpClient(handler);

        var url = type switch
        {
            DataTypes.Donor.Entry => $"{_feedOptions.DonorsHost}/{Urls.Donor.Entry}?review=false",
            DataTypes.Donor.Treatment => $"{_feedOptions.DonorsHost}/{Urls.Donor.Treatment}?review=false",
            DataTypes.Image.Entry.Mr => $"{_feedOptions.ImagesHost}/{Urls.Image.Entry.Mr}?review=false",
            DataTypes.Image.Entry.Ct => $"{_feedOptions.ImagesHost}/{Urls.Image.Entry.Ct}?review=false",
            DataTypes.Image.Feature => $"{_feedOptions.ImagesHost}/{Urls.Image.Feature}?review=false",
            DataTypes.Specimen.Entry.Material => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Entry.Material}?review=false",
            DataTypes.Specimen.Entry.Line => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Entry.Line}?review=false",
            DataTypes.Specimen.Entry.Organoid => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Entry.Organoid}?review=false",
            DataTypes.Specimen.Entry.Xenograft => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Entry.Xenograft}?review=false",
            DataTypes.Specimen.Intervention => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Intervention}?review=false",
            DataTypes.Specimen.Drug => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Drug}?review=false",
            DataTypes.Omics.Dna.Sample => $"{_feedOptions.OmicsHost}/{Urls.Omics.Dna.Sample}?review=false",
            DataTypes.Omics.Dna.Sm => $"{_feedOptions.OmicsHost}/{Urls.Omics.Dna.Sm}?review=false",
            DataTypes.Omics.Dna.Cnv => $"{_feedOptions.OmicsHost}/{Urls.Omics.Dna.Cnv}?review=false",
            DataTypes.Omics.Dna.Sv => $"{_feedOptions.OmicsHost}/{Urls.Omics.Dna.Sv}?review=false",
            DataTypes.Omics.Meth.Sample => $"{_feedOptions.OmicsHost}/{Urls.Omics.Meth.Sample}?review=false",
            DataTypes.Omics.Meth.Level => $"{_feedOptions.OmicsHost}/{Urls.Omics.Meth.Level}?review=false",
            DataTypes.Omics.Rna.Sample => $"{_feedOptions.OmicsHost}/{Urls.Omics.Rna.Sample}?review=false",
            DataTypes.Omics.Rna.Exp => $"{_feedOptions.OmicsHost}/{Urls.Omics.Rna.Exp}?review=false",
            DataTypes.Omics.Rnasc.Sample => $"{_feedOptions.OmicsHost}/{Urls.Omics.Rnasc.Sample}?review=false",
            DataTypes.Omics.Rnasc.Exp => $"{_feedOptions.OmicsHost}/{Urls.Omics.Rnasc.Exp}?review=false",
            _ => throw new ArgumentException($"Unknown data type '{type}'")
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("Authorization", $"Bearer {_workerOptions.Token}");

        request.Content = new StringContent(content, Encoding.UTF8, "text/tab-separated-values");

        var result = client.SendAsync(request).Result;

        if (!result.IsSuccessStatusCode)
        {
            var error = $"Uploading to '{url}' resulted in '{result.StatusCode}'\n{result.Content.ReadAsStringAsync().Result}";

            throw new Exception(error);
        }
    }

    private static Process PrepareProcess(string path, params string[] arguments)
    {
        var process = new Process();

        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
        process.StartInfo.FileName = Path.GetFullPath(path);
        process.StartInfo.Arguments = string.Join(" ", arguments);
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        return process;
    }

    private static async Task<string> RunProcess(Process process)
    {
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode > 0)
            throw new Exception(error);

        if (error.Length > 0)
            Console.Error.WriteLine(error);
            
        return output;
    }

    private static async Task<string> ReadContent(string path, params string[] arguments)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Process file '{path}' not found.");

        var process = PrepareProcess(path, arguments);

        var content = await RunProcess(process);

        if (string.IsNullOrWhiteSpace(content))
            throw new Exception($"Process '{path}' returned empty content.");

        return content;
    }

    private static string GetPath(string dataPath, string filePath)
    {
        if (string.IsNullOrWhiteSpace(dataPath))
            return filePath;

        return filePath[dataPath.Length..];
    }

    private static bool IsResourceType(string type)
    {
        return type switch
        {
            DataTypes.Omics.Dna.Sample => true,
            DataTypes.Omics.Meth.Sample => true,
            DataTypes.Omics.Meth.Level => true,
            DataTypes.Omics.Rna.Sample => true,
            DataTypes.Omics.Rnasc.Sample => true,
            DataTypes.Omics.Rnasc.Exp => true,
            
            _ => false
        };
    }

    private static string Merge(string metadata, string data)
    {
        if (metadata.EndsWith(Environment.NewLine))
            return metadata + data;
        else
            return metadata + Environment.NewLine + data;
    }
}
