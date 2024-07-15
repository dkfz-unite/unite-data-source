using Microsoft.AspNetCore.Mvc;

namespace Unite.Data.Source.Web.Controllers;

[Route("api/")]
public class DefaultController : Controller
{
    [HttpGet]
    public IActionResult Get()
    {
        var date = DateTime.UtcNow;

        return Json(date);
    }
}
