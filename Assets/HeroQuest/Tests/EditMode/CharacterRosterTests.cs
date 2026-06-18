using HeroQuest.Domain;
using HeroQuest.Systems.Character;
using NUnit.Framework;
using System.Collections.Generic;

namespace HeroQuest.Tests
{
    public sealed class CharacterRosterTests
    {
        [Test]
        public void EverySelectableClassHasMaleAndFemalePlayableResources()
        {
            foreach (var characterClass in CharacterRoster.AvailableClasses)
            {
                AssertPlayableResources(CharacterRoster.Get(characterClass, CharacterGender.Male));
                AssertPlayableResources(CharacterRoster.Get(characterClass, CharacterGender.Female));
            }
        }

        [Test]
        public void EverySelectableVariantUsesDistinctWalkResources()
        {
            var rightPaths = new HashSet<string>();
            var leftPaths = new HashSet<string>();

            foreach (var characterClass in CharacterRoster.AvailableClasses)
            {
                AssertDistinctWalkResources(CharacterRoster.Get(characterClass, CharacterGender.Male), rightPaths, leftPaths);
                AssertDistinctWalkResources(CharacterRoster.Get(characterClass, CharacterGender.Female), rightPaths, leftPaths);
            }
        }

        private static void AssertPlayableResources(CharacterDefinition definition)
        {
            Assert.IsNotNull(definition);
            Assert.IsNotEmpty(definition.ResourcePath);
            Assert.IsNotEmpty(definition.WalkRightResourcePath);
            Assert.IsNotEmpty(definition.WalkLeftResourcePath);
            Assert.Greater(definition.WalkColumns, 0);
            Assert.Greater(definition.WalkRows, 0);
        }

        private static void AssertDistinctWalkResources(
            CharacterDefinition definition,
            HashSet<string> rightPaths,
            HashSet<string> leftPaths)
        {
            Assert.IsTrue(rightPaths.Add(definition.WalkRightResourcePath), definition.WalkRightResourcePath);
            Assert.IsTrue(leftPaths.Add(definition.WalkLeftResourcePath), definition.WalkLeftResourcePath);
        }
    }
}
