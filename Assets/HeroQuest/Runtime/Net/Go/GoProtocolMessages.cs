using System;
using System.Collections.Generic;

namespace HeroQuest.Net.Go
{
    [Serializable]
    public sealed class GoLoginRequest
    {
        public string token;
    }

    [Serializable]
    public sealed class GoLoginResponse
    {
        public uint code;
        public GoPlayerData player;
    }

    [Serializable]
    public sealed class GoCreatePlayerRequest
    {
        public string token;
        public string name;
        public int @class;
    }

    [Serializable]
    public sealed class GoCreatePlayerResponse
    {
        public uint code;
        public GoPlayerData player;
    }

    [Serializable]
    public sealed class GoPlayerData
    {
        public ulong id;
        public string name;
        public int @class;
        public int level;
        public long exp;
        public long gold;
        public int honor;
        public int kill_value;
        public int str;
        public int agi;
        public int @int;
        public int con;
        public int attr_points;
        public int max_layer;
        public long hp;
        public long max_hp;
        public long mp;
        public long max_mp;
        public Dictionary<uint, int> items;
    }

    [Serializable]
    public sealed class GoEnterDungeonRequest
    {
        public int layer;
    }

    [Serializable]
    public sealed class GoEnterDungeonResponse
    {
        public uint code;
        public int layer;
        public string zone;
        public GoMonsterData[] monsters;
        public GoPlayerBrief[] players;
        public GoResourceData[] resources;
    }

    [Serializable]
    public sealed class GoLayerTeleportRequest
    {
        public int target_layer;
    }

    [Serializable]
    public sealed class GoDungeonInfo
    {
        public int current_layer;
        public int max_layer;
    }

    [Serializable]
    public sealed class GoMonsterRefresh
    {
        public GoMonsterData[] monsters;
    }

    [Serializable]
    public sealed class GoMonsterData
    {
        public ulong id;
        public string name;
        public long hp;
        public long max_hp;
        public double x;
        public double y;
    }

    [Serializable]
    public sealed class GoPlayerBrief
    {
        public ulong id;
        public string name;
        public int @class;
        public int level;
        public long hp;
        public long max_hp;
        public double x;
        public double y;
        public int pet_id;
    }

    [Serializable]
    public sealed class GoResourceData
    {
        public ulong id;
        public int type;
        public string name;
        public double x;
        public double y;
        public bool harvested;
    }

    [Serializable]
    public sealed class GoMoveRequest
    {
        public double x;
        public double y;
    }

    [Serializable]
    public sealed class GoPlayerMove
    {
        public ulong player_id;
        public double x;
        public double y;
    }

    [Serializable]
    public sealed class GoAttackRequest
    {
        public ulong target_id;
        public int skill_id;
    }

    [Serializable]
    public sealed class GoDamage
    {
        public ulong target_id;
        public long damage;
        public long curr_hp;
        public bool is_dead;
        public long pet_damage;
        public bool pet_crit;
        public bool pet_dead;
    }

    [Serializable]
    public sealed class GoCollectResourceRequest
    {
        public ulong resource_id;
    }

    [Serializable]
    public sealed class GoCollectResult
    {
        public uint code;
        public ulong resource_id;
        public ulong item_id;
        public string item_name;
        public int count;
    }

    [Serializable]
    public sealed class GoHeartbeat
    {
        public long timestamp;
    }

    [Serializable]
    public sealed class GoBroadcast
    {
        public int type;
        public string content;
    }

    [Serializable]
    public sealed class GoKick
    {
        public string reason;
    }

    // --- Auto Battle ---

    [Serializable]
    public sealed class GoAutoBattleRequest
    {
        public bool enable;
    }

    [Serializable]
    public sealed class GoAutoBattleResponse
    {
        public uint code;
        public bool enable;
    }

    // --- UseItem ---

    [Serializable]
    public sealed class GoUseItemRequest
    {
        public uint item_id;
    }

    [Serializable]
    public sealed class GoUseItemResponse
    {
        public uint code;
        public uint item_id;
        public int count;
        public long hp;
        public long max_hp;
        public long mp;
        public long max_mp;
    }

