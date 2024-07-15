# Data Source Service

## General
Data source service provides the following functionality:
- Uses custom crawlers to find required files.
- Uses custom readers to read the files.
- Hosts required files, and provides protected API to access the by a url.

## Access
Environment|Address|Port
-----------|-------|----
Host|http://localhost:5300|5300

## Configuration
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
- `UNITE_GENOME_FEED_HOST` - Public url of the genome feed service (`https://portal.unite.net/api/genome-feed`).  
    This parameter is and optional, if `UNITE_PORTAL_HOST` is set.
- `UNITE_CONFIG_PATH` - Path to the configuration folder (`./config`).
- `UNITE_CACHE_PATH` - Path to the cache folder (`./cache`).
- `UNITE_DATA_PATH` - Path to the data folder (`/data`).  
    Allows to set custom root files path.

## Installation

[.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) SDK is **required** to build and publish the application.

1) Open the terminal.

2) Add UNITE GitHub packages source for required **user** and **token** ([create](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens) it if needed):
    ```bash
    dotnet nuget add source https://nuget.pkg.github.com/dkfz-unite/index.json -n github -u ${USER} -p ${TOKEN} --store-password-in-clear-text
    ```

3) Open project sources folder, e.g. `~/projects/unite-data-source`:
    ```bash
    cd ~/projects/unite-data-source
    ```

4) Build and publish the application to desired location where it's gonna be running on the server, e.g. `/srv/app`:
    ```bash
    dotnet publish Unite.Data.Source.Web -c Release -o /srv/unite-data-source -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=None --self-contained   
    ```

5) Navigate to just published application folder:
    ```bash
    cd /srv/unite-data-source
    ```

6) Run and host the application publicly on the server:
    ```bash
    ./Unite.Data.Source.Web --urls http://0.0.0.0:80
    ```

7) Configure the application:

    If configuration location wasn't changed in the previous steps, the application will expect configuration files to be in `./config` folder relative to where the app is running (e.g. `/srv/app/config`).

    - Prepare the configuration file `config.tsv` with information about the crawlers, which folders they should explore to find files with data of considered types.  
    E.g. you have one crawler `org` which should look for files with genomic data of different types in different project folders relative to configured `/data` root folder:
        ```tsv
        folder  crawler types
        org/proj_a    org dna, dna-ssm, dna-cnv, dna-sv, rna, rna-exp
        org/proj_b    org dna, dna-ssm, dna-cnv, dna-sv, rna, rna-exp
        org/proj_c    org dna, dna-ssm, dna-cnv, dna-sv, rna, rna-exp
        ```
    - Put created configuration, required crawler and it's data readers to configuration folder to have the following structure:
        ```txt
        - /srv/unite-data-source
            Unite.Data.Source.Web
            - config
                config.tsv
                - org
                    crawler.app
                    - readers
                        ssm.app
                        cnv.app
                        sv.app
                        exp.app
        ```

8) Application will do the folling:
    - Use the crawler `org` and run it for every listed (`dna`, `dna-ssm`, `dna-cnv`, `dna-sv`, `rna`, `rna-exp`) data type to find corresponding files in configured project folders relative to configured `/data/` folder: `/data/org/proj_a`, `/data/org/proj_b`, `/data/org/proj_c`.
    - If the found file is a resource (e.g. **BAM** file):
        - File metadata will be sent to UNITE Portal with newly generated file key (e.g. `org-1234567890`).
        - File will be hosted by the application and will be accessible by the public application url: `http://source.data.unite.net/api/files/org-1234567890`.
    - If the found file contains data (e.g. DNA ssm data):
        - Corresponding to the data type `dna-ssm` reader will be used to read the content of the file.
        - File data and it's metadata will be sent to UNITE Portal for further processing and integration.

> [!Warning]
> If you stop the application, the hosted files will be no longer accessible by the public url.

> [!Note]
> It's recommended to host the application separatelly from UNITE Portal and closer to the data, to keep access to the files fast and reliable.

![alt text](./Docs/architecture.jpg)
