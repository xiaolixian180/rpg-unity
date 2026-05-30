using HeroQuest.Domain;
using HeroQuest.Systems.Dungeon;
using HeroQuest.Systems.Equipment;
using HeroQuest.Systems.Pets;
using HeroQuest.Systems.PvP;
using HeroQuest.Systems.Shop;
using HeroQuest.Systems.Skills;
using HeroQuest.Systems.Trading;
using NUnit.Framework;

namespace HeroQuest.Tests
{
    public sealed class ProductFrameworkRulesTests
    {
        [Test]
        public void EquipmentRules_UsesQualityPowerMultiplier()
        {
            Assert.AreEqual(1f, EquipmentRules.GetQualityPowerMultiplier(Quality.Common));
            Assert.AreEqual(1.75f, EquipmentRules.GetQualityPowerMultiplier(Quality.Epic));
        }

        [Test]
        public void SkillRules_UsesDefaultMultiplierWhenConfigMissing()
        {
            Assert.AreEqual(1.5f, SkillRules.ResolveDamageMultiplier(0f));
            Assert.IsTrue(SkillRules.CanUpgrade(1, 10, 1));
            Assert.IsFalse(SkillRules.CanUpgrade(10, 10, 1));
        }

        [Test]
        public void PetRules_RequiresSameQualityForSynthesis()
        {
            Assert.IsTrue(PetRules.CanSynthesize(Quality.Rare, Quality.Rare, Quality.Rare));
            Assert.IsFalse(PetRules.CanSynthesize(Quality.Rare, Quality.Epic, Quality.Rare));
        }

        [Test]
        public void PvpRules_CalculatesRobbedGoldAndRedName()
        {
            Assert.AreEqual(123, PvpRules.CalculateRobbedGold(1234, 0.1f));
            Assert.IsTrue(PvpRules.IsRedName(5, 5));
        }

        [Test]
        public void ShopRules_ReturnsProductErrorCodes()
        {
            var goods = new ShopGoods { stock = 1, requiredLevel = 3 };

            Assert.AreEqual(GameErrorCode.LevelNotEnough, ShopRules.ValidateBuy(goods, 1, 1));
            Assert.AreEqual(GameErrorCode.StockNotEnough, ShopRules.ValidateBuy(goods, 3, 2));
        }

        [Test]
        public void TradingRules_PreventsBuyingOwnOrder()
        {
            var order = new TradeOrder { sellerId = "player-a", status = TradeOrderStatus.Listed };

            Assert.IsFalse(TradingRules.CanBuy("player-a", order));
            Assert.IsTrue(TradingRules.CanBuy("player-b", order));
        }

        [Test]
        public void DungeonResourceRules_PreventsDuplicateCollect()
        {
            var node = new DungeonResourceNode { remainingCount = 0, isCollected = true };

            Assert.AreEqual(GameErrorCode.ResourceCollected, DungeonResourceRules.CanCollect(node));
        }
    }
}
