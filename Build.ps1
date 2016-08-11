param(
    [Parameter(Mandatory=$false)][bool]   $RestorePackages  = $false,
    [Parameter(Mandatory=$false)][string] $Configuration    = "Release",
    [Parameter(Mandatory=$false)][string] $VersionSuffix    = "",
    [Parameter(Mandatory=$false)][bool]   $RunTests         = $true
)

$ErrorActionPreference = "Stop"

$solutionPath  = Split-Path $MyInvocation.MyCommand.Definition
$getDotNet     = Join-Path $solutionPath "tools\install.ps1"
$dotnetVersion = $env:CLI_VERSION

$env:DOTNET_INSTALL_DIR = "$(Convert-Path "$PSScriptRoot")\.dotnetcli"

if ($env:CI -ne $null) {
    $RestorePackages = $true
}

if (!(Test-Path $env:DOTNET_INSTALL_DIR)) {
    mkdir $env:DOTNET_INSTALL_DIR | Out-Null
    $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.ps1"
    Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1" -OutFile $installScript
    & $installScript -Version "$dotnetVersion" -InstallDir "$env:DOTNET_INSTALL_DIR" -NoPath
}

$env:PATH = "$env:DOTNET_INSTALL_DIR;$env:PATH"
$dotnet   = "$env:DOTNET_INSTALL_DIR\dotnet"

function DotNetRestore { param([string]$Project)
    & $dotnet restore $Project --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE"
    }
}

function DotNetBuild { param([string]$Project, [string]$Configuration, [string]$VersionSuffix)
    if ($VersionSuffix) {
        & $dotnet build $Project --configuration $Configuration --version-suffix "$VersionSuffix"
    } else {
        & $dotnet build $Project --configuration $Configuration
    }
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

function DotNetPack { param([string]$Project, [string]$Configuration, [string]$VersionSuffix)
    if ($VersionSuffix) {
        & $dotnet pack $Project --configuration $Configuration --version-suffix "$VersionSuffix"
    } else {
        & $dotnet pack $Project --configuration $Configuration
    }
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed with exit code $LASTEXITCODE"
    }
}

function DotNetTest { param([string]$Project)
    & $dotnet test $Project --no-build
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }
}

$projects = @(
    (Join-Path $solutionPath "src\JustEat.StatsD\project.json"),
    (Join-Path $solutionPath "src\PerfTestHarness\project.json")
)

$packageProjects = @(
    (Join-Path $solutionPath "src\JustEat.StatsD\project.json")
)

$testProjects = @(
    #(Join-Path $solutionPath "src\JustEat.StatsD.Tests\project.json")
)

if ($RestorePackages -eq $true) {
    Write-Host "Restoring NuGet packages for $($projects.Count) projects..." -ForegroundColor Green
    ForEach ($project in $projects) {
        DotNetRestore $project
    }
}

Write-Host "Building $($projects.Count) projects..." -ForegroundColor Green
ForEach ($project in $projects) {
    DotNetBuild $project $Configuration $PrereleaseSuffix
}

Write-Host "Packaging $($packageProjects.Count) projects..." -ForegroundColor Green
ForEach ($project in $packageProjects) {
    DotNetPack $project $Configuration $PrereleaseSuffix
}

if ($RunTests -eq $true) {
    Write-Host "Testing $($testProjects.Count) project(s)..." -ForegroundColor Green
    ForEach ($project in $testProjects) {
        DotNetTest $project
    }
}
