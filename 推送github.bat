@echo off
setlocal enabledelayedexpansion
:: ---------------------------------------------------------------------
:: Google Standard Script: Git Automation Pipeline
:: 功能：读取环境变量，执行标准化 Git 提交与推送
:: ---------------------------------------------------------------------
chcp 65001 >nul

echo =========================================
echo       Git 自动化流水线 (V2.0 - Decoupled)
echo =========================================

:: 1. 检查并加载 .gitenv 配置文件
if not exist ".gitenv" (
    echo [ERROR] 缺失配置文件 .gitenv，流程终止。
    pause
    exit /b 1
)

:: 遍历读取 .gitenv 并注入当前进程环境变量
for /f "usebackq delims== tokens=1,2" %%i in (".gitenv") do (
    set "%%i=%%j"
)

:: 2. 环境完整性验证 (防范空变量)
if "%GITHUB_TOKEN%"=="" (
    echo [ERROR] .gitenv 中未配置 GITHUB_TOKEN。
    pause
    exit /b 1
)

:: 3. 检查仓库状态
if not exist ".git" (
    echo [ERROR] 当前非 Git 仓库，请先运行 git init。
    pause
    exit /b 1
)

:: 4. 交互式获取提交信息 (支持默认值)
echo [INFO] 请输入本次提交的备注。
echo [TIPS] 直接回车将使用预设值：[%COMMIT_PREFIX% %DATE% %TIME%]
set /p USER_MSG=">> "

:: 逻辑判断：若用户直接回车，则应用预设值
if "%USER_MSG%"=="" (
    set "FINAL_MSG=%COMMIT_PREFIX% %DATE% %TIME%"
) else (
    set "FINAL_MSG=%USER_MSG%"
)

:: 5. 执行原子化 Git 操作
echo [INFO] 变更集暂存中...
git add .
if %ERRORLEVEL% neq 0 (
    echo [ERROR] git add 失败，请检查文件权限或占用。
    goto :FAIL
)

echo [INFO] 正在生成记录: "%FINAL_MSG%"
git commit -m "%FINAL_MSG%"
:: 注意：若无文件变更，git commit 会返回非0，此处记录日志但不中断

:: 6. 构建加密传输链路并推送
set "AUTH_URL=https://%GITHUB_USER%:%GITHUB_TOKEN%@%REPO_URL%"

echo [INFO] 正在推送到远程分支 [%TARGET_BRANCH%]...
:: 静默执行推送，防止在报错信息中泄露 Token
git push %AUTH_URL% %TARGET_BRANCH% >nul 2>git_error.log

if %ERRORLEVEL% neq 0 (
    echo [ERROR] 推送失败！详细错误已记录至 git_error.log。
    echo [HINT] 请检查网络连接、Token 权限或远端是否存在历史冲突。
    type git_error.log
    goto :FAIL
)

echo =========================================
echo [SUCCESS] 自动化任务执行完毕。
echo =========================================
pause
exit /b 0

:FAIL
echo [FATAL] 任务流在执行过程中异常中断。
pause
exit /b 1