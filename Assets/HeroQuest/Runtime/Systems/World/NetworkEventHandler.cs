using System.Collections.Generic;
using System.Threading;
using HeroQuest.Net.Go;
using HeroQuest.UI.HUD;
using UnityEngine;

namespace HeroQuest.Systems.World
{
    /// <summary>
    /// 网络事件分发器，负责订阅/取消订阅 NetworkManager 事件并路由到处理逻辑。
    /// 从 PrototypeGameplayFlow 中提取，减少主类的职责。
    /// </summary>
    public sealed class NetworkEventHandler
    {
        private readonly GameplayState _state;
        private readonly NetworkManager _network;
        private readonly GameplayHudController _hud;
        private readonly ICombatVisuals _visuals;

        public NetworkEventHandler(GameplayState state, ICombatVisuals visuals)
        {
            _state = state;
            _network = state.Network;
            _hud = state.Hud;
            _visuals = visuals;
        }

        public void Subscribe()
        {
            if (_network == null) return;

            _network.DamageReceived += OnDamageReceived;
            _network.MonsterRefresh += OnMonsterRefresh;
            _network.ServerBroadcast += OnBroadcast;
            _network.PlayerDie += OnPlayerDie;
            _network.PlayerRevive += OnPlayerRevive;
            _network.EnterDungeonResult += OnEnterDungeonResult;
            _network.CollectResult += OnCollectResult;
            _network.DungeonInfo += OnDungeonInfo;
            _network.BossSpawn += OnBossSpawn;
            _network.BossDie += OnBossDie;
            _network.SkillEffectReceived += OnSkillEffect;
            _network.AutoBattleResult += OnAutoBattleResult;
            _network.UseItemResult += OnUseItemResult;
            _network.InventorySync += OnInventorySync;
            _network.LoginResult += OnPlayerDataReceived;
            _network.CreatePlayerResult += OnPlayerDataReceived;
            _network.EquipStrengthenResult += OnEquipStrengthenResult;
            _network.EquipEnchantResult += OnEquipEnchantResult;
            _network.EquipWearResult += OnEquipWearResult;
            _network.EquipUnloadResult += OnEquipUnloadResult;
            _network.ForgeResult += OnForgeResult;
            _network.PvpAttackResult += OnPvpAttackResult;
            _network.RedNameListReceived += OnRedNameList;
            _network.BountyRewardReceived += OnBountyReward;
            _network.RevengeResult += OnRevengeResult;
            _network.PetSummonResult += OnPetSummonResult;
            _network.PetRecallResult += OnPetRecallResult;
            _network.PetLevelUpResult += OnPetLevelUpResult;
            _network.PetEvolveResult += OnPetEvolveResult;
            _network.PetExploreResult += OnPetExploreResult;
            _network.PetComposeResult += OnPetComposeResult;
            _network.PetEquipResult += OnPetEquipResult;
            _network.PetUnequipResult += OnPetUnequipResult;
            _network.TradeListResult += OnTradeListResult;
            _network.TradePublishResult += OnTradePublishResult;
            _network.TradeBuyResult += OnTradeBuyResult;
            _network.TradeCancelResult += OnTradeCancelResult;
            _network.ShopListResult += OnShopListResult;
            _network.ShopBuyResult += OnShopBuyResult;
            _network.SkillLevelUpResult += OnSkillLevelUpResult;
            _network.SkillResetResult += OnSkillResetResult;
            _network.AttrAssignResult += OnAttrAssignResult;
            _network.RankingListResult += OnRankingListResult;
            _network.TeamInfoResult += OnTeamInfoResult;
            _network.TeamInvitePushReceived += OnTeamInvitePush;
            _network.TeamInviteResultReceived += OnTeamInviteResult;
            _network.TeamLeaveResult += OnTeamLeaveResult;
            _network.TeamDismissResult += OnTeamDismissResult;
            _network.TeamKickResult += OnTeamKickResult;
            _network.TeamUpdateReceived += OnTeamUpdate;
            _network.ChatSendResult += OnChatSendResult;
            _network.ChatMessageReceived += OnChatMessage;
            _network.ChatHistoryResult += OnChatHistoryResult;
            _network.RaidEnterResult += OnRaidEnterResult;
            _network.RaidLeaveResult += OnRaidLeaveResult;
            _network.RaidTimer += OnRaidTimer;
            _network.RaidDeath += OnRaidDeath;
            _network.RaidExtractResult += OnRaidExtractResult;
            _network.RaidExtractProgress += OnRaidExtractProgress;
            _network.RaidLootOpenResult += OnRaidLootOpenResult;
            _network.RaidLootPickupResult += OnRaidLootPickupResult;
            _network.RaidLootDiscardResult += OnRaidLootDiscardResult;
            _network.RaidInventorySync += OnRaidInventorySync;
            _network.RaidPvpResult += OnRaidPvpResult;
            _network.RaidMapListResult += OnRaidMapListResult;
            _network.RaidStashResult += OnRaidStashResult;
            _network.RaidInfoReceived += OnRaidInfo;
        }

