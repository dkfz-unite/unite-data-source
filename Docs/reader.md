# Reader
Reader is a **custom** application used to read the content of the file and extract the data from it.

Application should fulfill the following requirements:
- It should be executable console application.
- If application requires any dependencies, they should be installed on the same server where the application is used.

## Input
Application should accept **one** argument:
1. Path to the file to read.

### Example
If the reader should read the file `/mnt/data/file2.vcf`, the command to run it should look as following:
```bash
./reader /mnt/data/file2.vcf
```

## Output
Application should output to the console the content of the file (with required metadata) in UNITE **tsv** format of the corresponding data type.  
Please refer to the data types [documentation](./types.md) for the list of available data types.

### Example
If the reader should read the file `/mnt/data/file2.vcf` with `dna-ssm` data, the output should look as following:
```tsv
# tsample_donor_id: D01
# tsample_specimen_id: S02
# tsample_specimen_type: Material
# tsample_analysis_type: WGS
# msample_donor_id: D01
# msample_specimen_id: S01
# msample_specimen_type: Material
# msample_analysis_type: WGS
chromosome  position    ref alt
1   123456  A   T
1   234567  C   G
1   345678  G   A
```
