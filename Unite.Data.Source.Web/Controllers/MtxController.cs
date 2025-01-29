using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Data.Source.Web.Configuration.Options;

namespace Unite.Data.Source.Web.Controllers;

// [Authorize]
[Route("api/mtx")]
public class MtxController : FileController
{
    public MtxController(ConfigOptions configOptions) : base(configOptions)
    {
    }


    [HttpGet("{key}/features")]
    public IActionResult GetFeatures(string key)
    {
        var path = Path.GetDirectoryName(GetPath(_configOptions.DataPath, _filesCache.Get(key)));

        return GetDirectoryFile(path, "features");
    }

    [HttpGet("{key}/barcodes")]
    public IActionResult GetBarcodes(string key)
    {
        var path = Path.GetDirectoryName(GetPath(_configOptions.DataPath, _filesCache.Get(key)));

        return GetDirectoryFile(path, "barcodes");
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
