using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Data.Source.Web.Configuration.Options;

namespace Unite.Data.Source.Web.Controllers;

[Authorize]
[Route("api/bam")]
public class BamController : FileController
{
    public BamController(ConfigOptions configOptions) : base(configOptions)
    {
    }


    [HttpGet("{key}/index")]
    public IActionResult GetIndex(string key)
    {
        var path = GetPath(_configOptions.DataPath, _filesCache.Get(key)) + ".bai";

        return GetFile(path);
    }

    [HttpGet("{key}/hash")]
    public IActionResult GetHash(string key)
    {
        var path = GetPath(_configOptions.DataPath, _filesCache.Get(key)) + ".bai.md5";

        return GetFile(path);
    }
}
