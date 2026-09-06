# MaxGodotFrame

用于 Godot .NET 项目的通用 C# 基础插件，后续范围仅为 **RNG 和声音管理**。

当前版本 `0.1.0` 只有可启用、禁用的编辑器插件入口。RNG、声音管理和运行时 API 尚未实现；启用后不会出现额外面板，也不会修改自动加载项、项目设置或音频总线。

## 环境与安装

宿主必须使用 Godot **.NET 版**，并安装对应的 .NET SDK。即使游戏使用 GDScript，也需要 .NET 版引擎及宿主 C# 工程来编译本插件。骨架以 Windows、Godot 4.7.2 .NET、Godot.NET.Sdk 4.7.2 和 `net8.0` 为验证基线；其他版本和系统尚未验证。

本仓库根目录就是插件目录，不包含独立的 `project.godot` 或 `.csproj`。在已有 Godot .NET 项目的根目录执行：

```shell
git submodule add https://github.com/AnonNetizen/MaxGodotFrame.git addons/max_godot_frame
```

也可以把仓库内容复制到 `addons/max_godot_frame/`，确保入口位于 `addons/max_godot_frame/plugin.cfg`，不要再套一层同名目录。

1. 宿主尚无 C# 工程时，先在 .NET 版编辑器中创建一个 C# 脚本，让编辑器生成工程。
2. 构建宿主 C# 工程，使插件脚本参与编译；标准 Godot 工程默认包含项目目录内的 C# 源文件，自定义工程需保留插件目录。
3. 在「项目 → 项目设置 → 插件」启用 MaxGodotFrame。禁用时取消启用即可。

已有宿主仓库的其他开发者可执行 `git submodule update --init --recursive` 获取固定版本。子模块更新由宿主提交新的 Git 引用，不自动追踪最新版。

## 后续边界

- RNG 提供与玩法无关的随机能力；算法和完整 API 在实现任务中确定。算法内核可保持纯 C#，游戏负责调用时机、随机流的用途及存档接入。
- 声音管理提供通用播放能力，游戏负责音效选择和触发时机。插件不识别卡牌、战斗或奖励事件。
- 未来通过 Godot 对象、兼容参数与信号提供 GDScript 调用入口。纯 C# 内核由包装暴露，两种脚本语言不直接互相继承。
- 不增加文件存储、设置、日志等模块，不依赖 MaxGodotUI、具体游戏或 Steam。

## 开发与验证

源码入口为 [MaxGodotFramePlugin.cs](editor/MaxGodotFramePlugin.cs)，使用独立命名空间及 `#if TOOLS`。使用独立测试宿主构建，再检查插件启用、禁用、重新启用及关闭后重新打开；不要把宿主工程、缓存或构建产物提交到插件仓库。

当前验证只覆盖骨架加载；GDScript 运行时调用及 RNG、声音功能的验收留待实现后进行。公共接口使用跨系统路径，资源引用大小写与文件保持一致，通用逻辑不依赖 Windows 专用接口。

随源码保留 Godot 生成的 `.cs.uid` 文件，使脚本资源身份在不同宿主中保持一致。

插件工作先在本仓库完成检查、提交及推送，再由使用它的宿主更新子模块引用。保持本仓库可独立使用，文档和实现不引用宿主的私有文件。

安装机制参考 [Godot 插件文档](https://docs.godotengine.org/en/stable/tutorials/plugins/editor/making_plugins.html)，语言边界参考 [Godot 跨语言调用文档](https://docs.godotengine.org/en/stable/tutorials/scripting/cross_language_scripting.html)。

## 许可

采用 [MIT License](LICENSE)，版权署名为 `2026 AnonNetizen`。