        public void Unsubscribe()
        {
            if (_network == null) return;

            _network.DamageReceived -= OnDamageReceived;
            _network.MonsterRefresh -= OnMonsterRefresh;
            _network.ServerBroadcast -= OnBroadcast;
            _network.PlayerDie -= OnPlayerDie;
            _network.PlayerRevive -= OnPlayerRevive;
            _network.EnterDungeonResult -= OnEnterDungeonResult;
            _network.CollectResult -= OnCollectResult;
            _network.DungeonInfo -= OnDungeonInfo;
            _network.BossSpawn -= OnBossSpawn;
            _network.BossDie -= OnBossDie;
            _network.SkillEffectReceived -= OnSkillEffect;
            _network.AutoBattleResult -= OnAutoBattleResult;
            _network.UseItemResult -= OnUseItemResult;
            _network.InventorySync -= OnInventorySync;
            _network.LoginResult -= OnPlayerDataReceived;
            _network.CreatePlayerResult -= OnPlayerDataReceived;
            _network.EquipStrengthenResult -= OnEquipStrengthenResult;
            _network.EquipEnchantResult -= OnEquipEnchantResult;
            _network.EquipWearResult -= OnEquipWearResult;
            _network.EquipUnloadResult -= OnEquipUnloadResult;
            _network.ForgeResult -= OnForgeResult;
            _network.PvpAttackResult -= OnPvpAttackResult;
            _network.RedNameListReceived -= OnRedNameList;
            _network.BountyRewardReceived -= OnBountyReward;
            _network.RevengeResult -= OnRevengeResult;
            _network.PetSummonResult -= OnPetSummonResult;
            _network.PetRecallResult -= OnPetRecallResult;
            _network.PetLevelUpResult -= OnPetLevelUpResult;
            _network.PetEvolveResult -= OnPetEvolveResult;
            _network.PetExploreResult -= OnPetExploreResult;
            _network.PetComposeResult -= OnPetComposeResult;
            _network.PetEquipResult -= OnPetEquipResult;
            _network.PetUnequipResult -= OnPetUnequipResult;
            _network.TradeListResult -= OnTradeListResult;
            _network.TradePublishResult -= OnTradePublishResult;
            _network.TradeBuyResult -= OnTradeBuyResult;
            _network.TradeCancelResult -= OnTradeCancelResult;
            _network.ShopListResult -= OnShopListResult;
            _network.ShopBuyResult -= OnShopBuyResult;
            _network.SkillLevelUpResult -= OnSkillLevelUpResult;
            _network.SkillResetResult -= OnSkillResetResult;
            _network.AttrAssignResult -= OnAttrAssignResult;
            _network.RankingListResult -= OnRankingListResult;
            _network.TeamInfoResult -= OnTeamInfoResult;
            _network.TeamInvitePushReceived -= OnTeamInvitePush;
            _network.TeamInviteResultReceived -= OnTeamInviteResult;
            _network.TeamLeaveResult -= OnTeamLeaveResult;
            _network.TeamDismissResult -= OnTeamDismissResult;
            _network.TeamKickResult -= OnTeamKickResult;
            _network.TeamUpdateReceived -= OnTeamUpdate;
            _network.ChatSendResult -= OnChatSendResult;
            _network.ChatMessageReceived -= OnChatMessage;
            _network.ChatHistoryResult -= OnChatHistoryResult;
            _network.RaidEnterResult -= OnRaidEnterResult;
            _network.RaidLeaveResult -= OnRaidLeaveResult;
            _network.RaidTimer -= OnRaidTimer;
            _network.RaidDeath -= OnRaidDeath;
            _network.RaidExtractResult -= OnRaidExtractResult;
            _network.RaidExtractProgress -= OnRaidExtractProgress;
            _network.RaidLootOpenResult -= OnRaidLootOpenResult;
            _network.RaidLootPickupResult -= OnRaidLootPickupResult;
            _network.RaidLootDiscardResult -= OnRaidLootDiscardResult;
            _network.RaidInventorySync -= OnRaidInventorySync;
            _network.RaidPvpResult -= OnRaidPvpResult;
            _network.RaidMapListResult -= OnRaidMapListResult;
            _network.RaidStashResult -= OnRaidStashResult;
            _network.RaidInfoReceived -= OnRaidInfo;
        }

