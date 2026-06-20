using HeroQuest.Config;
using HeroQuest.Net.Go;
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

        private bool alreadyInitialized;

        public static GameBootstrap Ensure()
        {
            var existing = UnityEngine.Object.FindObjectOfType<GameBootstrap>();
            if (existing != null)
            {
                existing.InitializeRegistry();
                return existing;
            }

            var go = new GameObject("Game Bootstrap");
            var bootstrap = go.AddComponent<GameBootstrap>();
            bootstrap.InitializeRegistry();
            return bootstrap;
        }

        private void InitializeRegistry()
        {
            if (alreadyInitialized)
            {
                return;
            }
            alreadyInitialized = true;

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

            var networkManager = new NetworkManager();
            ServiceRegistry.Register(networkManager);

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

        private void Awake()
        {
            InitializeRegistry();
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (ServiceRegistry.TryResolve<NetworkManager>(out var network))
            {
                network.Dispose();
            }
        }
    }
}
