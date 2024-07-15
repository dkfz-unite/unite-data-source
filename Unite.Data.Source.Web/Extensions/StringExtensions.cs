namespace Unite.Data.Source.Web.Extensions;

public static class StringExtensions
{
    public static string AbsolutePath(this string path)
    {
        if (path.StartsWith('/'))
            return path;

        var relativePath = Path.GetRelativePath(Environment.CurrentDirectory, path);
        var absolutePath = Path.GetFullPath(relativePath);
        return absolutePath;
    }
}
