using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HeroQuest.Net.Go
{
    /// <summary>
    /// 核心网络门面：封装 GoWebSocketConnection，提供高层 Send 方法和事件回调。
    /// 非 MonoBehaviour，通过 ServiceRegistry 注册/解析。
    /// 必须在 Unity 主线程的 Update 中调用 PumpMainThread() 以分发消息事件。
    /// </summary>
    public sealed class NetworkManager : IDisposable
    {
        private static readonly Uri DefaultServerUri = new("ws://localhost:8088/ws");

        private readonly GoWebSocketConnection connection;
        private readonly ConcurrentQueue<Action> mainThreadQueue = new();
        private CancellationTokenSource heartbeatCts;
        private bool disposed;

        public bool IsConnected => connection.IsConnected;

        // ================================================================
        // 事件声明 — 与 PrototypeGameplayFlow / PrototypeNetworkClient 对齐
        // ================================================================

        // Login / Auth
        public event Action<uint, GoPlayerData> LoginResult;
        public event Action<uint, GoPlayerData> CreatePlayerResult;

        // Dungeon
        public event Action<uint, GoEnterDungeonResponse> EnterDungeonResult;
        public event Action<uint> LeaveDungeonResult;
        public event Action<GoDungeonInfo> DungeonInfo;
        public event Action<GoMonsterData[]> MonsterRefresh;
        public event Action<GoEnterDungeonResponse> LayerTeleportResult;

        // Combat
        public event Action<GoDamage> DamageReceived;
        public event Action<ulong> BossSpawn;
        public event Action<ulong> BossDie;
        public event Action<GoSkillEffect> SkillEffectReceived;
        public event Action<ulong> PlayerDie;
        public event Action<ulong> PlayerRevive;
        public event Action<uint, GoCollectResult> CollectResult;
        public event Action<bool> AutoBattleResult;
        public event Action<GoUseItemResponse> UseItemResult;
        public event Action<GoInventorySync> InventorySync;

        // Equipment
        public event Action<GoEquipStrengthenResponse> EquipStrengthenResult;
        public event Action<GoEquipEnchantResponse> EquipEnchantResult;
        public event Action<GoEquipWearResponse> EquipWearResult;
        public event Action<GoEquipUnloadResponse> EquipUnloadResult;
        public event Action<GoForgeResponse> ForgeResult;

        // PvP
        public event Action<GoPvpResult> PvpAttackResult;
        public event Action<GoRedNameInfo[]> RedNameListReceived;
        public event Action<GoBountyReward> BountyRewardReceived;
        public event Action<GoRevengeResponse> RevengeResult;

        // Pet
        public event Action<GoPetSummonResponse> PetSummonResult;
        public event Action<GoPetRecallResponse> PetRecallResult;
        public event Action<GoPetLevelUpResponse> PetLevelUpResult;
        public event Action<GoPetEvolveResponse> PetEvolveResult;
        public event Action<GoPetExploreResponse> PetExploreResult;
        public event Action<GoPetComposeResponse> PetComposeResult;
        public event Action<GoPetEquipResponse> PetEquipResult;
        public event Action<GoPetUnequipResponse> PetUnequipResult;

        // Trading
        public event Action<GoTradeListResponse> TradeListResult;
        public event Action<GoTradePublishResponse> TradePublishResult;
        public event Action<GoTradeBuyResponse> TradeBuyResult;
        public event Action<GoTradeCancelResponse> TradeCancelResult;

        // Shop
        public event Action<GoShopListResponse> ShopListResult;
        public event Action<GoShopBuyResponse> ShopBuyResult;

        // Skill
        public event Action<GoSkillLevelUpResponse> SkillLevelUpResult;
        public event Action<GoSkillResetResponse> SkillResetResult;

        // Attribute
        public event Action<GoAttrAssignResponse> AttrAssignResult;

        // Ranking
        public event Action<GoRankingListResponse> RankingListResult;

        // Team
        public event Action<GoTeamInfoResponse> TeamInfoResult;
        public event Action<GoTeamInvitePush> TeamInvitePushReceived;
        public event Action<GoTeamInviteResult> TeamInviteResultReceived;
        public event Action<GoTeamLeaveResponse> TeamLeaveResult;
        public event Action<GoTeamDismissResponse> TeamDismissResult;
        public event Action<GoTeamKickResponse> TeamKickResult;
        public event Action<GoTeamUpdate> TeamUpdateReceived;

        // Chat
        public event Action<GoChatSendResponse> ChatSendResult;
        public event Action<GoChatMessage> ChatMessageReceived;
        public event Action<GoChatHistoryResponse> ChatHistoryResult;

        // Movement
        public event Action<ulong, double, double> PlayerMove;

        // System
        public event Action<string> ServerBroadcast;
        public event Action<string> Kicked;
        public event Action<Exception> ConnectionLost;

        // ================================================================
        // 构造 / 生命周期
        // ================================================================

        public NetworkManager()
        {
            connection = new GoWebSocketConnection();
            connection.MessageReceived += OnFrameReceived;
            connection.ConnectionError += OnConnectionError;
        }

        public async Task ConnectAsync(CancellationToken ct)
        {
            await connection.ConnectAsync(DefaultServerUri, ct).ConfigureAwait(false);
            heartbeatCts = new CancellationTokenSource();
            _ = HeartbeatLoopAsync(heartbeatCts.Token);
            Debug.Log($"[NetworkManager] 已连接到 {DefaultServerUri}");
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            heartbeatCts?.Cancel();
            heartbeatCts?.Dispose();
            heartbeatCts = null;

            connection.MessageReceived -= OnFrameReceived;
            connection.ConnectionError -= OnConnectionError;
            connection.Dispose();

            while (mainThreadQueue.TryDequeue(out _)) { }
        }

        /// <summary>
        /// 在 Unity 主线程 Update 中调用，分发消息事件到 UI / Gameplay 回调。
        /// </summary>
        public void PumpMainThread()
        {
            while (mainThreadQueue.TryDequeue(out var action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[NetworkManager] PumpMainThread error: {ex}");
                }
            }
        }

        // ================================================================
        // 发送方法
        // ================================================================

        // --- Login ---
        public Task SendLoginAsync(string token, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.Login, new GoLoginRequest { token = token }, ct);

        public Task SendCreatePlayerAsync(string token, string name, int classId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.CreatePlayer, new GoCreatePlayerRequest { token = token, name = name, @class = classId }, ct);

        // --- Dungeon ---
        public Task SendEnterDungeonAsync(int layer, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.EnterDungeon, new GoEnterDungeonRequest { layer = layer }, ct);

        public Task SendLeaveDungeonAsync(CancellationToken ct)
            => connection.SendAsync(GoMessageIds.LeaveDungeon, Array.Empty<byte>(), ct);

        public Task SendLayerTeleportAsync(int targetLayer, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.LayerTeleport, new GoLayerTeleportRequest { target_layer = targetLayer }, ct);

        // --- Combat ---
        public Task SendAttackAsync(ulong targetId, int skillId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.Attack, new GoAttackRequest { target_id = targetId, skill_id = skillId }, ct);

        public Task SendSkillCastAsync(int skillId, ulong targetId, double x, double y, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.SkillCast, new GoSkillCastRequest { skill_id = skillId, target_id = targetId, x = x, y = y }, ct);

        public Task SendCollectResourceAsync(ulong resourceId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.CollectResource, new GoCollectResourceRequest { resource_id = resourceId }, ct);

        public Task SendAutoBattleAsync(bool enable, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.AutoBattle, new GoAutoBattleRequest { enable = enable }, ct);

        public Task SendUseItemAsync(uint itemId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.UseItem, new GoUseItemRequest { item_id = itemId }, ct);

        // --- Movement ---
        public Task SendMoveAsync(double x, double y, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.Move, new GoMoveRequest { x = x, y = y }, ct);

        // --- Equipment ---
        public Task SendEquipStrengthenAsync(int slot, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.EquipStrengthen, new GoEquipStrengthenRequest { slot = slot }, ct);

        public Task SendEquipEnchantAsync(int slot, ulong materialId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.EquipEnchant, new GoEquipEnchantRequest { slot = slot, material_id = materialId }, ct);

        public Task SendEquipWearAsync(int slot, ulong equipId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.EquipWear, new GoEquipWearRequest { slot = slot, equip_id = equipId }, ct);

        public Task SendEquipUnloadAsync(int slot, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.EquipUnload, new GoEquipUnloadRequest { slot = slot }, ct);

        public Task SendForgeAsync(ulong recipeId, ulong[] materials, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.Forge, new GoForgeRequest { recipe_id = recipeId, materials = materials }, ct);

        // --- PvP ---
        public Task SendPvpAttackAsync(ulong targetId, int skillId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.PvpAttack, new GoPvpAttackRequest { target_id = targetId, skill_id = skillId }, ct);

        public Task SendBountyHuntAsync(ulong targetId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.BountyHunt, new GoBountyHuntRequest { target_id = targetId }, ct);

        public Task SendRevengeAsync(ulong targetId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.Revenge, new GoRevengeRequest { target_id = targetId }, ct);

        // --- Pet ---
        public Task SendPetSummonAsync(ulong petUid, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.PetSummon, new GoPetSummonRequest { pet_uid = petUid }, ct);

        public Task SendPetRecallAsync(ulong petUid, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.PetRecall, new GoPetRecallRequest { pet_uid = petUid }, ct);

        public Task SendPetLevelUpAsync(ulong petUid, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.PetLevelUp, new GoPetLevelUpRequest { pet_uid = petUid }, ct);

        public Task SendPetEvolveAsync(ulong petUid, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.PetEvolve, new GoPetEvolveRequest { pet_uid = petUid }, ct);

        public Task SendPetExploreAsync(ulong petUid, int duration, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.PetExplore, new GoPetExploreRequest { pet_uid = petUid, duration = duration }, ct);

        public Task SendPetComposeAsync(ulong[] petUids, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.PetCompose, new GoPetComposeRequest { pet_uids = petUids }, ct);

        public Task SendPetEquipAsync(ulong petUid, int slot, int equipId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.PetEquip, new GoPetEquipRequest { pet_uid = petUid, slot = slot, equip_id = equipId }, ct);

        public Task SendPetUnequipAsync(ulong petUid, int slot, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.PetUnequip, new GoPetUnequipRequest { pet_uid = petUid, slot = slot }, ct);

        // --- Shop ---
        public Task SendShopListAsync(int type, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.ShopList, new GoShopListRequest { type = type }, ct);

        public Task SendShopBuyAsync(ulong itemId, int count, int currencyType, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.ShopBuy, new GoShopBuyRequest { item_id = itemId, count = count, currency_type = currencyType }, ct);

        // --- Trading ---
        public Task SendTradeListAsync(int category, int page, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.TradeList, new GoTradeListRequest { category = category, page = page }, ct);

        public Task SendTradePublishAsync(int slot, long price, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.TradePublish, new GoTradePublishRequest { slot = slot, price = price }, ct);

        public Task SendTradeBuyAsync(ulong orderId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.TradeBuy, new GoTradeBuyRequest { order_id = orderId }, ct);

        public Task SendTradeCancelAsync(ulong orderId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.TradeCancel, new GoTradeCancelRequest { order_id = orderId }, ct);

        // --- Skill ---
        public Task SendSkillLevelUpAsync(int skillId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.SkillLevelUp, new GoSkillLevelUpRequest { skill_id = skillId }, ct);

        public Task SendSkillResetAsync(CancellationToken ct)
            => connection.SendAsync(GoMessageIds.SkillReset, Array.Empty<byte>(), ct);

        // --- Attribute ---
        public Task SendAttrAssignAsync(string attr, int val, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.AttrAssign, new GoAttrAssignRequest { attr = attr, val = val }, ct);

        // --- Ranking ---
        public Task SendRankingListAsync(int type, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.RankingList, new GoRankingListRequest { type = type }, ct);

        // --- Chat ---
        public Task SendChatSendAsync(int channel, ulong targetId, string content, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.ChatSend, new GoChatSendRequest { channel = channel, target_id = targetId, content = content }, ct);

        public Task SendChatHistoryAsync(int channel, int count, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.ChatHistory, new GoChatHistoryRequest { channel = channel, count = count }, ct);

        // --- Team ---
        public Task SendTeamCreateAsync(CancellationToken ct)
            => connection.SendAsync(GoMessageIds.TeamCreate, Array.Empty<byte>(), ct);

        public Task SendTeamInviteAsync(ulong targetId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.TeamInvite, new GoTeamInviteRequest { target_id = targetId }, ct);

        public Task SendTeamInviteReplyAsync(ulong teamId, bool accept, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.TeamInviteReply, new GoTeamInviteReplyRequest { team_id = teamId, accept = accept }, ct);

        public Task SendTeamLeaveAsync(CancellationToken ct)
            => connection.SendAsync(GoMessageIds.TeamLeave, Array.Empty<byte>(), ct);

        public Task SendTeamDismissAsync(CancellationToken ct)
            => connection.SendAsync(GoMessageIds.TeamDismiss, Array.Empty<byte>(), ct);

        public Task SendTeamKickAsync(ulong targetId, CancellationToken ct)
            => connection.SendJsonAsync(GoMessageIds.TeamKick, new GoTeamKickRequest { target_id = targetId }, ct);

        public Task SendTeamQueryAsync(CancellationToken ct)
            => connection.SendAsync(GoMessageIds.TeamQuery, Array.Empty<byte>(), ct);

        // ================================================================
        // 接收分发
        // ================================================================

        private void OnFrameReceived(GoProtocolFrame frame)
        {
            // 所有事件入队，由主线程 PumpMainThread 分发
            mainThreadQueue.Enqueue(() => DispatchFrame(frame));
        }

        private void DispatchFrame(GoProtocolFrame frame)
        {
            try
            {
                switch (frame.MessageId)
                {
                    // --- Login ---
                    case GoMessageIds.LoginResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoLoginResponse>(frame);
                        LoginResult?.Invoke(r.code, r.player);
                        break;
                    }
                    case GoMessageIds.CreatePlayerResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoCreatePlayerResponse>(frame);
                        CreatePlayerResult?.Invoke(r.code, r.player);
                        break;
                    }

                    // --- Dungeon ---
                    case GoMessageIds.EnterDungeonResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoEnterDungeonResponse>(frame);
                        EnterDungeonResult?.Invoke(r.code, r);
                        break;
                    }
                    case GoMessageIds.LeaveDungeonResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoLeaveDungeonResponse>(frame);
                        LeaveDungeonResult?.Invoke(r.code);
                        break;
                    }
                    case GoMessageIds.DungeonInfo:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoDungeonInfo>(frame);
                        DungeonInfo?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.MonsterRefresh:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoMonsterRefresh>(frame);
                        MonsterRefresh?.Invoke(r.monsters);
                        break;
                    }
                    case GoMessageIds.LayerTeleportResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoEnterDungeonResponse>(frame);
                        LayerTeleportResult?.Invoke(r);
                        break;
                    }

                    // --- Combat ---
                    case GoMessageIds.Damage:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoDamage>(frame);
                        DamageReceived?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.BossSpawn:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoBossSpawn>(frame);
                        BossSpawn?.Invoke(r.boss_id);
                        break;
                    }
                    case GoMessageIds.BossDie:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoBossDie>(frame);
                        BossDie?.Invoke(r.boss_id);
                        break;
                    }
                    case GoMessageIds.SkillEffect:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoSkillEffect>(frame);
                        SkillEffectReceived?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.PlayerDie:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPlayerDie>(frame);
                        PlayerDie?.Invoke(r.player_id);
                        break;
                    }
                    case GoMessageIds.PlayerRevive:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPlayerRevive>(frame);
                        PlayerRevive?.Invoke(r.player_id);
                        break;
                    }
                    case GoMessageIds.CollectResult:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoCollectResult>(frame);
                        CollectResult?.Invoke(r.code, r);
                        break;
                    }
                    case GoMessageIds.AutoBattleResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoAutoBattleResponse>(frame);
                        AutoBattleResult?.Invoke(r.enable);
                        break;
                    }
                    case GoMessageIds.UseItemResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoUseItemResponse>(frame);
                        UseItemResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.InventorySync:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoInventorySync>(frame);
                        InventorySync?.Invoke(r);
                        break;
                    }

                    // --- Equipment ---
                    case GoMessageIds.EquipStrengthenResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoEquipStrengthenResponse>(frame);
                        EquipStrengthenResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.EquipEnchantResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoEquipEnchantResponse>(frame);
                        EquipEnchantResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.EquipWearResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoEquipWearResponse>(frame);
                        EquipWearResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.EquipUnloadResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoEquipUnloadResponse>(frame);
                        EquipUnloadResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.ForgeResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoForgeResponse>(frame);
                        ForgeResult?.Invoke(r);
                        break;
                    }

                    // --- PvP ---
                    case GoMessageIds.PvpResult:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPvpResult>(frame);
                        PvpAttackResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.RedNameList:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoRedNameList>(frame);
                        RedNameListReceived?.Invoke(r.players);
                        break;
                    }
                    case GoMessageIds.BountyReward:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoBountyReward>(frame);
                        BountyRewardReceived?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.RevengeResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoRevengeResponse>(frame);
                        RevengeResult?.Invoke(r);
                        break;
                    }

                    // --- Movement ---
                    case GoMessageIds.PlayerMove:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPlayerMove>(frame);
                        PlayerMove?.Invoke(r.player_id, r.x, r.y);
                        break;
                    }

                    // --- Pet ---
                    case GoMessageIds.PetSummonResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPetSummonResponse>(frame);
                        PetSummonResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.PetRecallResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPetRecallResponse>(frame);
                        PetRecallResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.PetLevelUp:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPetLevelUpResponse>(frame);
                        PetLevelUpResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.PetEvolveResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPetEvolveResponse>(frame);
                        PetEvolveResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.PetExploreResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPetExploreResponse>(frame);
                        PetExploreResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.PetComposeResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPetComposeResponse>(frame);
                        PetComposeResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.PetEquipResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPetEquipResponse>(frame);
                        PetEquipResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.PetUnequipResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoPetUnequipResponse>(frame);
                        PetUnequipResult?.Invoke(r);
                        break;
                    }

                    // --- Trading ---
                    case GoMessageIds.TradeListResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTradeListResponse>(frame);
                        TradeListResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.TradePublishResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTradePublishResponse>(frame);
                        TradePublishResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.TradeBuyResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTradeBuyResponse>(frame);
                        TradeBuyResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.TradeCancelResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTradeCancelResponse>(frame);
                        TradeCancelResult?.Invoke(r);
                        break;
                    }

                    // --- Shop ---
                    case GoMessageIds.ShopListResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoShopListResponse>(frame);
                        ShopListResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.ShopBuyResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoShopBuyResponse>(frame);
                        ShopBuyResult?.Invoke(r);
                        break;
                    }

                    // --- Skill ---
                    case GoMessageIds.SkillLevelUpResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoSkillLevelUpResponse>(frame);
                        SkillLevelUpResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.SkillResetResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoSkillResetResponse>(frame);
                        SkillResetResult?.Invoke(r);
                        break;
                    }

                    // --- Attribute ---
                    case GoMessageIds.AttrAssignResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoAttrAssignResponse>(frame);
                        AttrAssignResult?.Invoke(r);
                        break;
                    }

                    // --- Ranking ---
                    case GoMessageIds.RankingListResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoRankingListResponse>(frame);
                        RankingListResult?.Invoke(r);
                        break;
                    }

                    // --- Team ---
                    case GoMessageIds.TeamInfoResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTeamInfoResponse>(frame);
                        TeamInfoResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.TeamInvitePush:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTeamInvitePush>(frame);
                        TeamInvitePushReceived?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.TeamInviteResult:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTeamInviteResult>(frame);
                        TeamInviteResultReceived?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.TeamLeaveResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTeamLeaveResponse>(frame);
                        TeamLeaveResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.TeamDismissResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTeamDismissResponse>(frame);
                        TeamDismissResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.TeamKickResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTeamKickResponse>(frame);
                        TeamKickResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.TeamUpdate:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoTeamUpdate>(frame);
                        TeamUpdateReceived?.Invoke(r);
                        break;
                    }

                    // --- Chat ---
                    case GoMessageIds.ChatSendResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoChatSendResponse>(frame);
                        ChatSendResult?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.ChatMessage:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoChatMessage>(frame);
                        ChatMessageReceived?.Invoke(r);
                        break;
                    }
                    case GoMessageIds.ChatHistoryResponse:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoChatHistoryResponse>(frame);
                        ChatHistoryResult?.Invoke(r);
                        break;
                    }

                    // --- System ---
                    case GoMessageIds.Broadcast:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoBroadcast>(frame);
                        ServerBroadcast?.Invoke(r.content);
                        break;
                    }
                    case GoMessageIds.Heartbeat:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoHeartbeat>(frame);
                        long rtt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - r.timestamp;
                        Debug.Log($"[NetworkManager] 心跳 RTT={rtt}ms");
                        break;
                    }
                    case GoMessageIds.Kick:
                    {
                        var r = GoBinaryProtocolCodec.DecodeJson<GoKick>(frame);
                        Debug.LogWarning($"[NetworkManager] 被踢下线: {r.reason}");
                        Kicked?.Invoke(r.reason);
                        break;
                    }

                    default:
                        Debug.LogWarning($"[NetworkManager] 未处理的消息 ID: {frame.MessageId}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkManager] 消息分发异常 MsgID={frame.MessageId}: {ex}");
            }
        }

        // ================================================================
        // 心跳
        // ================================================================

        private async Task HeartbeatLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(15000, ct).ConfigureAwait(false);
                    if (connection.IsConnected)
                    {
                        var hb = new GoHeartbeat { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };
                        await connection.SendJsonAsync(GoMessageIds.Heartbeat, hb, ct).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkManager] 心跳循环异常: {ex}");
            }
        }

        // ================================================================
        // 连接错误处理
        // ================================================================

        private void OnConnectionError(Exception ex)
        {
            mainThreadQueue.Enqueue(() =>
            {
                Debug.LogError($"[NetworkManager] 连接错误: {ex.Message}");
                ConnectionLost?.Invoke(ex);
            });
        }
    }
}
