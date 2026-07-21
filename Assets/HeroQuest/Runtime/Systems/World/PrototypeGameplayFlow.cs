using System.Collections.Generic;
using System.Threading;
using HeroQuest.Core;
using HeroQuest.Domain;
using HeroQuest.Net.Auth;
using HeroQuest.Net.Go;
using HeroQuest.Systems.Character;
using HeroQuest.UI.Core;
using HeroQuest.UI.HUD;
using HeroQuest.UI.Screens;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeroQuest.Systems.World
{
    public sealed class PrototypeGameplayFlow : MonoBehaviour
    {
        private TopDownPlayerController playerController;
        private GridSpriteSheetAnimator animator;
        private Canvas flowCanvas;
        private GameObject loginPanel;
        private GameObject characterPanel;
        private CharacterClass selectedClass = CharacterClass.Warrior;
        private CharacterGender selectedGender = CharacterGender.Male;
        private Button activeLoginButton;
        private GameplayHudController hud;
        private NetworkManager network;

        private string authToken;
        private ulong playerId;
        private int currentLayer;
        private bool autoBattleEnabled;
        private bool gameplayActive;
        private bool playerDead;
        private readonly Dictionary<ulong, GoMonsterData> visibleMonsters = new();
        private readonly Dictionary<ulong, GameObject> dungeonMonsterObjects = new();
        private ulong selectedTargetId;
        private GoPlayerData localPlayerData;
        private CharacterPanelView characterPanelView;
        private TeamPanelView teamPanelView;
        private ChatPanelView chatPanelView;
        private PetPanelView petPanelView;
        private ForgePanelView forgePanelView;
        private RaidLobbyPanelView raidLobbyPanelView;
        private RaidHudOverlay raidHudOverlay;
        private RaidInventoryPanelView raidInventoryPanelView;
        private bool isInRaid; // 是否在战局中
        private ulong pendingWearEquipId; // 穿戴请求时暂存 equipId，响应后用于更新本地状态
        private GoLootContainerData[] _raidContainers; // 当前战局的容器列表
        private GoExtractionPointData[] _raidExtractionPoints; // 当前战局的撤离点
        private GoRaidZoneData[] _raidZones; // 当前战局的区域数据
        private ulong _raidNearestContainerId; // 最近的可交互容器ID

        // 动作条绑定（12个槽位：0=技能/普攻，1=消耗品）
        private enum BarSlotType { Empty, Skill, Item }
        private struct BarSlot { public BarSlotType type; public int id; }
        private readonly BarSlot[] actionBarSlots = new BarSlot[12];
        private int targetCycleIndex;

        public static PrototypeGameplayFlow Ensure(TopDownPlayerController controller)
        {
            var existing = FindFirstObjectByType<PrototypeGameplayFlow>();
            if (existing != null)
            {
                existing.Bind(controller);
                return existing;
            }

            var flowObject = new GameObject("Prototype Gameplay Flow");
            var flow = flowObject.AddComponent<PrototypeGameplayFlow>();
            flow.Bind(controller);
            return flow;
        }

        private void Bind(TopDownPlayerController controller)
        {
            playerController = controller;
            animator = controller.GetComponentInChildren<GridSpriteSheetAnimator>(true);
        }

        private void Start()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<TopDownPlayerController>();
                animator = playerController != null ? playerController.GetComponentInChildren<GridSpriteSheetAnimator>(true) : null;
            }

            GameBootstrap.Ensure();

            if (ServiceRegistry.TryResolve<NetworkManager>(out var nm))
            {
                network = nm;
            }

            SetGameplayEnabled(false);
            BuildCanvas();

            // 初始化音频系统
            HeroQuest.Systems.Audio.AudioAssets.LoadFromGenerators();
            var audio = HeroQuest.Systems.Audio.AudioManager.Ensure();
            if (HeroQuest.Systems.Audio.AudioAssets.BgmLogin != null)
                audio.PlayBGM(HeroQuest.Systems.Audio.AudioAssets.BgmLogin);

            ShowLogin();
        }

        private void Update()
        {
            network?.PumpMainThread();

            if (activeLoginButton != null && Input.GetKeyDown(KeyCode.Return))
            {
                activeLoginButton.onClick.Invoke();
                return;
            }

            // 快捷键（仅在已连接服务器且进入玩法阶段时生效）
            if (network == null || !network.IsConnected || !gameplayActive)
            {
                return;
            }

            // 动作条 1-9, 0, -, = （12个槽位）
            if (Input.GetKeyDown(KeyCode.Alpha1)) { UseActionBarSlot(0); return; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { UseActionBarSlot(1); return; }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { UseActionBarSlot(2); return; }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { UseActionBarSlot(3); return; }
            if (Input.GetKeyDown(KeyCode.Alpha5)) { UseActionBarSlot(4); return; }
            if (Input.GetKeyDown(KeyCode.Alpha6)) { UseActionBarSlot(5); return; }
            if (Input.GetKeyDown(KeyCode.Alpha7)) { UseActionBarSlot(6); return; }
            if (Input.GetKeyDown(KeyCode.Alpha8)) { UseActionBarSlot(7); return; }
            if (Input.GetKeyDown(KeyCode.Alpha9)) { UseActionBarSlot(8); return; }
            if (Input.GetKeyDown(KeyCode.Alpha0)) { UseActionBarSlot(9); return; }
            if (Input.GetKeyDown(KeyCode.Minus))  { UseActionBarSlot(10); return; }
            if (Input.GetKeyDown(KeyCode.Equals)) { UseActionBarSlot(11); return; }

            // Tab = 切换最近目标
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SelectNearestTarget();
                return;
            }

            // C = 角色面板
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCharacterPanel();
                return;
            }

            // P = 技能书（预留）
            if (Input.GetKeyDown(KeyCode.P))
            {
                hud?.AddLog("[系统] 技能书面板开发中。");
                return;
            }

            // I = 背包（预留）
            if (Input.GetKeyDown(KeyCode.I))
            {
                hud?.AddLog("[系统] 背包面板开发中。");
                return;
            }

            // M = 地图（预留）
            if (Input.GetKeyDown(KeyCode.M))
            {
                hud?.AddLog("[系统] 地图面板开发中。");
                return;
            }

            // O = 社交（预留）
            if (Input.GetKeyDown(KeyCode.O))
            {
                hud?.AddLog("[系统] 社交面板开发中。");
                return;
            }

            // T = 队伍面板
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleTeamPanelView();
                return;
            }

            // Y = 宠物面板
            if (Input.GetKeyDown(KeyCode.Y))
            {
                TogglePetPanelView();
                return;
            }

            // F = 锻造面板
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleForgePanelView();
                return;
            }

            // N = 战局大厅
            if (Input.GetKeyDown(KeyCode.N))
            {
                ToggleRaidLobbyPanelView();
                return;
            }

            // R = 战局背包（战局内）
            if (Input.GetKeyDown(KeyCode.R) && isInRaid)
            {
                ToggleRaidInventoryPanelView();
                return;
            }

            // V = 撤离 / 离开（战局内）
            if (Input.GetKeyDown(KeyCode.V) && isInRaid)
            {
                TryRaidExtract();
                return;
            }

            // L = 离开战局
            if (Input.GetKeyDown(KeyCode.L) && isInRaid)
            {
                _ = network.SendRaidLeaveAsync(CancellationToken.None);
                hud?.AddLog("[战局] 正在离开战局...");
                return;
            }

            // F = 搜索容器（战局内）
            if (Input.GetKeyDown(KeyCode.F) && isInRaid)
            {
                TryOpenNearestContainer();
                return;
            }

            // G = 拾取容器物品（战局内）
            if (Input.GetKeyDown(KeyCode.G) && isInRaid)
            {
                TryPickupContainerLoot();
                return;
            }

            // Enter = 聊天面板（仅在非聊天输入框聚焦时触发）
            if (Input.GetKeyDown(KeyCode.Return) && !IsChatInputFocused())
            {
                ToggleChatPanelView();
                return;
            }

            // ` = 自动战斗开关
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                ToggleAutoBattle();
                return;
            }

            // Escape = 关闭面板 / 取消选中
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscape();
                return;
            }

            // 鼠标左键：选中目标 + 自动攻击 / 点击地面移动
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                HandleMouseClick();
                return;
            }

            // 鼠标右键：自动攻击 / 交互
            if (Input.GetMouseButtonDown(1) && !IsPointerOverUI())
            {
                HandleRightClick();
                return;
            }
        }

        private static bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null
                && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }

        private void BuildCanvas()
        {
            EnsureEventSystem();

            if (flowCanvas != null)
            {
                return;
            }

            var canvasObject = new GameObject("Prototype Flow Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            flowCanvas = canvasObject.GetComponent<Canvas>();
            flowCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            flowCanvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void ShowLogin()
        {
            ClearPanels();

            loginPanel = CreatePanel("Login Panel", new Color(0.035f, 0.043f, 0.038f, 0.96f));
            CreateText(loginPanel.transform, "勇者远征", 54, new Vector2(0.5f, 0.66f), new Vector2(520f, 72f));

            var account = CreateInput(loginPanel.transform, "账号", LocalTestAuthService.TestAccount, false, new Vector2(0.5f, 0.53f));
            var password = CreateInput(loginPanel.transform, "密码", LocalTestAuthService.TestPassword, true, new Vector2(0.5f, 0.44f));
            var message = CreateText(loginPanel.transform, "测试账号：test / test", 22, new Vector2(0.5f, 0.30f), new Vector2(520f, 44f));
            var loginButton = CreateButton(loginPanel.transform, "登录", new Vector2(0.5f, 0.36f), new Vector2(220f, 58f));
            activeLoginButton = loginButton;
            loginButton.onClick.AddListener(async () =>
            {
                message.text = "连接服务器中...";
                loginButton.interactable = false;

                var authService = new LocalTestAuthService(network);
                var result = await authService.LoginAsync(account.text.Trim(), password.text, CancellationToken.None);
                loginButton.interactable = true;
                message.text = result.Message;

                if (result.Success)
                {
                    authToken = JwtHelper.GenerateToken(ulong.Parse(result.PlayerId));
                    playerId = ulong.Parse(result.PlayerId);
                    ShowCharacterSelect();
                }
            });

            EventSystem.current.SetSelectedGameObject(account.gameObject);
            account.ActivateInputField();
        }

        private void ShowCharacterSelect()
        {
            ClearPanels();

            characterPanel = CreatePanel("Character Select Panel", new Color(0.025f, 0.030f, 0.026f, 0.97f));
            CreateText(characterPanel.transform, "选择角色", 44, new Vector2(0.5f, 0.91f), new Vector2(720f, 66f));
            CreateText(characterPanel.transform, "选择职业与性别后进入地图", 20, new Vector2(0.5f, 0.855f), new Vector2(760f, 38f));

            var portraitFrame = new GameObject("Portrait Frame", typeof(RectTransform), typeof(Image));
            portraitFrame.transform.SetParent(characterPanel.transform, false);
            var frameRect = portraitFrame.GetComponent<RectTransform>();
            SetAnchor(frameRect, new Vector2(0.50f, 0.54f), new Vector2(620f, 580f));
            portraitFrame.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.08f, 0.96f);

            var portrait = new GameObject("Character Portrait", typeof(RectTransform), typeof(Image));
            portrait.transform.SetParent(portraitFrame.transform, false);
            var portraitRect = portrait.GetComponent<RectTransform>();
            portraitRect.anchorMin = Vector2.zero;
            portraitRect.anchorMax = Vector2.one;
            portraitRect.offsetMin = new Vector2(28f, 28f);
            portraitRect.offsetMax = new Vector2(-28f, -28f);
            var portraitImage = portrait.GetComponent<Image>();
            portraitImage.preserveAspect = true;

            var className = CreateText(characterPanel.transform, string.Empty, 34, new Vector2(0.82f, 0.70f), new Vector2(380f, 52f));
            var genderLabel = CreateText(characterPanel.transform, string.Empty, 23, new Vector2(0.82f, 0.64f), new Vector2(380f, 40f));
            var description = CreateText(characterPanel.transform, string.Empty, 21, new Vector2(0.82f, 0.53f), new Vector2(390f, 100f));
            var stats = CreateText(characterPanel.transform, string.Empty, 21, new Vector2(0.82f, 0.38f), new Vector2(390f, 150f));

            var classButtons = new Button[CharacterRoster.AvailableClasses.Count];
            for (var i = 0; i < CharacterRoster.AvailableClasses.Count; i++)
            {
                var classIndex = i;
                var characterClass = CharacterRoster.AvailableClasses[i];
                var button = CreateButton(
                    characterPanel.transform,
                    GetClassLabel(characterClass),
                    new Vector2(0.16f, 0.72f - i * 0.13f),
                    new Vector2(260f, 68f));
                classButtons[i] = button;
                button.onClick.AddListener(() =>
                {
                    selectedClass = CharacterRoster.AvailableClasses[classIndex];
                    RefreshCharacterPreview(portraitImage, className, genderLabel, description, stats, classButtons, null, null);
                });
            }

            var maleButton = CreateButton(characterPanel.transform, "男性", new Vector2(0.43f, 0.17f), new Vector2(190f, 58f));
            var femaleButton = CreateButton(characterPanel.transform, "女性", new Vector2(0.57f, 0.17f), new Vector2(190f, 58f));
            maleButton.onClick.AddListener(() =>
            {
                selectedGender = CharacterGender.Male;
                RefreshCharacterPreview(portraitImage, className, genderLabel, description, stats, classButtons, maleButton, femaleButton);
            });
            femaleButton.onClick.AddListener(() =>
            {
                selectedGender = CharacterGender.Female;
                RefreshCharacterPreview(portraitImage, className, genderLabel, description, stats, classButtons, maleButton, femaleButton);
            });

            var enterButton = CreateButton(characterPanel.transform, "进入地图", new Vector2(0.82f, 0.17f), new Vector2(280f, 64f));
            enterButton.onClick.AddListener(EnterMap);

            RefreshCharacterPreview(portraitImage, className, genderLabel, description, stats, classButtons, maleButton, femaleButton);
        }

        private void RefreshCharacterPreview(
            Image portraitImage,
            Text className,
            Text genderLabel,
            Text description,
            Text stats,
            Button[] classButtons,
            Button maleButton,
            Button femaleButton)
        {
            var definition = CharacterRoster.Get(selectedClass, selectedGender);
            portraitImage.sprite = definition.LoadPortrait();
            className.text = GetClassLabel(selectedClass);
            genderLabel.text = selectedGender == CharacterGender.Male ? "男性" : "女性";
            description.text = GetClassDescription(selectedClass);
            stats.text =
                $"力量  {definition.BaseStats.strength}\n" +
                $"敏捷  {definition.BaseStats.agility}\n" +
                $"智力  {definition.BaseStats.intelligence}\n" +
                $"体质  {definition.BaseStats.constitution}\n" +
                $"防御  {definition.BaseStats.defense}";

            for (var i = 0; i < classButtons.Length; i++)
            {
                var selected = CharacterRoster.AvailableClasses[i] == selectedClass;
                classButtons[i].GetComponent<Image>().color = selected
                    ? new Color(0.54f, 0.42f, 0.20f, 1f)
                    : new Color(0.22f, 0.30f, 0.22f, 1f);
            }

            if (maleButton != null && femaleButton != null)
            {
                maleButton.GetComponent<Image>().color = selectedGender == CharacterGender.Male
                    ? new Color(0.54f, 0.42f, 0.20f, 1f)
                    : new Color(0.22f, 0.30f, 0.22f, 1f);
                femaleButton.GetComponent<Image>().color = selectedGender == CharacterGender.Female
                    ? new Color(0.54f, 0.42f, 0.20f, 1f)
                    : new Color(0.22f, 0.30f, 0.22f, 1f);
            }
        }

        private static string GetClassLabel(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => "战士",
                CharacterClass.Mage => "法师",
                CharacterClass.Archer => "弓箭手",
                CharacterClass.Priest => "牧师",
                _ => characterClass.ToString()
            };
        }

        private static string GetClassDescription(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => "近战前排职业，生存能力强，适合新手进入地图测试。",
                CharacterClass.Mage => "远程法术职业，爆发伤害高，后续可扩展元素技能。",
                CharacterClass.Archer => "远程敏捷职业，偏向持续输出和暴击成长。",
                CharacterClass.Priest => "辅助职业，后续可扩展治疗、护盾和团队增益。",
                _ => string.Empty
            };
        }

        private void EnterMap()
        {
            var definition = CharacterRoster.Get(selectedClass, selectedGender);
            var characterRenderer = playerController != null ? playerController.GetComponent<ProceduralCharacterRenderer>() : null;
            if (characterRenderer != null)
            {
                characterRenderer.Configure(
                    definition.ResourcePath,
                    definition.WalkRightResourcePath,
                    definition.WalkLeftResourcePath,
                    definition.WalkColumns,
                    definition.WalkRows);
                animator = playerController.GetComponentInChildren<GridSpriteSheetAnimator>(true);
            }

            if (animator != null)
            {
                animator.Configure(
                    definition.WalkRightResourcePath,
                    definition.WalkLeftResourcePath,
                    definition.WalkColumns,
                    definition.WalkRows);
            }

            ClearPanels();
            SetGameplayEnabled(true);
            PrototypeRuntimeInstaller.EnsureRuntimeObjects(playerController.transform);

            hud = GameplayHudController.Ensure();
            hud.SetCharacter($"{GetClassLabel(selectedClass)} {(selectedGender == CharacterGender.Male ? "男" : "女")}", definition.LoadPortrait());
            hud.AddLog($"[角色] 已选择{GetClassLabel(selectedClass)}。");

            // 进入地图后切换 BGM
            if (HeroQuest.Systems.Audio.AudioAssets.BgmMap != null)
                HeroQuest.Systems.Audio.AudioManager.Ensure().PlayBGM(HeroQuest.Systems.Audio.AudioAssets.BgmMap);

            hud.CommandClicked += OnCommandClicked;
            hud.SkillSlotClicked += OnSkillSlotClicked;
            PopulateActionBar();

            if (network != null)
            {
                WireNetworkEvents();
                _ = network.SendEnterDungeonAsync(1, CancellationToken.None);
                hud.AddLog("[系统] 正在进入地下城第1层...");
            }
            else
            {
                hud.AddLog("[系统] 离线模式，未连接服务器。");
            }
        }

        private void WireNetworkEvents()
        {
            // Combat / Dungeon
            network.DamageReceived += OnDamageReceived;
            network.MonsterRefresh += OnMonsterRefresh;
            network.ServerBroadcast += OnBroadcast;
            network.PlayerDie += OnPlayerDie;
            network.PlayerRevive += OnPlayerRevive;
            network.EnterDungeonResult += OnEnterDungeonResult;
            network.CollectResult += OnCollectResult;
            network.DungeonInfo += OnDungeonInfo;
            network.BossSpawn += OnBossSpawn;
            network.BossDie += OnBossDie;
            network.SkillEffectReceived += OnSkillEffect;
            network.AutoBattleResult += OnAutoBattleResult;
            network.UseItemResult += OnUseItemResult;
            network.InventorySync += OnInventorySync;

            // Player data（角色面板使用）
            network.LoginResult += OnPlayerDataReceived;
            network.CreatePlayerResult += OnPlayerDataReceived;

            // Equipment
            network.EquipStrengthenResult += OnEquipStrengthenResult;
            network.EquipEnchantResult += OnEquipEnchantResult;
            network.EquipWearResult += OnEquipWearResult;
            network.EquipUnloadResult += OnEquipUnloadResult;
            network.ForgeResult += OnForgeResult;

            // PvP
            network.PvpAttackResult += OnPvpAttackResult;
            network.RedNameListReceived += OnRedNameList;
            network.BountyRewardReceived += OnBountyReward;
            network.RevengeResult += OnRevengeResult;

            // Pet
            network.PetSummonResult += OnPetSummonResult;
            network.PetRecallResult += OnPetRecallResult;
            network.PetLevelUpResult += OnPetLevelUpResult;
            network.PetEvolveResult += OnPetEvolveResult;
            network.PetExploreResult += OnPetExploreResult;
            network.PetComposeResult += OnPetComposeResult;
            network.PetEquipResult += OnPetEquipResult;
            network.PetUnequipResult += OnPetUnequipResult;

            // Trading
            network.TradeListResult += OnTradeListResult;
            network.TradePublishResult += OnTradePublishResult;
            network.TradeBuyResult += OnTradeBuyResult;
            network.TradeCancelResult += OnTradeCancelResult;

            // Shop
            network.ShopListResult += OnShopListResult;
            network.ShopBuyResult += OnShopBuyResult;

            // Skill
            network.SkillLevelUpResult += OnSkillLevelUpResult;
            network.SkillResetResult += OnSkillResetResult;

            // Attribute
            network.AttrAssignResult += OnAttrAssignResult;

            // Ranking
            network.RankingListResult += OnRankingListResult;

            // Team
            network.TeamInfoResult += OnTeamInfoResult;
            network.TeamInvitePushReceived += OnTeamInvitePush;
            network.TeamInviteResultReceived += OnTeamInviteResult;
            network.TeamLeaveResult += OnTeamLeaveResult;
            network.TeamDismissResult += OnTeamDismissResult;
            network.TeamKickResult += OnTeamKickResult;
            network.TeamUpdateReceived += OnTeamUpdate;

            // Chat
            network.ChatSendResult += OnChatSendResult;
            network.ChatMessageReceived += OnChatMessage;
            network.ChatHistoryResult += OnChatHistoryResult;

            // Raid
            network.RaidEnterResult += OnRaidEnterResult;
            network.RaidLeaveResult += OnRaidLeaveResult;
            network.RaidTimer += OnRaidTimer;
            network.RaidDeath += OnRaidDeath;
            network.RaidExtractResult += OnRaidExtractResult;
            network.RaidExtractProgress += OnRaidExtractProgress;
            network.RaidLootOpenResult += OnRaidLootOpenResult;
            network.RaidLootPickupResult += OnRaidLootPickupResult;
            network.RaidLootDiscardResult += OnRaidLootDiscardResult;
            network.RaidInventorySync += OnRaidInventorySync;
            network.RaidPvpResult += OnRaidPvpResult;
            network.RaidMapListResult += OnRaidMapListResult;
            network.RaidStashResult += OnRaidStashResult;
            network.RaidInfoReceived += OnRaidInfo;
        }

        private void OnDestroy()
        {
            if (hud != null)
            {
                hud.CommandClicked -= OnCommandClicked;
                hud.SkillSlotClicked -= OnSkillSlotClicked;
            }

            if (network != null)
            {
                network.DamageReceived -= OnDamageReceived;
                network.MonsterRefresh -= OnMonsterRefresh;
                network.ServerBroadcast -= OnBroadcast;
                network.PlayerDie -= OnPlayerDie;
                network.PlayerRevive -= OnPlayerRevive;
                network.EnterDungeonResult -= OnEnterDungeonResult;
                network.CollectResult -= OnCollectResult;
                network.DungeonInfo -= OnDungeonInfo;
                network.BossSpawn -= OnBossSpawn;
                network.BossDie -= OnBossDie;
                network.SkillEffectReceived -= OnSkillEffect;
                network.AutoBattleResult -= OnAutoBattleResult;
                network.UseItemResult -= OnUseItemResult;
                network.InventorySync -= OnInventorySync;
                network.LoginResult -= OnPlayerDataReceived;
                network.CreatePlayerResult -= OnPlayerDataReceived;
                network.EquipStrengthenResult -= OnEquipStrengthenResult;
                network.EquipEnchantResult -= OnEquipEnchantResult;
                network.EquipWearResult -= OnEquipWearResult;
                network.EquipUnloadResult -= OnEquipUnloadResult;
                network.ForgeResult -= OnForgeResult;
                network.PvpAttackResult -= OnPvpAttackResult;
                network.RedNameListReceived -= OnRedNameList;
                network.BountyRewardReceived -= OnBountyReward;
                network.RevengeResult -= OnRevengeResult;
                network.PetSummonResult -= OnPetSummonResult;
                network.PetRecallResult -= OnPetRecallResult;
                network.PetLevelUpResult -= OnPetLevelUpResult;
                network.PetEvolveResult -= OnPetEvolveResult;
                network.PetExploreResult -= OnPetExploreResult;
                network.PetComposeResult -= OnPetComposeResult;
                network.PetEquipResult -= OnPetEquipResult;
                network.PetUnequipResult -= OnPetUnequipResult;
                network.TradeListResult -= OnTradeListResult;
                network.TradePublishResult -= OnTradePublishResult;
                network.TradeBuyResult -= OnTradeBuyResult;
                network.TradeCancelResult -= OnTradeCancelResult;
                network.ShopListResult -= OnShopListResult;
                network.ShopBuyResult -= OnShopBuyResult;
                network.SkillLevelUpResult -= OnSkillLevelUpResult;
                network.SkillResetResult -= OnSkillResetResult;
                network.AttrAssignResult -= OnAttrAssignResult;
                network.RankingListResult -= OnRankingListResult;
                network.TeamInfoResult -= OnTeamInfoResult;
                network.TeamInvitePushReceived -= OnTeamInvitePush;
                network.TeamInviteResultReceived -= OnTeamInviteResult;
                network.TeamLeaveResult -= OnTeamLeaveResult;
                network.TeamDismissResult -= OnTeamDismissResult;
                network.TeamKickResult -= OnTeamKickResult;
                network.TeamUpdateReceived -= OnTeamUpdate;
                network.ChatSendResult -= OnChatSendResult;
                network.ChatMessageReceived -= OnChatMessage;
                network.ChatHistoryResult -= OnChatHistoryResult;
                // Raid
                network.RaidEnterResult -= OnRaidEnterResult;
                network.RaidLeaveResult -= OnRaidLeaveResult;
                network.RaidTimer -= OnRaidTimer;
                network.RaidDeath -= OnRaidDeath;
                network.RaidExtractResult -= OnRaidExtractResult;
                network.RaidExtractProgress -= OnRaidExtractProgress;
                network.RaidLootOpenResult -= OnRaidLootOpenResult;
                network.RaidLootPickupResult -= OnRaidLootPickupResult;
                network.RaidLootDiscardResult -= OnRaidLootDiscardResult;
                network.RaidInventorySync -= OnRaidInventorySync;
                network.RaidPvpResult -= OnRaidPvpResult;
                network.RaidMapListResult -= OnRaidMapListResult;
                network.RaidStashResult -= OnRaidStashResult;
                network.RaidInfoReceived -= OnRaidInfo;
            }
        }

        private void OnEnterDungeonResult(uint code, GoEnterDungeonResponse resp)
        {
            if (code != 0)
            {
                hud?.AddLog($"[错误] 进入地下城失败: code={code}");
                return;
            }

            currentLayer = resp.layer;
            hud?.AddLog($"[地下城] 已进入第{resp.layer}层，区域: {resp.zone}");

            // 关闭野怪装饰刷新
            var wildSpawner = FindFirstObjectByType<WildMonsterSpawner>();
            if (wildSpawner != null)
            {
                wildSpawner.enabled = false;
            }

            visibleMonsters.Clear();
            ClearDungeonMonsterVisuals();
            if (resp.monsters != null)
            {
                foreach (var m in resp.monsters)
                {
                    visibleMonsters[m.id] = m;
                    SpawnDungeonMonsterVisual(m);
                }
                hud?.AddLog($"[地下城] 发现 {resp.monsters.Length} 只怪物。");
                SelectNearestTarget();
            }
            if (resp.players != null)
            {
                hud?.AddLog($"[地下城] 当前有 {resp.players.Length} 名玩家。");
            }
        }

        private void ClearDungeonMonsterVisuals()
        {
            foreach (var kv in dungeonMonsterObjects)
            {
                if (kv.Value != null) Destroy(kv.Value);
            }
            dungeonMonsterObjects.Clear();
        }

        private void SpawnDungeonMonsterVisual(GoMonsterData m)
        {
            var monster = new GameObject($"Monster_{m.id}");
            monster.transform.position = new Vector3((float)m.x, (float)m.y, 0f);

            // 阴影
            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(monster.transform, false);
            shadow.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            shadow.transform.localScale = new Vector3(0.66f, 0.24f, 1f);
            var shadowR = shadow.AddComponent<SpriteRenderer>();
            shadowR.sprite = PrototypeSpriteFactory.CreateEllipseSprite(72, 22, new Color(0f, 0f, 0f, 0.22f), 64);
            shadowR.sortingOrder = 1;

            // 怪物精灵
            var visual = new GameObject("Visual");
            visual.transform.SetParent(monster.transform, false);
            visual.transform.localPosition = Vector3.zero;
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = PrototypeSpriteFactory.CreateEllipseSprite(64, 84, new Color(0.65f, 0.18f, 0.12f, 1f), 64);
            renderer.sortingOrder = 3;
            visual.transform.localScale = Vector3.one * (1.45f / 84f * 64f);

            // 生命条背景
            var barY = 1.45f + 0.13f;
            var bg = new GameObject("HP_BG");
            bg.transform.SetParent(monster.transform, false);
            bg.transform.localPosition = new Vector3(0f, barY, 0f);
            bg.transform.localScale = new Vector3(0.68f, 0.42f, 1f);
            var bgR = bg.AddComponent<SpriteRenderer>();
            bgR.sprite = PrototypeSpriteFactory.CreateRectangleSprite(80, 10, new Color(0.10f, 0.08f, 0.07f, 0.95f), 64);
            bgR.sortingOrder = 10;

            // 生命值填充
            var fill = new GameObject("HP_Fill");
            fill.transform.SetParent(bg.transform, false);
            fill.transform.localPosition = Vector3.zero;
            fill.transform.localScale = Vector3.one;
            var fillR = fill.AddComponent<SpriteRenderer>();
            fillR.sprite = PrototypeSpriteFactory.CreateRectangleSprite(76, 6, new Color(0.72f, 0.13f, 0.10f, 1f), 64);
            fillR.sortingOrder = 11;

            // 名称
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(monster.transform, false);
            nameObj.transform.localPosition = new Vector3(0f, barY + 0.20f, 0f);
            var text = nameObj.AddComponent<TextMesh>();
            text.text = m.name ?? "怪物";
            text.font = ChineseFontProvider.GetFont();
            text.fontSize = 32;
            text.characterSize = 0.065f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(0.96f, 0.92f, 0.78f, 1f);
            text.GetComponent<MeshRenderer>().sortingOrder = 12;

            // 挂载 WildMonster 组件用于更新血条
            var wm = monster.AddComponent<WildMonster>();
            wm.Initialize(m.name ?? "怪物", fill.transform);
            if (m.max_hp > 0)
            {
                wm.SetHealthPercent((float)m.hp / m.max_hp);
            }

            dungeonMonsterObjects[m.id] = monster;
        }

        private void OnDamageReceived(GoDamage damage)
        {
            // 怪物攻击玩家（target_id == playerId）
            if (damage.target_id == playerId)
            {
                var dmgLabel = damage.is_dead ? "受到致命伤害" : "受到伤害";
                hud?.AddLog($"[战斗] {dmgLabel} {damage.damage}，剩余HP {damage.curr_hp}");

                // 浮动伤害文字（显示在玩家位置）
                if (playerController != null)
                {
                    var dmgColor = new Color(1f, 0.2f, 0.1f, 1f);
                    hud?.ShowFloatingText(playerController.transform.position, $"-{damage.damage}", dmgColor);
                }

                // 更新玩家HP条
                if (localPlayerData != null)
                {
                    localPlayerData.hp = damage.curr_hp;
                    var hpPct = localPlayerData.max_hp > 0 ? (float)localPlayerData.hp / localPlayerData.max_hp : 0f;
                    var mpPct = localPlayerData.max_mp > 0 ? (float)localPlayerData.mp / localPlayerData.max_mp : 0f;
                    hud?.SetVitals(hpPct, mpPct);
                    hud?.SetVitalsText(damage.curr_hp, localPlayerData.max_hp, localPlayerData.mp, localPlayerData.max_mp);
                }

                if (damage.is_dead)
                {
                    HandlePlayerDeath();
                }
                return;
            }

            // 玩家攻击怪物
            var label = damage.is_dead ? "击杀" : "命中";
            hud?.AddLog($"[战斗] {label} 目标 {damage.target_id}，伤害 {damage.damage}，剩余HP {damage.curr_hp}");

            // 浮动战斗文字
            if (visibleMonsters.TryGetValue(damage.target_id, out var dmgMonster))
            {
                var dmgText = damage.damage.ToString();
                var dmgColor = new Color(1f, 0.35f, 0.2f, 1f);
                var monsterPos = new Vector3((float)dmgMonster.x, (float)dmgMonster.y, 0f);
                hud?.ShowFloatingText(monsterPos, dmgText, dmgColor);
                // 攻击命中特效
                SpawnHitEffect(monsterPos);
                // 命中音效
                HeroQuest.Systems.Audio.AudioManager.Ensure().PlaySFX(
                    damage.is_dead
                        ? HeroQuest.Systems.Audio.AudioAssets.SfxMonsterDeath
                        : HeroQuest.Systems.Audio.AudioAssets.SfxHit,
                    1f, 0.1f);
            }

            if (damage.pet_damage > 0)
            {
                var critTag = damage.pet_crit ? " 暴击！" : "";
                hud?.AddLog($"[宠物] 造成 {damage.pet_damage} 伤害{critTag}");
            }
            if (damage.is_dead)
            {
                hud?.AddLog($"[战斗] 目标 {damage.target_id} 已被消灭！");
                visibleMonsters.Remove(damage.target_id);
                if (dungeonMonsterObjects.TryGetValue(damage.target_id, out var deadObj))
                {
                    Destroy(deadObj);
                    dungeonMonsterObjects.Remove(damage.target_id);
                }
                if (damage.target_id == selectedTargetId)
                {
                    selectedTargetId = 0;
                    SelectNearestTarget();
                }
            }
            else
            {
                if (visibleMonsters.TryGetValue(damage.target_id, out var monster))
                {
                    monster.hp = damage.curr_hp;
                    // 更新视觉血条
                    if (dungeonMonsterObjects.TryGetValue(damage.target_id, out var monsterObj))
                    {
                        var wm = monsterObj.GetComponent<WildMonster>();
                        if (wm != null && monster.max_hp > 0)
                        {
                            wm.SetHealthPercent((float)monster.hp / monster.max_hp);
                        }
                    }
                    if (damage.target_id == selectedTargetId)
                    {
                        hud?.SetTarget(monster.name, 0, (float)monster.hp / monster.max_hp, monster.hp, monster.max_hp);
                    }
                }
            }
        }

        private void OnMonsterRefresh(GoMonsterData[] monsters)
        {
            if (monsters == null) return;

            // 收集新数据中的ID
            var newIds = new HashSet<ulong>();
            foreach (var m in monsters)
            {
                newIds.Add(m.id);
            }

            // 移除已不存在的怪物视觉
            var toRemove = new List<ulong>();
            foreach (var kv in dungeonMonsterObjects)
            {
                if (!newIds.Contains(kv.Key))
                {
                    if (kv.Value != null) Destroy(kv.Value);
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var id in toRemove) dungeonMonsterObjects.Remove(id);

            // 更新或创建怪物
            visibleMonsters.Clear();
            foreach (var m in monsters)
            {
                visibleMonsters[m.id] = m;
                if (dungeonMonsterObjects.TryGetValue(m.id, out var existing) && existing != null)
                {
                    // 更新位置和血条
                    existing.transform.position = new Vector3((float)m.x, (float)m.y, 0f);
                    var wm = existing.GetComponent<WildMonster>();
                    if (wm != null && m.max_hp > 0)
                    {
                        wm.SetHealthPercent((float)m.hp / m.max_hp);
                    }
                }
                else
                {
                    SpawnDungeonMonsterVisual(m);
                }
            }
            hud?.AddLog($"[地下城] 怪物刷新，共 {monsters.Length} 只。");
            if (selectedTargetId == 0 || !visibleMonsters.ContainsKey(selectedTargetId))
            {
                SelectNearestTarget();
            }
        }

        private void OnBroadcast(string content)
        {
            hud?.AddLog($"[公告] {content}");
        }

        private void OnPlayerDie(ulong pid)
        {
            if (pid == playerId)
            {
                HandlePlayerDeath();
            }
            else
            {
                hud?.AddLog($"[战斗] 玩家 {pid} 已阵亡。");
            }
        }

        private void HandlePlayerDeath()
        {
            if (playerDead) return;
            playerDead = true;
            SetGameplayEnabled(false);
            hud?.AddLog("[战斗] 你已阵亡！3秒后自动复活...");
            // 3秒后自动复活
            StartCoroutine(AutoReviveAfterDelay(3f));
        }

        private System.Collections.IEnumerator AutoReviveAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (playerDead)
            {
                HandlePlayerRevive();
            }
        }

        private void HandlePlayerRevive()
        {
            playerDead = false;
            SetGameplayEnabled(true);

            // 复活时传送到安全位置（地图左下角附近）
            if (playerController != null)
            {
                var mapRenderer = FindFirstObjectByType<ProceduralMapRenderer>();
                if (mapRenderer != null)
                {
                    var bounds = mapRenderer.GetWorldBounds();
                    var safePos = bounds.min + new Vector3(5f, 5f, 0f);
                    playerController.transform.position = safePos;
                }
            }

            // 恢复HP显示
            if (localPlayerData != null)
            {
                localPlayerData.hp = localPlayerData.max_hp;
                localPlayerData.mp = localPlayerData.max_mp;
                var hpPct = localPlayerData.max_hp > 0 ? (float)localPlayerData.hp / localPlayerData.max_hp : 1f;
                var mpPct = localPlayerData.max_mp > 0 ? (float)localPlayerData.mp / localPlayerData.max_mp : 1f;
                hud?.SetVitals(hpPct, mpPct);
                hud?.SetVitalsText(localPlayerData.hp, localPlayerData.max_hp, localPlayerData.mp, localPlayerData.max_mp);
            }
            hud?.AddLog("[系统] 你已复活。");
        }

        private void OnPlayerRevive(ulong pid)
        {
            if (pid == playerId)
            {
                HandlePlayerRevive();
            }
        }

        private void OnCollectResult(uint code, GoCollectResult result)
        {
            if (code != 0)
            {
                hud?.AddLog($"[采集] 采集失败: code={code}");
                return;
            }
            hud?.AddLog($"[采集] 获得 {result.item_name} x{result.count}");
        }

        private void OnDungeonInfo(GoDungeonInfo info)
        {
            currentLayer = info.current_layer;
            hud?.AddLog($"[地下城] 当前第{info.current_layer}层，最高第{info.max_layer}层。");
        }

        private void OnBossSpawn(ulong bossId)
        {
            hud?.AddLog($"[BOSS] BOSS出现！ID={bossId}");
            selectedTargetId = bossId;
            hud?.SetTarget($"BOSS #{bossId}", 0, 1f, 0, 0);
        }

        private void OnBossDie(ulong bossId)
        {
            hud?.AddLog($"[BOSS] BOSS已被击杀！ID={bossId}");
            visibleMonsters.Remove(bossId);
            if (dungeonMonsterObjects.TryGetValue(bossId, out var bossObj))
            {
                Destroy(bossObj);
                dungeonMonsterObjects.Remove(bossId);
            }
            if (bossId == selectedTargetId)
            {
                selectedTargetId = 0;
                SelectNearestTarget();
            }
        }

        private void OnSkillEffect(GoSkillEffect effect)
        {
            if (effect.targets == null) return;
            foreach (var t in effect.targets)
            {
                var dead = t.is_dead ? " 击杀！" : "";
                hud?.AddLog($"[技能] 技能{effect.skill_id} 命中目标{t.target_id}，伤害{t.damage}{dead}");

                // 浮动伤害数字 + 特效
                if (visibleMonsters.TryGetValue(t.target_id, out var skillMonster))
                {
                    var monsterPos = new Vector3((float)skillMonster.x, (float)skillMonster.y, 0f);
                    var dmgColor = new Color(1f, 0.35f, 0.2f, 1f);
                    hud?.ShowFloatingText(monsterPos, t.damage.ToString(), dmgColor);
                    SpawnHitEffect(monsterPos);
                }

                if (t.is_dead)
                {
                    visibleMonsters.Remove(t.target_id);
                    if (dungeonMonsterObjects.TryGetValue(t.target_id, out var skillDeadObj))
                    {
                        Destroy(skillDeadObj);
                        dungeonMonsterObjects.Remove(t.target_id);
                    }
                    if (t.target_id == selectedTargetId)
                    {
                        selectedTargetId = 0;
                        SelectNearestTarget();
                    }
                }
                else
                {
                    // 更新血条
                    if (visibleMonsters.TryGetValue(t.target_id, out var sm) && sm.max_hp > 0)
                    {
                        sm.hp = t.curr_hp;
                        if (dungeonMonsterObjects.TryGetValue(t.target_id, out var sObj))
                        {
                            var wm = sObj.GetComponent<WildMonster>();
                            if (wm != null) wm.SetHealthPercent((float)sm.hp / sm.max_hp);
                        }
                    }
                }
            }
        }

        private void OnAutoBattleResult(bool enabled)
        {
            autoBattleEnabled = enabled;
            hud?.AddLog($"[自动战斗] {(enabled ? "已开启" : "已关闭")}");
        }

        private void OnCommandClicked(string command)
        {
            switch (command)
            {
                case "攻击":
                    DoAttack(0);
                    break;
                case "技能":
                    hud?.AddLog("[系统] 技能书面板开发中。");
                    break;
                case "宠物":
                    if (network != null && network.IsConnected)
                        _ = network.SendPetSummonAsync(0, CancellationToken.None);
                    hud?.AddLog("[宠物] 正在查询宠物...");
                    break;
                case "战局":
                    ToggleRaidLobbyPanelView();
                    break;
                case "背包":
                case "角色":
                    ToggleCharacterPanel();
                    break;
                case "锻造":
                    if (network != null && network.IsConnected)
                        _ = network.SendForgeAsync(1, new ulong[0], CancellationToken.None);
                    hud?.AddLog("[锻造] 正在查询锻造...");
                    break;
                case "商店":
                    if (network != null && network.IsConnected)
                        _ = network.SendShopListAsync(0, CancellationToken.None);
                    hud?.AddLog("[商店] 正在加载商店...");
                    break;
                case "交易":
                    if (network != null && network.IsConnected)
                        _ = network.SendTradeListAsync(0, 1, 20, CancellationToken.None);
                    hud?.AddLog("[交易] 正在加载交易行...");
                    break;
                case "排行":
                    if (network != null && network.IsConnected)
                        _ = network.SendRankingListAsync(0, 50, CancellationToken.None);
                    hud?.AddLog("[排行] 正在加载排行榜...");
                    break;
                case "移动":
                    hud?.AddLog("[移动] 使用WASD移动，鼠标左键选目标，右键攻击。");
                    break;
                case "任务":
                    hud?.AddLog("[系统] 任务系统开发中。");
                    break;
                case "队伍":
                    ToggleTeamPanel();
                    break;
                case "设置":
                    hud?.AddLog("[系统] 设置面板开发中。");
                    break;
                case "地图":
                    hud?.AddLog("[系统] 地图面板开发中。");
                    break;
                case "社交":
                    hud?.AddLog("[系统] 社交面板开发中。");
                    break;
            }
        }

        private void OnSkillSlotClicked(int index)
        {
            UseActionBarSlot(index);
        }

        private void UseActionBarSlot(int index)
        {
            if (index < 0 || index >= actionBarSlots.Length) return;
            var slot = actionBarSlots[index];
            if (slot.type == BarSlotType.Empty)
            {
                hud?.AddLog($"[系统] 动作条槽位 {index + 1} 未绑定技能。");
                return;
            }
            if (slot.type == BarSlotType.Skill)
            {
                DoAttack(slot.id);
            }
            else if (slot.type == BarSlotType.Item)
            {
                UseItemFromSlot(slot.id);
            }
        }

        private void PopulateActionBar()
        {
            // 槽位1 = 普通攻击
            actionBarSlots[0] = new BarSlot { type = BarSlotType.Skill, id = 0 };
            // 槽位2-6 = 职业技能1-5
            var classSkillBase = (int)selectedClass * 5 + 1;
            for (var i = 0; i < 5; i++)
            {
                actionBarSlots[1 + i] = new BarSlot { type = BarSlotType.Skill, id = classSkillBase + i };
            }
            // 槽位7-8 = 消耗品（小HP，小MP）
            actionBarSlots[6] = new BarSlot { type = BarSlotType.Item, id = 1 };
            actionBarSlots[7] = new BarSlot { type = BarSlotType.Item, id = 3 };
            // 槽位9-12 = 空
            for (var i = 8; i < 12; i++)
            {
                actionBarSlots[i] = new BarSlot { type = BarSlotType.Empty, id = 0 };
            }
            // 更新 HUD 显示
            var skillNames = new[] { "普攻", "技能1", "技能2", "技能3", "技能4", "技能5", "小HP", "小MP", "", "", "", "" };
            for (var i = 0; i < 12; i++)
            {
                var name = actionBarSlots[i].type == BarSlotType.Empty ? "" : skillNames[i];
                hud?.UpdateActionBarSlot(i, name);
            }
        }

        private void ToggleAutoBattle()
        {
            if (network == null || !network.IsConnected)
            {
                hud?.AddLog("[系统] 未连接服务器。");
                return;
            }
            autoBattleEnabled = !autoBattleEnabled;
            _ = network.SendAutoBattleAsync(autoBattleEnabled, CancellationToken.None);
            hud?.AddLog($"[战斗] 自动战斗 {(autoBattleEnabled ? "开启" : "关闭")}。");
        }

        private void HandleEscape()
        {
            // 关闭角色面板
            if (characterPanelView != null && characterPanelView.gameObject.activeSelf)
            {
                characterPanelView.Hide();
                return;
            }
            // 取消选中
            if (selectedTargetId != 0)
            {
                selectedTargetId = 0;
                hud?.SetTarget("", 0, 0, 0, 0);
                return;
            }
        }

        private void HandleRightClick()
        {
            var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            var clickedId = FindMonsterAtPosition(mouseWorld, 1.5f);
            if (clickedId != 0)
            {
                selectedTargetId = clickedId;
                var m = visibleMonsters[clickedId];
                hud?.SetTarget(m.name, 0, (float)m.hp / m.max_hp, m.hp, m.max_hp);
                DoAttack(0); // 自动攻击
            }
        }

        private ulong FindMonsterAtPosition(Vector3 worldPos, float radius)
        {
            ulong nearest = 0;
            float bestDist = radius;
            foreach (var kv in visibleMonsters)
            {
                if (kv.Value.hp <= 0) continue;
                var pos = new Vector3((float)kv.Value.x, (float)kv.Value.y, 0f);
                var dist = Vector3.Distance(worldPos, pos);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = kv.Key;
                }
            }
            return nearest;
        }

        private void DoAttack(int skillId)
        {
            if (network == null || !network.IsConnected)
            {
                hud?.AddLog("[战斗] 未连接服务器。");
                return;
            }

            if (selectedTargetId == 0)
            {
                SelectNearestTarget();
                if (selectedTargetId == 0)
                {
                    hud?.AddLog("[战斗] 没有可攻击的目标。");
                    return;
                }
            }

            _ = network.SendAttackAsync(selectedTargetId, skillId, CancellationToken.None);
            var skillText = skillId == 0 ? "普通攻击" : $"技能{skillId}";
            hud?.AddLog($"[战斗] 对目标{selectedTargetId}使用{skillText}。");

            // 攻击起手特效：玩家位置发出闪光
            if (playerController != null)
            {
                SpawnAttackFlash(playerController.transform.position, skillId);
            }

            // 攻击/技能音效
            var sfx = skillId == 0
                ? HeroQuest.Systems.Audio.AudioAssets.SfxAttack
                : HeroQuest.Systems.Audio.AudioAssets.SfxSkill;
            HeroQuest.Systems.Audio.AudioManager.Ensure().PlaySFX(sfx, 1f, 0.05f);
        }

        /// <summary>
        /// 在目标位置生成命中特效（短暂闪光圆 + 扩散动画）。
        /// </summary>
        private void SpawnHitEffect(Vector3 worldPos)
        {
            var go = new GameObject("HitEffect");
            go.transform.position = worldPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PrototypeSpriteFactory.CreateEllipseSprite(48, 48, new Color(1f, 0.85f, 0.2f, 0.9f), 64);
            sr.sortingOrder = 20;
            var anim = go.AddComponent<HitEffectAnimator>();
            anim.StartAnim();
            Destroy(go, 0.4f);
        }

        /// <summary>
        /// 在玩家位置生成攻击起手闪光。
        /// </summary>
        private void SpawnAttackFlash(Vector3 playerPos, int skillId)
        {
            var color = skillId == 0
                ? new Color(0.8f, 0.8f, 0.8f, 0.7f)
                : new Color(0.3f, 0.5f, 1f, 0.8f);
            var go = new GameObject("AttackFlash");
            go.transform.position = playerPos + new Vector3(0f, 0.5f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PrototypeSpriteFactory.CreateEllipseSprite(40, 40, color, 64);
            sr.sortingOrder = 15;
            var anim = go.AddComponent<HitEffectAnimator>();
            anim.StartAnim();
            Destroy(go, 0.3f);
        }

        // 消耗品槽位映射（槽位 -> item_id）
        private static readonly uint[] ItemSlotMap = { 0, 1, 2, 3, 4, 5, 0, 0 };
        private static readonly Dictionary<uint, string> ItemNames = new()
        {
            { 1, "小型生命药水" },
            { 2, "大型生命药水" },
            { 3, "小型魔法药水" },
            { 4, "大型魔法药水" },
            { 5, "全能药水" },
        };

        private void UseItemFromSlot(int itemId)
        {
            if (network == null || !network.IsConnected)
            {
                hud?.AddLog("[系统] 未连接服务器。");
                return;
            }

            if (itemId <= 0)
            {
                hud?.AddLog("[物品] 无效的物品ID。");
                return;
            }

            _ = network.SendUseItemAsync((uint)itemId, CancellationToken.None);
            var itemName = ItemNames.TryGetValue((uint)itemId, out var n) ? n : $"物品{itemId}";
            hud?.AddLog($"[物品] 使用 {itemName}...");
        }

        private void OnUseItemResult(GoUseItemResponse resp)
        {
            if (resp.code != 0)
            {
                var errMsg = resp.code switch
                {
                    1200 => "物品无效",
                    1201 => "物品不足",
                    1202 => "冷却中",
                    _ => $"错误码 {resp.code}"
                };
                hud?.AddLog($"[物品] 使用失败：{errMsg}");
                return;
            }

            var itemName = ItemNames.TryGetValue(resp.item_id, out var n) ? n : $"物品{resp.item_id}";
            hud?.AddLog($"[物品] 使用 {itemName} 成功，剩余 {resp.count} 个。");

            // 更新 HUD
            hud?.SetVitals(resp.max_hp > 0 ? (float)resp.hp / resp.max_hp : 0, resp.max_mp > 0 ? (float)resp.mp / resp.max_mp : 0);
            hud?.SetVitalsText(resp.hp, resp.max_hp, resp.mp, resp.max_mp);

            // 更新消耗品槽位数量
            for (var i = 1; i < ItemSlotMap.Length; i++)
            {
                if (ItemSlotMap[i] == resp.item_id)
                {
                    hud?.UpdateInventorySlot(i - 1, itemName, (int)resp.count);
                    break;
                }
            }
        }

        private void OnInventorySync(GoInventorySync sync)
        {
            if (sync.items == null) return;
            RefreshConsumableBar(sync.items);
        }

        private void RefreshConsumableBar(GoItemCount[] items)
        {
            // 槽位1-6对应 ItemSlotMap[1..6]
            for (var i = 0; i < 6; i++)
            {
                var slotIndex = i + 1;
                if (slotIndex < ItemSlotMap.Length && ItemSlotMap[slotIndex] != 0)
                {
                    var itemId = ItemSlotMap[slotIndex];
                    var name = ItemNames.TryGetValue(itemId, out var n) ? n : "";
                    var count = 0;
                    for (var j = 0; j < items.Length; j++)
                    {
                        if (items[j].item_id == itemId)
                        {
                            count = items[j].count;
                            break;
                        }
                    }
                    hud?.UpdateInventorySlot(i, name, count);
                }
                else
                {
                    hud?.UpdateInventorySlot(i, "", 0);
                }
            }
        }

        private void SelectNearestTarget()
        {
            if (visibleMonsters.Count == 0)
            {
                selectedTargetId = 0;
                hud?.SetTarget("", 0, 0, 0, 0);
                return;
            }

            // 收集所有存活怪物，按距离排序
            var playerPos = playerController != null ? playerController.transform.position : Vector3.zero;
            var alive = new System.Collections.Generic.List<(ulong id, GoMonsterData data, float dist)>();
            foreach (var kv in visibleMonsters)
            {
                if (kv.Value.hp <= 0) continue;
                var pos = new Vector3((float)kv.Value.x, (float)kv.Value.y, 0f);
                alive.Add((kv.Key, kv.Value, Vector3.Distance(playerPos, pos)));
            }
            if (alive.Count == 0)
            {
                selectedTargetId = 0;
                hud?.SetTarget("", 0, 0, 0, 0);
                return;
            }
            alive.Sort((a, b) => a.dist.CompareTo(b.dist));

            // 循环选择下一个目标
            var currentIdx = alive.FindIndex(a => a.id == selectedTargetId);
            var nextIdx = (currentIdx + 1) % alive.Count;
            var next = alive[nextIdx];
            selectedTargetId = next.id;
            hud?.SetTarget(next.data.name, 0, (float)next.data.hp / next.data.max_hp, next.data.hp, next.data.max_hp);
        }

        private void HandleMouseClick()
        {
            var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            var clickedId = FindMonsterAtPosition(mouseWorld, 1.5f);
            if (clickedId != 0)
            {
                selectedTargetId = clickedId;
                var m = visibleMonsters[clickedId];
                hud?.SetTarget(m.name, 0, (float)m.hp / m.max_hp, m.hp, m.max_hp);
                // 左键点击怪物 = 选中 + 自动攻击
                DoAttack(0);
            }
            else if (playerController != null)
            {
                playerController.SetClickMoveTarget(mouseWorld);
            }
        }

        // --- Equipment ---

        private void OnPlayerDataReceived(uint code, GoPlayerData player)
        {
            if (code != 0 || player == null)
            {
                return;
            }
            localPlayerData = player;
            playerId = player.id;

            // 服务端 toPlayerData() 不下发 hp/max_hp/mp/max_mp，客户端本地计算初始值
            if (player.max_hp <= 0)
            {
                player.max_hp = 100 + player.con * 20 + player.level * 50;
                player.hp = player.max_hp;
            }
            if (player.max_mp <= 0)
            {
                player.max_mp = 50 + player.@int * 15 + player.level * 20;
                player.mp = player.max_mp;
            }

            // 更新玩家 HP/MP 条
            var hp = player.hp;
            var maxHp = player.max_hp;
            var mp = player.mp;
            var maxMp = player.max_mp;
            hud?.SetVitals(maxHp > 0 ? (float)hp / maxHp : 0, maxMp > 0 ? (float)mp / maxMp : 0);
            hud?.SetVitalsText(hp, maxHp, mp, maxMp);
            hud?.SetCharacter($"{player.name} Lv.{player.level}", null);

            // 更新消耗品栏
            if (player.items != null)
            {
                RefreshConsumableBar(player.items);
            }

            if (characterPanelView != null && characterPanelView.gameObject.activeSelf)
            {
                PopulateCharacterPanel();
            }
        }

        private void ToggleCharacterPanel()
        {
            if (characterPanelView == null)
            {
                characterPanelView = CharacterPanelView.Create(flowCanvas);
                characterPanelView.OnCloseRequested += () => hud?.AddLog("[角色] 关闭角色面板。");
                characterPanelView.OnStrengthenRequested += slot =>
                {
                    if (network != null)
                    {
                        _ = network.SendEquipStrengthenAsync((int)slot, CancellationToken.None);
                        hud?.AddLog($"[装备] 请求强化 {slot} 槽。");
                    }
                };
                characterPanelView.OnEnchantRequested += slot =>
                {
                    if (network != null)
                    {
                        // 使用默认附魔材料 ID 2001，后续背包系统完善后改为材料选择器
                        _ = network.SendEquipEnchantAsync((int)slot, 2001, CancellationToken.None);
                        hud?.AddLog($"[装备] 请求附魔 {slot} 槽。");
                    }
                };
                characterPanelView.OnUnequipRequested += slot =>
                {
                    if (network != null)
                    {
                        _ = network.SendEquipUnloadAsync((int)slot, CancellationToken.None);
                        hud?.AddLog($"[装备] 请求卸下 {slot} 槽。");
                    }
                };
                characterPanelView.OnAttrAssignRequested += attr =>
                {
                    if (network != null)
                    {
                        _ = network.SendAttrAssignAsync(attr, 1, CancellationToken.None);
                        hud?.AddLog($"[属性] 请求分配 {attr} +1。");
                    }
                };
            }

            if (characterPanelView.gameObject.activeSelf)
            {
                characterPanelView.Hide();
                return;
            }

            PopulateCharacterPanel();
            characterPanelView.Show();
            hud?.AddLog("[角色] 打开角色·装备面板。");
        }

        private void ToggleTeamPanel()
        {
            ToggleTeamPanelView();
        }

        // ================================================================
        // 组队面板
        // ================================================================

        private void ToggleTeamPanelView()
        {
            if (teamPanelView == null)
            {
                teamPanelView = TeamPanelView.Create(flowCanvas);
                teamPanelView.SetCurrentPlayerId(playerId);
                teamPanelView.OnCloseRequested += () => teamPanelView.Hide();
                teamPanelView.OnCreateTeamRequested += () =>
                {
                    if (network != null) _ = network.SendTeamCreateAsync(CancellationToken.None);
                    hud?.AddLog("[队伍] 请求创建队伍。");
                };
                teamPanelView.OnInviteRequested += id =>
                {
                    if (network != null) _ = network.SendTeamInviteAsync(id, CancellationToken.None);
                    hud?.AddLog($"[队伍] 邀请玩家 {id}。");
                };
                teamPanelView.OnKickRequested += id =>
                {
                    if (network != null) _ = network.SendTeamKickAsync(id, CancellationToken.None);
                    hud?.AddLog($"[队伍] 踢出玩家 {id}。");
                };
                teamPanelView.OnLeaveTeamRequested += () =>
                {
                    if (network != null) _ = network.SendTeamLeaveAsync(CancellationToken.None);
                    hud?.AddLog("[队伍] 请求离开队伍。");
                };
                teamPanelView.OnDismissTeamRequested += () =>
                {
                    if (network != null) _ = network.SendTeamDismissAsync(CancellationToken.None);
                    hud?.AddLog("[队伍] 请求解散队伍。");
                };
                teamPanelView.OnInviteReplyRequested += (teamId, accept) =>
                {
                    if (network != null) _ = network.SendTeamInviteReplyAsync(teamId, accept, CancellationToken.None);
                    hud?.AddLog($"[队伍] {(accept ? "接受" : "拒绝")}邀请。");
                };
            }

            if (teamPanelView.gameObject.activeSelf)
            {
                teamPanelView.Hide();
                return;
            }

            // 查询当前队伍状态
            if (network != null) _ = network.SendTeamQueryAsync(CancellationToken.None);
            teamPanelView.ApplyNoTeam();
            teamPanelView.Show();
            hud?.AddLog("[队伍] 打开组队面板。");
        }

        private void OnTeamInfoResult(GoTeamInfoResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[队伍] 操作失败: code={resp.code}");
                if (teamPanelView != null && teamPanelView.gameObject.activeSelf)
                    teamPanelView.ApplyNoTeam();
                return;
            }
            if (teamPanelView != null && teamPanelView.gameObject.activeSelf)
            {
                if (resp.team != null && resp.team.members != null && resp.team.members.Length > 0)
                    teamPanelView.ApplyTeamInfo(resp.team);
                else
                    teamPanelView.ApplyNoTeam();
            }
            if (resp.team != null && resp.team.members != null)
                hud?.AddLog($"[队伍] 队伍信息更新: {resp.team.member_count}人");
        }

        private void OnTeamInvitePush(GoTeamInvitePush push)
        {
            hud?.AddLog($"[队伍] {push.inviter_name} 邀请你加入队伍！");
            if (teamPanelView == null || !teamPanelView.gameObject.activeSelf)
            {
                ToggleTeamPanelView();
            }
            teamPanelView?.ApplyInviteNotification(push);
        }

        private void OnTeamInviteResult(GoTeamInviteResult resp)
        {
            var result = resp.accept ? "接受了邀请" : "拒绝了邀请";
            hud?.AddLog($"[队伍] {resp.target_name} {result}");
        }

        private void OnTeamLeaveResult(GoTeamLeaveResponse resp)
        {
            if (resp.code != 0) { hud?.AddLog($"[队伍] 离开失败: code={resp.code}"); return; }
            hud?.AddLog("[队伍] 已离开队伍。");
            if (teamPanelView != null && teamPanelView.gameObject.activeSelf)
                teamPanelView.ApplyNoTeam();
        }

        private void OnTeamDismissResult(GoTeamDismissResponse resp)
        {
            if (resp.code != 0) { hud?.AddLog($"[队伍] 解散失败: code={resp.code}"); return; }
            hud?.AddLog("[队伍] 队伍已解散。");
            teamPanelView?.Hide();
        }

        private void OnTeamKickResult(GoTeamKickResponse resp)
        {
            if (resp.code != 0) { hud?.AddLog($"[队伍] 踢出失败: code={resp.code}"); return; }
            hud?.AddLog($"[队伍] 已踢出玩家 {resp.target_id}。");
        }

        private void OnTeamUpdate(GoTeamUpdate update)
        {
            hud?.AddLog($"[队伍] {update.reason}");
            if (teamPanelView != null && teamPanelView.gameObject.activeSelf)
            {
                if (update.action == 3) // 解散
                    teamPanelView.ApplyNoTeam();
                else if (update.members != null)
                {
                    var info = new GoTeamInfo
                    {
                        team_id = update.team_id,
                        leader_id = update.leader_id,
                        member_count = update.members.Length,
                        members = update.members
                    };
                    teamPanelView.ApplyTeamInfo(info);
                }
            }
        }

        // ================================================================
        // 聊天面板
        // ================================================================

        private void ToggleChatPanelView()
        {
            if (chatPanelView == null)
            {
                chatPanelView = ChatPanelView.Create(flowCanvas);
                chatPanelView.SetCurrentPlayer(playerId);
                chatPanelView.OnCloseRequested += () => chatPanelView.Hide();
                chatPanelView.OnSendRequested += (channel, targetId, content) =>
                {
                    if (network != null && !string.IsNullOrWhiteSpace(content))
                        _ = network.SendChatSendAsync(channel, targetId, content, CancellationToken.None);
                };
                chatPanelView.OnHistoryRequested += (channel, count) =>
                {
                    if (network != null) _ = network.SendChatHistoryAsync(channel, count, CancellationToken.None);
                };
                chatPanelView.OnPrivateChatRequested += (targetId, targetName) =>
                {
                    hud?.AddLog($"[聊天] 切换到私聊 {targetName}");
                };
            }

            if (chatPanelView.gameObject.activeSelf)
            {
                chatPanelView.Hide();
                return;
            }

            chatPanelView.Show();
            // 打开时自动拉取世界频道历史
            if (network != null) _ = network.SendChatHistoryAsync(1, 50, CancellationToken.None);
            hud?.AddLog("[聊天] 打开聊天面板。");
        }

        private bool IsChatInputFocused()
        {
            if (chatPanelView == null || !chatPanelView.gameObject.activeSelf) return false;
            var current = EventSystem.current?.currentSelectedGameObject;
            if (current == null) return false;
            return current.GetComponent<UnityEngine.UI.InputField>() != null;
        }

        private void OnChatSendResult(GoChatSendResponse resp)
        {
            if (resp.code != 0)
                hud?.AddLog($"[聊天] 发送失败: code={resp.code}");
        }

        private void OnChatMessage(GoChatMessage msg)
        {
            chatPanelView?.AddMessage(msg.channel, msg.sender_name, msg.sender_id, msg.content, msg.timestamp, msg.target_id);
            // 不在聊天面板时，HUD 也显示世界频道消息
            if (msg.channel == 1 && (chatPanelView == null || !chatPanelView.gameObject.activeSelf))
                hud?.AddLog($"[世界] {msg.sender_name}: {msg.content}");
        }

        private void OnChatHistoryResult(GoChatHistoryResponse resp)
        {
            if (resp.code != 0) { hud?.AddLog($"[聊天] 历史加载失败: code={resp.code}"); return; }
            chatPanelView?.LoadHistory(resp.messages);
        }

        // ================================================================
        // 宠物面板
        // ================================================================

        private void TogglePetPanelView()
        {
            if (petPanelView == null)
            {
                petPanelView = PetPanelView.Create(flowCanvas);
                petPanelView.OnCloseRequested += () => petPanelView.Hide();
                petPanelView.OnSummonRequested += uid =>
                {
                    if (network != null) _ = network.SendPetSummonAsync(uid, CancellationToken.None);
                    hud?.AddLog($"[宠物] 召唤出战。");
                };
                petPanelView.OnRecallRequested += uid =>
                {
                    if (network != null) _ = network.SendPetRecallAsync(uid, CancellationToken.None);
                    hud?.AddLog($"[宠物] 召回。");
                };
                petPanelView.OnLevelUpRequested += uid =>
                {
                    if (network != null) _ = network.SendPetLevelUpAsync(uid, CancellationToken.None);
                    hud?.AddLog($"[宠物] 请求升级。");
                };
                petPanelView.OnEvolveRequested += uid =>
                {
                    if (network != null) _ = network.SendPetEvolveAsync(uid, CancellationToken.None);
                    hud?.AddLog($"[宠物] 请求进化。");
                };
                petPanelView.OnExploreRequested += (uid, duration) =>
                {
                    if (network != null) _ = network.SendPetExploreAsync(uid, duration, CancellationToken.None);
                    hud?.AddLog($"[宠物] 派遣探索 {duration} 分钟。");
                };
            }

            if (petPanelView.gameObject.activeSelf)
            {
                petPanelView.Hide();
                return;
            }

            petPanelView.ApplyNoPets();
            petPanelView.Show();
            hud?.AddLog("[宠物] 打开宠物面板。");
        }

        private void ToggleForgePanelView()
        {
            if (forgePanelView == null)
            {
                forgePanelView = ForgePanelView.Create(flowCanvas.transform);
                forgePanelView.OnCloseRequested += () => forgePanelView.Hide();
                forgePanelView.OnForgeRequested += (recipeId, materials) =>
                {
                    if (network != null)
                    {
                        _ = network.SendForgeAsync(recipeId, materials, CancellationToken.None);
                        hud?.AddLog($"[锻造] 请求锻造配方 {recipeId}...");
                    }
                };
            }

            if (forgePanelView.gameObject.activeSelf)
            {
                forgePanelView.Hide();
                return;
            }

            // 传入玩家等级和金币
            int level = localPlayerData != null ? localPlayerData.level : 1;
            long gold = localPlayerData != null ? localPlayerData.gold : 0;
            forgePanelView.SetPlayerInfo(level, gold);
            forgePanelView.Show();
            hud?.AddLog("[锻造] 打开锻造面板。");
        }

        private void SetGameplayEnabled(bool enabled)
        {
            gameplayActive = enabled;
            if (playerController != null)
            {
                playerController.SetInputEnabled(enabled);
            }
        }

        private void PopulateCharacterPanel()
        {
            if (characterPanelView == null)
            {
                return;
            }

            string name = "勇者";
            string classLabel = "战士";
            int level = 1;
            long exp = 0;
            int str = 0, agi = 0, intel = 0, con = 0, defVal = 0;
            int attrPoints = 0;
            int gold = 0, honor = 0;
            GoEquipmentData[] equipment = null;

            if (localPlayerData != null)
            {
                name = string.IsNullOrWhiteSpace(localPlayerData.name) ? $"勇者{localPlayerData.id}" : localPlayerData.name;
                classLabel = GetClassLabel((CharacterClass)localPlayerData.@class);
                level = localPlayerData.level;
                exp = localPlayerData.exp;
                str = localPlayerData.str;
                agi = localPlayerData.agi;
                intel = localPlayerData.@int;
                con = localPlayerData.con;
                defVal = localPlayerData.def;
                attrPoints = localPlayerData.attr_points;
                gold = (int)localPlayerData.gold;
                honor = localPlayerData.honor;
                equipment = localPlayerData.equipment;
            }

            // 基础属性（不含装备）
            int baseAtk = Mathf.RoundToInt(str * 2 + level * 5 + agi * 0.5f);
            int baseDef = Mathf.RoundToInt(defVal * 3 + con * 1 + level * 2);

            // 装备加成：遍历 equipment 数组，按强化公式 base*(1+level*0.1) 累加
            int equipAtk = 0, equipDef = 0, equipHp = 0;
            if (equipment != null)
            {
                foreach (var eq in equipment)
                {
                    if (eq == null) continue;
                    float mul = 1.0f + eq.strengthen_level * 0.1f;
                    equipAtk += Mathf.RoundToInt(eq.base_atk * mul);
                    equipDef += Mathf.RoundToInt(eq.base_def * mul);
                    equipHp += Mathf.RoundToInt(eq.base_hp * mul);
                }
            }

            int totalAtk = baseAtk + equipAtk;
            int totalDef = baseDef + equipDef;
            int hp = 100 + con * 20 + level * 50 + str * 5 + defVal * 3 + equipHp;
            int mp = 50 + intel * 15 + level * 20;
            int spd = agi;
            float crtRate = agi * 0.3f + str * 0.1f; // 百分比
            int power = totalAtk * 3 + totalDef * 2 + hp + level * 10;

            long expNext = 100L * level * level;
            characterPanelView.ApplyCharacter(
                name, classLabel, level, exp, expNext,
                hp, hp, mp, mp,
                baseAtk, equipAtk,
                baseDef, equipDef,
                equipHp,
                str, agi, intel, con, defVal,
                spd, crtRate, power,
                gold, honor, attrPoints);

            characterPanelView.ApplyEquipment(equipment);
        }

        private void OnEquipStrengthenResult(GoEquipStrengthenResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[装备] 强化失败: code={resp.code}");
                return;
            }
            var success = resp.is_success ? "成功" : "失败";
            hud?.AddLog($"[装备] 槽位{resp.slot} 强化{success}，等级→{resp.new_level}，消耗{resp.cost_gold}金币");

            // 更新本地装备数据以便面板立即刷新
            if (localPlayerData != null && resp.is_success)
            {
                if (localPlayerData.equipment != null)
                {
                    foreach (var eq in localPlayerData.equipment)
                    {
                        if (eq != null && eq.slot == resp.slot)
                        {
                            eq.strengthen_level = resp.new_level;
                            break;
                        }
                    }
                }
                localPlayerData.gold -= resp.cost_gold;
            }
            RefreshCharacterPanelIfVisible();
        }

        private void OnEquipEnchantResult(GoEquipEnchantResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[装备] 附魔失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[装备] 槽位{resp.slot} 附魔成功: {resp.attr_name}+{resp.attr_val}");

            // 更新本地装备附魔属性
            if (localPlayerData != null && localPlayerData.equipment != null)
            {
                foreach (var eq in localPlayerData.equipment)
                {
                    if (eq != null && eq.slot == resp.slot)
                    {
                        eq.enchant_attr = $"{resp.attr_name}+{resp.attr_val}";
                        break;
                    }
                }
            }
            RefreshCharacterPanelIfVisible();
        }

        private void OnEquipWearResult(GoEquipWearResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[装备] 穿戴失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[装备] 槽位{resp.slot} 穿戴成功");

            // 更新本地装备数据：构造简化的 EquipmentData
            if (localPlayerData != null && pendingWearEquipId > 0)
            {
                if (localPlayerData.equipment == null)
                    localPlayerData.equipment = new GoEquipmentData[8];

                // 查找模板数据
                int equipId = (int)pendingWearEquipId;
                if (ForgePanelView.EquipTemplates.TryGetValue(equipId, out var tmpl))
                {
                    var eq = new GoEquipmentData
                    {
                        slot = resp.slot,
                        equip_id = equipId,
                        name = tmpl.name,
                        quality = tmpl.quality,
                        strengthen_level = 0,
                        enchant_attr = "",
                        base_atk = tmpl.baseAtk,
                        base_def = tmpl.baseDef,
                        base_hp = tmpl.baseHp,
                        require_level = tmpl.requireLevel
                    };
                    // 填充技能特效
                    if (tmpl.skillEffects != null && tmpl.skillEffects.Length > 0)
                    {
                        eq.skill_effects = new GoSkillEffectData[tmpl.skillEffects.Length];
                        for (int j = 0; j < tmpl.skillEffects.Length; j++)
                        {
                            eq.skill_effects[j] = new GoSkillEffectData
                            {
                                skill_id = tmpl.skillEffects[j].skillId,
                                effect_type = tmpl.skillEffects[j].effectType,
                                value = tmpl.skillEffects[j].value,
                                desc = tmpl.skillEffects[j].desc
                            };
                        }
                    }
                    // 替换或添加
                    bool replaced = false;
                    for (int i = 0; i < localPlayerData.equipment.Length; i++)
                    {
                        if (localPlayerData.equipment[i] != null && localPlayerData.equipment[i].slot == resp.slot)
                        {
                            localPlayerData.equipment[i] = eq;
                            replaced = true;
                            break;
                        }
                    }
                    if (!replaced)
                    {
                        for (int i = 0; i < localPlayerData.equipment.Length; i++)
                        {
                            if (localPlayerData.equipment[i] == null)
                            {
                                localPlayerData.equipment[i] = eq;
                                break;
                            }
                        }
                    }
                }
                pendingWearEquipId = 0;
            }
            RefreshCharacterPanelIfVisible();
        }

        private void OnEquipUnloadResult(GoEquipUnloadResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[装备] 卸下失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[装备] 槽位{resp.slot} 卸下成功");

            // 从本地装备数据中移除该槽位
            if (localPlayerData != null && localPlayerData.equipment != null)
            {
                for (int i = 0; i < localPlayerData.equipment.Length; i++)
                {
                    if (localPlayerData.equipment[i] != null && localPlayerData.equipment[i].slot == resp.slot)
                    {
                        localPlayerData.equipment[i] = null;
                        break;
                    }
                }
            }
            RefreshCharacterPanelIfVisible();
        }

        private void OnForgeResult(GoForgeResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[锻造] 锻造失败: code={resp.code}");
                forgePanelView?.ApplyForgeResult(resp.code, "", 0);
                return;
            }
            var qualityNames = new[] { "白", "绿", "蓝", "紫", "橙", "红" };
            var q = resp.quality >= 0 && resp.quality < qualityNames.Length ? qualityNames[resp.quality] : "?";
            hud?.AddLog($"[锻造] 锻造成功: [{q}] {resp.result_name}");

            // 更新锻造面板结果
            forgePanelView?.ApplyForgeResult(resp.code, resp.result_name, resp.quality);

            // 扣除金币（本地预估，等服务端推送精确数据后覆盖）
            if (localPlayerData != null && forgePanelView != null)
            {
                var recipes = ForgePanelView.GetRecipeCosts();
                if (recipes != null)
                {
                    foreach (var kv in recipes)
                    {
                        if (kv.Key == resp.result_id && kv.Value > 0)
                        {
                            localPlayerData.gold -= kv.Value;
                            break;
                        }
                    }
                }
            }
            RefreshCharacterPanelIfVisible();
        }

        // --- PvP ---

        private void OnPvpAttackResult(GoPvpResult resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[PvP] 攻击失败: code={resp.code}");
                return;
            }
            var kill = resp.is_dead ? " 击杀！" : "";
            hud?.AddLog($"[PvP] 对{resp.target_id}造成{resp.damage}伤害{kill}");
            if (resp.gold_gain > 0) hud?.AddLog($"[PvP] 获得{resp.gold_gain}金币，{resp.honor_gain}荣誉");
        }

        private void OnRedNameList(GoRedNameInfo[] players)
        {
            if (players == null || players.Length == 0)
            {
                hud?.AddLog($"[PvP] 当前无红名玩家。");
                return;
            }
            hud?.AddLog($"[PvP] 红名玩家 {players.Length} 人:");
            foreach (var p in players)
            {
                hud?.AddLog($"  {p.name} 杀戮值:{p.kill_value} 赏金:{p.bounty}");
            }
        }

        private void OnBountyReward(GoBountyReward resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[悬赏] 追杀失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[悬赏] 完成悬赏！获得{resp.gold_gain}金币，{resp.honor_gain}荣誉");
        }

        private void OnRevengeResult(GoRevengeResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[复仇] 复仇失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[复仇] 对{resp.target_id}标记复仇成功");
        }

        // --- Pet ---

        private void OnPetSummonResult(GoPetSummonResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[宠物] 召唤失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[宠物] 召唤 {resp.pet.name} Lv.{resp.pet.level} 出战");
        }

        private void OnPetRecallResult(GoPetRecallResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[宠物] 收回失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[宠物] 宠物已收回");
        }

        private void OnPetLevelUpResult(GoPetLevelUpResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[宠物] 升级失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[宠物] 宠物升级至 Lv.{resp.level}");
        }

        private void OnPetEvolveResult(GoPetEvolveResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[宠物] 进阶失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[宠物] 进阶成功！新品质:{resp.new_quality}");
        }

        private void OnPetExploreResult(GoPetExploreResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[宠物] 探险派遣失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[宠物] 已派遣探险，预计完成时间戳:{resp.end_time}");
        }

        private void OnPetComposeResult(GoPetComposeResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[宠物] 合成失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[宠物] 合成成功！获得新宠物ID:{resp.pet_id} 品质:{resp.quality}");
        }

        private void OnPetEquipResult(GoPetEquipResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[宠物] 装备穿戴失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[宠物] 槽位{resp.slot} 装备成功");
        }

        private void OnPetUnequipResult(GoPetUnequipResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[宠物] 装备卸下失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[宠物] 槽位{resp.slot} 卸下成功");
        }

        // --- Trading ---

        private void OnTradeListResult(GoTradeListResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[交易] 加载失败: code={resp.code}");
                return;
            }
            if (resp.items == null || resp.items.Length == 0)
            {
                hud?.AddLog("[交易] 交易行暂无商品。");
                return;
            }
            hud?.AddLog($"[交易] 共{resp.total}件商品，显示{resp.items.Length}件:");
            for (var i = 0; i < Mathf.Min(resp.items.Length, 5); i++)
            {
                var item = resp.items[i];
                hud?.AddLog($"  {item.name} +{item.strengthen_level} 售价:{item.price} 卖家:{item.seller_name}");
            }
        }

        private void OnTradePublishResult(GoTradePublishResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[交易] 上架失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[交易] 上架成功，订单ID:{resp.order_id}");
        }

        private void OnTradeBuyResult(GoTradeBuyResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[交易] 购买失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[交易] 购买成功！");
        }

        private void OnTradeCancelResult(GoTradeCancelResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[交易] 取消失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[交易] 已取消上架");
        }

        // --- Shop ---

        private void OnShopListResult(GoShopListResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[商店] 加载失败: code={resp.code}");
                return;
            }
            if (resp.items == null || resp.items.Length == 0)
            {
                hud?.AddLog("[商店] 商店暂无商品。");
                return;
            }
            hud?.AddLog($"[商店] 商品列表 ({resp.items.Length}件):");
            for (var i = 0; i < Mathf.Min(resp.items.Length, 5); i++)
            {
                var item = resp.items[i];
                var currency = item.currency_type == 0 ? "金币" : "荣誉";
                hud?.AddLog($"  {item.name} 价格:{item.price}{currency} 库存:{item.stock}");
            }
        }

        private void OnShopBuyResult(GoShopBuyResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[商店] 购买失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[商店] 购买成功 x{resp.count}");
        }

        // --- Skill ---

        private void OnSkillLevelUpResult(GoSkillLevelUpResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[技能] 升级失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[技能] 技能{resp.skill_id} 升级至 Lv.{resp.new_level}");
        }

        private void OnSkillResetResult(GoSkillResetResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[技能] 重置失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[技能] 技能已重置，返还{resp.refund_points}技能点");
        }

        // --- Attribute ---

        private void OnAttrAssignResult(GoAttrAssignResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[属性] 分配失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[属性] {resp.attr}+{resp.val}，剩余{resp.attr_points}点");
            RefreshCharacterPanelIfVisible();
        }

        /// <summary>
        /// 如果角色面板当前可见，则重新填充数据（装备/属性变更后调用）。
        /// 注意：装备变更仅返回 slot/level 等简要信息，完整装备数据需要等服务端重新下发 PlayerData。
        /// 这里先用本地已有数据刷新，等后续 LoginResult 事件到达时会自动再次刷新。
        /// </summary>
        private void RefreshCharacterPanelIfVisible()
        {
            if (characterPanelView != null && characterPanelView.gameObject.activeSelf)
            {
                PopulateCharacterPanel();
            }
        }

        // --- Ranking ---

        private void OnRankingListResult(GoRankingListResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[排行] 加载失败: code={resp.code}");
                return;
            }
            if (resp.rankings == null || resp.rankings.Length == 0)
            {
                hud?.AddLog("[排行] 暂无排行数据。");
                return;
            }
            var typeName = resp.type == 0 ? "等级" : resp.type == 1 ? "战力" : resp.type == 2 ? "荣誉" : "未知";
            hud?.AddLog($"[排行] {typeName}排行 Top{resp.rankings.Length}:");
            for (var i = 0; i < Mathf.Min(resp.rankings.Length, 10); i++)
            {
                var r = resp.rankings[i];
                hud?.AddLog($"  #{r.rank} {r.name} {r.value}");
            }
        }

        private void ClearPanels()
        {
            activeLoginButton = null;

            if (loginPanel != null)
            {
                Destroy(loginPanel);
            }

            if (characterPanel != null)
            {
                Destroy(characterPanel);
            }

            if (teamPanelView != null)
            {
                Destroy(teamPanelView.gameObject);
                teamPanelView = null;
            }

            if (chatPanelView != null)
            {
                Destroy(chatPanelView.gameObject);
                chatPanelView = null;
            }

            if (petPanelView != null)
            {
                Destroy(petPanelView.gameObject);
                petPanelView = null;
            }

            if (forgePanelView != null)
            {
                Destroy(forgePanelView.gameObject);
                forgePanelView = null;
            }

            if (raidLobbyPanelView != null)
            {
                Destroy(raidLobbyPanelView.gameObject);
                raidLobbyPanelView = null;
            }

            if (raidHudOverlay != null)
            {
                Destroy(raidHudOverlay.gameObject);
                raidHudOverlay = null;
            }

            if (raidInventoryPanelView != null)
            {
                Destroy(raidInventoryPanelView.gameObject);
                raidInventoryPanelView = null;
            }

            isInRaid = false;
        }

        private GameObject CreatePanel(string name, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(flowCanvas.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private Text CreateText(Transform parent, string text, float size, Vector2 anchor, Vector2 sizeDelta)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<Text>();
            label.text = text;
            label.font = ChineseFontProvider.GetFont();
            label.fontSize = Mathf.RoundToInt(size);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.92f, 0.90f, 0.82f, 1f);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            SetAnchor(label.rectTransform, anchor, sizeDelta);
            return label;
        }

        private InputField CreateInput(Transform parent, string placeholder, string value, bool password, Vector2 anchor)
        {
            var inputObject = new GameObject(placeholder, typeof(RectTransform), typeof(Image), typeof(InputField));
            inputObject.transform.SetParent(parent, false);
            inputObject.GetComponent<Image>().color = new Color(0.15f, 0.17f, 0.16f, 1f);
            SetAnchor(inputObject.GetComponent<RectTransform>(), anchor, new Vector2(420f, 56f));

            var input = inputObject.GetComponent<InputField>();
            input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            input.text = value;

            var text = CreateInputText(inputObject.transform, string.Empty, new Color(0.92f, 0.90f, 0.82f, 1f));
            var placeholderText = CreateInputText(inputObject.transform, placeholder, new Color(0.62f, 0.66f, 0.62f, 1f));
            input.textComponent = text;
            input.placeholder = placeholderText;
            return input;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private Text CreateInputText(Transform parent, string text, Color color)
        {
            var textObject = new GameObject("Input Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<Text>();
            label.text = text;
            label.font = ChineseFontProvider.GetFont();
            label.fontSize = 24;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(18f, 0f);
            label.rectTransform.offsetMax = new Vector2(-18f, 0f);
            return label;
        }

        private Button CreateButton(Transform parent, string text, Vector2 anchor, Vector2 sizeDelta)
        {
            var buttonObject = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.30f, 1f);
            SetAnchor(buttonObject.GetComponent<RectTransform>(), anchor, sizeDelta);
            var label = CreateText(buttonObject.transform, text, 24, new Vector2(0.5f, 0.5f), sizeDelta);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return buttonObject.GetComponent<Button>();
        }

        private static void SetAnchor(RectTransform rectTransform, Vector2 anchor, Vector2 sizeDelta)
        {
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = sizeDelta;
        }

        // --- 战局 ---

        private void ToggleRaidLobbyPanelView()
        {
            if (raidLobbyPanelView != null)
            {
                Destroy(raidLobbyPanelView.gameObject);
                raidLobbyPanelView = null;
                return;
            }
            raidLobbyPanelView = RaidLobbyPanelView.Create(flowCanvas);
            raidLobbyPanelView.OnCloseRequested += ToggleRaidLobbyPanelView;
            raidLobbyPanelView.OnRaidEnterRequested += OnRaidEnterRequested;
            raidLobbyPanelView.OnStashRequested += () => network.SendRaidStashAsync(CancellationToken.None);
            _ = network.SendRaidMapListAsync(CancellationToken.None);
        }

        private void ToggleRaidInventoryPanelView()
        {
            if (raidInventoryPanelView != null)
            {
                Destroy(raidInventoryPanelView.gameObject);
                raidInventoryPanelView = null;
                return;
            }
            raidInventoryPanelView = RaidInventoryPanelView.Create(flowCanvas);
            raidInventoryPanelView.OnCloseRequested += ToggleRaidInventoryPanelView;
            raidInventoryPanelView.OnDiscardRequested += idx => network.SendRaidLootDiscardAsync(idx, CancellationToken.None);
        }

        private void OnRaidEnterRequested(int templateId)
        {
            _ = network.SendRaidEnterAsync(templateId, CancellationToken.None);
        }

        private void OnRaidEnterResult(GoRaidEnterResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[战局] 进入失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[战局] 进入 {resp.map_name}，限时 {resp.duration / 60} 分钟");
            isInRaid = true;

            // 存储容器、撤离点、区域数据
            _raidContainers = resp.loot_containers;
            _raidExtractionPoints = resp.extraction_points;
            _raidZones = resp.zones;
            _raidNearestContainerId = 0;
            if (resp.loot_containers != null)
            {
                hud?.AddLog($"[战局] 地图中有 {resp.loot_containers.Length} 个容器");
            }
            if (resp.extraction_points != null)
            {
                hud?.AddLog($"[战局] 共 {resp.extraction_points.Length} 个撤离点");
            }

            // 关闭大厅面板
            if (raidLobbyPanelView != null)
            {
                Destroy(raidLobbyPanelView.gameObject);
                raidLobbyPanelView = null;
            }

            // 创建战局HUD
            if (raidHudOverlay == null)
            {
                raidHudOverlay = RaidHudOverlay.Create(flowCanvas);
                raidHudOverlay.OnRaidInventoryRequested += ToggleRaidInventoryPanelView;
                raidHudOverlay.OnRaidLeaveRequested += () =>
                {
                    _ = network.SendRaidLeaveAsync(CancellationToken.None);
                    hud?.AddLog("[战局] 正在离开战局...");
                };
                raidHudOverlay.OnRaidExtractRequested += TryRaidExtract;
            }
            raidHudOverlay.SetTimer(resp.duration);
            raidHudOverlay.SetZone(resp.map_name, false);
            raidHudOverlay.Show();
        }

        private void OnRaidLeaveResult(GoRaidLeaveResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[战局] 离开失败: code={resp.code}");
                return;
            }
            hud?.AddLog("[战局] 已离开战局");
            ExitRaidState();
        }

        private void OnRaidTimer(GoRaidTimer timer)
        {
            raidHudOverlay?.SetTimer(timer.remaining);
        }

        private void OnRaidDeath(GoRaidDeath death)
        {
            hud?.AddLog($"[战局] 死亡 ({death.reason})，战利品已丢失");
            ExitRaidState();
        }

        private void OnRaidExtractResult(GoRaidExtractResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[战局] 撤离失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[战局] 撤离倒计时: {resp.timer} 秒");
            raidHudOverlay?.SetExtractDuration(resp.timer);
            raidHudOverlay?.SetExtraction(resp.timer, true);
        }

        private void OnRaidExtractProgress(GoRaidExtractProgress prog)
        {
            raidHudOverlay?.SetExtraction(prog.timer, prog.timer > 0);
            if (prog.timer <= 0)
            {
                hud?.AddLog("[战局] 撤离成功！战利品已保留");
                ExitRaidState();
            }
        }

        private void OnRaidLootOpenResult(GoRaidLootOpenResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[战局] 打开容器失败: code={resp.code}");
                return;
            }
            if (resp.items != null)
            {
                hud?.AddLog($"[战局] 容器中发现 {resp.items.Length} 件物品");
                for (int i = 0; i < resp.items.Length; i++)
                {
                    if (resp.items[i] == null) continue;
                    hud?.AddLog($"  #{i} {resp.items[i].name} x{resp.items[i].count}");
                }
            }
        }

        private void OnRaidLootPickupResult(GoRaidLootPickupResponse resp)
        {
            if (resp.code != 0)
                hud?.AddLog($"[战局] 拾取失败: code={resp.code}");
            else
                hud?.AddLog("[战局] 拾取成功");
        }

        private void OnRaidLootDiscardResult(GoRaidLootDiscardResponse resp)
        {
            if (resp.code != 0)
                hud?.AddLog($"[战局] 丢弃失败: code={resp.code}");
            else
                hud?.AddLog("[战局] 已丢弃物品");
        }

        private void OnRaidInventorySync(GoRaidInventory inv)
        {
            raidInventoryPanelView?.SetItems(inv.items);
        }

        private void OnRaidPvpResult(GoRaidPvpResult resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[战局PvP] 攻击失败: code={resp.code}");
                return;
            }
            hud?.AddLog($"[战局PvP] 对目标造成 {resp.damage} 伤害{(resp.is_dead ? "（击杀）" : "")}");
        }

        private void OnRaidMapListResult(GoRaidMapListResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[战局] 获取地图列表失败: code={resp.code}");
                return;
            }
            raidLobbyPanelView?.SetMaps(resp.maps);
        }

        private void ExitRaidState()
        {
            isInRaid = false;
            _raidContainers = null;
            _raidExtractionPoints = null;
            _raidZones = null;
            _raidNearestContainerId = 0;
            raidHudOverlay?.SetExtraction(0, false);
            raidHudOverlay?.Hide();
            if (raidInventoryPanelView != null)
            {
                Destroy(raidInventoryPanelView.gameObject);
                raidInventoryPanelView = null;
            }
        }

        // -------------------------------------------------------------------
        // Raid interaction methods
        // -------------------------------------------------------------------

        private void TryRaidExtract()
        {
            if (network == null || !network.IsConnected) return;

            if (_raidExtractionPoints == null || _raidExtractionPoints.Length == 0)
            {
                hud?.AddLog("[战局] 没有可用的撤离点。");
                return;
            }

            // 查找距离玩家最近的撤离点
            var playerPos = playerController != null ? playerController.transform.position : Vector3.zero;
            int nearestIdx = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < _raidExtractionPoints.Length; i++)
            {
                var ep = _raidExtractionPoints[i];
                var dist = Vector2.Distance(new Vector2(playerPos.x, playerPos.y), new Vector2(ep.x, ep.y));
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearestIdx = i;
                }
            }

            var point = _raidExtractionPoints[nearestIdx];
            _ = network.SendRaidExtractAsync(point.id, CancellationToken.None);
            hud?.AddLog($"[战局] 正在撤离点 {point.id} 开始撤离...");
        }

        private void TryOpenNearestContainer()
        {
            if (network == null || !network.IsConnected) return;

            if (_raidContainers == null || _raidContainers.Length == 0)
            {
                hud?.AddLog("[战局] 附近没有可搜索的容器。");
                return;
            }

            // 查找最近未开启的容器
            var playerPos = playerController != null ? playerController.transform.position : Vector3.zero;
            float bestDist = 1.5f; // 交互范围
            ulong nearestId = 0;
            for (int i = 0; i < _raidContainers.Length; i++)
            {
                var c = _raidContainers[i];
                if (c.opened) continue;
                var dist = Vector2.Distance(new Vector2(playerPos.x, playerPos.y), new Vector2(c.x, c.y));
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearestId = c.id;
                }
            }

            if (nearestId == 0)
            {
                hud?.AddLog("[战局] 附近没有可搜索的容器。");
                return;
            }

            _raidNearestContainerId = nearestId;
            _ = network.SendRaidLootOpenAsync(nearestId, CancellationToken.None);
            hud?.AddLog($"[战局] 正在搜索容器 {nearestId}...");
        }

        private void TryPickupContainerLoot()
        {
            if (network == null || !network.IsConnected) return;

            // 拾取当前容器中第一件物品
            _ = network.SendRaidLootPickupAsync(0, CancellationToken.None);
            hud?.AddLog("[战局] 正在拾取物品...");
        }

        private void OnRaidStashResult(GoRaidStashResponse resp)
        {
            if (resp.code != 0)
            {
                hud?.AddLog($"[仓库] 查询失败: code={resp.code}");
                return;
            }
            if (resp.items == null || resp.items.Length == 0)
            {
                hud?.AddLog("[仓库] 仓库为空。");
                return;
            }
            hud?.AddLog($"[仓库] 共 {resp.items.Length} 件物品:");
            for (int i = 0; i < Mathf.Min(resp.items.Length, 8); i++)
            {
                var item = resp.items[i];
                if (item == null) continue;
                var q = GetQualityLabel(item.quality);
                hud?.AddLog($"  [{q}] {item.name} x{item.count}");
            }
            if (resp.items.Length > 8)
                hud?.AddLog($"  ...等共 {resp.items.Length} 件");
        }

        private void OnRaidInfo(GoRaidInfo info)
        {
            if (_raidZones != null)
            {
                for (int i = 0; i < _raidZones.Length; i++)
                {
                    if (_raidZones[i].id == info.zone_id)
                    {
                        raidHudOverlay?.SetZone(_raidZones[i].name, info.pvp_flag);
                        return;
                    }
                }
            }
            raidHudOverlay?.SetZone($"区域{info.zone_id}", info.pvp_flag);
        }

        private static string GetQualityLabel(int quality)
        {
            return quality switch
            {
                1 => "精良",
                2 => "稀有",
                3 => "史诗",
                4 => "传说",
                _ => "普通"
            };
        }
    }
}
