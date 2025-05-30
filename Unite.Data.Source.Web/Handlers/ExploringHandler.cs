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
            DataTypes.Genome.Dna.Sample => $"{_feedOptions.GenomeHost}/{Urls.Genome.Dna.Sample}?review=false",
            DataTypes.Genome.Dna.Sm => $"{_feedOptions.GenomeHost}/{Urls.Genome.Dna.Sm}?review=false",
            DataTypes.Genome.Dna.Cnv => $"{_feedOptions.GenomeHost}/{Urls.Genome.Dna.Cnv}?review=false",
            DataTypes.Genome.Dna.Sv => $"{_feedOptions.GenomeHost}/{Urls.Genome.Dna.Sv}?review=false",
            DataTypes.Genome.Meth.Sample => $"{_feedOptions.GenomeHost}/{Urls.Genome.Meth.Sample}?review=false",
            DataTypes.Genome.Meth.Level => $"{_feedOptions.GenomeHost}/{Urls.Genome.Meth.Level}?review=false",
            DataTypes.Genome.Rna.Sample => $"{_feedOptions.GenomeHost}/{Urls.Genome.Rna.Sample}?review=false",
            DataTypes.Genome.Rna.Exp => $"{_feedOptions.GenomeHost}/{Urls.Genome.Rna.Exp}?review=false",
            DataTypes.Genome.Rnasc.Sample => $"{_feedOptions.GenomeHost}/{Urls.Genome.Rnasc.Sample}?review=false",
            DataTypes.Genome.Rnasc.Exp => $"{_feedOptions.GenomeHost}/{Urls.Genome.Rnasc.Exp}?review=false",
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
            DataTypes.Genome.Dna.Sample => true,
            DataTypes.Genome.Meth.Sample => true,
            DataTypes.Genome.Meth.Level => true,
            DataTypes.Genome.Rna.Sample => true,
            DataTypes.Genome.Rnasc.Sample => true,
            DataTypes.Genome.Rnasc.Exp => true,
            
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
