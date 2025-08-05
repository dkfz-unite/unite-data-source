using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers.Contract;

public class Resource
{
    [Column("name")]
    public string Name { get; set; }

    /// <summary>
    /// Type of the data e.g. "dna", "rna", "meth"
    /// </summary>
    [Column("type")]
    public string Type { get; set; }

    /// <summary>
    /// Format of the data e.g. "fastq", "bam", "tsv"
    /// </summary>
    [Column("format")]
    public string Format { get; set; }

    [Column("url")]
    public string Url { get; set; }
    

    public Resource(string name, string type, string format, string url)
    {
        Name = name;
        Type = type;
        Format = format;
        Url = url;
    }
}
