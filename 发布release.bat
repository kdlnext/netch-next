@echo off
setlocal EnableExtensions EnableDelayedExpansion
chcp 65001 >nul
title Netch-Next Windows x64 Release Publisher

set "ROOT=%~dp0"
cd /d "%ROOT%"

set "CONFIG=Release"
set "RID=win-x64"
set "OUT_DIR=%ROOT%release"
set "PACKAGE_DIR=%OUT_DIR%\Netch-Next"
set "BIN_DIR=%PACKAGE_DIR%\bin"
set "ZIP_PATH=%OUT_DIR%\Netch-Next.zip"
set "CHANGELOG=%ROOT%更新日志.txt"
set "RELEASE_DESC=%OUT_DIR%\release-description.md"

echo ==============================================
echo Publishing Netch-Next Windows x64 release
echo Output: %OUT_DIR%
echo ==============================================

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] dotnet SDK was not found in PATH.
    exit /b 1
)

where powershell >nul 2>&1
if errorlevel 1 (
    echo [ERROR] PowerShell was not found in PATH.
    exit /b 1
)

if not exist "%CHANGELOG%" (
    echo [ERROR] Missing changelog file: %CHANGELOG%
    echo [HINT] Create 更新日志.txt in the repository root, then rerun this script.
    exit /b 1
)

if exist "%OUT_DIR%" (
    echo [1/7] Cleaning previous release output...
    rmdir /s /q "%OUT_DIR%"
)

mkdir "%OUT_DIR%" >nul 2>&1
mkdir "%PACKAGE_DIR%" >nul 2>&1
mkdir "%BIN_DIR%" >nul 2>&1

echo [2/7] Publishing Netch-Next...
dotnet publish "%ROOT%Netch\Netch.csproj" ^
    -c %CONFIG% ^
    -r %RID% ^
    -p:Platform=x64 ^
    -p:SelfContained=true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o "%PACKAGE_DIR%"

if errorlevel 1 (
    echo [ERROR] dotnet publish failed.
    exit /b 1
)

echo [3/7] Copying resource directories...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$ErrorActionPreference='Stop';" ^
    "Copy-Item -Recurse -Force '%ROOT%Storage\i18n' '%PACKAGE_DIR%' ;" ^
    "Copy-Item -Recurse -Force '%ROOT%Storage\mode' '%PACKAGE_DIR%' ;" ^
    "if (Test-Path '%ROOT%Netch\NOTICE.txt') { Copy-Item -Force '%ROOT%Netch\NOTICE.txt' '%PACKAGE_DIR%' }"

if errorlevel 1 (
    echo [ERROR] Failed to copy i18n/mode resources.
    exit /b 1
)

echo [4/7] Copying runtime binaries...
call :copy_if_exists "%ROOT%Storage\stun.txt" "%BIN_DIR%\stun.txt"
call :copy_if_exists "%ROOT%Storage\nfdriver.sys" "%BIN_DIR%\nfdriver.sys"
call :copy_if_exists "%ROOT%Storage\aiodns.conf" "%BIN_DIR%\aiodns.conf"
call :copy_if_exists "%ROOT%Storage\tun2socks.bin" "%BIN_DIR%\tun2socks.bin"
call :copy_if_exists "%ROOT%Other\release\pcap2socks.exe" "%BIN_DIR%\pcap2socks.exe"
call :copy_if_exists "%ROOT%Other\release\wintun.dll" "%BIN_DIR%\wintun.dll"
call :copy_if_exists "%ROOT%Redirector\static\nfapi.dll" "%BIN_DIR%\nfapi.dll"
call :copy_if_exists "%ROOT%singbox\sing-box.exe" "%BIN_DIR%\sing-box.exe"
call :copy_if_exists "%ROOT%singbox\libcronet.dll" "%BIN_DIR%\libcronet.dll"

