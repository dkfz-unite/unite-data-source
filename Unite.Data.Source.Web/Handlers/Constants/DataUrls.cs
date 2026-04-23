namespace Unite.Data.Source.Web.Handlers.Constants;

public static class DataUrls
{
    public static class Donor
    {
        public const string Entry = $"entries"; // Donors
        public const string Treatment = $"treatments"; // Treatments
    }

    public static class Image
    {
        public static class Entry
        {
            public const string Mr = $"entries/mr"; // MRs
            public const string Ct = $"entries/ct"; // CTs
        }

        public const string Feature = $"analysis/radiomics"; // Radiomics features (RFE analysis)
    }

    public static class Specimen
    {
        public static class Entry // Specimens with molecular data
        {
            public const string Material = $"entries/material"; // Materials
            public const string Line = $"entries/line"; // Cell lines
            public const string Organoid = $"entries/organoid"; // Organoids
            public const string Xenograft = $"entries/xenograft"; // Xenografts
        }

        public const string Intervention = $"interventions"; // Interventions
        public const string Drug = $"analysis/dsa"; // Drugs screenings (DSA analysis)
    }

    public static class Omics
    {
        public static class Dna
        {
            public const string Sample = $"dna/sample"; // DNA samples
            public const string Sm = $"dna/analysis/sm"; // SMs
            public const string Cnv = $"dna/analysis/cnv"; // CNVs
            public const string Sv = $"dna/analysis/sv"; // SVs
            public const string Cnvp = $"dna/analysis/cnvp"; // CNVPs
        }

        public static class Meth
        {
            public const string Sample = $"meth/sample"; // DNA Methylation samples
            public const string Level = $"meth/analysis/levels"; // DNA Methylation levels
        }

        public static class Rna
        {
            public const string Sample = $"rna/sample"; // Bulk RNA sample
            public const string Exp = $"rna/analysis/exp"; // Bulk RNA expressions
        }

        public static class Rnasc
        {
            public const string Sample = $"rnasc/sample"; // Single cell RNA sample
            public const string Exp = $"rnasc/analysis/exp"; // Single cell RNA expressions
        }

        public static class Prot
        {
            public const string Sample = $"prot/sample"; // Proteomics sample
            public const string Exp = $"prot/analysis/exp"; // Proteomics expressions
        }
    }
}
