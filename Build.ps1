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
$solutionFile  = Join-Path $solutionPath "JustEat.StatsD.sln"
$dotnetVersion = "1.0.4"

if ($OutputPath -eq "") {
    $OutputPath = "$(Convert-Path "$PSScriptRoot")\artifacts"
}

if ($env:CI -ne $null) {

    $RestorePackages = $true

    if (($VersionSuffix -eq "" -and $env:APPVEYOR_REPO_TAG -eq "false" -and $env:APPVEYOR_BUILD_NUMBER -ne "") -eq $true) {
        $ThisVersion = $env:APPVEYOR_BUILD_NUMBER -as [int]
        $VersionSuffix = "beta" + $ThisVersion.ToString("0000")
    }
}

$installDotNetSdk = $false;

if ((Get-Command "dotnet.exe" -ErrorAction SilentlyContinue) -eq $null)  {
    Write-Host "The .NET Core SDK is not installed."
    $installDotNetSdk = $true
}
else {
    $installedDotNetVersion = (dotnet --version | Out-String).Trim()
    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Host "The required version of the .NET Core SDK is not installed. Expected $dotnetVersion but $installedDotNetVersion was found."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk -eq $true) {
    $env:DOTNET_INSTALL_DIR = "$(Convert-Path "$PSScriptRoot")\.dotnetcli"

    if (!(Test-Path $env:DOTNET_INSTALL_DIR)) {
        mkdir $env:DOTNET_INSTALL_DIR | Out-Null
        $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.ps1"
        Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1" -OutFile $installScript
        & $installScript -Version "$dotnetVersion" -InstallDir "$env:DOTNET_INSTALL_DIR" -NoPath
    }

    $env:PATH = "$env:DOTNET_INSTALL_DIR;$env:PATH"
    $dotnet   = "$env:DOTNET_INSTALL_DIR\dotnet"
} else {
    $dotnet   = "dotnet"
}

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
        & $dotnet pack $Project --output $OutputPath --configuration $Configuration --version-suffix "$VersionSuffix" --include-symbols --include-source
    } else {
        & $dotnet pack $Project --output $OutputPath --configuration $Configuration --include-symbols --include-source
    }
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed with exit code $LASTEXITCODE"
    }
}

$projects = @(
    (Join-Path $solutionPath "src\JustEat.StatsD\JustEat.StatsD.csproj")
)

$testProjects = @(
    (Join-Path $solutionPath "src\JustEat.StatsD.Tests\JustEat.StatsD.Tests.csproj")
)

$packageProjects = @(
    (Join-Path $solutionPath "src\JustEat.StatsD\JustEat.StatsD.csproj")
)

if ($RestorePackages -eq $true) {
    Write-Host "Restoring NuGet packages for solution..." -ForegroundColor Green
    DotNetRestore $solutionFile
}

Write-Host "Building $($projects.Count) projects..." -ForegroundColor Green
ForEach ($project in $projects) {
    DotNetBuild $project $Configuration "netstandard1.6" $VersionSuffix
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
