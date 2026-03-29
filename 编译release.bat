@echo off
setlocal EnableExtensions EnableDelayedExpansion
chcp 65001 >nul
title Netch-Next Windows x64 Release Builder

set "ROOT=%~dp0"
cd /d "%ROOT%"

set "CONFIG=Release"
set "RID=win-x64"
set "OUT_DIR=%ROOT%release"
set "BIN_DIR=%OUT_DIR%\bin"

echo ==============================================
echo Building Netch-Next Windows x64 release
echo Output: %OUT_DIR%
echo ==============================================

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] dotnet SDK was not found in PATH.
    exit /b 1
)

if exist "%OUT_DIR%" (
    echo [1/5] Cleaning previous release output...
    rmdir /s /q "%OUT_DIR%"
)

mkdir "%OUT_DIR%" >nul 2>&1
mkdir "%BIN_DIR%" >nul 2>&1

echo [2/5] Publishing Netch-Next...
dotnet publish "%ROOT%Netch\Netch.csproj" ^
    -c %CONFIG% ^
    -r %RID% ^
    -p:Platform=x64 ^
    -p:SelfContained=true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o "%OUT_DIR%"

if errorlevel 1 (
    echo [ERROR] dotnet publish failed.
    exit /b 1
)

echo [3/5] Copying resource directories...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$ErrorActionPreference='Stop';" ^
    "Copy-Item -Recurse -Force '%ROOT%Storage\i18n' '%OUT_DIR%' ;" ^
    "Copy-Item -Recurse -Force '%ROOT%Storage\mode' '%OUT_DIR%' ;" ^
    "if (Test-Path '%ROOT%Netch\NOTICE.txt') { Copy-Item -Force '%ROOT%Netch\NOTICE.txt' '%OUT_DIR%' }"

if errorlevel 1 (
    echo [ERROR] Failed to copy i18n/mode resources.
    exit /b 1
)

echo [4/5] Copying runtime binaries...
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

echo [5/5] Verifying required files...
set "HAS_MISSING="
call :require "%OUT_DIR%\Netch-Next.exe"
call :require "%BIN_DIR%\sing-box.exe"
call :require "%BIN_DIR%\libcronet.dll"
call :require "%BIN_DIR%\stun.txt"
call :require "%BIN_DIR%\nfdriver.sys"
call :require "%BIN_DIR%\aiodns.conf"
call :require "%BIN_DIR%\tun2socks.bin"
call :require "%BIN_DIR%\pcap2socks.exe"
call :require "%BIN_DIR%\wintun.dll"
call :require "%BIN_DIR%\nfapi.dll"
call :require "%OUT_DIR%\i18n"
call :require "%OUT_DIR%\mode"

if defined HAS_MISSING (
    echo.
    echo [WARNING] Release build completed, but some required files or folders are missing.
    echo Please review the [MISSING] lines above, supply the missing files, and rerun release.bat.
    exit /b 2
)

echo.
echo Release is ready at:
echo   %OUT_DIR%
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
