$ErrorActionPreference = "Stop"
Write-Host "Setting up test environment resources..."
$outDir = "Netch\bin\Debug"
$binDir = "$outDir\bin"

function Copy-OptionalItem {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination
    )

    if (-not (Test-Path $Source)) { return }

    try {
        Copy-Item -Recurse -Force $Source $Destination
    } catch {
        Write-Warning "Failed to copy $Source to $Destination : $($_.Exception.Message)"
    }
}

if (-Not (Test-Path $outDir)) { New-Item -ItemType Directory -Force -Path $outDir | Out-Null }
if (-Not (Test-Path $binDir)) { New-Item -ItemType Directory -Force -Path $binDir | Out-Null }
foreach ($dir in @("data", "logging")) {
    $target = Join-Path $outDir $dir
    if (-Not (Test-Path $target)) { New-Item -ItemType Directory -Force -Path $target | Out-Null }
}

# Copy resources from Storage just like the production build
Copy-OptionalItem "Storage\i18n" $outDir
Copy-OptionalItem "Storage\mode" $outDir
Copy-OptionalItem "Storage\stun.txt" $binDir
Copy-OptionalItem "Storage\nfdriver.sys" $binDir
Copy-OptionalItem "Storage\aiodns.conf" $binDir
Copy-OptionalItem "Storage\tun2socks.bin" $binDir
Copy-OptionalItem "release\bin\GeoLite2-Country.mmdb" $binDir
Copy-OptionalItem "release\bin\README.md" $binDir
Copy-OptionalItem "Other\release\pcap2socks.exe" $binDir
Copy-OptionalItem "Other\release\wintun.dll" $binDir
Copy-OptionalItem "Redirector\static\nfapi.dll" $binDir
Copy-OptionalItem "singbox\libcronet.dll" $binDir
Copy-OptionalItem "singbox\sing-box.exe" $binDir

foreach ($fallbackDir in @("Netch\bin\x64\Debug\bin", "Netch\bin\Debug\bin")) {
    foreach ($existingBinary in @("Redirector.bin", "RouteHelper.bin", "aiodns.bin")) {
        $candidate = Join-Path $fallbackDir $existingBinary
        if ((Test-Path $candidate) -and ($candidate -ne (Join-Path $binDir $existingBinary))) {
            Copy-OptionalItem $candidate $binDir
        }
    }
}

if (-Not (Test-Path "$binDir\sing-box.exe")) {
    Write-Host ">> sing-box.exe kernel is missing from test environment! Automatically downloading..."
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $githubRelease = Invoke-RestMethod -Uri 'https://api.github.com/repos/SagerNet/sing-box/releases/latest'
        $singboxTag = $githubRelease.tag_name
        $singboxVersion = $singboxTag.Substring(1)
        $singboxUrl = "https://github.com/SagerNet/sing-box/releases/download/$singboxTag/sing-box-$singboxVersion-windows-amd64.zip"
        
        Write-Host "Fetching from $singboxUrl ..."
        Invoke-WebRequest -Uri $singboxUrl -OutFile "$binDir\test-singbox.zip"
        Expand-Archive -Path "$binDir\test-singbox.zip" -DestinationPath "$binDir\test-singbox-extracted" -Force
        Copy-Item -Force "$binDir\test-singbox-extracted\sing-box-$singboxVersion-windows-amd64\sing-box.exe" -Destination "$binDir\sing-box.exe"
        Remove-Item -Recurse -Force "$binDir\test-singbox-extracted"
        Remove-Item -Force "$binDir\test-singbox.zip"
        Write-Host "Kernel downloaded and placed successfully into $binDir\ !"
    } catch {
        Write-Host "Failed to download sing-box kernel: $_"
    }
} else {
    Write-Host ">> sing-box kernel is already present."
}

$requiredFiles = @(
    "sing-box.exe",
    "libcronet.dll",
    "nfdriver.sys",
    "nfapi.dll",
    "pcap2socks.exe",
    "tun2socks.bin",
    "wintun.dll",
    "GeoLite2-Country.mmdb",
    "aiodns.conf",
    "stun.txt"
)

$missingFiles = $requiredFiles | Where-Object { -not (Test-Path (Join-Path $binDir $_)) }
if ($missingFiles.Count -gt 0) {
    Write-Warning ("The following runtime files are still missing: " + ($missingFiles -join ", "))
} else {
    Write-Host ">> Core test runtime files are present in $outDir"
}

$featureFiles = @(
    "Redirector.bin",
    "RouteHelper.bin",
    "aiodns.bin"
)

$missingFeatureFiles = $featureFiles | Where-Object { -not (Test-Path (Join-Path $binDir $_)) }
if ($missingFeatureFiles.Count -gt 0) {
    Write-Warning ("The following feature-specific files are missing and may limit some modes or protocols: " + ($missingFeatureFiles -join ", "))
}
