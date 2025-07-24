# Crawler
Crawler (explorer) is **custom** application used to find required files in desired location.

Application should fulfill the following requirements:
- It should be executable console application.
- If application requires any dependencies, they should be installed on the same server where the application is used (or in the same docker container).

> [!Note]
> One crawler can be used to find information about various data types in different folders.

## Input
Application should accept **two** arguments:
1. Type of the data in the file, which crawler should find.
2. Path to the folder, where the crawler should look for files of given type.

Refer to the data types [documentation](./types.md) for the list of available data types.

### Example
If the crawler should look for files with `dna-sm` data in folder `/mnt/data`, the command to run the crawler should look as following:
```bash
./crawler dna-sm /mnt/data
```

## Output
Application should output to the console **tsv** the [metadata](./types.md#metadata) depending on the type of the file:

- [Result File](./types.md#result-files) - processing pipelines result files (e.g. `snv`, `indel`, `cnv`, `sv` result files).
```tsv
donor_id  specimen_id specimen_type matched_specimen_id matched_specimen_type analysis_type analysis_date genome format reader      path
D01       Tumor       Material      Control             Material              WGS           2020-01-01    GRCh37        vcf         /mnt/data/snv.vcf
D01       Tumor       Material      Control             Material              WGS           2020-01-01    GRCh37        vcf         /mnt/data/indel.vcf
D01       Tumor       Material      Control             Material              WGS           2020-01-01    GRCh37        aceseq      /mnt/data/cnv.tsv
D01       Tumor       Material      Control             Material              WGS           2020-01-01    GRCh37        dkfz/sophia /mnt/data/snv.tsv
D01       Tumor       Material      Control             Material              RNASeq        2020-01-01    GRCh37        dkfz/rnaseq /mnt/data/snv.tsv
D01       Tumor       Material      Control             Material              scRNASeq      2020-01-01    GRCh38 tsv                /mnt/data/features.tsv.gz
D01       Tumor       Material      Control             Material              scRNASeq      2020-01-01    GRCh38 tsv                /mnt/data/barcodes.tsv.gz
D01       Tumor       Material      Control             Material              scRNASeq      2020-01-01    GRCh38 mtx                /mnt/data/matrix.tsv.gz
```

- [Sample File](./types.md#sample-files) - sample files (e.g. `bam` or `idat` files).
```tsv
donor_id  specimen_id specimen_type analysis_type analysis_date genome format      path
D01       Control     Material      WGS           2020-01-01    GRCh37 bam         /mnt/data/control.bam
D01       Control     Material      WGS           2020-01-01    GRCh37 bam.bai     /mnt/data/control.bam.bai
D01       Tumor       Material      WGS           2020-01-01    GRCh37 bam         /mnt/data/tumor.bam
D01       Tumor       Material      WGS           2020-01-01    GRCh37 bam.bai     /mnt/data/tumor.bam.bai
D01       Tumor       Material      RNASeq        2020-01-01    GRCh37 bam         /mnt/data/rna.bam
D01       Tumor       Material      RNASeq        2020-01-01    GRCh37 bam.bai     /mnt/data/rna.bam.bai
D01       Tumor       Material      scRNASeq      2020-01-01    GRCh38 bam         /mnt/data/rnasc.bam
D01       Tumor       Material      scRNASeq      2020-01-01    GRCh38 bam.bai     /mnt/data/rnasc.bam.bai
D01       Tumor       Material      MethArray     2020-01-01    GRCh38 idat        /mnt/data/meth_red.idat
D01       Tumor       Material      MethArray     2020-01-01    GRCh38 idat        /mnt/data/meth_grn.idat
```

### Format
The file can be in any format, but some of the formats for certain data types will be treated differently by the application.
Format should be passed as an **extension** of the file in **lowercase**, **without** the leading **dot**, e.g.: `tsv`, `vcf`, `bam`, `mtx`.
Files of certain data types will be hosted by the application as [resources](./types.md#resources).

### Default Readers
If the reader is default (`def`), the application exprects, that the file is in standard UNITE **tsv** format for the corresponding data type.

#### Example
If the crawler found the file `/mnt/data/file1.tsv` with `dna-sm` data, which should be read by default reader, the output should look as following:
```tsv
format  reader  path
tsv def /mnt/data/file1.tsv
```

### Custom Readers
If the reader is custom (`cmd/{name}`), the application expects, that the reader application with corresponding **name** is placed in the `readers` folder of the crawler and can be used to read the content of the file.  
Please refer to the readers [documentation](./reader.md) for more information.

#### Example
If the crawler found the file `/mnt/data/file2.vcf` with `dna-sm` data, which should be read by custom reader `sm`, the output should look as following:
```tsv
format  reader  path
vcf cmd/sm /mnt/data/file2.vcf
```

The custom reader `sm` should be placed in the `readers` folder of corresponding crawler.
```txt
- /srv/config
    config.tsv
    - my-crawler
        crawler
        - readers
            sm
```
