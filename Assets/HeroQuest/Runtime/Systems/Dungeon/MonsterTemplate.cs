namespace HeroQuest.Systems.Dungeon
{
    public readonly struct MonsterTemplate
    {
        public MonsterTemplate(int layer, string name, int maxHp, int attack, int defense, int expReward, int goldReward)
        {
            Layer = layer;
            Name = name;
            MaxHp = maxHp;
            Attack = attack;
            Defense = defense;
            ExpReward = expReward;
            GoldReward = goldReward;
        }

        public int Layer { get; }
        public string Name { get; }
        public int MaxHp { get; }
        public int Attack { get; }
        public int Defense { get; }
        public int ExpReward { get; }
        public int GoldReward { get; }
    }
}