call :copy_first_existing "%BIN_DIR%\GeoLite2-Country.mmdb" "%ROOT%Netch\bin\Debug\bin\GeoLite2-Country.mmdb" "%ROOT%Netch\bin\x64\Debug\bin\GeoLite2-Country.mmdb"
call :copy_first_existing "%BIN_DIR%\Redirector.bin" "%ROOT%Netch\bin\Debug\bin\Redirector.bin" "%ROOT%Netch\bin\x64\Debug\bin\Redirector.bin"
call :copy_first_existing "%BIN_DIR%\RouteHelper.bin" "%ROOT%Netch\bin\Debug\bin\RouteHelper.bin" "%ROOT%Netch\bin\x64\Debug\bin\RouteHelper.bin"
call :copy_first_existing "%BIN_DIR%\aiodns.bin" "%ROOT%Netch\bin\Debug\bin\aiodns.bin" "%ROOT%Netch\bin\x64\Debug\bin\aiodns.bin"

echo [5/7] Verifying required files...
set "HAS_MISSING="
call :require "%PACKAGE_DIR%\Netch-Next.exe"
call :require "%BIN_DIR%\sing-box.exe"
call :require "%BIN_DIR%\libcronet.dll"
call :require "%BIN_DIR%\stun.txt"
call :require "%BIN_DIR%\nfdriver.sys"
call :require "%BIN_DIR%\aiodns.conf"
call :require "%BIN_DIR%\tun2socks.bin"
call :require "%BIN_DIR%\pcap2socks.exe"
call :require "%BIN_DIR%\wintun.dll"
call :require "%BIN_DIR%\nfapi.dll"
call :require "%PACKAGE_DIR%\i18n"
call :require "%PACKAGE_DIR%\mode"

if defined HAS_MISSING (
    echo.
    echo [WARNING] Release build completed, but some required files or folders are missing.
    echo [HINT] Review the [MISSING] lines above, supply the missing files, and rerun this script.
    exit /b 2
)

echo [6/7] Generating release description...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$ErrorActionPreference='Stop';" ^
    "$changelog = Get-Content -Path '%CHANGELOG%' -Raw -Encoding UTF8;" ^
    "$lines = @();" ^
    "$lines += '# Netch-Next';" ^
    "$lines += '';" ^
    "$lines += 'Windows x64 release package: `Netch-Next.zip`';" ^
    "$lines += '';" ^
    "$lines += '## Release Notes';" ^
    "$lines += '';" ^
    "$lines += $changelog.TrimEnd();" ^
    "[System.IO.File]::WriteAllLines('%RELEASE_DESC%', $lines, [System.Text.UTF8Encoding]::new($false))"

if errorlevel 1 (
    echo [ERROR] Failed to generate release description.
    exit /b 1
)

echo [7/7] Creating Netch-Next.zip...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$ErrorActionPreference='Stop';" ^
    "if (Test-Path '%ZIP_PATH%') { Remove-Item -Force '%ZIP_PATH%' };" ^
    "Compress-Archive -Path '%PACKAGE_DIR%' -DestinationPath '%ZIP_PATH%' -CompressionLevel Optimal"

if errorlevel 1 (
    echo [ERROR] Failed to create zip package.
    exit /b 1
)

echo.
echo Release artifacts are ready:
echo   %PACKAGE_DIR%
echo   %ZIP_PATH%
echo   %RELEASE_DESC%
exit /b 0

:copy_if_exists
if exist "%~1" (
    copy /y "%~1" "%~2" >nul
) else (
    echo [WARN] Missing optional source: %~1
)
exit /b 0

:copy_first_existing
if exist "%~2" (
    copy /y "%~2" "%~1" >nul
    exit /b 0
)
if exist "%~3" (
    copy /y "%~3" "%~1" >nul
    exit /b 0
)
echo [WARN] Missing optional source: %~2 or %~3
exit /b 0

:require
if not exist "%~1" (
    echo [MISSING] %~1
    set "HAS_MISSING=1"
)
exit /b 0
