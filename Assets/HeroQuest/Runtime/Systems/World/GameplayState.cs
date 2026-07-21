using System.Collections.Generic;
using HeroQuest.Domain;
using HeroQuest.Net.Go;
using HeroQuest.Systems.Character;
using HeroQuest.UI.HUD;
using UnityEngine;

namespace HeroQuest.Systems.World
{
    /// <summary>
    /// 共享游戏状态容器，被各子管理器引用而非各自持有散落字段。
    /// </summary>
    public sealed class GameplayState
    {
        public NetworkManager Network { get; set; }
        public GameplayHudController Hud { get; set; }
        public Canvas FlowCanvas { get; set; }
        public TopDownPlayerController PlayerController { get; set; }
        public GridSpriteSheetAnimator Animator { get; set; }

        public string AuthToken;
        public ulong PlayerId;
        public int CurrentLayer;
        public bool AutoBattleEnabled;
        public bool GameplayActive;
        public bool PlayerDead;
        public bool IsInRaid;

        public GoPlayerData LocalPlayerData;
        public CharacterClass SelectedClass = CharacterClass.Warrior;
        public CharacterGender SelectedGender = CharacterGender.Male;

        public readonly Dictionary<ulong, GoMonsterData> VisibleMonsters = new();
        public readonly Dictionary<ulong, GameObject> DungeonMonsterObjects = new();
        public ulong SelectedTargetId;

        public ulong PendingWearEquipId;
        public GoLootContainerData[] RaidContainers;
        public GoExtractionPointData[] RaidExtractionPoints;
        public GoRaidZoneData[] RaidZones;
        public ulong RaidNearestContainerId;
    }
}
