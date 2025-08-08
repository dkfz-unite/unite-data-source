using Unite.Data.Source.Web.Handlers.Contract.Extensions;
using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers.Contract;

public class SampleFile
{
    public const string DonorIdColumn = "donor_id";
    public const string SpecimenIdColumn = "specimen_id";
    public const string SpecimenTypeColumn = "specimen_type";
    public const string AnalysisTypeColumn = "analysis_type";
    public const string AnalysisDateColumn = "analysis_date";
    public const string AnalysisDayColumn = "analysis_day";
    public const string GenomeColumn = "genome";
    public const string FormatColumn = "format";
    public const string PathColumn = "path";
    public const string ResourcesColumn = "resources";


    [Column(DonorIdColumn)]
    public string DonorId { get; set; }

    [Column(SpecimenIdColumn)]
    public string SpecimenId { get; set; }

    [Column(SpecimenTypeColumn)]
    public string SpecimenType { get; set; }

    [Column(AnalysisTypeColumn)]
    public string AnalysisType { get; set; }

    [Column(AnalysisDateColumn)]
    public string AnalysisDate { get; set; }

    [Column(AnalysisDayColumn)]
    public string AnalysisDay { get; set; }

    [Column(GenomeColumn)]
    public string Genome { get; set; }

    [Column(FormatColumn)]
    public string Format { get; set; }

    [Column(PathColumn)]
    public string Path { get; set; }


    public override bool Equals(object obj)
    {
        return obj is SampleFile file &&
               DonorId == file.DonorId &&
               SpecimenId == file.SpecimenId &&
               SpecimenType == file.SpecimenType &&
               AnalysisType == file.AnalysisType &&
               AnalysisDate == file.AnalysisDate &&
               AnalysisDay == file.AnalysisDay &&
               Genome == file.Genome;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DonorId, SpecimenId, SpecimenType, AnalysisType, AnalysisDate, AnalysisDay, Genome);
    }

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
            .AddField(FormatColumn, Format);
    }

    public Resource AsResource(string type, string url)
    {
        var name = System.IO.Path.GetFileName(Path);

        return new Resource(name, type, Format, url);
    }
}
