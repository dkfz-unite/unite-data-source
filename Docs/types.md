# Types
Data types are the types of data UNITE Data Source Service can work with.

Types have two subtypes:
- **Data** - Data is read from the files and sent to the UNITE portal content wise.  
             Such data is deeply integrated with other data in the UNITE portal.
- **Resource** - Data is not read, such files are hosted by the Data Source Service as resources and only their metadata (with access url) is sent to the UNITE portal.  
                 Such data is integrated only metadata wise, the portal will know it exists and will be able to access it via the generated url for the analysis purposes.

## DNA

### Sample
DNA sample files in different formats.

Key: `dna`.  
Sheet file name: `dna.tsv`.  
Subtype: `resource`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `analysis_type`__*__ - Sample analysis (sequencing) type (`WES`, `WGS`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `format`__*__ - File format (`fasta`, `fastq`, `bam`, `bam.bai`, `bam.bai.md5`).
- `path`__*__ - Path to the file.

#### Example
`/mnt/data/project/dna.tsv`
```tsv
donor_id	specimen_id	specimen_type	analysis_type	analysis_date	genome	format	path
Donor1    Normal    Material	WGS    2023-01-01    GRCh37    bam    omics/WGS/Donor1/normal.bam
Donor1    Normal    Material	WGS    2023-01-01    GRCh37    bam.bai    omics/WGS/Donor1/normal.bam.bai
Donor1    Normal    Material	WGS    2023-01-01    GRCh37    bam.bai.md5    omics/WGS/Donor1/normal.bam.bai.md5
Donor1    Tumor    Material	WGS    2023-01-01    GRCh37    bam    omics/WGS/Donor1/tumor.bam
Donor1    Tumor    Material	WGS    2023-01-01    GRCh37    bam.bai    omics/WGS/Donor1/tumor.bam.bai
Donor1    Tumor    Material	WGS    2023-01-01    GRCh37    bam.bai.md5    omics/WGS/Donor1/tumor.bam.bai.md5
```

### Simple Mutations
Simple mutations data, which is a result of the mutations (SNV, INDEL) calling pipeline.

Key: `dna-sm`.  
Sheet file name: `dna-sm.tsv`.  
Subtype: `data`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `matched_specimen_id` - Matched specimen identifier, if the file is matched to another specimen during the processing.
- `matched_specimen_type` - Matched specimen type, if the file is matched to another specimen during the processing.
- `analysis_type`__*__ - Sample analysis (sequencing) type (`WES`, `WGS`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `reader` - File reaeder / data format (`tsv`, `vcf` or custom `cmd/{name}`).
- `path`__*__ - Path to the file.

More information about supported formats is available [here](https://github.com/dkfz-unite/unite-feed-omics/blob/Docs/models-dna-sm.md#formats).

#### Example
`/mnt/data/project/dna-sm.tsv`
```tsv
donor_id	specimen_id	specimen_type	matched_specimen_id	matched_specimen_type    analysis_type	analysis_date	genome	reader	path
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    tsv    omics/WGS/Donor1/snv.tsv
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    tsv    omics/WGS/Donor1/indel.tsv
Donor2    Tumor    Material    Normal    Material    WGS    2023-01-02    GRCh37    vcf    omics/WGS/Donor2/snv.vcf
Donor2    Tumor    Material    Normal    Material    WGS    2023-01-02    GRCh37    vcf    omics/WGS/Donor2/indel.vcf
Donor3    Tumor    Material    Normal    Material    WGS    2023-01-03    GRCh37    cmd/dna-sm   omics/WGS/Donor3/snv.tsv
Donor3    Tumor    Material    Normal    Material    WGS    2023-01-03    GRCh37    cmd/dna-sm   omics/WGS/Donor3/indel.tsv
```

### Copy Number Variants
Copy number variants data, which is a result of the CNVs calling pipeline.

Key: `dna-cnv`.  
Sheet file name: `dna-cnv.tsv`.  
Subtype: `data`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `matched_specimen_id` - Matched specimen identifier, if the file is matched to another specimen during the processing.
- `matched_specimen_type` - Matched specimen type, if the file is matched to another specimen during the processing.
- `analysis_type`__*__ - Sample analysis (sequencing) type (`WES`, `WGS`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `purity` - Estimated sample purity, if available (e.g. `0.8` for 80%).
- `ploidy` - Estimated sample ploidy, if available (e.g. `2` for diploid, used by default).
- `reader` - File reader / data format (`tsv`, `aceseq` or custom `cmd/{name}`).
- `path`__*__ - Path to the file.

More information about supported formats is available [here](https://github.com/dkfz-unite/unite-feed-omics/blob/Docs/models-dna-cnv.md#formats).

#### Example
`/mnt/data/project/dna-cnv.tsv`
```tsv
donor_id	specimen_id	specimen_type	matched_specimen_id	matched_specimen_type    analysis_type	analysis_date	genome	purity	ploidy	reader	path
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    0.8    2    tsv    omics/WGS/Donor1/cnv.tsv
Donor2    Tumor    Material    Normal    Material    WGS    2023-01-02    GRCh37    0.9    2    aceseq    omics/WGS/Donor2/cnv.tsv
Donor3    Tumor    Material    Normal    Material    WGS    2023-01-03    GRCh37    0.85   2    cmd/cnv   omics/WGS/Donor3/cnv.tsv
```

### Structural Variants
Structural variants data, which is a result of the SVs calling pipeline.

Key: `dna-sv`.  
Sheet file name: `dna-sv.tsv`.  
Subtype: `data`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `matched_specimen_id` - Matched specimen identifier, if the file is matched to another specimen during the processing.
- `matched_specimen_type` - Matched specimen type, if the file is matched to another specimen during the processing.
- `analysis_type`__*__ - Sample analysis (sequencing) type (`WES`, `WGS`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `reader` - File reader / data format (`tsv`, `dkfz-sophia` or custom `cmd/{name}`).
- `path`__*__ - Path to the file.

More information about supported formats is available [here](https://github.com/dkfz-unite/unite-feed-omics/blob/Docs/models-dna-sv.md#formats).

#### Example
`/mnt/data/project/dna-sv.tsv`
```tsv
donor_id	specimen_id	specimen_type	matched_specimen_id	matched_specimen_type    analysis_type	analysis_date	genome	reader	path
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    tsv    omics/WGS/Donor1/sv.tsv
Donor2    Tumor    Material    Normal    Material    WGS    2023-01-02    GRCh37    dkfz/sophia    omics/WGS/Donor2/sv.tsv
Donor3    Tumor    Material    Normal    Material    WGS    2023-01-03    GRCh37    cmd/dna-sv   omics/WGS/Donor3/sv.tsv
```

## Methylation

### Sample
DNA methylation sample files in different formats.

Key: `meth`.  
Sheet file name: `meth.tsv`.  
Subtype: `resource`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `analysis_type`__*__ - Sample analysis (sequencing) type (`MethArray`, `WGBS`, `RRBS`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `format`__*__ - Sample file format (`idat`, `fasta`, `fastq`, `bam`, `bam.bai`, `bam.bai.md5`).
- `path`__*__ - Path to the file.

The data in the `idat` format requires 2 files per donor: `*_Grn.idat` and `*_Red.idat`.  
Do not compress the files, they should be sent as is.  
Do not change the file names.

#### Example
`/mnt/data/project/meth.tsv`
```tsv
donor_id	specimen_id	specimen_type	analysis_type	analysis_date	genome	format	path
Donor1    Tumor    Material	MethArray    2023-01-01    GRCh38    idat    omics/MethArray/Donor1/001-300_Grn.idat
Donor1    Tumor    Material	MethArray    2023-01-01    GRCh38    idat    omics/MethArray/Donor1/001-300_Red.idat
```

## Bulk RNA

### Sample
Bulk RNA sample files in different formats.

Key: `rna`.  
Sheet file name: `rna.tsv`.  
Subtype: `resource`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `analysis_type`__*__ - Sample analysis (sequencing) type (`RNASeq`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `format`__*__ - Sample file format (`fasta`, `fastq`, `bam`, `bam.bai`, `bam.bai.md5`).
- `path`__*__ - Path to the file.


#### Example
`/mnt/data/project/rna.tsv`
```tsv
donor_id	specimen_id	specimen_type	analysis_type	analysis_date	genome	format	path
Donor1    Tumor    Material	RNASeq    2023-01-01    GRCh37    bam    omics/RNASeq/Donor1/tumor.bam
Donor1    Tumor    Material	RNASeq    2023-01-01    GRCh37    bam.bai    omics/RNASeq/Donor1/tumor.bam.bai
Donor1    Tumor    Material	RNASeq    2023-01-01    GRCh37    bam.bai.md5    omics/RNASeq/Donor1/tumor.bam.bai.md5
```

### Expressions
Bulk RNA expressions data, which is a result of the RNA expressions calling pipeline.

Key: `rna-exp`.  
Sheet file name: `rna-exp.tsv`.  
Subtype: `data`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `analysis_type`__*__ - Sample analysis (sequencing) type (`RNASeq`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `reader` - File reader / data format (`tsv`, `dkfz-rnaseq` or custom `cmd/{name}`).
- `path`__*__ - Path to the file.

More information about supported formats is available [here](https://github.com/dkfz-unite/unite-feed-omics/blob/Docs/models-rna-exp.md#formats).

#### Example
`/mnt/data/project/rna-exp.tsv`
```tsv
donor_id	specimen_id	specimen_type	analysis_type	analysis_date	genome	reader	path
Donor1    Tumor    Material	RNASeq    2023-01-01    GRCh37    tsv    omics/RNASeq/Donor1/tumor.tsv
Donor2    Tumor    Material	RNASeq    2023-01-02    GRCh37    dkfz/rnaseq    omics/RNASeq/Donor2/tumor.tsv
Donor3    Tumor    Material	RNASeq    2023-01-03    GRCh37    cmd/rna-exp   omics/RNASeq/Donor3/tumor.tsv
```

## Single Cell RNA

### Sample
Single Cell RNA sample files in different formats.

Key: `rnasc`.  
Sheet file name: `rnasc.tsv`.  
Subtype: `resource`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `analysis_type`__*__ - Sample analysis (sequencing) type (`scRNASeq`, `snRNASeq`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `format`__*__ - Sample file format (`fasta`, `fastq`, `bam`, `bam.bai`, `bam.bai.md5`).
- `path`__*__ - Path to the file.

#### Example
`/mnt/data/project/rnasc.tsv`
```tsv
donor_id	specimen_id	specimen_type	analysis_type	analysis_date	genome	format	path
Donor1    Tumor    Material	scRNASeq    2023-01-01    GRCh38    bam    omics/scRNASeq/Donor1/tumor.bam
Donor1    Tumor    Material	scRNASeq    2023-01-01    GRCh38    bam.bai    omics/scRNASeq/Donor1/tumor.bam.bai
Donor1    Tumor    Material	scRNASeq    2023-01-01    GRCh38    bam.bai.md5    omics/scRNASeq/Donor1/tumor.bam.bai.md5
```

### Expressions
Single Cell RNA expressions data, which is a result of the Single Cell RNA expressions calling pipeline.

Key: `rnasc-exp`.  
Sheet file name: `rnasc-exp.tsv`.  
Subtype: `resource`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `analysis_type`__*__ - Sample analysis (sequencing) type (`scRNASeq`, `snRNASeq`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `cells` - Number of cells.
- `format`__*__ - Resource file format (`mtx`, `tsv`).
- `path`__*__ - Path to the file.

The data in the `10x Genomics` format requires 3 files per donor: `barcodes.tsv.gz`, `features.tsv.gz`, `matrix.mtx.gz`.  
Do not extract the files, they should be sent as is, compressed.  
Do not change the file names.

#### Example
`/mnt/data/project/rnasc-exp.tsv`
```tsv
donor_id	specimen_id	specimen_type	analysis_type	analysis_date	genome  cells	format	path
Donor1    Tumor    Material	scRNASeq    2023-01-01    GRCh38    tsv 5700    omics/scRNASeq/Donor1/barcodes.tsv.gz
Donor1    Tumor    Material	scRNASeq    2023-01-01    GRCh38    tsv 5700    omics/scRNASeq/Donor1/features.tsv.gz
Donor1    Tumor    Material	scRNASeq    2023-01-01    GRCh38    mtx 5700    omics/scRNASeq/Donor1/matrix.mtx.gz
```

## Proteomics

### Sample
Proteomics sample files in different formats.

Key: `prot`.  
Sheet file name: `prot.tsv`.  
Subtype: `resource`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `analysis_type`__*__ - Sample analysis (sequencing) type (`MS`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `format`__*__ - Sample file format (`mzML`, `mzXML`).
- `path`__*__ - Path to the file.


#### Example
`/mnt/data/project/prot.tsv`
```tsv
donor_id	specimen_id	specimen_type	analysis_type	analysis_date	genome	format	path
Donor1    Tumor    Material	MS    2023-01-01    GRCh37    bam    omics/MS/Donor1/tumor.mzML
Donor2    Tumor    Material	MS    2023-01-02    GRCh37    bam    omics/MS/Donor2/tumor.mzXML
Donor3    Tumor    Material	MS    2023-01-03    GRCh37    bam    omics/MS/Donor3/tumor.mzML
```

### Expressions
Proteomics expressions data, which is a result of the Proteomics expressions calling pipeline.

Key: `prot-exp`.  
Sheet file name: `prot-exp.tsv`.  
Subtype: `data`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `analysis_type`__*__ - Sample analysis (sequencing) type (`MS`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome`__*__ - Sample genome version (`GRCh37` or `GRCh38`).
- `reader` - File reader / data format (`tsv`, `diann` or custom `cmd/{name}`).
- `path`__*__ - Path to the file.

More information about supported formats is available [here](https://github.com/dkfz-unite/unite-feed-omics/blob/Docs/models-prot-exp.md#formats).

#### Example
`/mnt/data/project/prot-exp.tsv`
```tsv
donor_id	specimen_id	specimen_type	analysis_type	analysis_date	genome	reader	path
Donor1    Tumor    Material	MS    2023-01-01    GRCh37    tsv    omics/MS/Donor1/tumor.tsv
Donor2    Tumor    Material	MS    2023-01-02    GRCh37    diann    omics/MS/Donor2/tumor.tsv
Donor3    Tumor    Material	MS    2023-01-03    GRCh37    cmd/prot-exp   omics/MS/Donor3/tumor.tsv
```

### Copy Number Variant Profiles
Copy number variants profiles data, which is an aggregation of CNvs per chromosome arm.

Key: `dna-cnvp`.  
Sheet file name: `dna-cnvp.tsv`.  
Subtype: `data`.

Metadata:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `matched_specimen_id` - Matched specimen identifier, if the file is matched to another specimen during the processing.
- `matched_specimen_type` - Matched specimen type, if the file is matched to another specimen during the processing.
- `analysis_type`__*__ - Sample analysis (sequencing) type (`WES`, `WGS`).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `reader` - File reader / data format (`tsv`).
- `path`__*__ - Path to the file.

More information about supported formats is available [here](https://github.com/dkfz-unite/unite-feed-omics/blob/Docs/models-dna-cnvp.md#formats).

#### Example
`/mnt/data/project/dna-cnvp.tsv`
```tsv
donor_id	specimen_id	specimen_type	matched_specimen_id	matched_specimen_type    analysis_type	analysis_date	genome	reader	path
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    tsv    omics/WGS/Donor1/cnvp.tsv
Donor2    Tumor    Material    Normal    Material    WGS    2023-01-02    GRCh37    tsv    omics/WGS/Donor2/cnvp.tsv
Donor3    Tumor    Material    Normal    Material    WGS    2023-01-03    GRCh37    tsv   omics/WGS/Donor3/cnvp.tsv
```

#
__*__ - Required metadata fields.