# Data Source Service

## General
Data source service provides the following functionality:
- Scans configured folders in the file system for the files with data of different types.
    - Uses sample sheets with required metadata and location of the files (if available).
    - Can use custom [crawler](./Docs/crawler.md) applications to locate the files (with required metadata) in configured folders.
    - Can use custom [reader](./Docs/reader.md) applications to convert the files to the format required by the UNITE portal.
- Hosts required files, and provides protected [web API](./Docs/api.md) to access them remotely.

## Access
Environment|Address|Port
-----------|-------|----
Host|http://localhost:5400|5400

## Configuration

### Application
To configure the application, change environment variables in either docker or [launchSettings.json](Unite.Data.Source.Web/Properties/launchSettings.json) file (if running locally):

- `ASPNETCORE_ENVIRONMENT` - ASP.NET environment (`Release`).
- `UNITE_API_KEY` - API key for decription of JWT token and user authorization.
- `UNITE_WORKER_HOST` - Public url of the current service (`http://source.data.unite.net`).
- `UNITE_WORKER_TOKEN` - UNITE worker JWT token to access the portal.
- `UNITE_PORTAL_HOST` - Public url of the portal service (`https://portal.unite.net`).  
    This parameter will be used, if custom feed services are not set.
- `UNITE_DONORS_FEED_HOST` - Public url of the donors feed service (`https://portal.unite.net/api/donors-feed`).  
    This parameter is and optional, if `UNITE_PORTAL_HOST` is set.
- `UNITE_IMAGES_FEED_HOST` - Public url of the images feed service (`https://portal.unite.net/api/images-feed`).  
    This parameter is and optional, if `UNITE_PORTAL_HOST` is set.
- `UNITE_SPECIMENS_FEED_HOST` - Public url of the specimens feed service (`https://portal.unite.net/api/specimens-feed`).  
    This parameter is and optional, if `UNITE_PORTAL_HOST` is set.
- `UNITE_OMICS_FEED_HOST` - Public url of the omics feed service (`https://portal.unite.net/api/omics-feed`).  
    This parameter is and optional, if `UNITE_PORTAL_HOST` is set.
- `UNITE_CONFIG_PATH` - Path to the configuration folder (`./config`).
- `UNITE_CACHE_PATH` - Path to the cache folder (`./cache`).
- `UNITE_DATA_PATH` - Path to the data folder (`/data`).  
    Allows to set custom root files path.

### Folders Configuration
To configure the service, to explore required data, create a configuration file `config.tsv` in the configuration folder (`UNITE_CONFIG_PATH`) with the following structure:
```tsv
path    types   crawler args
relative/path/to/folder dna, dna-sm, dna-cnv, dna-sv, meth, rna, rna-exp, rnasc, rnasc-exp  default mapping.tsv
```

Where:
- `path` - Relative (if `UNITE_DATA_PATH` is set) or absolute path to the folder where the service should look for the files.
- `types` - Comma separated list of the [data types](./Docs/types.md), files of which the service should look for in configured folder.
- `crawler` - Name of the crawler directory with custom crawler and readers.
- `args` - Optional arguments for the crawler application, e.g. `mapping.tsv` - to specify the mapping file for the crawler (path is always relative to the crawler).
    

### Workflow
The workflow of the service is following:

- The service reads the `config.tsv` file line by line and for each data [type](./Docs/types.md) from the `types` list of the line
    - Service is looking for corresponding samples sheet file with name `<type>.tsv` in the configured `path` folder.
        - If the file **exists**, the service will use it.
        - If the file **does not exist**, the service will try to use custom crawler in the configured `crawler` folder.  
          It will expect the application with the name **'crawler'** in the configured `crawler` folder.
    - For each file either from the samples sheet or found by the crawler:
        - If the file is a resource (`fasta`, `bam`, `idat`, etc.) the service will **host** it and send **only it's metadata** to the Portal.
        - If the file is a data file (`snv.vcf`, `indel.vcf`, `cnv.tsv`, etc.), the service will send it to the Portal for further processing and integration.
            - If the format of the file is not supported by the Portal, the service will try to use a custom reader from the file metadata.  
              In this case it will expect the application with the corresponding name in the configured `crawler/readers` folder.
        
