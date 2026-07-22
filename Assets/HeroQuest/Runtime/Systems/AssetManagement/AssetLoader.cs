using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HeroQuest.Systems.AssetManagement
{
    /// <summary>
    /// 统一资源加载器：优先使用 Addressables 异步加载，回退到 Resources.Load。
    /// 迁移期间逐步将 Resources 路径标记为 Addressable，
    /// 调用方无需感知资源是从哪里加载的。
    /// </summary>
    public static class AssetLoader
    {
        /// <summary>
        /// 异步加载资源。优先尝试 Addressables key，失败则回退到 Resources 路径。
        /// </summary>
        public static async Task<T> LoadAsync<T>(string addressableKey, string resourcesFallback = null) where T : Object
        {
            if (!string.IsNullOrEmpty(addressableKey))
            {
                try
                {
                    var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(addressableKey);
                    var asset = await handle.Task;
                    if (asset != null) return asset;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[AssetLoader] Addressables 加载失败: {addressableKey}, 回退到 Resources。错误: {e.Message}");
                }
            }

            if (!string.IsNullOrEmpty(resourcesFallback))
            {
                var asset = Resources.Load<T>(resourcesFallback);
                if (asset != null) return asset;
                Debug.LogWarning($"[AssetLoader] Resources 加载也失败: {resourcesFallback}");
            }

            return null;
        }

        public static async Task<Sprite> LoadSpriteAsync(string addressableKey, string resourcesFallback = null)
            => await LoadAsync<Sprite>(addressableKey, resourcesFallback);

        public static async Task<AudioClip> LoadAudioAsync(string addressableKey, string resourcesFallback = null)
            => await LoadAsync<AudioClip>(addressableKey, resourcesFallback);

        public static void Release<T>(T asset) where T : Object
        {
            if (asset != null) UnityEngine.AddressableAssets.Addressables.Release(asset);
        }
    }
}
