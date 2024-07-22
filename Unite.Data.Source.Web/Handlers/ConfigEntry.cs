using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv;
using Unite.Essentials.Tsv.Attributes;
using Unite.Essentials.Tsv.Converters;

namespace Unite.Data.Source.Web.Handlers;

public class ConfigEntry
{
    /// <summary>
    /// Name of the data crawler to use (e.g. "mycrawler/").
    /// </summary>
    [Column("crawler")]
    public string Crawler { get; set; }

    /// <summary>
    /// Types of the data to be found by the crawler (e.g. dna, dna/ssm, dna/cnv, dna/sv, rna, rna/exp, rnasc, rnasc/exp, etc.).
    /// </summary>
    [Column("types")]
    public string[] Types { get; set; }

    /// <summary>
    /// Absolute path to the folder to explore (e.g. "/data/project").
    /// </summary>
    [Column("path", typeof(StringArrayConverter))]
    public string Path { get; set; }

   
    public static ConfigEntry[] Read(string path)
    {
        using var reader = new StreamReader(path);

        return TsvReader.Read<ConfigEntry>(reader).ToArray();
    }
}

internal class StringArrayConverter : IConverter
{
    public object Convert(string value, string row)
    {
        return value?
            .Split(',')
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Select(v => v.ToLower())
            .ToArrayOrNull();
    }

    public string Convert(object value, object row)
    {
        throw new NotImplementedException();
    }
}