### Custom Crawlers
Crawler is a custom command line application which locates files of given [type](./Docs/types.md) in given location and returns their metadata and locations as a sample sheet.

#### Input
The application should accept the following arguments as an input:
1. `<type>` as a first argument - [data type](./Docs/types.md) to look for.
2. `<path>` as a second argument - Absolute path to the folder where the crawler should look for files.

#### Output
The application should output the sample sheet in the format corresponding to the `<type>` argument passed to the crawler application.

For example, if the `<type>` is `dna-sm`, the output should be a sample sheet in [corresponding](./Docs/types.md#simple-mutations) format:
```tsv
donor_id	specimen_id	specimen_type	matched_specimen_id	matched_specimen_type    analysis_type	analysis_date	genome	reader	path
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    tsv    omics/WGS/Donor1/snv.tsv
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    tsv    omics/WGS/Donor1/indel.tsv
Donor2    Tumor    Material    Normal    Material    WGS    2023-01-02    GRCh37    vcf    omics/WGS/Donor2/snv.vcf
Donor2    Tumor    Material    Normal    Material    WGS    2023-01-02    GRCh37    vcf    omics/WGS/Donor2/indel.vcf
Donor3    Tumor    Material    Normal    Material    WGS    2023-01-03    GRCh37    cmd/dna-sm   omics/WGS/Donor3/snv.tsv
Donor3    Tumor    Material    Normal    Material    WGS    2023-01-03    GRCh37    cmd/dna-sm   omics/WGS/Donor3/indel.tsv
```

The `path` specified in the sample sheet should be relative to the `<path>` argument passed to the crawler application.

### Custom Readers
Reader is a custom command line application which reads the content of the file and returns it in the format required by the UNITE Portal.

#### Input
The application should accept the following arguments as an input:
1. `<path>` as a first argument - Absolute path to the file to read.

#### Output
The application should output the content of the file in the format of corresponding type required by the UNITE Portal.

### Example
Let's assume, that the data source service is running in the docker container.  
The data on the server located at `/mnt/data` location.  
The configuration of the service is located at `/srv/config` location (`config.tsv`, crawlers and their readers should be stored here).  
The cache of the service is located at `/srv/cache` location.

#### Environment Variables
The following environment variables are set:
- `UNITE_DATA_PATH` - is set to `/data`, so the service will look for files in `/data` folder (inside the container).
- `UNITE_CONFIG_PATH` - is set to `/config`, so the service will look for configuration files in `/config` folder (inside the container).
- `UNITE_CACHE_PATH`- is set to `/cache`, so the service will cache the files in `/cache` folder (inside the container).

#### Volumes
The following volumes are mounted to the container from the file system:
- `/mnt/data` to `/data` - to five container access to the data files.
- `/srv/config` to `/config` - to provide the service with configuration files.
- `/srv/cache` to `/cache` - to provide the service with cache folder.

#### Data Files
The data files are located in the `/mnt/data` folder on the server, e.g.:
```txt
- /mnt/data/project1/omics/WGS/
    - dna.tsv
    - dna-sm.tsv
    - dna-cnv.tsv
    - dna-sv.tsv
    - donor1
        - normal.bam
        - normal.bam.bai
        - tumor.bam
        - tumor.bam.bai
        - snv.vcf
        - indel.vcf
        - cnv.tsv
        - sv.tsv
```

Where:
- `../project1` - is a folder with data of the project (samples sheet files are expected to be here).
    - `dna.tsv` - is a sample sheet with metadata of the [DNA samples](./Docs//types.md#sample).
    - `dna-sm.tsv` - is a sample sheet with metadata of the [DNA simple mutations](./Docs/types.md#simple-mutations).
    - `dna-cnv.tsv` - is a sample sheet with metadata of the [DNA copy number variants](./Docs/types.md#copy-number-variants).
    - `dna-sv.tsv` - is a sample sheet with metadata of the [DNA structural variants](./Docs/types.md#structural-variants).
    - `donor1` - is a folder with data of the donor.
        - `normal.bam`, `normal.bam.bai`, `tumor.bam`, `tumor.bam.bai` - are [DNA sample](./Docs/types.md#sample) files.
        - `snv.vcf`, `indel.vcf` - are [DNA simple mutations](./Docs/types.md#simple-mutations) files in **vcf** format.
        - `cnv.tsv` - is a [DNA copy number variants](./Docs/types.md#copy-number-variants) file in **ACESeq** format.
        - `sv.tsv` - is a [DNA structural variants](./Docs/types.md#structural-variants) file in custom format.

Contend of the `dna.tsv` file:
```tsv
donor_id	specimen_id	specimen_type   analysis_type	analysis_date	genome  format  path
Donor1    Tumor    Material    WGS    2023-01-01    GRCh37    bam   omics/WGS/Donor1/normal.bam
Donor1    Tumor    Material    WGS    2023-01-01    GRCh37    bam   omics/WGS/Donor1/normal.bam.bai
Donor1    Normal   Material    WGS    2023-01-01    GRCh37    bam   omics/WGS/Donor1/tumor.bam
Donor1    Normal   Material    WGS    2023-01-01    GRCh37    bam   omics/WGS/Donor1/tumor.bam.bai
```

Content of the `dna-sm.tsv` file:
```tsv
donor_id	specimen_id	specimen_type	matched_specimen_id	matched_specimen_type    analysis_type	analysis_date	genome	reader	path
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    vcf    omics/WGS/Donor1/snv.vcf
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    vcf    omics/WGS/Donor1/indel.vcf
```

Content of the `dna-cnv.tsv` file:
```tsv
donor_id	specimen_id	specimen_type	matched_specimen_id	matched_specimen_type    analysis_type	analysis_date	genome  purity  ploidy    reader	path
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    0.9    2.0  aceseq    omics/WGS/Donor1/cnv.tsv
```

Content of the `dna-sv.tsv` file:
```tsv
donor_id	specimen_id	specimen_type	matched_specimen_id	matched_specimen_type    analysis_type	analysis_date	genome  reader	path
Donor1    Tumor    Material    Normal    Material    WGS    2023-01-01    GRCh37    cmd/sv    omics/WGS/Donor1/sv.tsv
```

#### Configuration Files
The configuration files are located in the `/srv/config` folder on the server, e.g.:
```txt
- /srv/config/
    config.tsv
    - my-crawler/
        mapping.tsv
        - readers/
            sv
```

Where:
- `config.tsv` - is a configuration file with information about the folders to explore.
- `my-crawler` - is a folder with custom crawler and readers.
    - `mapping.tsv` - mapping file for the custom crawler application.
    - `readers` - is a folder with custom readers for different data types.
        - `sv` - is a custom reader application for [DNA structural variants](./Docs/types.md#structural-variants) files.

#### Configuration File
The configuration file `config.tsv` in the `/srv/config` folder has the following content:
```tsv
path    types   crawler args
project dna, dna-sm, dna-cnv, dna-sv  my-crawler    mapping.tsv
```

Where:
- `path` - is set to `project` - relative path to the folder with data mapped to the location of the data files in the container (`/data/project`).  
           The service will look for the sample sheet files in this folder.
- `types` - is set to `dna`, `dna-sm`, `dna-cnv`, `dna-sv` - to look for files of these types in the configured folder.
- `crawler` - is set to `my-crawler` - to name of the folder with the custom crawler and readers in the configuration folder (`/config/my-crawler`).
- `args` - is set to `mapping.tsv` - to specify the mapping file for the crawler application (path is always relative to the crawler).