        // --- Combat Events ---

        private void OnEnterDungeonResult(uint code, GoEnterDungeonResponse resp)
        {
            if (code != 0)
            {
                _hud?.AddLog($"[错误] 进入地下城失败: code={code}");
                return;
            }

            _state.CurrentLayer = resp.layer;
            _hud?.AddLog($"[地下城] 已进入第{resp.layer}层，区域: {resp.zone}");

            var wildSpawner = Object.FindFirstObjectByType<WildMonsterSpawner>();
            if (wildSpawner != null) wildSpawner.enabled = false;

            _state.VisibleMonsters.Clear();
            _visuals.ClearDungeonMonsterVisuals();

            if (resp.monsters != null)
            {
                foreach (var m in resp.monsters)
                {
                    _state.VisibleMonsters[m.id] = m;
                    _visuals.SpawnDungeonMonsterVisual(m);
                }
                _hud?.AddLog($"[地下城] 发现 {resp.monsters.Length} 只怪物。");
                _visuals.SelectNearestTarget();
            }
            if (resp.players != null)
            {
                _hud?.AddLog($"[地下城] 当前有 {resp.players.Length} 名玩家。");
            }
        }

        private void OnDamageReceived(GoDamage damage)
        {
            if (damage.target_id == _state.PlayerId)
            {
                var dmgLabel = damage.is_dead ? "受到致命伤害" : "受到伤害";
                _hud?.AddLog($"[战斗] {dmgLabel} {damage.damage}，剩余HP {damage.curr_hp}");

                if (_state.PlayerController != null)
                {
                    _hud?.ShowFloatingText(_state.PlayerController.transform.position, $"-{damage.damage}", new Color(1f, 0.2f, 0.1f, 1f));
                }

                if (_state.LocalPlayerData != null)
                {
                    _state.LocalPlayerData.hp = damage.curr_hp;
                    var hpPct = _state.LocalPlayerData.max_hp > 0 ? (float)_state.LocalPlayerData.hp / _state.LocalPlayerData.max_hp : 0f;
                    var mpPct = _state.LocalPlayerData.max_mp > 0 ? (float)_state.LocalPlayerData.mp / _state.LocalPlayerData.max_mp : 0f;
                    _hud?.SetVitals(hpPct, mpPct);
                    _hud?.SetVitalsText(damage.curr_hp, _state.LocalPlayerData.max_hp, _state.LocalPlayerData.mp, _state.LocalPlayerData.max_mp);
                }

                if (damage.is_dead) _visuals.HandlePlayerDeath();
                return;
            }

            var label = damage.is_dead ? "击杀" : "命中";
            _hud?.AddLog($"[战斗] {label} 目标 {damage.target_id}，伤害 {damage.damage}，剩余HP {damage.curr_hp}");

            if (_state.VisibleMonsters.TryGetValue(damage.target_id, out var dmgMonster))
            {
                var monsterPos = new Vector3((float)dmgMonster.x, (float)dmgMonster.y, 0f);
                _hud?.ShowFloatingText(monsterPos, damage.damage.ToString(), new Color(1f, 0.35f, 0.2f, 1f));
                _visuals.SpawnHitEffect(monsterPos);
                _visuals.PlayHitSound(damage.is_dead);
            }

            if (damage.pet_damage > 0)
            {
                var critTag = damage.pet_crit ? " 暴击！" : "";
                _hud?.AddLog($"[宠物] 造成 {damage.pet_damage} 伤害{critTag}");
            }

            if (damage.is_dead)
            {
                _hud?.AddLog($"[战斗] 目标 {damage.target_id} 已被消灭！");
                _state.VisibleMonsters.Remove(damage.target_id);
                _visuals.RemoveDungeonMonster(damage.target_id);
                if (damage.target_id == _state.SelectedTargetId)
                {
                    _state.SelectedTargetId = 0;
                    _visuals.SelectNearestTarget();
                }
            }
            else
            {
                if (_state.VisibleMonsters.TryGetValue(damage.target_id, out var monster))
                {
                    monster.hp = damage.curr_hp;
                    _visuals.UpdateMonsterHealth(damage.target_id, monster.hp, monster.max_hp);
                    if (damage.target_id == _state.SelectedTargetId)
                    {
                        _hud?.SetTarget(monster.name, 0, (float)monster.hp / monster.max_hp, monster.hp, monster.max_hp);
                    }
                }
            }
        }

