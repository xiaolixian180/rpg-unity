namespace HeroQuest.Net.Protocol
{
    public static class GameMessageTypes
    {
        public const string Login = "login";
        public const string LoginResult = "login_result";
        public const string CreateCharacter = "create_character";
        public const string EnterDungeon = "enter_dungeon";
        public const string Move = "move";
        public const string Attack = "attack";
        public const string CastSkill = "cast_skill";
        public const string CollectResource = "collect_resource";
        public const string Equip = "equip";
        public const string StrengthenEquipment = "strengthen_equipment";
        public const string PetCommand = "pet_command";
        public const string PvpAttack = "pvp_attack";
        public const string ShopBuy = "shop_buy";
        public const string TradeBrowse = "trade_browse";
        public const string TradeBuy = "trade_buy";
        public const string RankingTop = "ranking_top";
        public const string Error = "error";
    }
}
