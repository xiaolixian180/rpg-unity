using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HeroQuest.Core
{
    /// <summary>
    /// Addressables 资源加载器，提供统一异步加载接口。
    /// 若 Addressables 未初始化或资源未标记，自动回退到 Resources.Load。
    /// </summary>
    public static class AddressableLoader
    {
        private static bool _initialized;

        /// <summary>
        /// 初始化 Addressables 系统（场景启动时调用一次）。
        /// </summary>
        public static async Task InitializeAsync()
        {
            if (_initialized) return;

            await Addressables.InitializeAsync().Task;
            _initialized = true;
            Debug.Log("[AddressableLoader] Addressables 已初始化。");
        }

        /// <summary>
        /// 异步加载资源。优先使用 Addressables，失败时回退到 Resources.Load。
        /// </summary>
        public static async Task<T> LoadAsync<T>(string address) where T : Object
        {
            if (string.IsNullOrEmpty(address)) return null;

            if (_initialized)
            {
                try
                {
                    var locationsHandle = Addressables.LoadResourceLocationsAsync(address, typeof(T));
                    await locationsHandle.Task;

                    if (locationsHandle.Result != null && locationsHandle.Result.Count > 0)
                    {
                        var handle = Addressables.LoadAssetAsync<T>(address);
                        await handle.Task;

                        if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                        {
                            return handle.Result;
                        }
                    }

                    Addressables.Release(locationsHandle);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[AddressableLoader] Addressables 加载失败: {address} — {e.Message}");
                }
            }

            return LoadFromResources<T>(address);
        }

        /// <summary>
        /// 同步加载资源（仅限 Editor）。
        /// </summary>
        public static T Load<T>(string address) where T : Object
        {
            if (string.IsNullOrEmpty(address)) return null;

#if UNITY_EDITOR
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(address);
            if (asset != null) return asset;
#endif
            return LoadFromResources<T>(address);
        }

        public static void Release<T>(T asset) where T : Object
        {
            if (asset == null) return;
        }

        private static T LoadFromResources<T>(string address) where T : Object
        {
            var resourcesPath = ToResourcesPath(address);
            if (string.IsNullOrEmpty(resourcesPath)) return null;

            var asset = Resources.Load<T>(resourcesPath);
            if (asset == null)
            {
                Debug.LogWarning($"[AddressableLoader] Resources.Load 失败: {resourcesPath}");
            }
            return asset;
        }

        private static string ToResourcesPath(string address)
        {
            if (string.IsNullOrEmpty(address)) return null;

            if (!address.StartsWith("Assets/"))
            {
                return RemoveExtension(address);
            }

            const string resourcesPrefix = "Assets/Resources/";
            if (address.StartsWith(resourcesPrefix))
            {
                return RemoveExtension(address.Substring(resourcesPrefix.Length));
            }

            return null;
        }

        private static string RemoveExtension(string path)
        {
            var dot = path.LastIndexOf('.');
            return dot > 0 ? path.Substring(0, dot) : path;
        }
    }
}
