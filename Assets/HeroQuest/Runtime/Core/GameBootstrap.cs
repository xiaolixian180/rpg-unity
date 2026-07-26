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
            var existing = UnityEngine.Object.FindFirstObjectByType<GameBootstrap>();
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

            // --- Batch3: Inventory/Pet/DungeonResource 已替换为真实网络实现 ---
            ServiceRegistry.Register<IInventoryService>(new InventoryService(networkManager));
            // --- Batch2: Equipment/Skill/PvP 已替换为真实网络实现 ---
            ServiceRegistry.Register<IEquipmentService>(new EquipmentService(networkManager));
            ServiceRegistry.Register<ISkillService>(new SkillService(networkManager));
            ServiceRegistry.Register<IPetService>(new PetService(networkManager));
            ServiceRegistry.Register<IPvpService>(new PvpService(networkManager));
            // --- Batch1: Ranking/Shop/Trading 已替换为真实网络实现 ---
            ServiceRegistry.Register<IShopService>(new ShopService(networkManager));
            ServiceRegistry.Register<ITradingService>(new TradingService(networkManager));
            ServiceRegistry.Register<IRankingService>(new RankingService(networkManager));
            ServiceRegistry.Register<ISaveService>(new SaveServiceStub()); // --- Batch4: 按决策保持 Stub，存档由服务端自动管理 ---
            ServiceRegistry.Register<IDungeonResourceService>(new DungeonResourceService(networkManager));
        }

        private void Awake()
        {
            InitializeRegistry();
            DontDestroyOnLoad(gameObject);
            _ = AddressableLoader.InitializeAsync();
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