    [Serializable]
    public sealed class GoInventorySync
    {
        public Dictionary<uint, int> items;
    }

    // --- Pet Equipment ---

    [Serializable]
    public sealed class GoPetEquipRequest
    {
        public ulong pet_uid;
        public int slot;
        public int equip_id;
    }

    [Serializable]
    public sealed class GoPetEquipResponse
    {
        public uint code;
        public ulong pet_uid;
        public int slot;
    }

    [Serializable]
    public sealed class GoPetUnequipRequest
    {
        public ulong pet_uid;
        public int slot;
    }

    [Serializable]
    public sealed class GoPetUnequipResponse
    {
        public uint code;
        public ulong pet_uid;
        public int slot;
    }

    // --- Skill Cast ---

    [Serializable]
    public sealed class GoSkillCastRequest
    {
        public int skill_id;
        public ulong target_id;
        public double x;
        public double y;
    }

    [Serializable]
    public sealed class GoSkillEffect
    {
        public ulong caster_id;
        public int skill_id;
        public double x;
        public double y;
        public GoDamageInfo[] targets;
    }

    [Serializable]
    public sealed class GoDamageInfo
    {
        public ulong target_id;
        public long damage;
        public long curr_hp;
        public bool is_dead;
    }

    // --- Player Die (extended) ---

    [Serializable]
    public sealed class GoPlayerDieInfo
    {
        public ulong player_id;
        public string killer_name;
    }

    // --- Equipment ---

    [Serializable]
    public sealed class GoEquipStrengthenRequest
    {
        public int slot;
    }

    [Serializable]
    public sealed class GoEquipStrengthenResponse
    {
        public uint code;
        public int slot;
        public int new_level;
        public long cost_gold;
        public bool is_success;
    }

    [Serializable]
    public sealed class GoEquipEnchantRequest
    {
        public int slot;
        public ulong material_id;
    }

    [Serializable]
    public sealed class GoEquipEnchantResponse
    {
        public uint code;
        public int slot;
        public string attr_name;
        public int attr_val;
    }

    [Serializable]
    public sealed class GoEquipWearRequest
    {
        public int slot;
        public ulong equip_id;
    }

    [Serializable]
    public sealed class GoEquipWearResponse
    {
        public uint code;
        public int slot;
    }

    [Serializable]
    public sealed class GoEquipUnloadRequest
    {
        public int slot;
    }

    [Serializable]
    public sealed class GoEquipUnloadResponse
    {
        public uint code;
        public int slot;
    }

    [Serializable]
    public sealed class GoForgeRequest
    {
        public ulong recipe_id;
        public ulong[] materials;
    }

    [Serializable]
    public sealed class GoForgeResponse
    {
        public uint code;
        public ulong result_id;
        public string result_name;
        public int quality;
    }

    // --- PvP ---

    [Serializable]
    public sealed class GoPvpAttackRequest
    {
        public ulong target_id;
        public int skill_id;
    }

    [Serializable]
    public sealed class GoPvpResult
    {
        public uint code;
        public ulong attacker_id;
        public ulong target_id;
        public long damage;
        public long gold_gain;
        public int honor_gain;
        public bool is_dead;
    }

    [Serializable]
    public sealed class GoRedNameList
    {
        public GoRedNameInfo[] players;
    }

    [Serializable]
    public sealed class GoRedNameInfo
    {
        public ulong player_id;
        public string name;
        public int kill_value;
        public long bounty;
    }

    [Serializable]
    public sealed class GoBountyHuntRequest
    {
        public ulong target_id;
    }

    [Serializable]
    public sealed class GoBountyReward
    {
        public uint code;
        public ulong target_id;
        public long gold_gain;
        public int honor_gain;
    }

    [Serializable]
    public sealed class GoRevengeRequest
    {
        public ulong target_id;
    }

    [Serializable]
    public sealed class GoRevengeResponse
    {
        public uint code;
        public ulong target_id;
    }

