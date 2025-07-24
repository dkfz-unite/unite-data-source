# Data Types
Data types are the types of data UNITE can work with.

- Donors (patients)
    - [don](https://github.com/dkfz-unite/unite-donors-feed/blob/main/Docs/api-models-donors.md) - donors general and clinical data.
    - [don-trt](https://github.com/dkfz-unite/unite-donors-feed/blob/main/Docs/api-models-treatments.md) - donors treatments data.
- Images
    - [mr](https://github.com/dkfz-unite/unite-images-feed/blob/main/Docs/api-models-mrs.md) - MR images data.
    - [img-rad](https://github.com/dkfz-unite/unite-images-feed/blob/main/Docs/api-models-radiomics.md) - image radiomics features data.
- Specimens
    - [mat](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-material.md) - all donor derived materials data.
    - [lne](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-line.md) - cell lines data.
    - [org](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-organoid.md) - organoids data.
    - [xen](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-xenograft.md) - xenografts data.
    - [spe-int](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-interventions.md) - specimens interventions data.
    - [spe-drg](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-drugs.md) - specimen drugs screening data.
- Genomic
    - [dna](https://github.com/dkfz-unite/unite-omics-feed/blob/main/Docs/api-models-sample.md) - DNA sample data.
    - [dna-sm](https://github.com/dkfz-unite/unite-omics-feed/blob/main/Docs/api-models-dna-sm.md) - DNA Simple Mutations data.
    - [dna-cnv](https://github.com/dkfz-unite/unite-omics-feed/blob/main/Docs/api-models-dna-cnv.md) - DNA Copy Number Variants data.
    - [dna-sv](https://github.com/dkfz-unite/unite-omics-feed/blob/main/Docs/api-models-dna-sv.md) - DNA Structural Variants data.
    - [rna](https://github.com/dkfz-unite/unite-omics-feed/blob/main/Docs/api-models-sample.md) - **bulk** RNA sample data.
    - [rna-exp](https://github.com/dkfz-unite/unite-omics-feed/blob/main/Docs/api-models-rna-exp.md) - **bulk** RNA expressions data.
    - [rnasc](https://github.com/dkfz-unite/unite-omics-feed/blob/main/Docs/api-models-sample.md) - **single cell** RNA sample data.
    - [rnasc-exp](https://github.com/dkfz-unite/unite-omics-feed/blob/main/Docs/api-models-rnasc-exp.md) - **single cell** RNA expressions data.

## Resources
Resource is a file, which has to be hosted on the server and can be later accessed by the url.  
Such files are usually to big to be directly uploaded to UNITE portal and integrated with other data, but still need to be accessible.

Application automatically hosts files of the following data types as resources:
- All genomic samples - `dna`, `rna`, `rnasc` (e.g. `.bam` files of sequencing samples).
- Single cell RNA expressions - `rnasc-exp` (e.g. `.mtx` files of single cell gene expressions from 10xGenomics).

> [!Note]
> If you don't want such files to be hosted as resources, exclude them from [crawlers](crawler.md) output.

### Access
If a file was hosted by the application as a resource, the application will generate a unique key for it and send to UNITE portal with file metadata.  
File can be later accessed by this key sending a request to corresponding [web API](./api.md) endpoint:
- Ordinary file - [file](./api.md#get-apifilekey) endpoint.
- Binary alignment map (BAM) file - [bam](./api.md#get-apibamkey) endpoint, if the file was provided in a `bam` format.
- 10xGenomics single cell RNA expressions matrix file - [mtx](./api.md#get-apimtxkey) endpoint, if the file was provided in a `mtx` format.

## Metadata
Metadata is an information about the file of specific data type, which is required to load the data into UNITE portal.

Metadata can be of two types:
- **Result** - metadata of the processing pipeline result files (e.g. `snv`, `indel`, `cnv`, `sv` result files).
- **Sample** - metadata of the sample files (e.g. `bam` or `idat` files).

### Result Files
Result files metadata is required for the following data types:
- `dna-sm` - Simple Mutations.
- `dna-cnv` - Copy Number Variants.
- `dna-sv` - Structural Variants.
- `rna-exp` - Bulk RNA Expressions.
- `rnasc-exp` - Single Cell RNA Expressions.

The metadata of such files includes the following fields:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `matched_specimen_id` - Matched specimen identifier, if the file is matched to another specimen during the processing.
- `matched_specimen_type` - Matched specimen type, if the file is matched to another specimen during the processing.
- `analysis_type`__*__ - Sample analysis (sequencing) type (`WES`, `WGS`, `RNASeq`, `scRNASeq`, etc.).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `purity` - Sample purity, if available (e.g. `0.8` for 80%). E.g. estimated by the processing pipeline.
- `ploidy` - Sample ploidy, if available (e.g. `2` for diploid). E.g. estimated by the processing pipeline.
- `genome` - Sample genome version (`GRCh37` or `GRCh38`).
- `format` - Resource file format (e.g. `mtx`, `tsv`, `vcf`, etc.).
- `reader` - Resource File reader, if the file data has to be processed.
- `path`__*__ - Absolute path to the file.

`*` - Required fields.

### Sample Files
Sample files metadata is required for the following data types:
- `dna` - DNA Sample.
- `meth` - DNA Methylation Sample.
- `rna` - Bulk RNA Sample.
- `rnasc` - Single Cell RNA Sample.

The metadata of such files includes the following fields:
- `donor_id`__*__ - Donor identifier.
- `specimen_id`__*__ - Specimen identifier.
- `specimen_type`__*__ - Specimen type (`Material`, `Line`, `Organoid`, `Xenograft`).
- `analysis_type`__*__ - Sample analysis (sequencing) type (`WES`, `WGS`, `RNASeq`, `scRNASeq`, etc.).
- `analysis_date` - Sample analysis (sequencing) date in ISO format (`yyyy-MM-dd`).
- `analysis_day` - Sample analysis (sequencing) day, relative to enrollment date, in days.
- `genome` - Sample genome version (`GRCh37` or `GRCh38`).
- `format`__*__ - Sample file format (`bam`, `bam.bai`, `idat`, etc.).
- `path`__*__ - Absolute path to the file.

`*` - Required fields.

### Readers
Reader is a type of the file reader, which is used to read the content of the file.

Readers can be of the following types:
- **Empty** - If the reader is not set, the file will be hosted as resource and it's metadata will be sent to the portal. In this case, the `format` is required.
- **{name}** - One of the default readers supported by the portal, which depends on the type of the data:
    - `tsv` - default format reader for UNITE data types.
    - `vcf` - VCF reader for `dna-sm` data.
    - `aceseq` - AceSeq reader for `dna-cnv` data.
    - `dkfz/sophia` - DKFZ Sophia reader for `dna-sv` data.
    - `dkfz-rnaseq` - DKFZ RNASeq reader for `rna-exp` data.
- **cmd/{name}** - custom command reader which defines the name of the CLI tool for reading the file content. Such reader has to be located in the `readers` folder of the crawler.   
