using Unite.Data.Source.Web.Handlers.Contract.Extensions;
using Unite.Essentials.Tsv.Attributes;

namespace Unite.Data.Source.Web.Handlers.Contract;

public record ResultFile
{
    public const string DonorIdColumn = "donor_id";
    public const string SpecimenIdColumn = "specimen_id";
    public const string SpecimenTypeColumn = "specimen_type";
    public const string MatchedSpecimenIdColumn = "matched_specimen_id";
    public const string MatchedSpecimenTypeColumn = "matched_specimen_type";
    public const string AnalysisTypeColumn = "analysis_type";
    public const string AnalysisDateColumn = "analysis_date";
    public const string AnalysisDayColumn = "analysis_day";
    public const string PurityColumn = "purity";
    public const string PloidyColumn = "ploidy";
    public const string CellsColumn = "cells";
    public const string GenomeColumn = "genome";
    public const string FormatColumn = "format";
    public const string ReaderColumn = "reader";
    public const string PathColumn = "path";
    public const string ResourcesColumn = "resources";
    public const string EntriesColumn = "entries";


    [Column(DonorIdColumn)]
    public string DonorId { get; set; }

    [Column(SpecimenIdColumn)]
    public string SpecimenId { get; set; }

    [Column(SpecimenTypeColumn)]
    public string SpecimenType { get; set; }

    [Column(MatchedSpecimenIdColumn)]
    public string MatchedSpecimenId { get; set; }

    [Column(MatchedSpecimenTypeColumn)]
    public string MatchedSpecimenType { get; set; }

    [Column(AnalysisTypeColumn)]
    public string AnalysisType { get; set; }

    [Column(AnalysisDateColumn)]
    public string AnalysisDate { get; set; }

    [Column(AnalysisDayColumn)]
    public string AnalysisDay { get; set; }

    [Column(PurityColumn)]
    public string Purity { get; set; }

    [Column(PloidyColumn)]
    public string Ploidy { get; set; }

    [Column(CellsColumn)]
    public string Cells { get; set; }

    [Column(GenomeColumn)]
    public string Genome { get; set; }

    [Column(FormatColumn)]
    public string Format { get; set; }

    [Column(ReaderColumn)]
    public string Reader { get; set; }

    [Column(PathColumn)]
    public string Path { get; set; }


    public MultipartFormDataContent AsForm()
    {
        return new MultipartFormDataContent()
            .AddField(DonorIdColumn, DonorId)
            .AddField(SpecimenIdColumn, SpecimenId)
            .AddField(SpecimenTypeColumn, SpecimenType)
            .AddField(MatchedSpecimenIdColumn, MatchedSpecimenId)
            .AddField(MatchedSpecimenTypeColumn, MatchedSpecimenType)
            .AddField(AnalysisTypeColumn, AnalysisType)
            .AddField(AnalysisDateColumn, AnalysisDate)
            .AddField(AnalysisDayColumn, AnalysisDay)
            .AddField(PurityColumn, Purity)
            .AddField(PloidyColumn, Ploidy)
            .AddField(CellsColumn, Cells)
            .AddField(GenomeColumn, Genome);
    }

    public Resource AsResource(string type, string url)
    {
        var name = System.IO.Path.GetFileName(Path);

        return new Resource(name, type, Format, url);
    }
}
