using System.Collections.Generic;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Character
{
    public static class CharacterRoster
    {
        private static readonly Dictionary<(CharacterClass, CharacterGender), CharacterDefinition> Definitions = new()
        {
            {
                (CharacterClass.Warrior, CharacterGender.Male),
                new CharacterDefinition(
                    CharacterClass.Warrior,
                    CharacterGender.Male,
                    "战士",
                    "近战物理职业，生存能力强，适合承担队伍前排。",
                    "HeroQuest/Characters/Warrior_Male",
                    "HeroQuest/Playable/Warrior_Male_Walk_Right",
                    "HeroQuest/Playable/Warrior_Male_Walk_Left",
                    8,
                    8,
                    new StatBlock(8, 4, 2, 7, 5))
            },
            {
                (CharacterClass.Warrior, CharacterGender.Female),
                new CharacterDefinition(
                    CharacterClass.Warrior,
                    CharacterGender.Female,
                    "战士",
                    "近战物理职业，生存能力强，适合承担队伍前排。",
                    "HeroQuest/Characters/Warrior_Female",
                    "HeroQuest/Playable/Warrior_Female_Walk_Right",
                    "HeroQuest/Playable/Warrior_Female_Walk_Left",
                    6,
                    5,
                    new StatBlock(8, 4, 2, 7, 5))
            },
            {
                (CharacterClass.Mage, CharacterGender.Male),
                new CharacterDefinition(
                    CharacterClass.Mage,
                    CharacterGender.Male,
                    "法师",
                    "远程法术职业，爆发能力强，适合后排输出。",
                    "HeroQuest/Characters/Mage_Male",
                    "HeroQuest/Playable/Mage_Male_Walk_Right",
                    "HeroQuest/Playable/Mage_Male_Walk_Left",
                    6,
                    5,
                    new StatBlock(2, 4, 9, 4, 2))
            },
            {
                (CharacterClass.Mage, CharacterGender.Female),
                new CharacterDefinition(
                    CharacterClass.Mage,
                    CharacterGender.Female,
                    "法师",
                    "远程法术职业，爆发能力强，适合后排输出。",
                    "HeroQuest/Characters/Mage_Female",
                    "HeroQuest/Playable/Mage_Female_Walk_Right",
                    "HeroQuest/Playable/Mage_Female_Walk_Left",
                    6,
                    5,
                    new StatBlock(2, 4, 9, 4, 2))
            },
            {
                (CharacterClass.Archer, CharacterGender.Male),
                new CharacterDefinition(
                    CharacterClass.Archer,
                    CharacterGender.Male,
                    "弓箭手",
                    "远程物理职业，机动性高，擅长持续输出。",
                    "HeroQuest/Characters/Archer_Male",
                    "HeroQuest/Playable/Archer_Male_Walk_Right",
                    "HeroQuest/Playable/Archer_Male_Walk_Left",
                    6,
                    5,
                    new StatBlock(5, 9, 3, 4, 2))
            },
            {
                (CharacterClass.Archer, CharacterGender.Female),
                new CharacterDefinition(
                    CharacterClass.Archer,
                    CharacterGender.Female,
                    "弓箭手",
                    "远程物理职业，机动性高，擅长持续输出。",
                    "HeroQuest/Characters/Archer_Female",
                    "HeroQuest/Playable/Archer_Female_Walk_Right",
                    "HeroQuest/Playable/Archer_Female_Walk_Left",
                    6,
                    5,
                    new StatBlock(5, 9, 3, 4, 2))
            },
            {
                (CharacterClass.Priest, CharacterGender.Male),
                new CharacterDefinition(
                    CharacterClass.Priest,
                    CharacterGender.Male,
                    "牧师",
                    "治疗辅助职业，擅长续航、保护和团队增益。",
                    "HeroQuest/Characters/Priest_Male",
                    "HeroQuest/Playable/Priest_Male_Walk_Right",
                    "HeroQuest/Playable/Priest_Male_Walk_Left",
                    6,
                    5,
                    new StatBlock(3, 4, 8, 6, 3))
            },
            {
                (CharacterClass.Priest, CharacterGender.Female),
                new CharacterDefinition(
                    CharacterClass.Priest,
                    CharacterGender.Female,
                    "牧师",
                    "治疗辅助职业，擅长续航、保护和团队增益。",
                    "HeroQuest/Characters/Priest_Female",
                    "HeroQuest/Playable/Priest_Female_Walk_Right",
                    "HeroQuest/Playable/Priest_Female_Walk_Left",
                    6,
                    5,
                    new StatBlock(3, 4, 8, 6, 3))
            }
        };

        public static IReadOnlyList<CharacterClass> AvailableClasses { get; } = new[]
        {
            CharacterClass.Warrior,
            CharacterClass.Mage,
            CharacterClass.Archer,
            CharacterClass.Priest
        };

        public static CharacterDefinition Get(CharacterClass characterClass, CharacterGender gender)
        {
            return Definitions[(characterClass, gender)];
        }
    }
}