    // --- Pet ---

    [Serializable]
    public sealed class GoPetSummonRequest
    {
        public ulong pet_uid;
    }

    [Serializable]
    public sealed class GoPetSummonResponse
    {
        public uint code;
        public GoPetData pet;
    }

    [Serializable]
    public sealed class GoPetRecallRequest
    {
        public ulong pet_uid;
    }

    [Serializable]
    public sealed class GoPetRecallResponse
    {
        public uint code;
        public ulong pet_uid;
    }

    [Serializable]
    public sealed class GoPetLevelUpRequest
    {
        public ulong pet_uid;
    }

    [Serializable]
    public sealed class GoPetLevelUpResponse
    {
        public uint code;
        public ulong pet_uid;
        public int level;
    }

    [Serializable]
    public sealed class GoPetEvolveRequest
    {
        public ulong pet_uid;
    }

    [Serializable]
    public sealed class GoPetEvolveResponse
    {
        public uint code;
        public ulong pet_uid;
        public int new_pet_id;
        public int new_quality;
    }

    [Serializable]
    public sealed class GoPetExploreRequest
    {
        public ulong pet_uid;
        public int duration;
    }

    [Serializable]
    public sealed class GoPetExploreResponse
    {
        public uint code;
        public ulong pet_uid;
        public long end_time;
    }

    [Serializable]
    public sealed class GoPetExploreDone
    {
        public ulong pet_uid;
        public GoDropItem[] rewards;
    }

    [Serializable]
    public sealed class GoPetComposeRequest
    {
        public ulong[] pet_uids;
    }

    [Serializable]
    public sealed class GoPetComposeResponse
    {
        public uint code;
        public ulong result_id;
        public int pet_id;
        public int quality;
    }

    [Serializable]
    public sealed class GoPetData
    {
        public ulong uid;
        public int pet_id;
        public string name;
        public int level;
        public int quality;
        public int type;
        public string skills;
    }

    [Serializable]
    public sealed class GoDropItem
    {
        public ulong item_id;
        public string name;
        public int quality;
        public int count;
    }

    // --- Trading ---

    [Serializable]
    public sealed class GoTradeListRequest
    {
        public int category;
        public int page;
    }

    [Serializable]
    public sealed class GoTradeListResponse
    {
        public uint code;
        public GoTradeItem[] items;
        public int total;
    }

    [Serializable]
    public sealed class GoTradeItem
    {
        public ulong order_id;
        public ulong seller_id;
        public string seller_name;
        public int equip_id;
        public string name;
        public int quality;
        public int strengthen_level;
        public long price;
    }

    [Serializable]
    public sealed class GoTradePublishRequest
    {
        public int slot;
        public long price;
    }

    [Serializable]
    public sealed class GoTradePublishResponse
    {
        public uint code;
        public ulong order_id;
    }

    [Serializable]
    public sealed class GoTradeBuyRequest
    {
        public ulong order_id;
    }

    [Serializable]
    public sealed class GoTradeBuyResponse
    {
        public uint code;
        public ulong order_id;
    }

    [Serializable]
    public sealed class GoTradeCancelRequest
    {
        public ulong order_id;
    }

    [Serializable]
    public sealed class GoTradeCancelResponse
    {
        public uint code;
        public ulong order_id;
    }

    // --- Shop ---

    [Serializable]
    public sealed class GoShopListRequest
    {
        public int type;
    }

    [Serializable]
    public sealed class GoShopListResponse
    {
        public uint code;
        public GoShopItem[] items;
    }

    [Serializable]
    public sealed class GoShopItem
    {
        public ulong id;
        public string name;
        public long price;
        public int currency_type;
        public int stock;
        public int require_level;
    }

    [Serializable]
    public sealed class GoShopBuyRequest
    {
        public ulong item_id;
        public int count;
        public int currency_type;
    }

    [Serializable]
    public sealed class GoShopBuyResponse
    {
        public uint code;
        public ulong item_id;
        public int count;
    }

    // --- Skill ---

