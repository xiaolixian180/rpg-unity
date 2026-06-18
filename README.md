# Hero Quest 2D RPG 项目说明

## 项目概述

Hero Quest 是一个基于 Unity 的 2D RPG 游戏原型工程。当前阶段的目标不是一次性完成全部玩法，而是先搭建清晰、可扩展、可测试的项目框架，方便后续逐步接入副本探索、战斗、装备养成、宠物、PvP、交易行和多人在线通信等系统。

本项目参考产品业务文档中的核心设定：最大等级 60 级、副本 30 层、5 个职业、8 个装备槽位、6 个品质等级，并优先把角色属性、战斗公式、副本规则和在线通信边界拆成独立模块。

## 技术栈

- 游戏引擎：Unity 2022.3.62t8 / 团结引擎 1.9.0 项目格式
- 开发语言：C#
- 项目类型：2D RPG Unity 工程
- Unity 包：2D Sprite、2D Tilemap、UGUI、TextMeshPro、Unity Test Framework、Visual Scripting、Timeline 等
- 测试框架：Unity Test Framework + NUnit EditMode 测试
- 版本管理：Git
- 配置方式：ScriptableObject 预留配置入口，后续可扩展为 JSON、表格导入或服务端下发
- 网络方向：预留 WebSocket 连接抽象，后续替换为真实长连接实现

## 架构概览

项目采用分层结构，先把容易变化的玩法内容和相对稳定的规则边界分开。

- Domain：纯规则层，包含角色职业、属性结构、战斗属性计算、伤害计算等可测试逻辑。
- Config：配置层，保存最大等级、副本层数、传送消耗、刷新时间、暴击/闪避上限等可调数值。
- Core：运行时核心层，包含启动器、服务注册表和事件总线，用于后续组织场景生命周期和模块通信。
- Systems：玩法系统层，目前包含战斗和副本模块骨架，后续装备、背包、宠物、PvP、交易行都可以继续放在这里。
- Player：玩家数据层，目前定义玩家基础档案、等级、经验、金币、职业、属性和副本解锁进度。
- Net：网络抽象层，目前保留多人在线 WebSocket 接口和占位实现，避免玩法逻辑直接依赖具体网络库。
- Tests：编辑器测试层，用来验证核心公式和规则，保证后续改动不破坏基础逻辑。

当前架构的设计原则是：玩法模块可以逐个补齐，核心公式可以独立测试，网络实现可以后换，配置数据可以从本地 ScriptableObject 逐步迁移到外部表格或服务端。

## 项目结构

```text
Demo-01/
  Assets/
    HeroQuest/
      Runtime/
        Core/
          EventBus.cs
          GameBootstrap.cs
          ServiceRegistry.cs
        Config/
          GameBalanceConfig.cs
        Domain/
          CharacterClass.cs
          CombatCalculator.cs
          CombatStats.cs
          StatBlock.cs
        Net/
          IGameConnection.cs
          WebSocketConnectionPlaceholder.cs
        Player/
          PlayerProfile.cs
        Systems/
          Combat/
            CombatModels.cs
          Dungeon/
            DungeonRules.cs
            MonsterTemplate.cs
          World/
            CameraFollow2D.cs
            ProceduralCharacterRenderer.cs
            ProceduralMapRenderer.cs
            TopDownPlayerController.cs
        UI/
          Core/
            UIView.cs
            UIManager.cs
          Screens/
            CharacterSelectView.cs
        HeroQuest.Runtime.asmdef
      Editor/
        CharacterSelectSceneBuilder.cs
        GameplayPrototypeSceneBuilder.cs
        HeroQuest.Editor.asmdef
      Tests/
        EditMode/
          CombatCalculatorTests.cs
          DungeonRulesTests.cs
          HeroQuest.Tests.EditMode.asmdef
    Resources/
      HeroQuest/
        Characters/
          Warrior_Male.png
          Warrior_Female.png
          Mage_Male.png
          Mage_Female.png
          Archer_Male.png
          Archer_Female.png
          Priest_Male.png
          Priest_Female.png
        Playable/
          Warrior_Male_Player.png
    Scenes/
      SampleScene.scene
      CharacterSelectScene.scene
      GameplayPrototypeScene.scene
  Packages/
    manifest.json
    packages-lock.json
  ProjectSettings/
  README.md
```

