using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers;

public class FileMetadata
{
    /// <summary>
    /// Format of the file (tsv, csv, vcf, bam, mex, etc.).
    /// </summary>
    [Column("format")]
    public string Format { get; set; }

    /// <summary>
    /// Type of the reader to read the file content (cmd/ssm, cmd/cnv, cmd/sv, cmd/exp, etc.).
    /// </summary>
    [Column("reader")] 
    public string Reader { get; set; }

    /// <summary>
    /// Path to the file or folder with the data.
    /// </summary>
    [Column("path")]
    public string Path { get; set; }
}
