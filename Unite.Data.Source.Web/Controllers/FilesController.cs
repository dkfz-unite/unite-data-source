using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Data.Source.Web.Configuration.Options;
using Unite.Data.Source.Web.Handlers;

namespace Unite.Data.Source.Web.Controllers;

[Authorize]
[Route("api/[controller]")]
public class FilesController : Controller
{
    private readonly ConfigOptions _configOptions;
    private readonly HostFilesCache _filesCache;


    public FilesController(ConfigOptions configOptions)
    {
        _configOptions = configOptions;
        _filesCache = new HostFilesCache(Path.Combine(_configOptions.CachePath, "files.tsv"));
    }

    [HttpGet("{key}")]
    public IActionResult GetFile(string key)
    {
        var resourcePath = GetPath(_configOptions.DataPath, _filesCache.Get(key));
            
        if (System.IO.File.Exists(resourcePath))
        {
            return PhysicalFile(resourcePath, "application/octet-stream", enableRangeProcessing: true);
        }
        else if (System.IO.Directory.Exists(resourcePath))
        {
            var stream = CreateArchive(resourcePath);

            return File(stream, "application/zip", $"{key}.zip");
        }
        else
        {
            return NotFound();
        }
    }

    [HttpGet("{key}/index")]
    public IActionResult GetIndex(string key)
    {
        var resourcePath = _filesCache.Get(key);
        var indexPath = Path.Combine(resourcePath, ".bai");

        if (System.IO.File.Exists(indexPath))
        {
            return PhysicalFile(indexPath, "application/octet-stream", enableRangeProcessing: true);
        }
        else
        {
            return NotFound();
        }
    }


    private static MemoryStream CreateArchive(string folderPath)
    {
        var stream = new MemoryStream();
        
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                var name = filePath.Substring(folderPath.Length + 1);
                var entry = archive.CreateEntry(name);

                using var entryStream = entry.Open();
                using var fileStream = System.IO.File.OpenRead(filePath);
                fileStream.CopyTo(entryStream);
            }
        }

        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

    private static string GetPath(string dataPath, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        if (string.IsNullOrWhiteSpace(dataPath))
            return filePath;
        
        return filePath[dataPath.Length..];
    }
}