## 目前已完成功能

- 初始化 Unity 2D RPG 项目基础结构。
- 建立 `Assets/HeroQuest` 独立业务目录，避免后续代码散落在默认目录中。
- 新增 Runtime 程序集定义 `HeroQuest.Runtime.asmdef`，为后续模块化编译做准备。
- 新增 EditMode 测试程序集定义 `HeroQuest.Tests.EditMode.asmdef`。
- 建立核心服务注册表 `ServiceRegistry`，用于后续统一注册配置、网络、数据仓库等服务。
- 建立事件总线 `EventBus`，为模块间事件通信预留基础能力。
- 建立游戏启动器 `GameBootstrap`，用于场景启动时初始化核心服务。
- 建立全局平衡配置 `GameBalanceConfig`，包含最大等级、最大副本层数、传送金币、怪物刷新时间、闪避/暴击上限等字段。
- 定义 5 个角色职业：战士、法师、射手、牧师、刺客。
- 定义角色基础属性结构 `StatBlock`：力量、敏捷、智力、体质、防御。
- 实现战斗属性计算：最大生命、攻击力、防御力、闪避率、暴击率、暴击伤害。
- 实现基础伤害计算，并保证最终伤害最低为 1。
- 定义玩家档案 `PlayerProfile`，包含玩家 ID、昵称、职业、等级、经验、金币、最大解锁副本层和基础属性。
- 建立副本规则 `DungeonRules`，支持 Boss 层判断、进入层数判断和未配置怪物的默认数值生成。
- 定义怪物模板 `MonsterTemplate`，用于后续接入配置表或服务端数据。
- 建立 WebSocket 网络接口 `IGameConnection`，保留连接、发送、断开和消息接收能力。
- 提供 WebSocket 占位实现 `WebSocketConnectionPlaceholder`，方便本地开发阶段先跑通依赖关系。
- 编写 EditMode 测试，覆盖战斗公式、最低伤害、副本 Boss 层规则、进入副本规则和默认怪物公式。
- 新增 Git 忽略规则，排除 `Library`、`Logs`、`UserSettings` 等 Unity 生成目录。
- 新增 `.gitattributes`，统一常见工程文件换行策略。
- 新增基础 README，并将项目说明集中维护在本文件中。

## 5.23日新增内容

- 导入 8 张角色展示资源，覆盖战士、法师、弓箭手、牧师的男/女版本。
- 新增角色配置模块 `Systems/Character`，包含角色性别、角色定义、角色列表和角色选择结果模型。
- 新增 UI 基础框架 `Runtime/UI`：
  - `UIView`：统一界面显示和隐藏生命周期。
  - `UIManager`：管理 Canvas 下的界面实例。
  - `CharacterSelectView`：角色选择界面逻辑，支持切换职业、切换性别、展示角色图、职业说明和基础属性。
- 新增 Unity Editor 工具 `CharacterSelectSceneBuilder`。
- 在 Unity 顶部菜单增加 `Hero Quest > Build Character Select Scene`，可一键生成角色选择场景。
- 新增 `Assets/Scenes/CharacterSelectScene.scene`，用于展示角色选择 UI 原型。
- Runtime 程序集补充 UGUI 和 TextMeshPro 引用，支持 UI 脚本编译。
- README 已合并项目说明、技术栈、架构概览、项目结构、已完成功能和后续计划。

## 地图与移动原型

