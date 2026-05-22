using System;

namespace HeroQuest.Domain
{
    [Serializable]
    public struct StatBlock
    {
        public int strength;
        public int agility;
        public int intelligence;
        public int constitution;
        public int defense;

        public StatBlock(int strength, int agility, int intelligence, int constitution, int defense)
        {
            this.strength = strength;
            this.agility = agility;
            this.intelligence = intelligence;
            this.constitution = constitution;
            this.defense = defense;
        }
    }
}
