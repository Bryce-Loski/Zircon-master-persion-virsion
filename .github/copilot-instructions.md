**Repo Context**
- **Purpose:** 管理和运行 Legend of Mir 3 — 包含 Server、Client、Launcher、Patcher、插件与编辑器工具。
- **Entry point (solution):** `Zircon Server.sln` (多个 WinForms 和库项目)

**Big Picture / Architecture**
- **Main apps:** `Server/` (服务器控制台/管理 WinForms), `Client/` (客户端 WinForms), `Launcher/`, `Patcher/`, `ImageManager/`.
- **Core libraries:** `LibraryCore/`, `Library/`, `ServerLibrary/` — 这些包含共享逻辑、数据模型和网络/DB 辅助。
- **Plugin system:** `PluginCore/` 提供 `AbstractPlugin`, `IPluginStart`, `IPluginType` 等接口；插件通过 `Plugin` 和 `PluginStandalone` 项目加载/集成。
- **Editor/tools:** `LibraryEditor/`, `PatchManager/` 等为内容编辑与打包工具。

**Data flows & integration points**
- **Database:** 服务器项目使用 Dapper + `System.Data.SqlClient` (见 `Server/Server.csproj`) — 查找 `MirDB` 命名空间以定位 DB session/模型。
- **Plugins:** 插件通过 `PluginCore` 的 `IPluginStart` 注入运行时（查看 `PluginCore/IPluginStart.cs`）。集成插件会接收 `MirDB.Session`。
- **Rendering / Assets:** `ImageManager/` 和 `Library*/Rendering` 相关文件管理资源；`Server` 引用 `SharpDX`、`DevExpress`（Windows-only UI/渲染依赖）。

**Build & developer workflows**
- **Recommended (Windows):** 打开 `Zircon Server.sln` 用 Visual Studio 2022/2023，选择 `Debug|Any CPU` 或 `Release|x64`，然后 Build。
- **CLI:** 可以用 `msbuild "Zircon Server.sln" /p:Configuration=Debug` 或 `dotnet build "Zircon Server.sln" -c Debug`（注意：部分 WinForms / DevExpress / SharpDX 项目在非 Windows 平台上可能无法正确编译或运行）。
- **Project-level build:** 若只改动库，可单独构建库项目，例如 `dotnet build LibraryCore/LibraryCore.csproj`。
- **Output paths:** 多个项目在 csproj 中设置自定义 `OutputPath`（例如 `Server/` 指向 `..\\..\\Debug\\Server\\`），搜索 `OutputPath` 来快速发现构建产物位置。

**Project-specific conventions & patterns**
- **WinForms + Designer:** UI 类分为 `*.cs` + `*.Designer.cs`，UI 表单通常放在 `Views/` 目录（例：`Server/Views`）。
- **Plugin pattern:** 实现 `AbstractPlugin` 并通过 `IPluginStart` 访问主程序服务（参见 `PluginCore/AbstractPlugin.cs` 和 `PluginCore/IPluginStart.cs`）。
- **DB models:** `MirDB` 命名空间用于数据库模型/会话，集成插件会收到 `MirDB.Session`。
- **Third-party UI libs:** `DevExpress` 被广泛使用（`Server/Server.csproj`），开发/测试环境需要有效的 DevExpress 运行时和安装。

**Where to look for common tasks (examples)**
- **启动/主流程:** `Server/Program.cs`, `Client/Program.cs`, `Launcher/LMain.cs`。
- **插件入口:** `PluginCore/AbstractPlugin.cs`, `PluginCore/IPluginStart.cs`。
- **数据库调用样例:** 搜索 `Dapper` 或 `MirDB`，例如在 `Server` 子目录中。
- **资源/图片处理:** `ImageManager/IMain.cs`, `LibraryEditor/LockBitmap.cs`。

**Quick debugging notes**
- **Windows-first:** 全功能运行需 Windows（WinForms、SharpDX、DevExpress）。在 macOS 上只推荐静态分析或编译非 UI 库。
- **Run order:** 一般先启动 `Server` 服务/管理端，再启动 `Client` 或 `Launcher`（视改动而定）。

**Tips for AI agents working on this repo**
- Always check `*.Designer.cs` when changing forms; UI state/code-split is common.
- When editing plugins, update `Plugin`/`PluginStandalone` projects and search for `Namespace`/`Name` properties used by `IPluginStart`.
- For DB changes, prefer locating `MirDB` model definitions and follow existing Dapper query conventions.
- Note platform constraints: mention Windows-only dependencies in PR descriptions when applicable.

**If you need more context**
- Ask for which area to deep-dive (Server behavior, plugin lifecycle, DB schemas, or UI flows). I can extract call graphs and example stack traces for the area you pick.

---
_Generated/merged by AI: summarize repo structure, build hints, and actionable file pointers._
