using System.Diagnostics;
using System.Text;
using Unite.Data.Source.Web.Configuration.Options;
using Unite.Data.Source.Web.Handlers.Constants;
using Unite.Data.Source.Web.Handlers.Contract;
using Unite.Data.Source.Web.Handlers.Files;
using Unite.Essentials.Tsv;

namespace Unite.Data.Source.Web.Handlers;

public class ExploringHandler
{
    private readonly WorkerOptions _workerOptions;
    private readonly ConfigOptions _configOptions;
    private readonly FeedOptions _feedOptions;
    
    private readonly string _configPath;
    private readonly FoundFilesCache _foundFilesCache;
    private readonly HostFilesCache _hostFilesCache;


    public ExploringHandler(
        WorkerOptions workerOptions,
        ConfigOptions confOptions,
        FeedOptions feedOptions)
    {
        _workerOptions = workerOptions;
        _configOptions = confOptions;
        _feedOptions = feedOptions;

        _configPath = Path.Combine(_configOptions.ConfigPath, "config.tsv");
        _foundFilesCache = new FoundFilesCache(Path.Combine(_configOptions.CachePath, "found-files.tsv"));
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
                var crawlerProcess = PrepareProcess(crawlerPath, type, Path.Combine(_configOptions.DataPath, folderConfig.Path));
                var crawlerOutput = await RunProcess(crawlerProcess);

                var filesMetadata = ReadFilesMetadata(type, crawlerOutput);

                foreach (var fileMetadata in filesMetadata)
                {
                    var filePath = GetPath(_configOptions.DataPath, fileMetadata.Path);

                    if (_foundFilesCache.Contains(filePath))
                    {
                        continue;
                    }

                    string key = null;
                    string content = fileMetadata.ToString();

                    if (fileMetadata.Reader != null)
                    {
                        if (fileMetadata.Reader.StartsWith("cmd/"))
                        {
                            var readerPath = Path.Combine(_configOptions.ConfigPath, folderConfig.Crawler, "readers", fileMetadata.Reader[4..]);
                            var readerProcess = PrepareProcess(readerPath, fileMetadata.Path);
                            var readerOutput = await RunProcess(readerProcess);
                            content += Environment.NewLine + readerOutput;
                        }
                        else
                        {
                            var readerOutput = await File.ReadAllTextAsync(fileMetadata.Path);
                            content += Environment.NewLine + readerOutput;
                        }
                    }
                    else
                    {
                        key = Guid.NewGuid().ToString();
                        var resource = new Resource(type, fileMetadata.Format, $"{_workerOptions.Host}/api/files/{key}");
                        var tsv = TsvWriter.Write([resource]);
                        content += Environment.NewLine + tsv;
                    }

                    var uploaded = UploadFileContent(type, content);

                    if (uploaded)
                    {
                        if (key != null)
                        {
                            _hostFilesCache.Add(key, filePath);
                        }

                        _foundFilesCache.Add(filePath);
                    }
                }
            }
        }

        await Task.CompletedTask;
    }


    private BaseFile[] ReadFilesMetadata(string type, string metadata)
    {
        if (type == DataTypes.Genome.Dna.Sample || type == DataTypes.Genome.Rna.Sample || type == DataTypes.Genome.Rnasc.Sample)
            return TsvReader.Read<SampleFile>(metadata).ToArray();
        else if (type == DataTypes.Genome.Dna.Ssm || type == DataTypes.Genome.Dna.Cnv || type == DataTypes.Genome.Dna.Sv)
            return TsvReader.Read<DnaAnalysisFile>(metadata).ToArray();
        else if (type == DataTypes.Genome.Rna.Exp)
            return TsvReader.Read<RnaAnalysisFile>(metadata).ToArray();
        else
            return TsvReader.Read<BaseFile>(metadata).ToArray();
    }

    private bool UploadFileContent(string type, string content)
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

        var response = client.SendAsync(request).Result;

        return response.IsSuccessStatusCode;
    }

    private static Process PrepareProcess(string path, params string[] arguments)
    {
        var process = new Process();

        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
        process.StartInfo.FileName = Path.GetFileName(path);
        process.StartInfo.Arguments = string.Join(" ", arguments);
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

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
        
        return output;
    }

    private static string GetPath(string dataPath, string filePath)
    {
        if (string.IsNullOrWhiteSpace(dataPath))
            return filePath;
        
        return filePath[dataPath.Length..];
    }
}
