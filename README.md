# PCL2-DLPatched

基于 [Plain Craft Launcher 2](https://github.com/Meloong-Git/PCL) 的第三方修改版，由玄剑公会（北域工作室 Northland Studio）维护。

本项目与龙腾猫跃及 PCL 官方项目**无关**。名称、版本标识与界面提示均已标明这是第三方修改版。

---

## 这是什么

上游 2.99.0.0 版本关闭了 Minecraft 原版游戏与整合包的下载功能。本分支恢复了下载能力，并在此基础上增加了面向公会使用的定制功能：

- **恢复原版游戏下载**：与 2.98 及更早版本一致，可正常获取版本列表、安装任意版本
- **可配置的自定义下载源**：支持内置源（Mojang 官方、BMCLAPI）与自建 CDN 的增删、排序与启停
- **正版登录**：内置自建的微软应用 Client ID（已通过 Minecraft 应用注册审核），支持使用正版账号登录
- **空壳「更多」页**：原有小工具已移除，页面保留待重做

### 相对上游的行为差异

| 功能 | 上游 2.99.0.0 | 本项目 |
| --- | --- | --- |
| 原版游戏下载 | 关闭 | 恢复 |
| 整合包安装 | 关闭 | 恢复 |
| 下载源 | 写死 Mojang / BMCLAPI | 可增删排序，支持自定义 CDN |
| 正版登录 | 需自行申请 Client ID | 已内置 |
| 主题切换 | 赞助解锁 | 移除入口，仅保留不透明度与背景自定义 |
| 百宝箱 | 未提供 | 已移除（保留「你知道吗」提示文本） |
| 更新检查、联网公告 | 有 | 不包含（开源实现中未提供） |

---

## 使用

### 直接使用

从 [Releases](https://github.com/homo1145147/PCL2-DLPatched/releases) 或 GitHub Actions 的 Artifacts 下载编译产物，解压后运行 `PCL2_DLPatched.exe`。

> 请将整个文件夹一起使用，`exe` 依赖同目录下的 `MeloongCore.dll`、`PCLCS.dll` 等文件。

**建议**：把启动器放在单独文件夹，不要与其他版本的 PCL 混放。各版本的设置共用注册表路径 `HKCU\Software\PCLDebug`，混放容易互相覆盖。

### 切换下载源

「下载 → 自定义下载源」中可以：

- 添加自定义源：填写名称与根地址（如 `https://mc-cdn.example.com`）
- 调整顺序：列表顺序即优先级，靠前的源会先被尝试
- 启停源：取消勾选即可临时停用
- 测试源：拉取第一个启用源的版本清单，报告耗时与版本数

版本列表加载完成后，若来源不是 Mojang 官方源，页面顶部会提示当前解析自哪个源。

#### 自定义源的目录约定

内置源（BMCLAPI）与自定义源采用**不同的解析模型**：

- **内置源**：按域名替换，`https://piston-data.mojang.com/...` → `https://bmclapi2.bangbang93.com/...`
- **自定义源**：只提供版本清单，其余地址**沿用清单与版本 JSON 中携带的原始 URL**

因此自定义源需要满足：

1. 版本清单可访问，地址为 `{根地址}/mc/game/version_manifest_v2.json`
   （取不到时会回退 `version_manifest.json`；也可直接填写完整的 `.json` 地址）
2. 清单中 `versions[].url` 指向的版本 JSON 可访问
3. 版本 JSON 中的 `downloads.client.url`、`libraries[].downloads.artifact.url`、`assetIndex.url` 等地址**指向该 CDN 自身**

满足以上条件时，整个下载链路（版本 JSON → 客户端 → 库文件 → 资源文件）会全部走该 CDN。

---

## 构建

### 环境要求

- Windows 10 / 11
- [.NET SDK](https://dotnet.microsoft.com/download)（用于还原 `PCLCS` 的 NuGet 依赖）
- [Visual Studio 2022 Build Tools](https://visualstudio.microsoft.com/downloads/) 或 Visual Studio，需包含：
  - **.NET 桌面开发**工作负载
  - **.NET Framework 4.8 目标包**与 **.NET Framework 4.8 SDK**

> 仅安装 .NET SDK 是不够的：主工程是 `.vbproj`（.NET Framework 4.8 + WPF），需要 Build Tools 提供的 MSBuild 与 VB 编译器。

### 构建步骤

```powershell
# 1. 拉取子模块（MeloongCore 是必需的依赖）
git submodule update --init --recursive

# 2. 还原 NuGet 包
dotnet restore PCLCS\PCLCS.csproj

# 3. 编译
msbuild "Plain Craft Launcher 2\Plain Craft Launcher 2.vbproj" -p:Configuration=Release
```

产物位于 `Plain Craft Launcher 2\bin\PCL2_DLPatched.exe`。

> 编译失败并提示文件被占用时，请先关闭正在运行的 `PCL2_DLPatched.exe`。

### 通过 GitHub Actions 构建

`.github/workflows/build.yml` 会在推送 `main` 分支或手动触发时构建 Debug 与 Release 两个配置，并上传产物。

> Fork 仓库的 Actions 默认不执行 workflow，首次使用请在 **Settings → Actions** 中启用。

---

## 项目结构

```
Plain Craft Launcher 2/
├── Modules/
│   ├── Minecraft/
│   │   ├── DlSourceRule.vb        单个下载源的规则定义
│   │   ├── DlSourceRegistry.vb    内置源（Mojang 官方、BMCLAPI）
│   │   ├── DlSourceConfig.vb      下载源配置的读写与解析
│   │   ├── ModDownload.vb         下载逻辑与源调度
│   │   └── ...
│   ├── ModSecret.vb               版本信息、请求头、正版登录 Client ID
│   └── ...
└── Pages/
    ├── PageDownload/
    │   ├── PageDownloadSource.xaml  自定义下载源配置页面
    │   └── PageDownloadInstall.xaml 原版游戏下载页
    └── PageOther/
        └── PageOtherTest.xaml       空壳页面（待重做）
```

---

## 已知限制

- **自定义源的版本数量**：自定义源不校验版本数量，只要求版本列表非空。若源返回了错误页面但被解析为合法 JSON，可能显示异常的空列表。
- **部分镜像的防盗链**：项目已移除向所有请求附加的伪造 `Referer`，但如果镜像方另有 UA 或 IP 限制，仍可能返回 403。
- **更新检查与联网公告**：上游这两部分依赖未公开的实现，本项目不包含。启动器不会自动更新。
- **主题切换**：为遵守上游的许可要求（界面主要色相应保持蓝色），移除了主题切换入口。

---

## 许可证与致谢

本项目是 **Plain Craft Launcher 2** 的修改版本，遵循上游的 [PCL 分发有限许可](./LICENCE) 与《PCL 存储库合理使用指南》。

- 修改内容以「PCL」开头命名（`PCL2-DLPatched`），并标注为第三方修改
- 保留作者龙腾猫跃的署名、赞助链接与正版游戏劝导
- 本项目开源，继续使用同一许可文件

**原始项目**：[Meloong-Git/PCL](https://github.com/Meloong-Git/PCL)　|　**PCL 下载**：[meloong.com/pcl](https://meloong.com/pcl)　|　**赞助作者**：[爱发电](https://meloong.com/afd/a/LTCat)

依赖 [MeloongCore](https://github.com/Meloong-Git/MeloongCore)（子模块）、[PCLCS](PCLCS/) 及各第三方库，版权归原作者所有。

---

## 注意

- 本项目的编译产物**未提供官方支持**，遇到问题请在本仓库提 issue，不要打扰 PCL 原作者
- 请支持正版游戏，通过本启动器使用正版账号登录需拥有有效的 Minecraft: Java Edition 授权
- 下载功能仅提供获取渠道，不构成对游戏本体的任何授权
