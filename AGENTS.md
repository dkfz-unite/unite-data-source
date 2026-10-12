# UNITE repository instructions

Before changing this repository, read the shared UNITE instructions and
relevant project documentation maintained in
[dkfz-unite/.github](https://github.com/dkfz-unite/.github/blob/main/README.md).

1. Prefer the local shared checkout: `../unite-meta/AGENTS.md`, relative to this
   repository root. A sibling `.github/AGENTS.md` or a user-provided path is also
   valid. Resolve its document links relative to that shared checkout.
2. Read the shared `AGENTS.md`, project overview, repository map, and topic
   documents relevant to the task, following the shared README's task index.
3. If no shared checkout is accessible, use the GitHub versions beginning with
   [shared AGENTS.md](https://github.com/dkfz-unite/.github/blob/main/AGENTS.md).
   If those are unavailable too, report the missing context and proceed only
   with what local documentation/code establishes; do not invent shared rules.
4. Read this repository's README and relevant local docs; inspect Git status.
   More specific instructions here supplement the shared guidance.

Keep common rules in `.github`; keep component details in this repository.

## Data Source

Status: reviewed with the owner (2026-10-12).

`unite-data-source` is a plugin service that runs next to the data: it reads a
configuration, discovers files through sample sheets or custom crawlers,
submits data and resource metadata to the feeds (through the portal proxy with
a worker token), and serves the original files for analysis preparation. It
does not validate, review or integrate data itself. The workflow (discovery,
sheet kinds, caches, deployment placement) is described in
[Data ingestion](../unite-meta/docs/data-ingestion.md#unite-data-source); read
it first. Configuration, sheet formats and data types are in the
[README](README.md) and [Docs/types.md](Docs/types.md).

### Code layout

One project, `Unite.Data.Source.Web`:

| Area | Contents |
| --- | --- |
| `Workers/ExploringWorker.cs` | Discovery loop: 5 s start delay, then a pass every minute |
| `Handlers/ExploringHandler.cs` | Reads `config.tsv` per pass, routes each type (direct TSV data, sample sheets, result sheets), runs crawlers and custom readers, uploads to the feeds, maintains the caches |
| `Handlers/Constants/DataTypes.cs`, `DataUrls.cs` | Data type keys (e.g. `dna-sm`) and feed routes per type |
| `Handlers/Contract/` | Sheet row models (`SampleFile`, `ResultFile`, `Resource`) and form building |
| `Handlers/*FilesCache.cs` | `found-files.tsv`, `error-files.tsv`, `host-files.tsv` in `UNITE_CACHE_PATH` |
| `Controllers/FileController.cs` | `GET api/file/{key}`: serves a hosted file by resource key, with range requests |
| `Configuration/Options/` | Paths, portal and feed hosts, worker host and token |

There are no test projects.

### Build and run

```sh
dotnet build Unite.Data.Source.sln
dotnet run --project Unite.Data.Source.Web --launch-profile Unite.Data.Source.Web
```

- Local URL `http://localhost:5400`; Docker `source.data.unite.net`, deployed
  from `unite-data-source-env` placed under the installation's `plugins/`.
- Needs a worker token issued by an administrator in the portal
  (`UNITE_WORKER_TOKEN`), the portal host (feed hosts default to
  `<portal>/api/feed-<type>`), and a data folder with `config.tsv`.
- Running it locally against real data submits real data. Use a separate
  config and cache folder with test data, and remove the test submissions
  afterwards.

### Change notes

- **Adding a data type**: add the key in `DataTypes.cs`, the route in
  `DataUrls.cs`, the routing set in `ExploringHandler` (`IsDataType`,
  `IsSampleType` or `IsResultType`) and the upload switch, and document it in
  `Docs/types.md`. See
  [Integrating a new data type](../unite-meta/docs/integrating-a-data-type.md).
- **Types without support yet**: radiomics (`img-rad`) and drug screenings
  (`spe-drg`) are defined but not routed (Jira UND-1077). CT (`ct`) is routed,
  but CT is a planned future type and the images feed has no CT endpoint yet.
- **Caches** are keyed by relative path only: a path in the found or error
  cache is never processed again until its line is removed. Changing a file
  does not trigger a new upload. Sample and result sheets are re-read every
  pass and each listed file is cached individually; direct data sheets (donors,
  treatments, specimens, interventions) are cached as a whole. Resubmission is
  done manually by an administrator, which is intended for now (owner,
  2026-10-12); change detection for data sheets and a management UI (portal
  plugin) are planned (Jira UND-1078, UND-1079).
- **Paths** from sheets must stay inside the configured folder
  (`ValidateFilePath`); keep this check for any new path input.
- Uploads go through the portal, so they pass the portal's worker-token check
  (revoked tokens are rejected there).

### Shared libraries

`Unite.Essentials` 1.4.0 (latest 1.6.0), `Unite.Essentials.Tsv` 4.2.0.

### CI

`docker-images.yml` builds amd64 and arm64 images under one manifest on pushes
to `main`.

### Known issues

Defects are tracked in the private Jira; see the shared
[Track defects](../unite-meta/AGENTS.md#track-defects).