        private void OnMonsterRefresh(GoMonsterData[] monsters)
        {
            if (monsters == null) return;

            var newIds = new HashSet<ulong>();
            foreach (var m in monsters) newIds.Add(m.id);

            _visuals.RemoveMissingMonsters(newIds);
            _state.VisibleMonsters.Clear();

            foreach (var m in monsters)
            {
                _state.VisibleMonsters[m.id] = m;
                _visuals.UpdateOrCreateMonster(m);
            }

            _hud?.AddLog($"[地下城] 怪物刷新，共 {monsters.Length} 只。");

            if (_state.SelectedTargetId == 0 || !_state.VisibleMonsters.ContainsKey(_state.SelectedTargetId))
            {
                _visuals.SelectNearestTarget();
            }
        }

        private void OnBroadcast(string content) => _hud?.AddLog($"[公告] {content}");

        private void OnPlayerDie(ulong pid)
        {
            if (pid == _state.PlayerId) _visuals.HandlePlayerDeath();
            else _hud?.AddLog($"[战斗] 玩家 {pid} 已阵亡。");
        }

        private void OnPlayerRevive(ulong pid)
        {
            if (pid == _state.PlayerId) _visuals.HandlePlayerRevive();
        }

        private void OnCollectResult(uint code, GoCollectResult result)
        {
            if (code != 0) { _hud?.AddLog($"[采集] 采集失败: code={code}"); return; }
            _hud?.AddLog($"[采集] 获得 {result.item_name} x{result.count}");
        }

        private void OnDungeonInfo(GoDungeonInfo info)
        {
            _state.CurrentLayer = info.current_layer;
            _hud?.AddLog($"[地下城] 当前第{info.current_layer}层，最高第{info.max_layer}层。");
        }

        private void OnBossSpawn(ulong bossId)
        {
            _hud?.AddLog($"[BOSS] BOSS出现！ID={bossId}");
            _state.SelectedTargetId = bossId;
            _hud?.SetTarget($"BOSS #{bossId}", 0, 1f, 0, 0);
        }

        private void OnBossDie(ulong bossId)
        {
            _hud?.AddLog($"[BOSS] BOSS已被击杀！ID={bossId}");
            _state.VisibleMonsters.Remove(bossId);
            _visuals.RemoveDungeonMonster(bossId);
            if (bossId == _state.SelectedTargetId)
            {
                _state.SelectedTargetId = 0;
                _visuals.SelectNearestTarget();
            }
        }

