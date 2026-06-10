using Microsoft.AspNetCore.Mvc;
using Unite.Data.Source.Web.Configuration.Options;
using Unite.Data.Source.Web.Handlers;

namespace Unite.Data.Source.Web.Controllers;

// [Authorize]
[Route("api/file")]
public class FileController : Controller
{
    protected readonly ConfigOptions _configOptions;
    protected readonly HostFilesCache _filesCache;
    private readonly ILogger<FileController> _logger;


    public FileController(ConfigOptions configOptions, ILogger<FileController> logger)
    {
        _configOptions = configOptions;
        _logger = logger;
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
            try
            {
                var stream = new StreamReader(path).BaseStream;

                return File(stream, "application/octet-stream", enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file: {Path}", path);
                return StatusCode(500, "Internal server error");
            }
        }
        else
        {
            _logger.LogWarning("File not found: {Path}", path);
            return NotFound();
        }
    }

    protected static string GetPath(string dataPath, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        if (string.IsNullOrWhiteSpace(dataPath))
            return filePath;
        
        return Path.GetFullPath(Path.Combine(dataPath, filePath));
    }
}
