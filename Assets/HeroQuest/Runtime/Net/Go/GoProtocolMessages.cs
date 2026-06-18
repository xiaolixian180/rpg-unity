using System;

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
}
