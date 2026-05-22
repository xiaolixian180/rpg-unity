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
        HeroQuest.Runtime.asmdef
      Tests/
        EditMode/
          CombatCalculatorTests.cs
          DungeonRulesTests.cs
          HeroQuest.Tests.EditMode.asmdef
    Resources/
    Scenes/
      SampleScene.scene
  Docs/
    architecture.md
    roadmap.md
    项目说明.md
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
- 新增基础 README、架构文档和路线图文档。

## 后续扩展方向

- 将产品文档中的职业、技能、怪物、副本、装备、宠物等表格整理为配置资产。
- 在场景中接入真实 `GameBootstrap` 对象和基础 UI。
- 先实现本地单人副本循环：移动、选怪、攻击、掉落、升级。
- 再接入 WebSocket，多人在线相关逻辑改为服务端权威。
- 逐步补齐装备、背包、宠物、PvP、交易行等独立模块。
