using HeroQuest.Config;
using UnityEngine;

namespace HeroQuest.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameBalanceConfig balanceConfig;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            ServiceRegistry.Clear();

            if (balanceConfig != null)
            {
                ServiceRegistry.Register(balanceConfig);
            }

            ServiceRegistry.Register<IEventBus>(new EventBus());
        }
    }
}
