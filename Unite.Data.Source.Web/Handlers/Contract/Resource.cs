using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers.Contract;

public class Resource
{
    [Column("type")]
    public string Type { get; set; }

    [Column("format")]
    public string Format { get; set; }

    [Column("path")]
    public string Path { get; set; }


    public Resource(string type, string format, string path)
    {
        Type = type;
        Format = format;
        Path = path;
    }
}
