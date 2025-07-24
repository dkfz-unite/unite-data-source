using Unite.Data.Source.Web.Handlers.Contract.Enums;
using Unite.Data.Source.Web.Handlers.Contract.Extensions;
using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers.Contract;

public class SampleFile
{
    private const string DonorIdColumn = "donor_id";
    private const string SpecimenIdColumn = "specimen_id";
    private const string SpecimenTypeColumn = "specimen_type";
    private const string AnalysisTypeColumn = "analysis_type";
    private const string AnalysisDateColumn = "analysis_date";
    private const string AnalysisDayColumn = "analysis_day";
    private const string GenomeColumn = "genome";
    private const string TypeColumn = "type";
    private const string PathColumn = "path";


    [Column(DonorIdColumn)]
    public string DonorId { get; set; }

    [Column(SpecimenIdColumn)]
    public string SpecimenId { get; set; }

    [Column(SpecimenTypeColumn)]
    public SpecimenType SpecimenType { get; set; }

    [Column(AnalysisTypeColumn)]
    public AnalysisType AnalysisType { get; set; }

    [Column(AnalysisDateColumn)]
    public DateOnly? AnalysisDate { get; set; }

    [Column(AnalysisDayColumn)]
    public int? AnalysisDay { get; set; }

    [Column(GenomeColumn)]
    public string Genome { get; set; }

    [Column(TypeColumn)]
    public string Type { get; set; }

    [Column(PathColumn)]
    public string Path { get; set; }


    public MultipartFormDataContent AsForm()
    {
        return new MultipartFormDataContent()
            .AddField(DonorIdColumn, DonorId)
            .AddField(SpecimenIdColumn, SpecimenId)
            .AddField(SpecimenTypeColumn, SpecimenType.ToDefinitionString())
            .AddField(AnalysisTypeColumn, AnalysisType.ToDefinitionString())
            .AddField(AnalysisDateColumn, AnalysisDate)
            .AddField(AnalysisDayColumn, AnalysisDay)
            .AddField(GenomeColumn, Genome)
            .AddField(TypeColumn, Type);
    }

    public Resource AsResource(string type, string url)
    {
        var name = System.IO.Path.GetFileName(Path);

        return new Resource(name, type, Type, null, url);
    }
}
