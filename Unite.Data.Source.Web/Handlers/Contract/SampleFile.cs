using Unite.Data.Source.Web.Handlers.Contract.Extensions;
using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers.Contract;

public record SampleFile : IResourceFile
{
    public const string DonorIdColumn = "donor_id";
    public const string SpecimenIdColumn = "specimen_id";
    public const string SpecimenTypeColumn = "specimen_type";
    public const string AnalysisTypeColumn = "analysis_type";
    public const string AnalysisDateColumn = "analysis_date";
    public const string AnalysisDayColumn = "analysis_day";
    public const string GenomeColumn = "genome";
    public const string BatchColumn = "batch";
    public const string FormatColumn = "format";
    public const string PathColumn = "path";
    public const string ResourcesColumn = "resources";


    [GroupKey]
    [Column(DonorIdColumn)]
    public string DonorId { get; set; }

    [GroupKey]
    [Column(SpecimenIdColumn)]
    public string SpecimenId { get; set; }

    [GroupKey]
    [Column(SpecimenTypeColumn)]
    public string SpecimenType { get; set; }

    [GroupKey]
    [Column(AnalysisTypeColumn)]
    public string AnalysisType { get; set; }

    [GroupKey]
    [Column(AnalysisDateColumn)]
    public string AnalysisDate { get; set; }

    [GroupKey]
    [Column(AnalysisDayColumn)]
    public string AnalysisDay { get; set; }

    [GroupKey]
    [Column(GenomeColumn)]
    public string Genome { get; set; }

    [GroupKey]
    [Column(BatchColumn)]
    public string Batch { get; set; }

    [Column(FormatColumn)]
    public string Format { get; set; }

    [Column(PathColumn)]
    public string Path { get; set; }


    public MultipartFormDataContent AsForm()
    {
        return new MultipartFormDataContent()
            .AddField(DonorIdColumn, DonorId)
            .AddField(SpecimenIdColumn, SpecimenId)
            .AddField(SpecimenTypeColumn, SpecimenType)
            .AddField(AnalysisTypeColumn, AnalysisType)
            .AddField(AnalysisDateColumn, AnalysisDate)
            .AddField(AnalysisDayColumn, AnalysisDay)
            .AddField(GenomeColumn, Genome)
            .AddField(BatchColumn, Batch);
    }

    public Resource AsResource(string type, string url)
    {
        var name = System.IO.Path.GetFileName(Path);

        return new Resource(name, type, Format, url);
    }
}
