using System.Text;
using Unite.Data.Source.Web.Configuration.Options;
using Unite.Data.Source.Web.Handlers.Constants;
using Unite.Data.Source.Web.Handlers.Contract;
using Unite.Data.Source.Web.Handlers.Contract.Extensions;
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
            foreach (var type in folderConfig.Types)
            {                
                var sheetPath = Path.Combine(_configOptions.DataPath, folderConfig.Path, $"{type}.tsv");
                var sheetExists = File.Exists(sheetPath);

                var crawlerPath = Path.Combine(_configOptions.ConfigPath, folderConfig.Crawler, "crawler");
                var crawlerExists = File.Exists(crawlerPath);

                if (!sheetExists && !sheetExists)
                {
                    _logger.LogWarning("Neither crawler nor sheet file exists for type '{type}'", type);
                    continue;
                }

                var tsv = sheetExists
                    ? await File.ReadAllTextAsync(sheetPath)
                    : await Command.Run(crawlerPath, type, folderConfig.Path);
               
                if (string.IsNullOrWhiteSpace(tsv))
                {
                    _logger.LogWarning("Sheet content is empty for type '{type}'", type);
                    continue;
                }

                if (type.Contains('-')) // Result file
                {
                    var files = TsvReader.Read<ResultFile>(tsv).ToArray();
                    
                    // TODO: Handle groups
                    // var groups = files.GroupBy(file => file);

                    foreach (var file in files)
                    {
                        var pathRelative = Path.Combine(folderConfig.Path, file.Path);
                        var pathAbsolute = Path.Combine(_configOptions.DataPath, pathRelative);

                        if (_foundFilesCache.Contains(pathRelative) || _errorFilesCache.Contains(pathRelative))
                            continue;

                        if (string.IsNullOrWhiteSpace(file.Reader)) // File is a resource
                        {
                            var key = Guid.NewGuid().ToString();
                            var url = $"{_workerOptions.Host}/api/file/{key}";
                            var resource = file.AsResource(type, url);

                            try
                            {
                                var content = TsvWriter.Write([resource]);
                                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                                var form = file.AsForm().AddField(ResultFile.ResourcesColumn, "resources.tsv", stream);

                                await UploadResult(type, form, null);

                                _hostFilesCache.Add(key, pathRelative);
                                _foundFilesCache.Add(pathRelative);
                                _logger.LogInformation("Uploaded and hosted file '{key}' '{path}'", key, pathRelative);
                            }
                            catch (Exception ex)
                            {
                                _errorFilesCache.Add(pathRelative);
                                _logger.LogError("Failed to upload file '{path}'\n{message}", pathRelative, ex.Message);

                                continue;
                            }
                        }
                        else if (file.Reader.StartsWith("cmd/")) // File has custom reader
                        {
                            var readerPath = Path.Combine(_configOptions.ConfigPath, folderConfig.Crawler, "readers", file.Reader[4..]);

                            try
                            {
                                var content = await Command.Run(readerPath, pathAbsolute);
                                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                                var form = file.AsForm().AddField(ResultFile.EntriesColumn, "entries.tsv", stream);

                                await UploadResult(type, form, file.Reader);

                                _foundFilesCache.Add(pathRelative);
                                _logger.LogInformation("Uploaded file '{path}'", pathRelative);
                            }
                            catch (Exception ex)
                            {
                                _errorFilesCache.Add(pathRelative);
                                _logger.LogError("Failed to upload file '{path}'\n{message}", pathRelative, ex.Message);

                                continue;
                            }
                        }
                        else // File has standard reader
                        {
                            try
                            {
                                using var stream = File.OpenRead(pathAbsolute);
                                var form = file.AsForm().AddField(ResultFile.EntriesColumn, "entries.tsv", stream);

                                await UploadResult(type, form, file.Reader);

                                _foundFilesCache.Add(pathRelative);
                                _logger.LogInformation("Uploaded file '{path}'", pathRelative);
                            }
                            catch (Exception ex)
                            {
                                _errorFilesCache.Add(pathRelative);
                                _logger.LogError("Failed to upload file '{path}'\n{message}", pathRelative, ex.Message);

                                continue;
                            }
                        }
                    }
                }
                else // Sample file
                {
                    var files = TsvReader.Read<SampleFile>(tsv).ToArray();
                    
                    // TODO: Handle groups
                    // var groups = files.GroupBy(file => file);

                    foreach (var file in files)
                    {
                        var pathRelative = Path.Combine(folderConfig.Path, file.Path);
                        var pathAbsolute = Path.Combine(_configOptions.DataPath, pathRelative);

                        if (_foundFilesCache.Contains(pathRelative) || _errorFilesCache.Contains(pathRelative))
                            continue;

                        var key = Guid.NewGuid().ToString();
                        var url = $"{_workerOptions.Host}/api/file/{key}";
                        var resource = file.AsResource(type, url);

                        try
                        {
                            var content = TsvWriter.Write([resource]);
                            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                            var form = file.AsForm().AddField(SampleFile.ResourcesColumn, "resources.tsv", stream);

                            await UploadSample(type, form);

                            _hostFilesCache.Add(key, pathRelative);
                            _foundFilesCache.Add(pathRelative);
                            _logger.LogInformation("Uploaded and hosted file '{key}' '{path}'", key, pathRelative);
                        }
                        catch (Exception ex)
                        {
                            _errorFilesCache.Add(pathRelative);
                            _logger.LogError("Failed to upload file '{path}'\n{message}", pathRelative, ex.Message);
                        }
                    }
                }
            }
        }

        await Task.CompletedTask;
    }

    private async Task UploadSample(string type, MultipartFormDataContent form, bool review = true)
    {
        using var handler = new HttpClientHandler { UseProxy = false };
        using var client = new HttpClient(handler);

        var url = type switch
        {
            DataTypes.Omics.Dna.Sample => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Dna.Sample}?review={review}",
            DataTypes.Omics.Meth.Sample => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Meth.Sample}?review={review}",
            DataTypes.Omics.Rna.Sample => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Rna.Sample}?review={review}",
            DataTypes.Omics.Rnasc.Sample => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Rnasc.Sample}?review={review}",
            _ => throw new ArgumentException($"Unknown data type '{type}'")
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_workerOptions.Token}");
        request.Content = form;

        var result = await client.SendAsync(request);

        if (!result.IsSuccessStatusCode)
            throw new Exception($"Uploading to '{url}' resulted in '{result.StatusCode}'\n{result.Content.ReadAsStringAsync().Result}");
    }

    public async Task UploadResult(string type, MultipartFormDataContent form, string format, bool review = true)
    {
        using var handler = new HttpClientHandler { UseProxy = false };
        using var client = new HttpClient(handler);

        var url = type switch
        {
            DataTypes.Omics.Dna.Sm => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Dna.Sm}?review={review}",
            DataTypes.Omics.Dna.Cnv => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Dna.Cnv}?review={review}",
            DataTypes.Omics.Dna.Sv => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Dna.Sv}?review={review}",
            DataTypes.Omics.Meth.Level => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Meth.Level}?review={review}",
            DataTypes.Omics.Rna.Exp => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Rna.Exp}?review={review}",
            DataTypes.Omics.Rnasc.Exp => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Rnasc.Exp}?review={review}",
            _ => throw new ArgumentException($"Unknown data type '{type}'")
        };

        if (!string.IsNullOrWhiteSpace(format))
            url += $"&format={format}";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_workerOptions.Token}");
        request.Content = form;

        try
        {
            var result = await client.SendAsync(request);

            if (!result.IsSuccessStatusCode)
            throw new Exception($"Uploading to '{url}' resulted in '{result.StatusCode}'\n{result.Content.ReadAsStringAsync().Result}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send request to '{url}'\n{ex.Message}");
        }
    }

    private static string GetPath(string dataPath, string filePath)
    {
        if (string.IsNullOrWhiteSpace(dataPath))
            return filePath;

        return filePath[dataPath.Length..];
    }
}
