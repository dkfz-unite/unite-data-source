namespace Unite.Data.Source.Web.Handlers.Constants;

public static class DataTypes
{
    // don
    // dont-trt
    // mri
    // ct
    // img-rad
    // mat
    // lne
    // org
    // xen
    // spe-int
    // spe-drg
    // dna
    // dna-ssm
    // dna-cnv
    // dna-sv
    // meth
    // meth-lvl
    // rna
    // rna-exp
    // rnasc
    // rnasc-exp

    public static class Donor
    {
        public const string Entry = "don"; // Donors with clinical data
        public const string Treatment = "don-trt"; // Treatments
    }

    public static class Image
    {
        public static class Entry
        {
            public const string Mri = "mri"; // MRIs
            public const string Ct = "ct"; // CTs
        }

        public const string Feature = "img-rad"; // Radiomics features (RFE analysis)
    }

    public static class Specimen
    {
        public static class Entry // Specimens with molecular data
        {
            public const string Material = "mat"; // Materials
            public const string Line = "lne"; // Cell lines
            public const string Organoid = "org"; // Organoids
            public const string Xenograft = "xen"; // Xenografts
        }

        public const string Intervention = "spe-int"; // Interventions
        public const string Drug = "spe-drg"; // Drugs screenings (DSA analysis)
    }

    public static class Genome
    {
        public static class Dna
        {
            public const string Sample = "dna"; // DNA samples
            public const string Ssm = "dna-ssm"; // SSMs
            public const string Cnv = "dna-cnv"; // CNVs
            public const string Sv = "dna-sv"; // SVs
        }

        public static class Meth
        {
            public const string Sample = "meth"; // Methylation samples (fasta, fastq, mab, idat)
            public const string Level = "meth-lvl"; // Methylation levels (beta and/or M-values)
        }

        public static class Rna
        {
            public const string Sample = "rna"; // Bulk RNA sample
            public const string Exp = "rna-exp"; // Bulk RNA expressions
        }

        public static class Rnasc
        {
            public const string Sample = "rnasc"; // Single cell RNA sample
            public const string Exp = "rnasc-exp"; // Single cell RNA expressions
        }
    }
}
