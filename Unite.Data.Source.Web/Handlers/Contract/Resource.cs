using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers.Contract;

public class Resource
{
    [Column("name")]
    public string Name { get; set; }

    [Column("type")]
    public string Type { get; set; }

    [Column("format")]
    public string Format { get; set; }

    [Column("archive")]
    public string Archive { get; set; }

    [Column("url")]
    public string Url { get; set; }
    

    public Resource(string name, string type, string format, string archive, string url)
    {
        Name = name;
        Type = type;
        Format = format;
        Archive = archive;
        Url = url;
    }
}