        private void OnSkillEffect(GoSkillEffect effect)
        {
            if (effect.targets == null) return;
            foreach (var t in effect.targets)
            {
                var dead = t.is_dead ? " 击杀！" : "";
                _hud?.AddLog($"[技能] 技能{effect.skill_id} 命中目标{t.target_id}，伤害{t.damage}{dead}");

                if (_state.VisibleMonsters.TryGetValue(t.target_id, out var skillMonster))
                {
                    var monsterPos = new Vector3((float)skillMonster.x, (float)skillMonster.y, 0f);
                    _hud?.ShowFloatingText(monsterPos, t.damage.ToString(), new Color(1f, 0.35f, 0.2f, 1f));
                    _visuals.SpawnHitEffect(monsterPos);
                }

                if (t.is_dead)
                {
                    _state.VisibleMonsters.Remove(t.target_id);
                    _visuals.RemoveDungeonMonster(t.target_id);
                    if (t.target_id == _state.SelectedTargetId)
                    {
                        _state.SelectedTargetId = 0;
                        _visuals.SelectNearestTarget();
                    }
                }
            }
        }

        private void OnAutoBattleResult(bool enabled)
        {
            _state.AutoBattleEnabled = enabled;
            _hud?.AddLog($"[自动战斗] {(enabled ? "已开启" : "已关闭")}");
        }

        private void OnUseItemResult(GoUseItemResponse resp)
        {
            if (resp.code != 0) { _hud?.AddLog($"[道具] 使用失败: code={resp.code}"); return; }
            _hud?.AddLog($"[道具] 使用道具 ID {resp.item_id}，当前HP {resp.hp}/{resp.max_hp}");
        }

        private void OnInventorySync(GoInventorySync sync)
        {
            if (sync.items != null) _visuals.RefreshConsumableBar(sync.items);
        }

        private void OnPlayerDataReceived(uint code, GoPlayerData player)
        {
            if (code != 0) { _hud?.AddLog($"[角色] 获取玩家数据失败: code={code}"); return; }
            _state.LocalPlayerData = player;
            _hud?.AddLog($"[角色] 已加载玩家数据: Lv.{player.level} {player.name}");
            _visuals.RefreshCharacterPanelIfVisible();
        }

        // --- Equipment Events ---

        private void OnEquipStrengthenResult(GoEquipStrengthenResponse resp)
        {
            if (resp.code != 0) { _hud?.AddLog($"[装备] 强化失败: code={resp.code}"); return; }
            _hud?.AddLog($"[装备] 强化成功！+{resp.new_level} (消耗 {resp.cost_gold} 金币)");
            _visuals.RefreshCharacterPanelIfVisible();
        }

        private void OnEquipEnchantResult(GoEquipEnchantResponse resp)
        {
            if (resp.code != 0) { _hud?.AddLog($"[装备] 附魔失败: code={resp.code}"); return; }
            _hud?.AddLog($"[装备] 附魔成功！");
            _visuals.RefreshCharacterPanelIfVisible();
        }

        private void OnEquipWearResult(GoEquipWearResponse resp)
        {
            if (resp.code != 0) { _hud?.AddLog($"[装备] 穿戴失败: code={resp.code}"); return; }
            _hud?.AddLog($"[装备] 穿戴成功！");
            _visuals.RefreshCharacterPanelIfVisible();
        }

        private void OnEquipUnloadResult(GoEquipUnloadResponse resp)
        {
            if (resp.code != 0) { _hud?.AddLog($"[装备] 卸下失败: code={resp.code}"); return; }
            _hud?.AddLog($"[装备] 卸下成功！");
            _visuals.RefreshCharacterPanelIfVisible();
        }

        private void OnForgeResult(GoForgeResponse resp)
        {
            if (resp.code != 0) { _hud?.AddLog($"[锻造] 失败: code={resp.code}"); return; }
            _hud?.AddLog($"[锻造] 成功！获得 {resp.result_name}");
            _visuals.RefreshCharacterPanelIfVisible();
        }

        // --- PvP Events ---

        private void OnPvpAttackResult(GoPvpResult resp) => _hud?.AddLog($"[PvP] 攻击目标 {resp.target_id}，伤害 {resp.damage}，金币 +{resp.gold_gain}");
        private void OnRedNameList(GoRedNameInfo[] players) { if (players != null) _hud?.AddLog($"[PvP] 红名列表: {players.Length} 人"); }
        private void OnBountyReward(GoBountyReward resp) => _hud?.AddLog($"[PvP] 悬赏奖励: 金币 +{resp.gold_gain}");
        private void OnRevengeResult(GoRevengeResponse resp) => _hud?.AddLog(resp.code == 0 ? "[PvP] 复仇成功" : $"[PvP] 复仇失败: code={resp.code}");

