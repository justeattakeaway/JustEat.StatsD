param(
    [Parameter(Mandatory=$false)][bool]   $RestorePackages  = $false,
    [Parameter(Mandatory=$false)][string] $Configuration    = "Release",
    [Parameter(Mandatory=$false)][string] $VersionSuffix    = "",
    [Parameter(Mandatory=$false)][string] $OutputPath       = "",
    [Parameter(Mandatory=$false)][bool]   $RunTests         = $true,
    [Parameter(Mandatory=$false)][bool]   $CreatePackages   = $true
)

$ErrorActionPreference = "Stop"

$solutionPath  = Split-Path $MyInvocation.MyCommand.Definition
$getDotNet     = Join-Path $solutionPath "tools\install.ps1"
$dotnetVersion = $env:CLI_VERSION

if ($OutputPath -eq "") {
    $OutputPath = "$(Convert-Path "$PSScriptRoot")\artifacts"
}

$env:DOTNET_INSTALL_DIR = "$(Convert-Path "$PSScriptRoot")\.dotnetcli"

if ($env:CI -ne $null) {

    $RestorePackages = $true

    if (($VersionSuffix -eq "" -and $env:APPVEYOR_REPO_TAG -eq "false" -and $env:APPVEYOR_BUILD_NUMBER -ne "") -eq $true) {
        $ThisVersion = $env:APPVEYOR_BUILD_NUMBER -as [int]
        $VersionSuffix = "beta-" + $ThisVersion.ToString("0000")
    }
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

function DotNetBuild { param([string]$Project, [string]$Configuration, [string]$Framework, [string]$VersionSuffix)
    if ($VersionSuffix) {
        & $dotnet build $Project --output (Join-Path $OutputPath $Framework) --framework $Framework --configuration $Configuration --version-suffix "$VersionSuffix"
    } else {
        & $dotnet build $Project --output (Join-Path $OutputPath $Framework) --framework $Framework --configuration $Configuration
    }
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

function DotNetTest { param([string]$Project)
    & $dotnet test $Project
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }
}

function DotNetPack { param([string]$Project, [string]$Configuration, [string]$VersionSuffix)
    if ($VersionSuffix) {
        & $dotnet pack $Project --output $OutputPath --configuration $Configuration --version-suffix "$VersionSuffix" --no-build
    } else {
        & $dotnet pack $Project --output $OutputPath --configuration $Configuration --no-build
    }
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed with exit code $LASTEXITCODE"
    }
}

$projects = @(
    (Join-Path $solutionPath "src\JustEat.StatsD\project.json")
)

$testProjects = @(
    #(Join-Path $solutionPath "src\JustEat.StatsD.Tests\project.json")
)

$packageProjects = @(
    (Join-Path $solutionPath "src\JustEat.StatsD\project.json")
)

$restoreProjects = @(
    (Join-Path $solutionPath "src\JustEat.StatsD\project.json")
    #(Join-Path $solutionPath "src\JustEat.StatsD.Tests\project.json")
)

if ($RestorePackages -eq $true) {
    Write-Host "Restoring NuGet packages for $($restoreProjects.Count) projects..." -ForegroundColor Green
    ForEach ($project in $restoreProjects) {
        DotNetRestore $project
    }
}

Write-Host "Building $($projects.Count) projects..." -ForegroundColor Green
ForEach ($project in $projects) {
    DotNetBuild $project $Configuration "netstandard1.3" $VersionSuffix
    DotNetBuild $project $Configuration "net451" $VersionSuffix
}

if ($RunTests -eq $true) {
    Write-Host "Testing $($testProjects.Count) project(s)..." -ForegroundColor Green
    ForEach ($project in $testProjects) {
        DotNetTest $project
    }
}

if ($CreatePackages -eq $true) {
    Write-Host "Creating $($packageProjects.Count) package(s)..." -ForegroundColor Green
    ForEach ($project in $packageProjects) {
        DotNetPack $project $Configuration $VersionSuffix
    }
}
