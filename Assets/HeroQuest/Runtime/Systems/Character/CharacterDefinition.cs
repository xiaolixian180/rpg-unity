using HeroQuest.Domain;
using UnityEngine;

namespace HeroQuest.Systems.Character
{
    public sealed class CharacterDefinition
    {
        public CharacterDefinition(
            CharacterClass characterClass,
            CharacterGender gender,
            string displayName,
            string roleDescription,
            string resourcePath,
            string walkRightResourcePath,
            string walkLeftResourcePath,
            int walkColumns,
            int walkRows,
            StatBlock baseStats)
        {
            CharacterClass = characterClass;
            Gender = gender;
            DisplayName = displayName;
            RoleDescription = roleDescription;
            ResourcePath = resourcePath;
            WalkRightResourcePath = walkRightResourcePath;
            WalkLeftResourcePath = walkLeftResourcePath;
            WalkColumns = walkColumns;
            WalkRows = walkRows;
            BaseStats = baseStats;
        }

        public CharacterClass CharacterClass { get; }
        public CharacterGender Gender { get; }
        public string DisplayName { get; }
        public string RoleDescription { get; }
        public string ResourcePath { get; }
        public string WalkRightResourcePath { get; }
        public string WalkLeftResourcePath { get; }
        public int WalkColumns { get; }
        public int WalkRows { get; }
        public StatBlock BaseStats { get; }

        public Sprite LoadPortrait()
        {
            return Resources.Load<Sprite>(ResourcePath);
        }
    }
}