- 新增 `Systems/World` 模块，作为地图、角色移动、镜头控制的客户端原型层。
- 新增 `ProceduralMapRenderer`，可生成偏暗色、手绘感方向的 2D 野外地图背景，包含草地、泥地、树、石头、草丛等占位元素。
- 新增 `ProceduralCharacterRenderer`，生成临时 2D 角色占位形象，后续可替换为正式角色 Sprite 或动画帧。
- 从现有 `Warrior_Male.png` 中裁切出 `Warrior_Male_Player.png`，作为地图移动角色的临时展示 Sprite。
- 新增 `TopDownPlayerController`，支持 WASD / 方向键控制角色上下左右移动。
- 新增 `CameraFollow2D`，让摄像机平滑跟随玩家。
- 新增 `GameplayPrototypeSceneBuilder`，可一键生成带地图、玩家和跟随摄像机的玩法测试场景。

生成方式：

```text
Hero Quest > Build Gameplay Prototype Scene
```

生成后打开：

```text
Assets/Scenes/GameplayPrototypeScene.scene
```

点击 Play 后，可以使用 WASD 或方向键移动人物。

当前玩家角色优先使用：

```text
Assets/Resources/HeroQuest/Playable/Warrior_Male_Player.png
```

这张图来自现有战士展示图的临时裁切，适合先做原型验证。后续如果需要更自然的战斗和待机表现，仍然建议替换为透明背景的正式单人 Sprite 或更规范的 Sprite Sheet。

## 5.27日新增内容

- 导入 `战士运动图.png` 为 `Warrior_Male_Walksheet.png`，并将白色背景处理为透明背景。
- 新增 `GridSpriteSheetAnimator`，支持按角色配置的网格规格切帧，玩家移动时播放运动动画，静止时停留在首帧。
- 动画帧会裁掉单格四周空白，玩家视觉高度调整为约 1.5 个地图格子。
- 调整玩法原型场景比例：
  - 地图扩大到 `72 x 48`
  - 地块尺寸略微增大
  - 摄像机正交尺寸增大到 `10.5`
  - 玩家移动速度降低到 `2.2`
- 新增左上角小地图 UI，使用 `MiniMap Camera` 渲染到 `RawImage`，用于缩略显示玩家周边区域。
- 新增 `WildMonsterSpawner` 和 `WildMonster`，进入场景后会立即刷新野怪，并在玩家周围维持目标数量；资源加载失败时会生成红色占位怪物，避免场景里完全看不到野怪。
- 新增 `PrototypeRuntimeInstaller`，玩家控制器启动时会自动补齐野怪刷新器和小地图 UI，避免旧场景没有重新生成时看不到这些对象。
- 新增野怪占位图：
  - `Archer_Male_Monster.png`
  - `Mage_Male_Monster.png`
  - `Priest_Male_Monster.png`
- 野怪占位图来自 `1` 目录下角色图裁切，并已做白底透明处理。
- 新增 `GameplayProtocolCodec` 和 `PrototypeNetworkClient`，保留与 Go 服务端通信的协议边界、消息编码和本地占位连接逻辑。
- 新增 `GameplayProtocolCodecTests`，覆盖客户端 `hello`、`move` 消息编码，以及服务端野怪刷新消息解析。

## 5.30日新增内容

- 导入新的战士男左右运动图：
  - `Warrior_Male_Walk_Right.png`
  - `Warrior_Male_Walk_Left.png`
- `GridSpriteSheetAnimator` 支持按玩家水平移动方向切换左/右运动表，玩家向左移动时播放左向动画，向右移动时播放右向动画。
- 新增登录功能原型：
  - 本地测试账号：`test`
  - 本地测试密码：`test`
  - 在 `GameplayPrototypeScene` 中点击 Play 后，会先显示登录 UI
  - 登录成功后选择战士男/女，确认后进入地图并启用移动、野怪和小地图
- 新增登录服务接口 `IAuthService` 和本地实现 `LocalTestAuthService`，后续可替换为 Go 服务端认证实现。
- 新增登录协议编解码 `AuthProtocolCodec`，预留 `login` / `login_result` JSON 消息格式。
- 新增 `LoginView` 和 `LoginSceneBuilder`，可通过 Unity 菜单生成登录场景。
- 新增 `AuthServiceTests`，覆盖测试账号登录、错误密码拒绝和登录请求编码。

