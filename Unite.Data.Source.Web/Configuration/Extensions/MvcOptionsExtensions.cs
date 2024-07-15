using Microsoft.AspNetCore.Mvc;
using Unite.Data.Source.Web.Configuration.Filters;

namespace Unite.Data.Source.Web.Configuration.Extensions;

public static class MvcOptionsExtensions
{
    public static void AddMvcOptions(this MvcOptions options)
    {
        options.Filters.Add(typeof(DefaultActionFilter));
        options.Filters.Add(typeof(DefaultExceptionFilter));
    }
}
