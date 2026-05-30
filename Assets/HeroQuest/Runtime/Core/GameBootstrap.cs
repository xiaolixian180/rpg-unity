using HeroQuest.Config;
using HeroQuest.Systems.Dungeon;
using HeroQuest.Systems.Equipment;
using HeroQuest.Systems.Inventory;
using HeroQuest.Systems.Pets;
using HeroQuest.Systems.PvP;
using HeroQuest.Systems.Ranking;
using HeroQuest.Systems.Save;
using HeroQuest.Systems.Shop;
using HeroQuest.Systems.Skills;
using HeroQuest.Systems.Trading;
using UnityEngine;

namespace HeroQuest.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameBalanceConfig balanceConfig;
        [SerializeField] private ProductRuleConfig productRuleConfig;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            ServiceRegistry.Clear();

            if (balanceConfig != null)
            {
                ServiceRegistry.Register(balanceConfig);
            }

            if (productRuleConfig != null)
            {
                ServiceRegistry.Register(productRuleConfig);
            }

            ServiceRegistry.Register<IEventBus>(new EventBus());
            ServiceRegistry.Register<IInventoryService>(new InventoryServiceStub());
            ServiceRegistry.Register<IEquipmentService>(new EquipmentServiceStub());
            ServiceRegistry.Register<ISkillService>(new SkillServiceStub());
            ServiceRegistry.Register<IPetService>(new PetServiceStub());
            ServiceRegistry.Register<IPvpService>(new PvpServiceStub());
            ServiceRegistry.Register<IShopService>(new ShopServiceStub());
            ServiceRegistry.Register<ITradingService>(new TradingServiceStub());
            ServiceRegistry.Register<IRankingService>(new RankingServiceStub());
            ServiceRegistry.Register<ISaveService>(new SaveServiceStub());
            ServiceRegistry.Register<IDungeonResourceService>(new DungeonResourceServiceStub());
        }
    }
}