登录场景生成方式：

```text
Hero Quest > Build Login Scene
```

生成后打开：

```text
Assets/Scenes/LoginScene.scene
```

完整原型流程推荐直接运行：

```text
Assets/Scenes/GameplayPrototypeScene.scene
```

运行流程：

```text
Login(test/test) -> Choose Warrior Gender -> Enter Map
```

## 可视化展示方式

在 Unity 中等待脚本编译完成后，点击：

```text
Hero Quest > Build Character Select Scene
```

然后打开：

```text
Assets/Scenes/CharacterSelectScene.scene
```

点击 Play 后可以看到角色选择界面原型。当前界面已能展示角色图片，并通过按钮切换职业和性别。

如果中文显示为方块，通常是 TextMeshPro 默认字体不包含中文字符。后续需要导入中文字体并配置 TMP Font Asset。

## 产品文档框架预设

已根据 `2d-rpg产品业务文档.md` 预先补齐客户端侧基础框架，当前阶段只做模块边界、数据模型、规则函数和服务接口，不直接把完整玩法写死。

新增通用基础类型：

- `GameErrorCode`：对齐产品文档错误码，例如未登录、层数未解锁、技能冷却、金币不足、交易单不存在等。
- `GameTypes`：定义货币、物品类型、装备槽位、品质、技能类型、宠物类型、交易状态、排行榜类型。
- `ServiceResult` / `ServiceResult<T>`：统一客户端服务返回结构，方便以后从本地 stub 切换到 Go 服务端响应。
- `ProductRuleConfig`：集中保存产品文档中的默认参数，例如最大等级、副本层数、红名阈值、无敌时间、PvP 奖励、Boss 掉率、自动存档间隔、限流和最大消息体等。

新增玩法模块骨架：

- `Systems/Inventory`：背包物品堆叠、背包快照、添加/消耗物品接口。
- `Systems/Equipment`：装备模型、8 个装备槽位、穿戴/卸下/强化/附魔接口，以及强化费用和品质战力倍率规则。
- `Systems/Skills`：技能定义、玩家技能、升级/释放/重置接口，预留主动、被动、终极技能。
- `Systems/Pets`：宠物状态、召唤/收回/升级/探险/合成接口，预留同品质合成规则。
- `Systems/PvP`：玩家攻击请求、击杀奖励预览、悬赏和复仇接口，预留红名、金币掠夺和悬赏奖励规则。
- `Systems/Shop`：金币商店/荣誉商店商品模型、购买接口，预留等级、库存校验。
- `Systems/Trading`：交易行订单、分页浏览、创建订单、购买、取消接口，预留不可购买自己商品等规则。
- `Systems/Ranking`：等级、战力、荣誉排行榜模型和 Top 50 查询接口。
- `Systems/Save`：脏数据标记、立即保存、存档状态接口，预留服务端 60 秒自动存档机制。
- `Systems/Dungeon`：新增资源节点采集模型和采集规则，补齐产品文档中的资源采集并发边界。

新增网络协议预留：

- `GameMessageTypes`：集中定义登录、创建角色、进副本、移动、攻击、技能、采集、装备、宠物、PvP、商店、交易、排行榜等消息类型。
- `ClientRequestEnvelope` / `ServerResponseEnvelope`：预留统一请求/响应信封，后续 Go WebSocket 服务端返回错误码和业务 payload 时可直接对接。

`GameBootstrap` 现在会统一注册这些本地 stub 服务。后续接真实后端时，优先替换对应接口实现，例如把 `IShopService` 从 `ShopServiceStub` 换成 `GoShopService`，而不是改 UI 或玩法调用方。

新增测试：

