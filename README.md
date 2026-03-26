# EVE Online Asset Extractor
This is a OpenSource tool created by [EVE Workbench](https://eveworkbench.com) to extract images and video's from EVE Online.

The tool was mainly created to extract some images used by CCP in the EVE Online game which were not available in the official image dump.

## Usage
You can either download the compiled source from the releases or compile it from source.

Both flags are optional:
- `--game-dir=<path>`: EVE install directory.
- `--output-dir=<path>`: where extracted files are written.

If `--game-dir` is not provided, the app tries to auto-detect EVE based on OS-specific default locations.
If `--output-dir` is not provided, output defaults to `assets` next to the executable.

Examples:

```bash
# Auto-detect EVE and write to ./assets (next to executable)
./EveAssetExtractor

# Explicit game dir and output dir
./EveAssetExtractor --game-dir="/path/to/EVE" --output-dir="/tmp/assets"
```

Windows example:

```powershell
EveAssetExtractor.exe --game-dir="C:\CCP\EVE" --output-dir="C:\temp\assets"
```

## How It Works
- Builds paths with `Path.Combine` for cross-platform separators.
- Reads `tq/resfileindex.txt` and `tq/resfileindex_prefetch.txt` from the game directory.
- Resolves source files from `ResFiles`.
- Parses CSV rows as `<logical file>,<hashed path>`.
- Extracts only:
  - `image`: `.png`, `.jpg`
  - `video`: `.webm`, `.srt`
- Flattens `res:/...` logical paths into output file names by replacing `/` and `\` with `_`.

Expected EVE directory layout:

```text
<game-dir>/tq/resfileindex.txt
<game-dir>/tq/resfileindex_prefetch.txt
<game-dir>/ResFiles/...
```

## Auto-Detection Notes
- Windows: tries `C:\CCP\EVE`.
- Linux: tries common Steam Proton `compatdata/8500` paths.
- macOS: no default path is currently implemented, so pass `--game-dir` explicitly.

## Compile
Use .NET 10 to compile, then run:

```bash
dotnet build
```
