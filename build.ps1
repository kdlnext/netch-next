param (
	[Parameter()]
	[ValidateSet('Debug', 'Release')]
	[string]
	$Configuration = 'Release',

	[Parameter()]
	[ValidateNotNullOrEmpty()]
	[string]
	$OutputPath = 'release',

	[Parameter()]
	[bool]
	$SelfContained = $True,

	[Parameter()]
	[bool]
	$PublishSingleFile = $True,

	[Parameter()]
	[bool]
	$PublishReadyToRun = $False
)

$ProgressPreference = 'SilentlyContinue'

Push-Location (Split-Path $MyInvocation.MyCommand.Path -Parent)

if ( Test-Path -Path $OutputPath ) {
    rm -Recurse -Force $OutputPath
}
New-Item -ItemType Directory -Name $OutputPath | Out-Null

Push-Location $OutputPath
New-Item -ItemType Directory -Name 'bin'  | Out-Null
cp -Recurse -Force '..\Storage\i18n' '.'  | Out-Null
cp -Recurse -Force '..\Storage\mode' '.'  | Out-Null
cp -Recurse -Force '..\Storage\stun.txt' 'bin'  | Out-Null
cp -Recurse -Force '..\Storage\nfdriver.sys' 'bin'  | Out-Null
cp -Recurse -Force '..\Storage\aiodns.conf' 'bin'  | Out-Null
Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/Loyalsoldier/geoip/release/Country.mmdb' -OutFile 'bin\GeoLite2-Country.mmdb'
#cp -Recurse -Force '..\Storage\GeoLite2-Country.mmdb' 'bin'  | Out-Null
cp -Recurse -Force '..\Storage\tun2socks.bin' 'bin'  | Out-Null
cp -Recurse -Force '..\Storage\README.md' 'bin'  | Out-Null

try {
    Write-Host 'Downloading latest sing-box...'
    $githubRelease = Invoke-RestMethod -Uri 'https://api.github.com/repos/SagerNet/sing-box/releases/latest'
    $singboxTag = $githubRelease.tag_name
    $singboxVersion = $singboxTag.Substring(1)
    $singboxUrl = "https://github.com/SagerNet/sing-box/releases/download/$singboxTag/sing-box-$singboxVersion-windows-amd64.zip"
    Invoke-WebRequest -Uri $singboxUrl -OutFile "bin\sing-box.zip"
    Expand-Archive -Path "bin\sing-box.zip" -DestinationPath "bin\sing-box-extracted" -Force
    cp -Force "bin\sing-box-extracted\sing-box-$singboxVersion-windows-amd64\sing-box.exe" "bin\"
    rm -Recurse -Force "bin\sing-box-extracted"
    rm -Force "bin\sing-box.zip"
} catch {
    Write-Host "Failed to download sing-box: $_"
}

Pop-Location

if ( -Not ( Test-Path '.\Other\release' ) ) {
	.\Other\build.ps1
	if ( -Not $? ) {
		exit $lastExitCode
	}
}
cp -Force '.\Other\release\*.bin' "$OutputPath\bin"
cp -Force '.\Other\release\*.dll' "$OutputPath\bin"
cp -Force '.\Other\release\*.exe' "$OutputPath\bin"

if ( -Not ( Test-Path ".\Netch\bin\$Configuration" ) ) {
	Write-Host
	Write-Host 'Building Netch'

	dotnet publish `
		-c $Configuration `
		-r 'win-x64' `
		-p:Platform='x64' `
		-p:SelfContained=$SelfContained `
		-p:PublishTrimmed=$PublishReadyToRun `
		-p:PublishSingleFile=$PublishSingleFile `
		-p:PublishReadyToRun=$PublishReadyToRun `
		-p:PublishReadyToRunShowWarnings=$PublishReadyToRun `
		-p:IncludeNativeLibrariesForSelfExtract=$SelfContained `
		-o ".\Netch\bin\$Configuration" `
		'.\Netch\Netch.csproj'
	if ( -Not $? ) { exit $lastExitCode }
}
cp -Force ".\Netch\bin\$Configuration\Netch.exe" $OutputPath

if ( -Not ( Test-Path ".\Redirector\bin\$Configuration" ) ) {
	Write-Host
	Write-Host 'Building Redirector'

	if (Get-Command msbuild -ErrorAction SilentlyContinue) {
		msbuild `
			-property:Configuration=$Configuration `
			-property:Platform=x64 `
			'.\Redirector\Redirector.vcxproj'
		if ( -Not $? ) { exit $lastExitCode }
		cp -Force ".\Redirector\bin\$Configuration\nfapi.dll"      "$OutputPath\bin"
		cp -Force ".\Redirector\bin\$Configuration\Redirector.bin" "$OutputPath\bin"
	} else {
		Write-Host "Warning: msbuild not found, skipping Redirector build. Legacy mode may not work."
	}
} elseif ( Test-Path ".\Redirector\bin\$Configuration\Redirector.bin" ) {
	cp -Force ".\Redirector\bin\$Configuration\nfapi.dll"      "$OutputPath\bin"
	cp -Force ".\Redirector\bin\$Configuration\Redirector.bin" "$OutputPath\bin"
}

if ( -Not ( Test-Path ".\RouteHelper\bin\$Configuration" ) ) {
	Write-Host
	Write-Host 'Building RouteHelper'

	if (Get-Command msbuild -ErrorAction SilentlyContinue) {
		msbuild `
			-property:Configuration=$Configuration `
			-property:Platform=x64 `
			'.\RouteHelper\RouteHelper.vcxproj'
		if ( -Not $? ) { exit $lastExitCode }
		cp -Force ".\RouteHelper\bin\$Configuration\RouteHelper.bin" "$OutputPath\bin"
	} else {
		Write-Host "Warning: msbuild not found, skipping RouteHelper build. Legacy mode may not work."
	}
} elseif ( Test-Path ".\RouteHelper\bin\$Configuration\RouteHelper.bin" ) {
	cp -Force ".\RouteHelper\bin\$Configuration\RouteHelper.bin" "$OutputPath\bin"
}

if ( $Configuration.Equals('Release') ) {
	rm -Force "$OutputPath\*.pdb"
	rm -Force "$OutputPath\*.xml"
}

Write-Host "Zipping the release folder..."
Compress-Archive -Path ".\$OutputPath\*" -DestinationPath '.\Netch-Next-Windows-x64.zip' -Force
Write-Host "Done! Netch-Next-Windows-x64.zip is ready."

Pop-Location
exit 0