- `ProductFrameworkRulesTests`：覆盖装备品质倍率、技能默认倍率、宠物合成、PvP 掠夺、商店购买校验、交易行购买限制、资源重复采集等基础规则。

## Unity-Go 联调协议层

已根据 `rpg-go-main` 后端代码补充 Unity 客户端侧 Go 协议适配层，位置：

```text
Assets/HeroQuest/Runtime/Net/Go/
```

后端联调地址：

```text
ws://localhost:8080/ws
```

后端通信不是普通 JSON 字符串，而是 WebSocket Binary 帧：

```text
[2字节长度][2字节消息ID][JSON Body]
```

其中长度和消息 ID 都是大端序。Unity 侧新增：

- `GoMessageIds`：对齐后端 `internal/protocol/msg_id.go`，包含登录、创角、进副本、移动、战斗、装备、宠物、交易、商店、技能、属性、排行榜和系统消息 ID。
- `GoBinaryProtocolCodec`：负责 Go 二进制帧编码/解码，以及 JSON Body 的 UTF-8 转换。
- `GoProtocolFrame`：保存解码后的 `messageId + body`。
- `GoProtocolMessages`：预设登录、创角、玩家数据、进副本、怪物刷新、移动、攻击、采集、心跳等核心 DTO。
- `GoWebSocketConnection`：基于 `ClientWebSocket` 的真实连接实现，发送 Binary 消息并接收服务端 Binary 推送。

已新增测试：

- `GoBinaryProtocolCodecTests`：覆盖大端帧头、消息 ID、登录 JSON 编码、进副本响应解码。

注意：当前 Go 后端登录请求是：

```json
{"token":"JWT_TOKEN"}
```

不是账号密码 `test/test`。所以正式联调前需要后端提供测试 JWT，或后端增加测试账号换 token 的接口；Unity 当前登录 UI 仍保留本地测试账号，后续再替换为真实 token 登录流程。

## 5.31日地图与小地图调整

- 原型地图默认尺寸从 `72 x 48` 扩大到 `128 x 88`，并增加场景装饰数量，避免宽屏视野下左右露出黑边。
- `PrototypeRuntimeInstaller` 会在运行时检查地图尺寸，旧场景没有重新生成时也会自动扩展地图。
- `CameraFollow2D` 新增地图边界约束，主摄像机跟随玩家时不会拉到地图外。
- 重新生成 `GameplayPrototypeScene` 时，场景生成器会直接创建大地图并绑定摄像机边界。
- 小地图改为固定左上角 HUD，不随主摄像机缩放丢失。
- 小地图 UI 改为圆形裁切，增加深色外圈、旧纸色边框、内圈黑线和玩家红点，整体更接近手绘生存游戏风格。
- 新增 `CircleMaskGraphic`，用于 UGUI 圆形遮罩，不依赖外部图片资源。

## 6.7日角色选择流程完善

- 登录成功后的角色选择不再写死为战士，现支持战士、法师、弓箭手、牧师四种职业。
- 每种职业均支持男性和女性角色选择。
- 角色选择界面会展示对应立绘、职业定位和基础属性，并高亮当前选择的职业与性别。
- 确认进入地图后，玩家会自动使用所选职业和性别对应的移动动画。
- `CharacterDefinition` 新增左右移动动画资源路径，角色定义统一管理立绘与可玩动画资源。
- 从根目录 `角色素材` 整理并导入 16 张职业男女移动图至：

```text
Assets/Resources/HeroQuest/Playable/
```

- 新增 `CharacterRosterTests`，检查所有可选职业的男女版本都配置了立绘和移动资源路径。
- 根目录 `角色素材/` 已加入 `.gitignore`，避免执行 `git add .` 时重复上传源素材；实际游戏使用的资源位于 `Assets/Resources`，会正常提交。

## 6.7日战斗 HUD 框架

