using System.Runtime.Serialization;

namespace Unite.Data.Source.Web.Handlers.Contract.Enums;

public enum AnalysisType
{
    /// <summary>
    /// DNA - Whole Genome Sequencing.
    /// Produces a fastq file.
    /// Can be aligned to a reference genome.
    /// Variants can be called (SM, CNV, SV).
    /// </summary>
    [EnumMember(Value = "WGS")]
    WGS = 1,

    /// <summary>
    /// DNA - Whole Exome Sequencing.
    /// Produces a fastq file.
    /// Can be aligned to a reference genome.
    /// Variants can be called (SM, CNV, SV).
    /// </summary>
    [EnumMember(Value = "WES")]
    WES = 2,

    /// <summary>
    /// Bulk RNA Sequencing.
    /// Produces a fastq file.
    /// Can be aligned to a reference genome.
    /// Gene expression levels can be quantified.
    /// </summary>
    [EnumMember(Value = "RNASeq")]
    RNASeq = 3,

    /// <summary>
    /// Single Cell RNA Sequencing.
    /// Produces a fastq file.
    /// Can be aligned to a reference genome.
    /// Gene expression levels can be quantified for each cell.
    /// </summary>
    [EnumMember(Value = "scRNASeq")]
    RNASeqSc = 4,

    /// <summary>
    /// Single Nucleus RNA Sequencing.
    /// Produces a fastq file.
    /// Can be aligned to a reference genome.
    /// Gene expression levels can be quantified for each cell nucleus.
    /// </summary>
    [EnumMember(Value = "snRNASeq")]
    RNASeqSn = 5,

    /// <summary>
    /// Bulk ATAC Sequencing.
    /// Produces a fastq file.
    /// Can be aligned to a reference genome.
    /// Can be used to identify open chromatin regions.
    /// </summary>
    [EnumMember(Value = "ATACSeq")]
    ATACSeq = 6,

    /// <summary>
    /// Single Cell ATAC Sequencing.
    /// Produces a fastq file.
    /// Can be aligned to a reference genome.
    /// Can be used to identify open chromatin regions for each cell.
    /// </summary>
    [EnumMember(Value = "scATACSeq")]
    ATACSeqSc = 7,

    /// <summary>
    /// Single Nucleus ATAC Sequencing.
    /// Produces a fastq file.
    /// Can be aligned to a reference genome.
    /// Can be used to identify open chromatin regions for each cell nucleus.
    /// </summary>
    [EnumMember(Value = "snATACSeq")]
    ATACSeqSn = 8,

    /// <summary>
    /// Illumina Infinium Methylation Arrays Assay.
    /// Produces IDAT files (Red and Green) from fluorescence intensity measurements.
    /// Can be used to quantify methylation levels (Beta and M-values) at predefined CpG sites.
    /// </summary>
    [EnumMember(Value = "MethArray")]
    MethArray = 9,

    /// <summary>
    /// Whole Genome Bisulfite Sequencing.
    /// Produces a bisulfite-converted DNA sequence in fastq format.
    /// The fastq files reflect methylation status by showing converted/unconverted bases.
    /// Can be aligned to a reference genome.
    /// Can be used to quantify methylation levels (Beta and M-values).
    /// </summary>
    [EnumMember(Value = "WGBS")]
    WGBS = 10,

    /// <summary>
    /// Reduced Representation Bisulfite Sequencing.
    /// Produces a bisulfite-converted DNA sequence in fastq format.
    /// The fastq files reflect methylation status by showing converted/unconverted bases.
    /// Can be aligned to a reference genome.
    /// Can be used to quantify methylation levels (Beta and M-values).
    /// </summary>
    [EnumMember(Value = "RRBS")]
    RRBS = 11
}
