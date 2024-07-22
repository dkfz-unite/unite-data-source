# Data Types
Data types are the types of data UNITE can work with.

- Donors (patients)
    - [don](https://github.com/dkfz-unite/unite-donors-feed/blob/main/Docs/api-models-donors.md) - donors general and clinical data.
    - [don-trt](https://github.com/dkfz-unite/unite-donors-feed/blob/main/Docs/api-models-treatments.md) - donors treatments data.
- Images
    - [mri](https://github.com/dkfz-unite/unite-images-feed/blob/main/Docs/api-models-mris.md) - MRI images data.
    - [img-rad](https://github.com/dkfz-unite/unite-images-feed/blob/main/Docs/api-models-radiomics.md) - image radiomics features data.
- Specimens
    - [mat](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-material.md) - all donor derived materials data.
    - [lne](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-line.md) - cell lines data.
    - [org](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-organoid.md) - organoids data.
    - [xen](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-xenograft.md) - xenografts data.
    - [spe-int](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-interventions.md) - specimens interventions data.
    - [spe-drg](https://github.com/dkfz-unite/unite-specimens-feed/blob/main/Docs/api-models-drugs.md) - specimen drugs screening data.
- Genomic
    - [dna](https://github.com/dkfz-unite/unite-genome-feed/blob/main/Docs/api-models-sample.md) - DNA sample data.
    - [dna-ssm](https://github.com/dkfz-unite/unite-genome-feed/blob/main/Docs/api-models-dna-ssm.md) - DNA Simple Somatic Mutations data.
    - [dna-cnv](https://github.com/dkfz-unite/unite-genome-feed/blob/main/Docs/api-models-dna-cnv.md) - DNA Copy Number Variants data.
    - [dna-sv](https://github.com/dkfz-unite/unite-genome-feed/blob/main/Docs/api-models-dna-sv.md) - DNA Structural Variants data.
    - [rna](https://github.com/dkfz-unite/unite-genome-feed/blob/main/Docs/api-models-sample.md) - **bulk** RNA sample data.
    - [rna-exp](https://github.com/dkfz-unite/unite-genome-feed/blob/main/Docs/api-models-rna-exp.md) - **bulk** RNA expressions data.
    - [rnasc](https://github.com/dkfz-unite/unite-genome-feed/blob/main/Docs/api-models-sample.md) - **single cell** RNA sample data.
    - [rnasc-exp](https://github.com/dkfz-unite/unite-genome-feed/blob/main/Docs/api-models-rnasc-exp.md) - **single cell** RNA expressions data.

## Resources
Resource is a file, which has to be hosted on the server and can be later accessed by the url.  
Such files are usually to big to be directly uploaded to application server and integrated with other data, but still need to be accessible.

Application automatically hosts files of the following data types as resources:
- All genomic samples - `dna`, `rna`, `rnasc` (e.g. BAM files of sequencing samples).
- Single cell RNA expressions - `rnasc-exp` (e.g. MEX files of single cell gene expressions from 10xGenomics).

> [!Note]
> If you don't want such files to be hosted as resources, exclude them from [crawlers](crawler.md) output.