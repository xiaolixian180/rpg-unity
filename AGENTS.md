# Hero Quest 项目协作约定

本文是后续开发者和 AI 代理进入本仓库时应优先阅读的项目说明。实现细节以代码和本文件为准，历史过程记录见 `README.md`。

## 项目定位

- 引擎：Unity 2022.3.62t8 / 团结引擎 1.9.0 项目格式。
- 客户端语言：C#。
- 后端：`rpg-go-main` 中的 Go WebSocket 服务。
- 当前重点：Unity 客户端原型、角色流程、地图、HUD、动画和 Go 协议适配。
- 主测试场景：`Assets/Scenes/GameplayPrototypeScene.scene`。
- 本地登录账号和密码均为 `test`。

## 运行流程

```text
登录 -> 选择职业与性别 -> 进入地图 -> 启用移动、HUD、小地图和野怪
```

原型入口由 `TopDownPlayerController` 创建 `PrototypeGameplayFlow`。选角数据来自 `CharacterRoster`，进入地图后依次传递给 `ProceduralCharacterRenderer` 和 `GridSpriteSheetAnimator`。

## 角色动画契约

- 立绘目录：`Assets/Resources/HeroQuest/Characters/`。
- 可玩动作图目录：`Assets/Resources/HeroQuest/Playable/`。
- 每个可玩角色必须提供 `_Walk_Right.png` 和 `_Walk_Left.png`。
- 战士男动作图为 `8 x 8`；当前其余职业和性别为 `6 x 5`。
- 行列数必须写入 `CharacterRoster`，禁止在动画组件中根据文件名硬编码。
- 玩家显示高度统一为 `1.5` 世界单位，不能直接使用素材原始像素大小。
- 动作图必须透明；不得保留白底、纸纹背景、黑色分隔线或相邻帧内容。
- 导入设置：Sprite、Point、关闭 Mipmap、启用 Alpha、启用 Read/Write。
- 新增角色时同步更新 `CharacterRosterTests`，并确认左右资源路径唯一。

## UI 约定

- 游戏内可见文本使用中文。
- 运行时生成的 UGUI 文本使用 `ChineseFontProvider`，避免中文显示为方块。
- 登录、选角和 HUD 当前由运行时代码生成；不要只修改未启用的场景预制界面。
- HUD 功能按钮允许保留占位事件，但玩家可见提示必须明确为“功能已预留”。

## 地图与野怪素材契约

- 地图运行资源位于 `Assets/Resources/HeroQuest/World/`。
- 地图背景资源路径为 `HeroQuest/World/Grassland_Background`，由 `ProceduralMapRenderer` 等比覆盖地图边界。
- 树木、石头和草丛位于 `HeroQuest/World/Props/`，禁止恢复旧的程序椭圆、矩形占位素材。
- 地图装饰资源必须为透明 PNG，采用脚底锚点，出生点半径内不得随机放置大型装饰。
- 野怪资源路径为 `HeroQuest/Enemies/WildMonster_01` 至 `WildMonster_03`。
- 野怪必须使用统一世界高度，并保留中文名称、阴影和生命条 UI。
- 根目录 `q/` 是原始素材来源，不参与 Git 提交；实际游戏只能引用 `Assets/Resources` 下的处理结果。
- 新增地图素材时应去除白底、水印和分隔线，并检查 Alpha 边界没有可见残留。

## 网络约定

- Go 联调默认地址：`ws://localhost:8080/ws`。
- 帧格式：`[2字节长度][2字节消息ID][JSON Body]`。
- 长度和消息 ID 使用大端序。
- 协议代码位于 `Assets/HeroQuest/Runtime/Net/Go/`。
- 正式 Go 登录使用 JWT；当前 `test/test` 仍是 Unity 本地认证流程。
- UI 和玩法模块只能依赖服务接口或协议适配层，不直接拼接 Go 二进制帧。

## 修改与验证

- 不删除或回退与当前任务无关的本地改动。
- 不提交 `Library/`、`Temp/`、`Logs/`、`UserSettings/` 和根目录原始素材文件夹。
- C# 修改后至少完成一次编译检查，并检查 Unity Console 无新增错误。
- 规则和协议修改应运行 EditMode 测试。
- 角色修改应手动验证至少：战士男、一个女性角色、一个非战士职业。
- 动画验收项：角色正确、左右方向正确、无背景块、无相邻帧、移动不闪烁、世界高度约 1.5 格。

## 已知说明

- Unity 生成的 csproj 目标框架可能要求本机安装 `.NET Framework 4.7.1 Developer Pack`；缺失时可在 Unity Test Runner 中运行测试。
- `LoginView` 和 `CharacterSelectView` 的序列化字段未绑定警告来自预留界面脚本，不是当前运行时原型的编译错误。
- 修改 PNG 或 `.meta` 后若 Unity 未刷新，应对 `Assets/Resources/HeroQuest/Playable` 执行 Reimport。
