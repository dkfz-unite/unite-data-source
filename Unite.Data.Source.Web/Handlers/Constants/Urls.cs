namespace Unite.Data.Source.Web.Handlers.Constants;

public static class Urls
{
    private const string _format = "tsv";

    public static class Donor
    {
        public const string Entry = $"entries/{_format}"; // Donors with clinical data
        public const string Treatment = $"treatments/{_format}"; // Treatments
    }

    public static class Image
    {
        public static class Entry
        {
            public const string Mr = $"entries/mr/{_format}"; // MRs
            public const string Ct = $"entries/ct/{_format}"; // CTs
        }

        public const string Feature = $"analysis/radiomics/{_format}"; // Radiomics features (RFE analysis)
    }

    public static class Specimen
    {
        public static class Entry // Specimens with molecular data
        {
            public const string Material = $"entries/material/{_format}"; // Materials
            public const string Line = $"entries/line/{_format}"; // Cell lines
            public const string Organoid = $"entries/organoid/{_format}"; // Organoids
            public const string Xenograft = $"entries/xenograft/{_format}"; // Xenografts
        }

        public const string Intervention = $"interventions/{_format}"; // Interventions
        public const string Drug = $"analysis/drugs/{_format}"; // Drugs screenings (DSA analysis)
    }

    public static class Genome
    {
        public static class Dna
        {
            public const string Sample = $"dna/sample/{_format}"; // DNA samples
            public const string Sm = $"dna/analysis/sm/{_format}"; // SMs
            public const string Cnv = $"dna/analysis/cnv/{_format}"; // CNVs
            public const string Sv = $"dna/analysis/sv/{_format}"; // SVs
        }

        public static class Meth
        {
            public const string Sample = $"meth/sample/{_format}"; // DNA Methylation samples
            public const string Level = $"meth/analysis/levels/{_format}"; // DNA Methylation levels
        }

        public static class Rna
        {
            public const string Sample = $"rna/sample/{_format}"; // Bulk RNA sample
            public const string Exp = $"rna/analysis/exp/{_format}"; // Bulk RNA expressions
        }

        public static class Rnasc
        {
            public const string Sample = $"rnasc/sample/{_format}"; // Single cell RNA sample
            public const string Exp = $"rnasc/analysis/exp/{_format}"; // Single cell RNA expressions
        }
    }
}
