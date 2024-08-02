# Crawler
Crawler (explorer) is **custom** application used to find required files in desired location.

Application should fulfill the following requirements:
- It should be executable console application.
- If application requires any dependencies, they should be installed on the same server where the application is used.

> [!Note]
> One crawler can be used to find information about various data types in different folders.

## Input
Application should accept **two** arguments:
1. Type of the data in the file, which crawler should find.
2. Path to the folder, where the crawler should look for files of given type.

Please refer to the data types [documentation](./types.md) for the list of available data types.

### Example
If the crawler should look for files with `dna-ssm` data in folder `/mnt/data`, the command to run the crawler should look as following:
```bash
./crawler dna-ssm /mnt/data
```

## Output
Application should output to the console **tsv** list of files of the required data type found in the given folder in the following format:
```tsv
format  reader  path
tsv def /mnt/data/file1.tsv
vcf cmd/ssm /mnt/data/file2.vcf
```
- `format` - File [format](#format).
- `reader` - File reader (`def` for default or `cmd/{name}` of the **custom** reader).
- `path` -  **Absolute** path to the file found by the crawler.

### Format
The file can be in any format, but some of the formats for certain data types will be treated differently by the application.
Format should be passed as an **extension** of the file in **lowercase**, **without** the leading **dot**, e.g.: `tsv`, `vcf`, `bam`, `mtx`.
Files of certain data types will be hosted by the application as [resources](./types.md#resources).

### Default Readers
If the reader is default (`def`), the application exprects, that the file is in standard UNITE **tsv** format for the corresponding data type.

#### Example
If the crawler found the file `/mnt/data/file1.tsv` with `dna-ssm` data, which should be read by default reader, the output should look as following:
```tsv
format  reader  path
tsv def /mnt/data/file1.tsv
```

### Custom Readers
If the reader is custom (`cmd/{name}`), the application expects, that the reader application with corresponding **name** is placed in the `readers` folder of the crawler and can be used to read the content of the file.  
Please refer to the readers [documentation](./reader.md) for more information.

#### Example
If the crawler found the file `/mnt/data/file2.vcf` with `dna-ssm` data, which should be read by custom reader `ssm`, the output should look as following:
```tsv
format  reader  path
vcf cmd/ssm /mnt/data/file2.vcf
```

The custom reader `ssm` should be placed in the `readers` folder of corresponding crawler.
```txt
- /srv/config
    config.tsv
    - my-crawler
        crawler
        - readers
            ssm
```