- 新增 `GameplayHudController`，进入地图后自动生成正式玩法 HUD 框架。
- HUD 结构参考传统俯视角 RPG/MMO 操作界面，包含：
  - 顶部状态条：游戏名、计时、背包、任务、队伍、设置入口。
  - 左上角色头像：角色立绘、名称、HP/MP 条。
  - 左侧战斗日志：系统、任务、技能提示等滚动占位。
  - 底部主操作台：小地图底座、目标/属性面板、快捷栏、技能区、命令区。
  - 右下基础命令：移动、攻击、技能、宠物、背包、锻造、商店、交易、排行。
- HUD 按钮已具备点击反馈和事件预留，当前先写入日志占位，后续可逐步接入背包、技能、任务、宠物、商店、交易行等系统。
- 小地图从左上角调整到底部 HUD 区域，与主操作台布局对齐。
- 角色进入地图后，HUD 会展示当前选择的职业与性别。

## 6.18日角色动画修复

- 修复选择法师、弓箭手、牧师或女性角色后，进入地图仍显示战士男的问题。
- `CharacterDefinition` 新增 `WalkColumns` 和 `WalkRows`，图集规格成为角色配置的一部分，不再由动画组件统一猜测。
- 当前动作图规格：
  - 战士男：`8 x 8`
  - 战士女、法师、弓箭手、牧师的男/女版本：`6 x 5`
- `PrototypeGameplayFlow` 进入地图时会把所选角色的立绘、左右动作路径和图集规格传给 `ProceduralCharacterRenderer`。
- `ProceduralCharacterRenderer` 会按当前角色重建玩家视觉对象，不再保留默认战士男资源。
- `GridSpriteSheetAnimator` 会按透明像素裁剪每一帧，并用同行有效帧填补图集中的空白格，避免角色闪烁或消失。
- 角色世界高度统一为约 `1.5` 个地图格子，避免不同分辨率素材导致角色大小不一致。
- 重新处理 16 张左右动作图：移除与图片边缘连通的浅色纸纹背景，并清理牧师素材中的横向黑色分隔线。
- 动作图导入设置保持 `Sprite + Point + Mipmap Off + Alpha + Read/Write Enabled`，供运行时透明边界分析使用。
- `CharacterRosterTests` 增加图集行列和角色动作资源唯一性检查。
- 新增根目录 `AGENTS.md`，记录场景入口、角色素材契约、网络协议和验证要求；`agentn.md` 作为兼容入口。

## 后续扩展方向

- 将产品文档中的职业、技能、怪物、副本、装备、宠物等表格整理为配置资产。
- 为 TextMeshPro 配置中文字体，修复中文显示为方块的问题。
- 将角色选择界面进一步整理为 Prefab，降低场景重复搭建成本。
- 在场景中接入真实 `GameBootstrap` 对象和基础 UI。
- 先实现本地单人副本循环：移动、选怪、攻击、掉落、升级。
- 再接入 WebSocket，多人在线相关逻辑改为服务端权威。
- 逐步补齐装备、背包、宠物、PvP、交易行等独立模块。

## 6.18日地图与野怪素材替换

- 地图不再使用程序生成的方格草地、椭圆树冠和占位石块。
- 新增素材背景 `HeroQuest/World/Grassland_Background`，运行时按地图边界等比放大并覆盖整个可移动区域。
- 从素材库切分并透明化以下地图资源：
  - 3 种树木
  - 6 种草丛
  - 2 种石头
- 地图会按固定随机种子分布真实装饰素材，并保留玩家出生点周围的安全空地。
- 小地图取消灰绿色叠色，直接显示地图和装饰物原色。
- 野怪替换为 3 种素材库角色：荒原角兽、沼泽掠夺者、林地蜥蜴。
- 野怪统一按世界高度缩放，并增加中文名称、阴影和生命条 UI。
- 处理后的运行资源位于：

```text
Assets/Resources/HeroQuest/World/
Assets/Resources/HeroQuest/Enemies/WildMonster_*.png
```

- 根目录 `q/` 仅保存原始大图，已加入 `.gitignore`；提交时只上传 `Assets/Resources` 中的处理结果。
