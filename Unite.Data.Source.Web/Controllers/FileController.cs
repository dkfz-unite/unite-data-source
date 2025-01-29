using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Data.Source.Web.Configuration.Options;
using Unite.Data.Source.Web.Handlers;

namespace Unite.Data.Source.Web.Controllers;

[Authorize]
[Route("api/file")]
public class FileController : Controller
{
    protected readonly ConfigOptions _configOptions;
    protected readonly HostFilesCache _filesCache;


    public FileController(ConfigOptions configOptions)
    {
        _configOptions = configOptions;
        _filesCache = new HostFilesCache(Path.Combine(_configOptions.CachePath, "host-files.tsv"));
    }

    [HttpGet("{key}")]
    public IActionResult Get(string key)
    {
        var path = GetPath(_configOptions.DataPath, _filesCache.Get(key));
            
        return GetFile(path);
    }


    protected IActionResult GetFile(string path)
    {
        if (System.IO.File.Exists(path))
        {
            var stream = new StreamReader(path).BaseStream;

            return File(stream, "application/octet-stream", enableRangeProcessing: true);
        }
        else
        {
            return NotFound();
        }
    }

    protected static string GetPath(string dataPath, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        if (string.IsNullOrWhiteSpace(dataPath))
            return filePath;
        
        return dataPath + filePath;
    }
}
