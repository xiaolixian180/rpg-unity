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
- 新增 `GridSpriteSheetAnimator`，按 8x8 网格切帧，玩家移动时播放战士运动动画，静止时停留在首帧。
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

## 后续扩展方向

- 将产品文档中的职业、技能、怪物、副本、装备、宠物等表格整理为配置资产。
- 为 TextMeshPro 配置中文字体，修复中文显示为方块的问题。
- 将角色选择界面进一步整理为 Prefab，降低场景重复搭建成本。
- 在场景中接入真实 `GameBootstrap` 对象和基础 UI。
- 先实现本地单人副本循环：移动、选怪、攻击、掉落、升级。
- 再接入 WebSocket，多人在线相关逻辑改为服务端权威。
- 逐步补齐装备、背包、宠物、PvP、交易行等独立模块。
