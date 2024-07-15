using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers.Files;

public class SampleFile : BaseFile
{
    [Column("donor_id")]
    public string DonorId { get; set; }

    [Column("specimen_id")]
    public string SpecimenId { get; set; }

    [Column("specimen_type")]
    public string SpecimenType { get; set; }

    [Column("analysis_type")]
    public string AnalysisType { get; set; }

    [Column("analysis_date")]
    public DateOnly? AnalysisDate { get; set; }

    [Column("analysis_day")]
    public int? AnalysisDay { get; set; }


    public override string ToString()
    {
        var comments = new List<string>();

        if (!string.IsNullOrWhiteSpace(DonorId))
            comments.Add($"donor_id: {DonorId}");

        if (!string.IsNullOrWhiteSpace(SpecimenId))
            comments.Add($"specimen_id: {SpecimenId}");

        if (!string.IsNullOrWhiteSpace(SpecimenType))
            comments.Add($"specimen_type: {SpecimenType}");

        if (!string.IsNullOrWhiteSpace(AnalysisType))
            comments.Add($"analysis_type: {AnalysisType}");

        if (AnalysisDate.HasValue)
            comments.Add($"analysis_date: {AnalysisDate.Value}");

        if (AnalysisDay.HasValue)
            comments.Add($"analysis_day: {AnalysisDay.Value}");

        return comments.IsNotEmpty() ? string.Join(Environment.NewLine, comments) : null;
    }
}
