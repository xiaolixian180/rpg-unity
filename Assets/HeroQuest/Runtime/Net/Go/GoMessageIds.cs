namespace HeroQuest.Net.Go
{
    public static class GoMessageIds
    {
        public const ushort Login = 1001;
        public const ushort LoginResponse = 1002;
        public const ushort CreatePlayer = 1003;
        public const ushort CreatePlayerResponse = 1004;

        public const ushort EnterDungeon = 1101;
        public const ushort EnterDungeonResponse = 1102;
        public const ushort LeaveDungeon = 1103;
        public const ushort LeaveDungeonResponse = 1104;
        public const ushort DungeonInfo = 1105;
        public const ushort MonsterRefresh = 1106;
        public const ushort LayerTeleport = 1107;
        public const ushort LayerTeleportResponse = 1108;

        public const ushort Attack = 1201;
        public const ushort Damage = 1202;
        public const ushort BossSpawn = 1203;
        public const ushort BossDie = 1204;
        public const ushort SkillCast = 1205;
        public const ushort SkillEffect = 1206;
        public const ushort PlayerDie = 1207;
        public const ushort PlayerRevive = 1208;
        public const ushort CollectResource = 1209;
        public const ushort CollectResult = 1210;
        public const ushort AutoBattle = 1211;
        public const ushort AutoBattleResponse = 1212;
        public const ushort UseItem = 1213;
        public const ushort UseItemResponse = 1214;
        public const ushort InventorySync = 1215;

        public const ushort EquipStrengthen = 1301;
        public const ushort EquipStrengthenResponse = 1302;
        public const ushort EquipEnchant = 1303;
        public const ushort EquipEnchantResponse = 1304;
        public const ushort EquipWear = 1305;
        public const ushort EquipWearResponse = 1306;
        public const ushort EquipUnload = 1307;
        public const ushort EquipUnloadResponse = 1308;
        public const ushort Forge = 1309;
        public const ushort ForgeResponse = 1310;

        public const ushort PvpAttack = 1401;
        public const ushort PvpResult = 1402;
        public const ushort RedNameList = 1403;
        public const ushort BountyHunt = 1404;
        public const ushort BountyReward = 1405;
        public const ushort Revenge = 1406;
        public const ushort RevengeResponse = 1407;

        public const ushort Move = 1501;
        public const ushort PlayerMove = 1502;

        public const ushort PetSummon = 1601;
        public const ushort PetSummonResponse = 1602;
        public const ushort PetRecall = 1603;
        public const ushort PetLevelUp = 1604;
        public const ushort PetEvolve = 1605;
        public const ushort PetEvolveResponse = 1606;
        public const ushort PetExplore = 1607;
        public const ushort PetExploreResponse = 1608;
        public const ushort PetCompose = 1609;
        public const ushort PetComposeResponse = 1610;
        public const ushort PetRecallResponse = 1611;
        public const ushort PetEquip = 1612;
        public const ushort PetEquipResponse = 1613;
        public const ushort PetUnequip = 1614;
        public const ushort PetUnequipResponse = 1615;

        public const ushort TradeList = 1701;
        public const ushort TradeListResponse = 1702;
        public const ushort TradePublish = 1703;
        public const ushort TradePublishResponse = 1704;
        public const ushort TradeBuy = 1705;
        public const ushort TradeBuyResponse = 1706;
        public const ushort TradeCancel = 1707;
        public const ushort TradeCancelResponse = 1708;

        public const ushort ShopList = 1801;
        public const ushort ShopListResponse = 1802;
        public const ushort ShopBuy = 1803;
        public const ushort ShopBuyResponse = 1804;

        public const ushort SkillLevelUp = 1901;
        public const ushort SkillLevelUpResponse = 1902;
        public const ushort SkillReset = 1903;
        public const ushort SkillResetResponse = 1904;

        public const ushort AttrAssign = 2001;
        public const ushort AttrAssignResponse = 2002;

        public const ushort RankingList = 2101;
        public const ushort RankingListResponse = 2102;

        public const ushort TeamCreate = 2201;
        public const ushort TeamInfoResponse = 2202;
        public const ushort TeamInvite = 2203;
        public const ushort TeamInvitePush = 2204;
        public const ushort TeamInviteReply = 2205;
        public const ushort TeamInviteResult = 2206;
        public const ushort TeamLeave = 2207;
        public const ushort TeamLeaveResponse = 2208;
        public const ushort TeamDismiss = 2209;
        public const ushort TeamDismissResponse = 2210;
        public const ushort TeamKick = 2211;
        public const ushort TeamKickResponse = 2212;
        public const ushort TeamQuery = 2213;
        public const ushort TeamUpdate = 2214;

        public const ushort ChatSend = 2301;
        public const ushort ChatSendResponse = 2302;
        public const ushort ChatMessage = 2303;
        public const ushort ChatHistory = 2304;
        public const ushort ChatHistoryResponse = 2305;

        public const ushort Broadcast = 9001;
        public const ushort Heartbeat = 9002;
        public const ushort Kick = 9003;
    }
}
