param(
    [string] $PackageRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$ErrorActionPreference = "Stop"

function Resolve-PackagePath {
    param([string] $RelativePath)
    return Join-Path $PackageRoot $RelativePath
}

function Assert-PathExists {
    param([string] $RelativePath)

    if (-not (Test-Path -LiteralPath (Resolve-PackagePath $RelativePath))) {
        throw "Missing required path: $RelativePath"
    }
}

Push-Location $PackageRoot
try {
    if (Test-Path -LiteralPath "Packages") {
        throw "Beacon should be a root-level UPM package; Packages/ must not contain package content."
    }

    $package = Get-Content -Raw package.json | ConvertFrom-Json
    if ($package.name -ne "com.kostasban.beacon") {
        throw "Unexpected package name: $($package.name)"
    }

    if ($package.version -ne "0.1.0") {
        throw "Unexpected package version: $($package.version)"
    }

    if ($package.unity -ne "6000.3") {
        throw "Unexpected Unity target: $($package.unity)"
    }

    if ($package.dependencies."com.unity.modules.unitywebrequest" -ne "1.0.0") {
        throw "package.json must depend on com.unity.modules.unitywebrequest because HttpConfigSource uses UnityWebRequest."
    }

    $packageInfo = Get-Content -Raw Runtime/BeaconPackageInfo.cs
    if ($packageInfo -notmatch "Version = `"$($package.version)`"") {
        throw "BeaconPackageInfo.Version does not match package.json version $($package.version)."
    }

    if ($packageInfo -notmatch "PackageName = `"$($package.name)`"") {
        throw "BeaconPackageInfo.PackageName does not match package.json name $($package.name)."
    }

    $runtimeAsmdef = Get-Content -Raw Runtime/KostasBan.Beacon.asmdef | ConvertFrom-Json
    if ($runtimeAsmdef.name -ne "KostasBan.Beacon") {
        throw "Unexpected runtime asmdef name: $($runtimeAsmdef.name)"
    }

    if ($runtimeAsmdef.references -notcontains "UnityEngine.UnityWebRequestModule") {
        throw "Runtime asmdef must reference UnityEngine.UnityWebRequestModule because HttpConfigSource uses UnityWebRequest."
    }

    $editorAsmdef = Get-Content -Raw Editor/KostasBan.Beacon.Editor.asmdef | ConvertFrom-Json
    if ($editorAsmdef.name -ne "KostasBan.Beacon.Editor") {
        throw "Unexpected editor asmdef name: $($editorAsmdef.name)"
    }

    $testsAsmdef = Get-Content -Raw Tests/Runtime/KostasBan.Beacon.Tests.asmdef | ConvertFrom-Json
    if ($testsAsmdef.name -ne "KostasBan.Beacon.Tests") {
        throw "Unexpected test asmdef name: $($testsAsmdef.name)"
    }

    if ($testsAsmdef.includePlatforms -notcontains "Editor") {
        throw "Test asmdef must include the Editor platform for EditMode test discovery."
    }

    if ($testsAsmdef.optionalUnityReferences -notcontains "TestAssemblies") {
        throw "Test asmdef must include optionalUnityReferences: TestAssemblies so Unity discovers EditMode tests."
    }

    $requiredPaths = @(
        "package.json",
        "package.json.meta",
        "Runtime/KostasBan.Beacon.asmdef",
        "Runtime/BeaconPackageInfo.cs",
        "Runtime/BeaconClient.cs",
        "Runtime/Repositories/HttpConfigSource.cs",
        "Editor/KostasBan.Beacon.Editor.asmdef",
        "Editor/BeaconDebugWindow.cs",
        "Tests/Runtime/KostasBan.Beacon.Tests.asmdef",
        "Samples~/Demo/README.md",
        "README.md",
        "CHANGELOG.md",
        "CONTRIBUTING.md",
        "SECURITY.md",
        "AGENTS.md",
        "Docs/api-overview.md",
        "Docs/architecture-decisions.md",
        "Docs/future-integrations.md",
        "Docs/agent-usage.md",
        "Docs/images/README.md",
        "Docs/media/README.md",
        "Tools/Validate-Package.ps1",
        "Tools/Validate-Package.ps1.meta",
        ".github/ISSUE_TEMPLATE/bug_report.md",
        ".github/ISSUE_TEMPLATE/feature_request.md",
        ".github/ISSUE_TEMPLATE/integration_question.md",
        ".github/workflows/package-validation.yml",
        ".github/workflows/unity-tests.yml",
        ".github/workflows/release.yml"
    )

    foreach ($path in $requiredPaths) {
        Assert-PathExists $path
    }

    $staleMatches = Get-ChildItem -Recurse -File -Include *.cs,*.asmdef,*.json,*.md,*.yml,*.ps1 |
        Where-Object { $_.FullName -notmatch "\\.git\\" } |
        Where-Object { $_.Name -ne "Validate-Package.ps1" } |
        Select-String -Pattern "KBanakakis|com[.]kbanakakis[.]beacon|Packages[/\\]com[.]kbanakakis[.]beacon|com[.]KostasBan[.]Beacon" -CaseSensitive -ErrorAction SilentlyContinue

    if ($staleMatches) {
        $staleMatches | ForEach-Object { Write-Host $_ }
        throw "Found stale Beacon package identity or nested package path."
    }

    $runtimeFiles = Get-ChildItem Runtime,Editor,Tests -Recurse -File | Where-Object {
        $_.Extension -in ".cs", ".asmdef", ".rsp"
    }

    foreach ($file in $runtimeFiles) {
        $metaPath = "$($file.FullName).meta"
        if (-not (Test-Path -LiteralPath $metaPath)) {
            throw "Missing Unity meta file for $($file.FullName)"
        }
    }

    Write-Host "Package validation passed for $($package.name)@$($package.version)."
}
finally {
    Pop-Location
}
