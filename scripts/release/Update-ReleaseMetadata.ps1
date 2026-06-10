param(
    [ValidateSet('patch', 'minor', 'major')]
    [string] $Bump = 'patch',

    [string] $Version = '',

    [switch] $DryRun,

    [switch] $SkipTagCheck
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$projectPath = Join-Path $repoRoot 'src\CarrotPatch\CarrotPatch.csproj'
$pluginManifestPath = Join-Path $repoRoot 'src\CarrotPatch\CarrotPatch.json'
$repoManifestPath = Join-Path $repoRoot 'repo.json'

function Get-ProjectVersion {
    $projectText = Get-Content -LiteralPath $projectPath -Raw

    if ($projectText -notmatch '<Version>(?<version>[^<]+)</Version>') {
        throw "Project file does not contain a <Version> element."
    }

    $rawVersion = $Matches.version

    if ($rawVersion -notmatch '^(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?$') {
        throw "Project version must be X.Y.Z or X.Y.Z.N, found '$rawVersion'."
    }

    [pscustomobject] @{
        Major = [int] $Matches[1]
        Minor = [int] $Matches[2]
        Patch = [int] $Matches[3]
    }
}

function Get-NextVersion {
    param(
        [string] $RequestedVersion,
        [string] $RequestedBump
    )

    if (-not [string]::IsNullOrWhiteSpace($RequestedVersion)) {
        $trimmedVersion = $RequestedVersion.Trim()
        if ($trimmedVersion -notmatch '^(\d+)\.(\d+)\.(\d+)$') {
            throw "Exact version must use X.Y.Z format, found '$RequestedVersion'."
        }

        return [pscustomobject] @{
            Major = [int] $Matches[1]
            Minor = [int] $Matches[2]
            Patch = [int] $Matches[3]
        }
    }

    $currentVersion = Get-ProjectVersion
    switch ($RequestedBump) {
        'major' {
            $currentVersion.Major += 1
            $currentVersion.Minor = 0
            $currentVersion.Patch = 0
        }
        'minor' {
            $currentVersion.Minor += 1
            $currentVersion.Patch = 0
        }
        'patch' {
            $currentVersion.Patch += 1
        }
    }

    $currentVersion
}

function ConvertTo-PrettyJson {
    param($Value)

    ConvertTo-Json -InputObject $Value -Depth 16
}

function Set-Utf8NoBomContent {
    param(
        [string] $Path,
        [string] $Content
    )

    $encoding = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Path, $Content, $encoding)
}

function ConvertTo-PrettyJsonArray {
    param([object[]] $Items)

    $newline = [Environment]::NewLine
    $itemJson = foreach ($item in $Items) {
        $json = ConvertTo-PrettyJson $item
        (($json -split '\r?\n') | ForEach-Object { "  $_" }) -join $newline
    }

    "[$newline$($itemJson -join ",$newline")$newline]"
}

$nextVersion = Get-NextVersion -RequestedVersion $Version -RequestedBump $Bump
$semver = '{0}.{1}.{2}' -f $nextVersion.Major, $nextVersion.Minor, $nextVersion.Patch
$assemblyVersion = "$semver.0"
$tag = "v$semver"

if (-not $SkipTagCheck) {
    git rev-parse -q --verify "refs/tags/$tag" *> $null
    if ($LASTEXITCODE -eq 0) {
        throw "Tag already exists locally: $tag"
    }

    git ls-remote --exit-code --tags origin $tag *> $null
    if ($LASTEXITCODE -eq 0) {
        throw "Tag already exists on origin: $tag"
    }
}

if ($DryRun) {
    Write-Host "Dry run prepared release $tag ($assemblyVersion)."
    exit 0
}

$projectText = Get-Content -LiteralPath $projectPath -Raw
$projectText = $projectText -replace '<Version>[^<]+</Version>', "<Version>$assemblyVersion</Version>"
Set-Utf8NoBomContent -Path $projectPath -Content $projectText

$pluginManifest = Get-Content -LiteralPath $pluginManifestPath -Raw | ConvertFrom-Json
$pluginManifest.AssemblyVersion = $assemblyVersion
$pluginManifestJson = (ConvertTo-PrettyJson $pluginManifest) + [Environment]::NewLine
Set-Utf8NoBomContent -Path $pluginManifestPath -Content $pluginManifestJson

$parsedRepoManifest = ConvertFrom-Json -InputObject (Get-Content -LiteralPath $repoManifestPath -Raw)
if ($parsedRepoManifest -is [System.Array]) {
    $repoManifest = $parsedRepoManifest
} else {
    $repoManifest = @($parsedRepoManifest)
}

$plugin = $repoManifest[0]
$downloadUrl = "https://github.com/lapinwhimsical/CarrotPatch/releases/download/$tag/CarrotPatch.zip"
$plugin.AssemblyVersion = $assemblyVersion
$plugin.DownloadLinkInstall = $downloadUrl
$plugin.DownloadLinkUpdate = $downloadUrl
$plugin.LastUpdate = [string][DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$repoManifestJson = (ConvertTo-PrettyJsonArray $repoManifest) + [Environment]::NewLine
Set-Utf8NoBomContent -Path $repoManifestPath -Content $repoManifestJson

if ($env:GITHUB_OUTPUT) {
    $output = "version=$semver$([Environment]::NewLine)assembly_version=$assemblyVersion$([Environment]::NewLine)tag=$tag$([Environment]::NewLine)"
    $encoding = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::AppendAllText($env:GITHUB_OUTPUT, $output, $encoding)
}

Write-Host "Prepared release $tag ($assemblyVersion)."
