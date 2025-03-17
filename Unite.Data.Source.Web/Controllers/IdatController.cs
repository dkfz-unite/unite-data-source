using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Data.Source.Web.Configuration.Options;

namespace Unite.Data.Source.Web.Controllers;

// [Authorize]
[Route("api/idat")]
public class IdatController : FileController
{
    public IdatController(ConfigOptions configOptions) : base(configOptions)
    {
    }


    [HttpGet("{key}")]
    public IActionResult GetGreen(string key)
    {
        var path = Path.GetDirectoryName(GetPath(_configOptions.DataPath, _filesCache.Get(key)));

        return GetDirectoryFile(path, "Green");
    }

    [HttpGet("{key}/red")]
    public IActionResult GetRed(string key)
    {
        var path = Path.GetDirectoryName(GetPath(_configOptions.DataPath, _filesCache.Get(key)));

        return GetDirectoryFile(path, "Red");
    }


    private IActionResult GetDirectoryFile(string directoryPath, string fileName)
    {
        if (System.IO.Directory.Exists(directoryPath))
        {
            var directory = new DirectoryInfo(directoryPath);

            var file = directory.GetFiles().FirstOrDefault(file => file.Name.Contains(fileName, StringComparison.InvariantCultureIgnoreCase));

            return GetFile(file.FullName);
        }
        else
        {
            return NotFound();
        }
    }
}
