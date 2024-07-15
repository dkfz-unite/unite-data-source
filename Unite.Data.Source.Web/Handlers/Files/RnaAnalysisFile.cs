using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers.Files;

public class RnaAnalysisFile : BaseFile
{
    [Column("tsample_donor_id")]
    public string TSampleDonorId { get; set; }

    [Column("tsample_specimen_id")]
    public string TSampleSpecimenId { get; set; }

    [Column("tsample_specimen_type")]
    public string TSampleSpecimenType { get; set; }

    [Column("tsample_analysis_type")]
    public string TSampleAnalysisType { get; set; }

    [Column("tsample_cells_number")]
    public int? TSampleCellsNumber { get; set; }

    [Column("tsample_genes_model")]
    public string TSampleGenesModel { get; set; }


    public override string ToString()
    {
        var comments = new List<string>();

        if (!string.IsNullOrWhiteSpace(TSampleDonorId))
            comments.Add($"tsample_donor_id: {TSampleDonorId}");

        if (!string.IsNullOrWhiteSpace(TSampleSpecimenId))
            comments.Add($"tsample_specimen_id: {TSampleSpecimenId}");

        if (!string.IsNullOrWhiteSpace(TSampleSpecimenType))
            comments.Add($"tsample_specimen_type: {TSampleSpecimenType}");

        if (!string.IsNullOrWhiteSpace(TSampleAnalysisType))
            comments.Add($"tsample_analysis_type: {TSampleAnalysisType}");

        if (TSampleCellsNumber.HasValue)
            comments.Add($"tsample_cells_number: {TSampleCellsNumber.Value}");

        if (!string.IsNullOrWhiteSpace(TSampleGenesModel))
            comments.Add($"tsample_genes_model: {TSampleGenesModel}");

        return comments.IsNotEmpty() ? string.Join(Environment.NewLine, comments) : null;
    }
}
