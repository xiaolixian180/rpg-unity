using HeroQuest.Domain;
using NUnit.Framework;

namespace HeroQuest.Tests
{
    public sealed class CombatCalculatorTests
    {
        [Test]
        public void CalculateStats_UsesProductFormulas()
        {
            var stats = CombatCalculator.CalculateStats(10, new StatBlock(10, 20, 0, 8, 5));

            Assert.AreEqual(445, stats.MaxHp);
            Assert.AreEqual(80f, stats.Attack);
            Assert.AreEqual(43f, stats.Defense);
            Assert.AreEqual(0.10f, stats.DodgeRate, 0.0001f);
            Assert.AreEqual(0.07f, stats.CriticalRate, 0.0001f);
            Assert.AreEqual(1.6f, stats.CriticalMultiplier, 0.0001f);
        }

        [Test]
        public void CalculateDamage_NeverDropsBelowOne()
        {
            var damage = CombatCalculator.CalculateDamage(5f, 1f, 999f, false, 1.5f);

            Assert.AreEqual(1, damage);
        }
    }
}
