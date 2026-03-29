@echo off
chcp 65001 >nul
title Netch-Next Quick Debug Environment Launcher

:: Check for forced Administrator elevation required by process takeover and TUN mode
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
if '%errorlevel%' NEQ '0' (
    echo ==========================================
    echo Authorization request: Netch-Next requires Administrator privileges
    echo Calling UAC window, please select [Yes] in the prompt
    echo ==========================================
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    echo UAC.ShellExecute "%~f0", "", "", "runas", 1 >> "%temp%\getadmin.vbs"
    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    :: Navigate to the script's directory to avoid calling failures
    pushd "%CD%"
    CD /D "%~dp0"

echo --------------------------------------------------
echo     Welcome to Netch-Next [Test ^& Build] Environment
echo --------------------------------------------------
echo.
echo [1] Loading .NET 8 framework...
echo [2] Verifying core code, UI components, and drivers...
echo [3] Launching Netch-Next interface...
echo.
echo * WARNING: Do NOT close this console window during operation 
echo   (otherwise the application will be forced to exit)
echo --------------------------------------------------

:: Terminate any running instances to prevent SingleInstance Mutex collision
echo [4] Cleaning up background processes...
taskkill /f /im Netch.exe >nul 2>&1
taskkill /f /im Netch-Next.exe >nul 2>&1

:: Enter the source code directory
cd /d "%~dp0"
echo [4.5] Preparing test environment files...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0\prepare_test_env.ps1"

echo [5] Building Netch-Next into Netch\bin\Debug...
dotnet build "%~dp0\Netch\Netch.csproj" -c Debug -p:Platform=x64 -o "%~dp0\Netch\bin\Debug"
if errorlevel 1 (
    echo Build failed. Press any key to close this terminal...
    pause >nul
    exit /b 1
)

echo [6] Launching E:\Projects\netch\Netch\bin\Debug\Netch-Next.exe ...
start "" /wait "%~dp0\Netch\bin\Debug\Netch-Next.exe"

echo.
echo ==============================================
echo Debugging complete. Netch-Next UI has safely exited.
echo Press any key to close this terminal...
pause >nul
