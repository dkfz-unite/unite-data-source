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

                foreach (var fileMetadata in filesMetadata)
                {
                    var filePath = GetPath(_configOptions.DataPath, fileMetadata.Path);
                    var isResource = IsResourceType(type);
                    var content = string.Empty;
                    var key = (string)null;

                    if (_foundFilesCache.Contains(filePath) || _errorFilesCache.Contains(filePath))
                    {
                        continue;
                    }

                    if (fileMetadata.Reader.StartsWith("cmd/"))
                    {
                        var readerPath = Path.Combine(_configOptions.ConfigPath, folderConfig.Crawler, "readers", fileMetadata.Reader[4..]);

                        if (!File.Exists(readerPath))
                        {
                            _logger.LogWarning("Reader '{path}' not found", readerPath);

                            continue;
                        }

                        var readerProcess = PrepareProcess(readerPath, fileMetadata.Path);

                        content += await RunProcess(readerProcess);
                    }
                    else
                    {
                        content += await File.ReadAllTextAsync(fileMetadata.Path);
                    }

                    if (isResource)
                    {
                        key = Guid.NewGuid().ToString();
                        var url = $"{_workerOptions.Host}/api/files/{key}";
                        var resource = new Resource(type, fileMetadata.Format, url);
                        content += Environment.NewLine + TsvWriter.Write([resource]);
                    }

                    try
                    {
                        UploadFileContent(type, content);

                        if (isResource)
                        {
                            _hostFilesCache.Add(key, filePath);
                        }

                        _foundFilesCache.Add(filePath);
                    }
                    catch (Exception ex)
                    {
                        _errorFilesCache.Add(filePath);

                        _logger.LogError(ex, "Failed to upload file '{path}'", filePath);
                    }
                }
            }
        }

        await Task.CompletedTask;
    }

    private void UploadFileContent(string type, string content)
    {
        using var client = new HttpClient();

        var url = type switch
        {
            DataTypes.Donor.Entry => $"{_feedOptions.DonorsHost}/{Urls.Donor.Entry}",
            DataTypes.Donor.Treatment => $"{_feedOptions.DonorsHost}/{Urls.Donor.Treatment}",
            DataTypes.Image.Entry.Mri => $"{_feedOptions.ImagesHost}/{Urls.Image.Entry.Mri}",
            DataTypes.Image.Entry.Ct => $"{_feedOptions.ImagesHost}/{Urls.Image.Entry.Ct}",
            DataTypes.Image.Feature => $"{_feedOptions.ImagesHost}/{Urls.Image.Feature}",
            DataTypes.Specimen.Entry.Material => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Entry.Material}",
            DataTypes.Specimen.Entry.Line => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Entry.Line}",
            DataTypes.Specimen.Entry.Organoid => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Entry.Organoid}",
            DataTypes.Specimen.Entry.Xenograft => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Entry.Xenograft}",
            DataTypes.Specimen.Intervention => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Intervention}",
            DataTypes.Specimen.Drug => $"{_feedOptions.SpecimensHost}/{Urls.Specimen.Drug}",
            DataTypes.Genome.Dna.Sample => $"{_feedOptions.GenomeHost}/{Urls.Genome.Dna.Sample}",
            DataTypes.Genome.Dna.Ssm => $"{_feedOptions.GenomeHost}/{Urls.Genome.Dna.Ssm}",
            DataTypes.Genome.Dna.Cnv => $"{_feedOptions.GenomeHost}/{Urls.Genome.Dna.Cnv}",
            DataTypes.Genome.Dna.Sv => $"{_feedOptions.GenomeHost}/{Urls.Genome.Dna.Sv}",
            DataTypes.Genome.Rna.Sample => $"{_feedOptions.GenomeHost}/{Urls.Genome.Rna.Sample}",
            DataTypes.Genome.Rna.Exp => $"{_feedOptions.GenomeHost}/{Urls.Genome.Rna.Exp}",
            DataTypes.Genome.Rnasc.Sample => $"{_feedOptions.GenomeHost}/{Urls.Genome.Rnasc.Sample}",
            DataTypes.Genome.Rnasc.Exp => $"{_feedOptions.GenomeHost}/{Urls.Genome.Rnasc.Exp}",
            _ => throw new ArgumentException($"Unknown data type '{type}'")
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("Authorization", $"Bearer {_workerOptions.Token}");
        
        request.Content = new StringContent(content, Encoding.UTF8, "text/tab-separated-values");

        var result = client.SendAsync(request).Result;

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to upload file content to '{url}'");
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
            DataTypes.Genome.Rna.Sample => true,
            DataTypes.Genome.Rnasc.Sample => true,
            DataTypes.Genome.Rnasc.Exp => true,
            _ => false
        };
    }
}