    [Serializable]
    public sealed class GoSkillLevelUpRequest
    {
        public int skill_id;
    }

    [Serializable]
    public sealed class GoSkillLevelUpResponse
    {
        public uint code;
        public int skill_id;
        public int new_level;
    }

    [Serializable]
    public sealed class GoSkillResetRequest
    {
    }

    [Serializable]
    public sealed class GoSkillResetResponse
    {
        public uint code;
        public int refund_points;
    }

    // --- Attribute ---

    [Serializable]
    public sealed class GoAttrAssignRequest
    {
        public string attr;
        public int val;
    }

    [Serializable]
    public sealed class GoAttrAssignResponse
    {
        public uint code;
        public string attr;
        public int val;
        public int attr_points;
    }

    // --- Ranking ---

    [Serializable]
    public sealed class GoRankingListRequest
    {
        public int type;
    }

    [Serializable]
    public sealed class GoRankingListResponse
    {
        public uint code;
        public int type;
        public GoRankingItem[] rankings;
    }

    [Serializable]
    public sealed class GoRankingItem
    {
        public int rank;
        public ulong player_id;
        public string name;
        public long value;
    }

    // --- Team ---

    [Serializable]
    public sealed class GoTeamMember
    {
        public ulong player_id;
        public string name;
        public int @class;
        public int level;
        public long hp;
        public long max_hp;
        public bool is_leader;
        public bool online;
        public int layer;
    }

    [Serializable]
    public sealed class GoTeamInfo
    {
        public ulong team_id;
        public ulong leader_id;
        public int member_count;
        public GoTeamMember[] members;
    }

    [Serializable]
    public sealed class GoTeamInfoResponse
    {
        public uint code;
        public GoTeamInfo team;
    }

    [Serializable]
    public sealed class GoTeamInviteRequest
    {
        public ulong target_id;
    }

    [Serializable]
    public sealed class GoTeamInvitePush
    {
        public ulong team_id;
        public ulong inviter_id;
        public string inviter_name;
        public int member_count;
    }

    [Serializable]
    public sealed class GoTeamInviteReplyRequest
    {
        public ulong team_id;
        public bool accept;
    }

    [Serializable]
    public sealed class GoTeamInviteResult
    {
        public uint code;
        public ulong target_id;
        public string target_name;
        public bool accept;
    }

    [Serializable]
    public sealed class GoTeamLeaveResponse
    {
        public uint code;
        public ulong team_id;
    }

    [Serializable]
    public sealed class GoTeamDismissResponse
    {
        public uint code;
        public ulong team_id;
    }

    [Serializable]
    public sealed class GoTeamKickRequest
    {
        public ulong target_id;
    }

    [Serializable]
    public sealed class GoTeamKickResponse
    {
        public uint code;
        public ulong target_id;
    }

    /// <summary>
    /// 队伍状态变更推送（全员）。
    /// Action: 1=加入 2=离开 3=解散 4=被踢 5=状态变更
    /// </summary>
    [Serializable]
    public sealed class GoTeamUpdate
    {
        public uint action;
        public ulong team_id;
        public ulong leader_id;
        public GoTeamMember[] members;
        public string reason;
    }

    // --- Chat ---

    public static class GoChatChannels
    {
        public const int World = 1;
        public const int Private = 2;
        public const int Team = 3;
    }

    [Serializable]
    public sealed class GoChatSendRequest
    {
        public int channel;
        public ulong target_id;
        public string content;
    }

    [Serializable]
    public sealed class GoChatSendResponse
    {
        public uint code;
        public int channel;
        public ulong target_id;
        public long timestamp;
    }

    [Serializable]
    public sealed class GoChatMessage
    {
        public int channel;
        public ulong sender_id;
        public string sender_name;
        public ulong target_id;
        public string content;
        public long timestamp;
    }

    [Serializable]
    public sealed class GoChatHistoryRequest
    {
        public int channel;
        public int count;
    }

    [Serializable]
    public sealed class GoChatHistoryResponse
    {
        public uint code;
        public int channel;
        public GoChatMessage[] messages;
    }
}