        // --- Pet Events ---

        private void OnPetSummonResult(GoPetSummonResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[宠物] 召唤成功" : $"[宠物] 召唤失败: code={resp.code}");
        private void OnPetRecallResult(GoPetRecallResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[宠物] 收回成功" : $"[宠物] 收回失败: code={resp.code}");
        private void OnPetLevelUpResult(GoPetLevelUpResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[宠物] 升级成功 Lv.{resp.level}" : $"[宠物] 升级失败: code={resp.code}");
        private void OnPetEvolveResult(GoPetEvolveResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[宠物] 进化成功" : $"[宠物] 进化失败: code={resp.code}");
        private void OnPetExploreResult(GoPetExploreResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[宠物] 探索完成" : $"[宠物] 探索失败: code={resp.code}");
        private void OnPetComposeResult(GoPetComposeResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[宠物] 合成成功" : $"[宠物] 合成失败: code={resp.code}");
        private void OnPetEquipResult(GoPetEquipResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[宠物] 装备成功" : $"[宠物] 装备失败: code={resp.code}");
        private void OnPetUnequipResult(GoPetUnequipResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[宠物] 卸装成功" : $"[宠物] 卸装失败: code={resp.code}");

        // --- Trading/Shop Events ---

        private void OnTradeListResult(GoTradeListResponse resp) { if (resp.items != null) _hud?.AddLog($"[交易] 列表: {resp.items.Length} 件"); }
        private void OnTradePublishResult(GoTradePublishResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[交易] 上架成功" : $"[交易] 上架失败: code={resp.code}");
        private void OnTradeBuyResult(GoTradeBuyResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[交易] 购买成功" : $"[交易] 购买失败: code={resp.code}");
        private void OnTradeCancelResult(GoTradeCancelResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[交易] 取消成功" : $"[交易] 取消失败: code={resp.code}");
        private void OnShopListResult(GoShopListResponse resp) { if (resp.items != null) _hud?.AddLog($"[商店] 列表: {resp.items.Length} 件"); }
        private void OnShopBuyResult(GoShopBuyResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[商店] 购买成功" : $"[商店] 购买失败: code={resp.code}");

        // --- Skill/Attribute Events ---

        private void OnSkillLevelUpResult(GoSkillLevelUpResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[技能] 升级成功 Lv.{resp.new_level}" : $"[技能] 升级失败: code={resp.code}");
        private void OnSkillResetResult(GoSkillResetResponse resp) => _hud?.AddLog(resp.code == 0 ? $"[技能] 重置成功" : $"[技能] 重置失败: code={resp.code}");
        private void OnAttrAssignResult(GoAttrAssignResponse resp) { if (resp.code == 0) _visuals.RefreshCharacterPanelIfVisible(); }

        // --- Ranking ---

        private void OnRankingListResult(GoRankingListResponse resp) { if (resp.rankings != null) _hud?.AddLog($"[排行] 列表: {resp.rankings.Length} 条"); }

        // --- Team Events ---

        private void OnTeamInfoResult(GoTeamInfoResponse resp) => _visuals.OnTeamInfoReceived(resp);
        private void OnTeamInvitePush(GoTeamInvitePush push) => _hud?.AddLog($"[队伍] 收到邀请: {push.inviter_name}");
        private void OnTeamInviteResult(GoTeamInviteResult resp) => _hud?.AddLog(resp.code == 0 ? "[队伍] 邀请成功" : $"[队伍] 邀请失败: code={resp.code}");
        private void OnTeamLeaveResult(GoTeamLeaveResponse resp) => _hud?.AddLog(resp.code == 0 ? "[队伍] 离开成功" : $"[队伍] 离开失败: code={resp.code}");
        private void OnTeamDismissResult(GoTeamDismissResponse resp) => _hud?.AddLog("[队伍] 队伍解散");
        private void OnTeamKickResult(GoTeamKickResponse resp) => _hud?.AddLog("[队伍] 成员被踢出");
        private void OnTeamUpdate(GoTeamUpdate update) => _visuals.OnTeamUpdateReceived(update);

        // --- Chat Events ---

        private void OnChatSendResult(GoChatSendResponse resp) { if (resp.code != 0) _hud?.AddLog($"[聊天] 发送失败: code={resp.code}"); }
        private void OnChatMessage(GoChatMessage msg) => _visuals.OnChatMessageReceived(msg);
        private void OnChatHistoryResult(GoChatHistoryResponse resp) => _visuals.OnChatHistoryReceived(resp);

        // --- Raid Events ---

        private void OnRaidEnterResult(GoRaidEnterResponse resp) => _visuals.OnRaidEnterResult(resp);
        private void OnRaidLeaveResult(GoRaidLeaveResponse resp) => _visuals.OnRaidLeaveResult(resp);
        private void OnRaidTimer(GoRaidTimer timer) => _visuals.OnRaidTimer(timer);
        private void OnRaidDeath(GoRaidDeath death) => _visuals.OnRaidDeath(death);
        private void OnRaidExtractResult(GoRaidExtractResponse resp) => _hud?.AddLog($"[战局] 撤离: code={resp.code}");
        private void OnRaidExtractProgress(GoRaidExtractProgress prog) => _visuals.OnRaidExtractProgress(prog);
        private void OnRaidLootOpenResult(GoRaidLootOpenResponse resp) => _visuals.OnRaidLootOpenResult(resp);
        private void OnRaidLootPickupResult(GoRaidLootPickupResponse resp) => _hud?.AddLog($"[战局] 拾取: code={resp.code}");
        private void OnRaidLootDiscardResult(GoRaidLootDiscardResponse resp) => _hud?.AddLog($"[战局] 丢弃: code={resp.code}");
        private void OnRaidInventorySync(GoRaidInventory inv) => _visuals.OnRaidInventorySync(inv);
        private void OnRaidPvpResult(GoRaidPvpResult resp) => _hud?.AddLog($"[战局] PvP: 攻击 {resp.target_id}，伤害 {resp.damage}");
        private void OnRaidMapListResult(GoRaidMapListResponse resp) => _visuals.OnRaidMapListReceived(resp);
        private void OnRaidStashResult(GoRaidStashResponse resp) => _visuals.OnRaidStashReceived(resp);
        private void OnRaidInfo(GoRaidInfo info) => _visuals.OnRaidInfoReceived(info);
    }

    /// <summary>
    /// 战斗视觉回调接口，由 PrototypeGameplayFlow 实现，
    /// 让 NetworkEventHandler 不直接依赖 MonoBehaviour。
    /// </summary>
    public interface ICombatVisuals
    {
        void SpawnDungeonMonsterVisual(GoMonsterData m);
        void ClearDungeonMonsterVisuals();
        void RemoveDungeonMonster(ulong id);
        void UpdateOrCreateMonster(GoMonsterData m);
        void UpdateMonsterHealth(ulong id, long hp, long maxHp);
        void RemoveMissingMonsters(HashSet<ulong> validIds);
        void SpawnHitEffect(Vector3 worldPos);
        void PlayHitSound(bool isDead);
        void SelectNearestTarget();
        void HandlePlayerDeath();
        void HandlePlayerRevive();
        void RefreshConsumableBar(GoItemCount[] items);
        void RefreshCharacterPanelIfVisible();
        void OnTeamInfoReceived(GoTeamInfoResponse resp);
        void OnTeamUpdateReceived(GoTeamUpdate update);
        void OnChatMessageReceived(GoChatMessage msg);
        void OnChatHistoryReceived(GoChatHistoryResponse resp);
        void OnRaidEnterResult(GoRaidEnterResponse resp);
        void OnRaidLeaveResult(GoRaidLeaveResponse resp);
        void OnRaidTimer(GoRaidTimer timer);
        void OnRaidDeath(GoRaidDeath death);
        void OnRaidExtractProgress(GoRaidExtractProgress prog);
        void OnRaidLootOpenResult(GoRaidLootOpenResponse resp);
        void OnRaidInventorySync(GoRaidInventory inv);
        void OnRaidMapListReceived(GoRaidMapListResponse resp);
        void OnRaidStashReceived(GoRaidStashResponse resp);
        void OnRaidInfoReceived(GoRaidInfo info);
    }
}
