using HeroQuest.Systems.Dungeon;
using NUnit.Framework;

namespace HeroQuest.Tests
{
    public sealed class DungeonRulesTests
    {
        [TestCase(10, true)]
        [TestCase(20, true)]
        [TestCase(9, false)]
        public void IsBossLayer_MatchesEveryTenthLayer(int layer, bool expected)
        {
            Assert.AreEqual(expected, DungeonRules.IsBossLayer(layer));
        }

        [Test]
        public void CanEnterLayer_AllowsClearedOrNextLayerOnly()
        {
            Assert.IsTrue(DungeonRules.CanEnterLayer(6, 5, 30));
            Assert.IsFalse(DungeonRules.CanEnterLayer(7, 5, 30));
        }

        [Test]
        public void GenerateFallbackMonster_UsesProductFallbackFormula()
        {
            var monster = DungeonRules.GenerateFallbackMonster(3);

            Assert.AreEqual(140, monster.MaxHp);
            Assert.AreEqual(14, monster.Attack);
            Assert.AreEqual(8, monster.Defense);
        }
    }
}
