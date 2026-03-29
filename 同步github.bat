@echo off
setlocal EnableExtensions EnableDelayedExpansion
chcp 65001 >nul

echo =========================================
echo         Git Push Helper
echo =========================================

if not exist ".gitenv" (
    echo [ERROR] Missing .gitenv in the current directory.
    pause
    exit /b 1
)

for /f "usebackq tokens=1,* delims==" %%A in (".gitenv") do (
    if not "%%~A"=="" set "%%~A=%%~B"
)

if "%GITHUB_USER%"=="" (
    echo [ERROR] GITHUB_USER is not set in .gitenv.
    pause
    exit /b 1
)

if "%GITHUB_TOKEN%"=="" (
    echo [ERROR] GITHUB_TOKEN is not set in .gitenv.
    pause
    exit /b 1
)

if "%REPO_URL%"=="" (
    echo [ERROR] REPO_URL is not set in .gitenv.
    pause
    exit /b 1
)

if "%TARGET_BRANCH%"=="" (
    set "TARGET_BRANCH=main"
)

git rev-parse --is-inside-work-tree >nul 2>nul
if errorlevel 1 (
    echo [ERROR] Current directory is not a Git repository.
    pause
    exit /b 1
)

set "DEFAULT_MSG=%COMMIT_PREFIX% %DATE% %TIME%"

echo Enter commit message.
echo Press Enter to use: [%DEFAULT_MSG%]
set /p USER_MSG=">> "

if "%USER_MSG%"=="" (
    set "FINAL_MSG=%DEFAULT_MSG%"
) else (
    set "FINAL_MSG=%USER_MSG%"
)

echo [INFO] Staging changes...
git add -A
if errorlevel 1 (
    echo [ERROR] git add failed.
    goto :FAIL
)

git diff --cached --quiet
if errorlevel 1 (
    echo [INFO] Creating commit: "%FINAL_MSG%"
    git commit -m "%FINAL_MSG%"
    if errorlevel 1 (
        echo [ERROR] git commit failed.
        goto :FAIL
    )
) else (
    echo [INFO] No staged changes to commit. Push will use current HEAD.
)

set "AUTH_URL=https://%GITHUB_USER%:%GITHUB_TOKEN%@%REPO_URL%"

echo [INFO] Pushing HEAD to %TARGET_BRANCH%...
git push "%AUTH_URL%" HEAD:%TARGET_BRANCH% >nul 2>git_error.log
if errorlevel 1 (
    echo [ERROR] git push failed. See git_error.log
    type git_error.log
    echo.
    echo [HINT] If remote branch is ahead, pull or rebase manually first.
    echo [HINT] If you want to overwrite remote history, use 强制推送github.bat instead.
    goto :FAIL
)

echo =========================================
echo [SUCCESS] Push completed.
echo =========================================
pause
exit /b 0

:FAIL
echo [FATAL] Script stopped with errors.
pause
exit /b 1
