namespace Unite.Data.Source.Web.Handlers.Constants;

public static class DataFormats
{
    public static class Omics
    {
        public static class Dna
        {
            public static class Sm
            {
                public const string Tsv = "tsv"; // Default unite format
                public const string Vcf = "vcf"; // Variant Call Format
            }

            public static class Cnv
            {
                public const string Tsv = "tsv"; // Default unite format
                public const string Aceseq = "aceseq"; // AceSeq format
            }

            public static class Sv
            {
                public const string Tsv = "tsv"; // Default unite format
                public const string DkfzSophia = "dkfz/sophia"; // DKFZ Sophia format
            }
        }

        public static class Rna
        {
            public static class Exp
            {
                public const string Tsv = "tsv"; // Default unite format
                public const string DkfzRnaseq = "dkfz/rnaseq"; // DKFZ RNAseq format
            }
        }
    }
}
