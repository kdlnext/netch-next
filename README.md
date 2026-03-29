# Netch-Next

[English](#english) | [中文](#中文)
## English

`Netch-Next` is a Windows proxy and traffic-routing client evolved from the original `Netch` project. The current branch uses `sing-box` as its primary core and focuses on modern multi-protocol node management, mode switching, subscription updates, and practical desktop usability.

### Project Focus

- A Windows desktop client for proxy management and traffic routing
- Unified multi-protocol configuration generation and startup based on `sing-box`
- Keeps the familiar Netch workflow around servers, modes, subscriptions, and launch control
- Designed for local debugging, test builds, and packaged releases

### Highlights

- Uses `sing-box` as the main proxy core
- Provides GUI-based management for servers, modes, and subscriptions
- Separates debug/test layout from release packaging layout
- Checks for new releases and opens the GitHub Release page directly
- Includes local scripts for testing, release builds, and repository sync

### Current Protocol Support

The current codebase includes support for the following protocols and node types:

- SOCKS5
- SSH
- Trojan
- VMess
- VLESS
- TUIC
- Hysteria2
- WireGuard

### Project Links

- Repository: [https://github.com/kdlnext/netch-next](https://github.com/kdlnext/netch-next)
- Telegram Channel: [https://t.me/netchnext](https://t.me/netchnext)
- Original upstream project: [https://github.com/netchx/netch](https://github.com/netchx/netch)

### Development and Debugging

- Current application version: `2.0.2`
- Target framework: `net8.0-windows`
- Main test directory: `E:\Projects\netch\Netch\bin\Debug\`
- Quick test launch: `启动测试版.bat`
- Windows x64 release build: `编译release.bat`
- Full release packaging flow with zip and release notes: `发布release.bat`

### Release Packaging

- The release script builds a Windows x64 client
- The final archive is `Netch-Next.zip`
- The zip file contains a `Netch-Next` folder with the full client payload
- Release notes are generated from `更新日志.txt`

### Update Behavior

- The application only checks whether a newer GitHub Release exists
- When a new version is found, the UI shows a label and a prompt
- Clicking the prompt opens the latest release page directly instead of downloading and replacing files in the background

### Repository Sync Scripts

- `同步github.bat` is used for normal sync against the latest remote state
- When the remote branch is ahead, the script prompts the user with common handling options before continuing
- `强制推送github.bat` force-pushes the local state to overwrite the remote branch

### Thanks

This project continues from the original `Netch` project and remains deeply rooted in its upstream work.

Special thanks to the original author and all upstream contributors whose work made this fork possible.

Upstream project:
[https://github.com/netchx/netch](https://github.com/netchx/netch)

---

## 中文

`Netch-Next` 是一个基于原始 `Netch` 项目继续演进的 Windows 代理与流量接管客户端，当前以 `sing-box` 为核心内核，面向新版多协议节点管理、模式切换、订阅更新和桌面端使用体验进行持续完善。

### 项目定位

- 面向 Windows 桌面环境的代理客户端与流量分流工具
- 基于 `sing-box` 的多协议统一配置生成与启动
- 保留 Netch 系列熟悉的服务器、模式、订阅、启动控制工作流
- 适合本地调试、测试版验证与正式发布打包

### 主要特性

- 使用 `sing-box` 作为核心代理内核
- 支持图形化管理服务器、模式与订阅
- 支持测试版目录与发布版目录的独立整理
- 检测到新版本后，仅提示并跳转到 GitHub Release 页面
- 提供本地调试、构建发布、同步仓库等批处理脚本

### 当前协议支持

当前代码已包含以下协议/节点类型支持：

- SOCKS5
- SSH
- Trojan
- VMess
- VLESS
- TUIC
- Hysteria2
- WireGuard

### 项目链接

- 仓库地址：[https://github.com/kdlnext/netch-next](https://github.com/kdlnext/netch-next)
- Telegram 频道：[https://t.me/netchnext](https://t.me/netchnext)
- 原始上游项目：[https://github.com/netchx/netch](https://github.com/netchx/netch)

### 开发与调试

- 当前主程序版本：`2.0.2`
- 目标框架：`net8.0-windows`
- 主要测试目录：`E:\Projects\netch\Netch\bin\Debug\`
- 快速测试可使用仓库根目录下的 `启动测试版.bat`
- Windows x64 发布构建可使用 `编译release.bat`
- 带压缩包与发布说明的发布流程可使用 `发布release.bat`

### 发布说明

- 发布脚本会生成 Windows x64 客户端
- 最终压缩包为 `Netch-Next.zip`
- 压缩包内部包含 `Netch-Next` 文件夹及完整客户端文件
- 发布描述会根据 `更新日志.txt` 自动生成

### 更新机制

- 程序仅检查 GitHub Release 是否有新版本
- 检测到新版本后，会显示界面提示与弹窗提醒
- 点击后不会后台自动下载或替换文件，而是直接打开新版 Release 页面

### 仓库同步脚本

- `同步github.bat` 用于正常同步远端最新状态
- 当远端有新提交时，脚本会提示常见处理方式，由用户决定是否 rebase、取消或强制覆盖
- `强制推送github.bat` 用于直接以本地版本覆盖远端

### 致谢

本项目基于原始 `Netch` 项目继续开发与调整。

感谢原始作者与所有上游贡献者的工作，使这个分支能够在现有基础上继续推进。

上游项目地址：
[https://github.com/netchx/netch](https://github.com/netchx/netch)


