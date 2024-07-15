using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers.Files;

public class DnaAnalysisFile : BaseFile
{
    [Column("tsample_donor_id")]
    public string TSampleDonorId { get; set; }

    [Column("tsample_specimen_id")]
    public string TSampleSpecimenId { get; set; }

    [Column("tsample_specimen_type")]
    public string TSampleSpecimenType { get; set; }

    [Column("tsample_analysis_type")]
    public string TSampleAnalysisType { get; set; }

    [Column("tsample_purity")]
    public double? TSamplePurity { get; set; }

    [Column("tsample_ploidy")]
    public int? TSamplePloidy { get; set; }

    [Column("msample_donor_id")]
    public string MSampleDonorId { get; set; }

    [Column("msample_specimen_id")]
    public string MSampleSpecimenId { get; set; }

    [Column("msample_specimen_type")]
    public string MSampleSpecimenType { get; set; }

    [Column("msample_analysis_type")]
    public string MSampleAnalysisType { get; set; }


    public override string ToString()
    {
        var comments = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(TSampleDonorId))
            comments.Add($"# tsample_donor_id: {TSampleDonorId}");
        
        if (!string.IsNullOrWhiteSpace(TSampleSpecimenId))
            comments.Add($"# tsample_specimen_id: {TSampleSpecimenId}");

        if (!string.IsNullOrWhiteSpace(TSampleSpecimenType))
            comments.Add($"# tsample_specimen_type: {TSampleSpecimenType}");

        if (!string.IsNullOrWhiteSpace(TSampleAnalysisType))
            comments.Add($"# tsample_analysis_type: {TSampleAnalysisType}");

        if (TSamplePurity.HasValue)
            comments.Add($"# tsample_purity: {TSamplePurity.Value}");

        if (TSamplePloidy.HasValue)
            comments.Add($"# tsample_ploidy: {TSamplePloidy.Value}");

        if (!string.IsNullOrWhiteSpace(MSampleDonorId))
            comments.Add($"# msample_donor_id: {MSampleDonorId}");

        if (!string.IsNullOrWhiteSpace(MSampleSpecimenId))
            comments.Add($"# msample_specimen_id: {MSampleSpecimenId}");

        if (!string.IsNullOrWhiteSpace(MSampleSpecimenType))
            comments.Add($"# msample_specimen_type: {MSampleSpecimenType}");

        if (!string.IsNullOrWhiteSpace(MSampleAnalysisType))
            comments.Add($"# msample_analysis_type: {MSampleAnalysisType}");

        return comments.IsNotEmpty() ? string.Join(Environment.NewLine, comments) : null;
    }
}
