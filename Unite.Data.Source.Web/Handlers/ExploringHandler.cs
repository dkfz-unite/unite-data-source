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
            await HandleConfigEntry(folderConfig);
        }

        await Task.CompletedTask;
    }
    
    private async Task HandleConfigEntry(ConfigEntry folderConfig)
    {
        foreach (var type in folderConfig.Types)
        {
            if (IsDataType(type))
            {
                await HandleDataFile(folderConfig, null, type);
            }
            else
            {
                var tsv = await LoadTsvSheet(folderConfig, type);

                if (!string.IsNullOrWhiteSpace(tsv))
                {
                    if (IsSampleType(type))
                        await HandleSampleFile(folderConfig, tsv, type);
                    else if (IsResultType(type))
                        await HandleResultFile(folderConfig, tsv, type);
                    else
                        _logger.LogWarning("Unknown data type '{type}'", type);
                }
                else
                {
                    _logger.LogWarning("No sheet content for type '{type}'", type);
                }
            }
        }
    }
    
    private async Task HandleDataFile(ConfigEntry folderConfig, string tsv, string type)
    {
        var relativePath = Path.Combine(folderConfig.Path, $"{type}.tsv");
        var absolutePath = Path.GetFullPath(Path.Combine(_configOptions.DataPath, relativePath));

        if (_foundFilesCache.Contains(relativePath) || _errorFilesCache.Contains(relativePath))
            return;

        if (File.Exists(absolutePath))
        {
            try
            {
                var content = await File.ReadAllTextAsync(absolutePath);
                await UploadData(type, content);

                _foundFilesCache.Add(relativePath);
                _logger.LogInformation("Uploaded file '{path}'", relativePath);
            }
            catch (Exception ex)
            {
                _errorFilesCache.Add(relativePath);
                _logger.LogError("Failed to upload file '{path}'\n{message}", relativePath, ex.Message);
            }
        }
    }

    private async Task HandleResultFile(ConfigEntry folderConfig, string tsv, string type)
    {
        // TODO: Handle groups
        // var groups = files.GroupBy(file => file);

        var files = TsvReader.Read<ResultFile>(tsv).ToArray();
        
        foreach (var file in files)
        {
            var pathRelative = Path.Combine(folderConfig.Path, file.Path);
            var pathAbsolute = Path.GetFullPath(Path.Combine(_configOptions.DataPath, pathRelative));

            if (_foundFilesCache.Contains(pathRelative) || _errorFilesCache.Contains(pathRelative))
                continue;

            if (!ValidateFilePath(folderConfig.Path, file.Path))
            {
                _logger.LogError("File path '{filePath}' goes beyond config path '{configPath}' for data type '{dataType}'.", file.Path, folderConfig.Path, type);
                _errorFilesCache.Add(pathRelative);
                continue;
            }

            //TODO: It would be better to use field 'ResourceType' instead of 'Reader' as an additional config field
            if (string.IsNullOrWhiteSpace(file.Reader)) // File is a resource
            {
                await FeedFileAsLargeResource(type, file, pathRelative);
            }
            else if (file.Reader.StartsWith("cmd/")) // File has custom reader
            {
                await FeedFileAsCustomResource(folderConfig, type, file, pathAbsolute, pathRelative);
            }
            else // File has standard reader
            {
                await FeedFileAsStandardResource(type, file, pathAbsolute, pathRelative);
            }
        }
    }
    
    private async Task FeedFileAsLargeResource(string type, ResultFile file, string pathRelative)
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
        }
    }
    
    private async Task FeedFileAsCustomResource(ConfigEntry folderConfig, string type, ResultFile file, string pathAbsolute, string pathRelative)
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
        }
    }

    private async Task FeedFileAsStandardResource(string type, ResultFile file, string pathAbsolute, string pathRelative)
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
        }
    }

    private async Task HandleSampleFile(ConfigEntry folderConfig, string tsv, string type)
    {
        var files = TsvReader.Read<SampleFile>(tsv).ToArray();

        // TODO: Handle groups
        // var groups = files.GroupBy(file => file);

        foreach (var file in files)
        {
            var pathRelative = Path.Combine(folderConfig.Path, file.Path);
            var pathAbsolute = Path.GetFullPath(Path.Combine(_configOptions.DataPath, pathRelative));

            if (_foundFilesCache.Contains(pathRelative) || _errorFilesCache.Contains(pathRelative))
                continue;

            if (!ValidateFilePath(folderConfig.Path, file.Path))
            {
                _logger.LogError("File path '{filePath}' goes beyond config path '{configPath}' for data type '{dataType}'.", file.Path, folderConfig.Path, type);
                _errorFilesCache.Add(pathRelative);
                continue;
            }

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

    private async Task<string> LoadTsvSheet(ConfigEntry folderConfig, string type)
    {
        var sheetPath = Path.Combine(_configOptions.DataPath, folderConfig.Path, $"{type}.tsv");
        var sheetExists = File.Exists(sheetPath);

        var crawlerPath = Path.Combine(_configOptions.ConfigPath, folderConfig.Crawler, "crawler");
        var crawlerExists = File.Exists(crawlerPath);

        if (!sheetExists && !crawlerExists)
        {
            _logger.LogWarning("Sheet file '{path}' does not exist for type '{type}'", sheetPath, type);
            _logger.LogWarning("Crawler '{path}' does not exist for type '{type}'", crawlerPath, type);
            return null;
        }

        var tsv = sheetExists
            ? await File.ReadAllTextAsync(sheetPath)
            : await Command.Run(crawlerPath, type, Path.Combine(_configOptions.DataPath, folderConfig.Path), folderConfig.Args);
        return tsv;
    }


    private async Task UploadData(string type, string content, bool review = true)
    {
        using var handler = new HttpClientHandler { UseProxy = false };
        using var client = new HttpClient(handler);

        var url = type switch
        {
            DataTypes.Donor.Entry => $"{_feedOptions.DonorsHost}/{DataUrls.Donor.Entry}/tsv?review={review}",
            DataTypes.Donor.Treatment => $"{_feedOptions.DonorsHost}/{DataUrls.Donor.Treatment}/tsv?review={review}",
            DataTypes.Image.Entry.Mr => $"{_feedOptions.ImagesHost}/{DataUrls.Image.Entry.Mr}/tsv?review={review}",
            DataTypes.Image.Entry.Ct => $"{_feedOptions.ImagesHost}/{DataUrls.Image.Entry.Ct}/tsv?review={review}",
            DataTypes.Specimen.Entry.Material => $"{_feedOptions.SpecimensHost}/{DataUrls.Specimen.Entry.Material}/tsv?review={review}",
            DataTypes.Specimen.Entry.Line => $"{_feedOptions.SpecimensHost}/{DataUrls.Specimen.Entry.Line}/tsv?review={review}",
            DataTypes.Specimen.Entry.Organoid => $"{_feedOptions.SpecimensHost}/{DataUrls.Specimen.Entry.Organoid}/tsv?review={review}",
            DataTypes.Specimen.Entry.Xenograft => $"{_feedOptions.SpecimensHost}/{DataUrls.Specimen.Entry.Xenograft}/tsv?review={review}",
            DataTypes.Specimen.Intervention => $"{_feedOptions.SpecimensHost}/{DataUrls.Specimen.Intervention}/tsv?review={review}",
            _ => throw new ArgumentException($"Unknown data type '{type}'")
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_workerOptions.Token}");
        request.Content = new StringContent(content, Encoding.UTF8, "text/tab-separated-values");

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
            DataTypes.Omics.Prot.Sample => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Prot.Sample}?review={review}",
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
            DataTypes.Omics.Prot.Exp => $"{_feedOptions.OmicsHost}/{DataUrls.Omics.Prot.Exp}?review={review}",
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

    private static bool ValidateFilePath(string configPath, string filePath)
    {
        // File path is relative to config path.
        // We need to make sure, that file path starting with '../' does not go beyond config path.
        var configParts = configPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fileParts = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var relativeBlocksCount = fileParts.TakeWhile(part => part == "..").Count();
        return relativeBlocksCount <= configParts.Length;
    }


    private static bool IsDataType(string type)
    {
        string[] types = [
            DataTypes.Donor.Entry,
            DataTypes.Donor.Treatment,
            DataTypes.Image.Entry.Mr,
            DataTypes.Image.Entry.Ct,
            DataTypes.Specimen.Entry.Material,
            DataTypes.Specimen.Entry.Line,
            DataTypes.Specimen.Entry.Organoid,
            DataTypes.Specimen.Entry.Xenograft,
            DataTypes.Specimen.Intervention
        ];

        return types.Contains(type);
    }

    private static bool IsSampleType(string type)
    {
        string[] types = [
            DataTypes.Omics.Dna.Sample,
            DataTypes.Omics.Meth.Sample,
            DataTypes.Omics.Rna.Sample,
            DataTypes.Omics.Rnasc.Sample,
            DataTypes.Omics.Prot.Sample
        ];

        return types.Contains(type);
    }

    private static bool IsResultType(string type)
    {
        string[] types = [
            DataTypes.Omics.Dna.Sm,
            DataTypes.Omics.Dna.Cnv,
            DataTypes.Omics.Dna.Sv,
            DataTypes.Omics.Meth.Level,
            DataTypes.Omics.Rna.Exp,
            DataTypes.Omics.Rnasc.Exp,
            DataTypes.Omics.Prot.Exp
        ];

        return types.Contains(type);
    }
}
