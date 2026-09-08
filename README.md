# MaxGodotFrame

Godot .NET 通用 C# 基础插件，范围仅为 RNG 与声音。当前 `0.3.0` 提供可恢复随机源、音乐与音效播放。运行时无需启用编辑器插件。

## 安装与编译

将本仓库根目录放在宿主 `addons/max_godot_frame/`。使用 Godot 4.7.2 .NET 与 .NET 8；宿主 `.csproj` 保留 Godot 默认源码包含规则和 `<EnableDynamicLoading>true</EnableDynamicLoading>`。不要把宿主工程或生成目录提交到插件仓库。

默认 Godot 主工程会一起编译 `core/` 和 `runtime/` 源码。纯 C# 使用者可以独立引用 [core/MaxGodotFrame.Random.csproj](core/MaxGodotFrame.Random.csproj)。宿主如果改用项目引用，应从宿主 Compile 项中排除 `addons/max_godot_frame/core/**/*.cs`，防止重复编译；运行时包装仍由 Godot 主工程编译。

## RNG 接口

[RandomStream](core/RandomStream.cs) 是不依赖 Godot 的 C# 内核：

- 构造与 `Reseed(ulong seed)`：以 SplitMix64 从种子初始化独立实例。
- `NextUInt64()`、`NextInt(int exclusiveMaximum)`：后者在 `[0, exclusiveMaximum)` 均匀取整数，上限必须为正；使用拒绝采样消除取模偏差。
- `ExportState()`：返回独立的 32 字节副本，按小端保存四个 64 位状态字。
- `ImportState(string algorithm, ReadOnlySpan<byte> state)`：验证算法版本、长度和非全零状态，错误时抛出参数异常且不改变旧状态。
- 算法标识为 `xoshiro256starstar-splitmix64-v1`。完整恢复同时保存算法标识和当前状态，不能仅保存初始种子。

[FrameRandom](runtime/FrameRandom.cs) 为 C# / GDScript 提供 Variant 兼容包装。GDScript 使用示例：

```gdscript
var rng = load("res://addons/max_godot_frame/runtime/FrameRandom.cs").new()
rng.Reseed(42)
var saved: PackedByteArray = rng.ExportState()
var value: int = rng.NextInt(10)
assert(rng.RestoreState(rng.GetAlgorithm(), saved))
assert(rng.NextInt(10) == value)
```

包装种子使用有符号 `long` 的完整位模式；状态用 `PackedByteArray`，避免 unsigned Variant 转换。`RestoreState` 对无效输入报告 Godot 错误并返回 `false`。同一实例不支持并发调用；不同实例互不干扰。本模块不持有全局随机状态，不实现存档文件、业务洗牌、随机流用途或游戏事件。

## 声音接口

[FrameAudio](runtime/FrameAudio.cs) 继承 `Node`，加入场景树后使用：

- `PlayMusic(AudioStream stream)`：返回音乐播放器；相同正在播放的资源保持进度。循环由传入资源的循环选项决定。
- `PlayEffect(AudioStream stream, bool pauseWithTree)`：返回独立音效播放器；`true` 随场景树暂停，`false` 可供暂停界面试听。C# 可省略第二参数（默认 `true`），GDScript 必须显式传入。
- `MusicBus`、`EffectsBus` 默认为 `Master`；宿主可指定已存在的总线。节点不创建总线或修改项目配置。
- `MaxVoices` 默认 8，实际限制在 1—32；达到上限时停止最早音效。`GetVoiceCount()` 返回当前音效数量。
- `PauseMusic(bool paused)` 暂停／恢复音乐；`StopMusic()`、`StopEffects()`、`StopAll()` 停止对应播放并释放资源引用。音乐默认不随场景树暂停。

```gdscript
var audio = load("res://addons/max_godot_frame/runtime/FrameAudio.cs").new()
add_child(audio)
audio.PlayMusic(preload("res://music.wav"))
audio.PlayEffect(preload("res://click.wav"), false)
```

宿主提供音频资源、音量、后台静音、播放时机和偏好保存；声音不读取 RNG。提前停止音频并留出一个混音周期再退出引擎，可让音频线程释放循环流。

## 验证与边界

在独立宿主中验证 C# 编译、GDScript 创建／状态恢复、编辑器启停和 ExportRelease 构建；另以纯 C# 验证参考序列、实例隔离、边界范围及无效状态。禁用编辑器入口不影响运行时随机源。

声音另在独立及共同宿主验证 GDScript 调用、音乐／音效路由、暂停、试听、并发上限和停止，确认不新增音频总线。不增加存储、设置或日志模块，不依赖其他插件、具体游戏或平台。

## 许可

本项目采用 [MIT License](LICENSE)。算法来源、原作者和公共领域声明见 [THIRD_PARTY_NOTICES.txt](THIRD_PARTY_NOTICES.txt)。保留 Godot 生成的脚本 `.cs.uid`。
