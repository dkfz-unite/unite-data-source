using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers;

public class FileMetadata
{
    private string _reader;
    private string _format;
    private string _archive;
    private string _path;

    /// <summary>
    /// Type of the reader to read the file content (cmd/ssm, cmd/cnv, cmd/sv, cmd/exp, etc.).
    /// </summary>
    [Column("reader")] 
    public string Reader { get => _reader?.Trim().ToLower(); set => _reader = value; }

    /// <summary>
    /// Format of the file (tsv, csv, vcf, bam, mtx, etc.).
    /// </summary>
    [Column("format")]
    public string Format { get => _format?.Trim().ToLower(); set => _format = value; }

    /// <summary>
    /// Archive type of the file (zip, tar, gz, etc.).
    /// </summary>
    [Column("archive")]
    public string Archive { get => _archive?.Trim().ToLower(); set => _archive = value; }

    /// <summary>
    /// Path to the file or folder with the data.
    /// </summary>
    [Column("path")]
    public string Path { get => _path?.Trim(); set => _path = value; }


    // private static string GetPath(string value)
    // {
    //     if (string.IsNullOrWhiteSpace(value))
    //         return null;

    //     return value.Trim().Replace(" ", "\\ ");
    // }
}
