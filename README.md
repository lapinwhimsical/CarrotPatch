# CarrotPatch

CarrotPatch is a minimal Dalamud/XIVLauncher plugin starter project for FFXIV.
This first milestone only proves the plugin can build, load, log, unload, and be
published later through GitHub Releases plus a custom `repo.json` plugin
repository.

## Project

- Plugin name: `CarrotPatch`
- Internal name: `CarrotPatch`
- Assembly name: `CarrotPatch`
- Author: `lapinwhimsical`
- Target framework: `.NET 10`
- Dalamud API level: `15`

## Build Locally

Install the .NET SDK and make sure your Dalamud development environment is set
up through XIVLauncher/Dalamud.

From the repository root:

```powershell
dotnet restore
dotnet build CarrotPatch.sln
```

To build the project directly:

```powershell
dotnet build .\src\CarrotPatch\CarrotPatch.csproj -p:Platform=x64
```

## Load as a Dev Plugin

1. Build the project.
2. Open Dalamud settings in game.
3. Enable developer plugin loading.
4. Add the built `CarrotPatch.dll` as a dev plugin.
5. Load the plugin.

Expected log output:

```text
CarrotPatch loaded.
```

Unload or disable the plugin.

Expected log output:

```text
CarrotPatch unloaded.
```

## Publish a Release

1. Push the repository to GitHub at `https://github.com/lapinwhimsical/CarrotPatch`.
2. Create a GitHub Release named `v1.0.0`.
3. Package the plugin release artifact as `CarrotPatch.zip`.
4. Upload `CarrotPatch.zip` to the `v1.0.0` release.
5. Confirm `repo.json` points to:

```text
https://github.com/lapinwhimsical/CarrotPatch/releases/download/v1.0.0/CarrotPatch.zip
```

## Custom Plugin Repository

Add this raw repository URL to Dalamud/XIVLauncher custom plugin repositories:

```text
https://raw.githubusercontent.com/lapinwhimsical/CarrotPatch/main/repo.json
```

After adding the custom repository, verify that `CarrotPatch` appears in the
plugin installer and installs from the uploaded GitHub Release zip.
