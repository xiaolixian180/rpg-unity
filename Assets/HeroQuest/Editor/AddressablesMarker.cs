using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace HeroQuest.Editor
{
    /// <summary>
    /// 批量标记项目资源为 Addressable。
    /// 通过菜单 Hero Quest > Mark Assets as Addressable 执行。
    /// </summary>
    public static class AddressablesMarker
    {
        private const string GroupName = "HeroQuest";

        [MenuItem("Hero Quest/Mark Assets as Addressable")]
        public static void MarkAll()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[AddressablesMarker] Addressables Settings 未初始化，请先打开 Window > Asset Management > Addressables > Groups。");
                return;
            }

            var group = settings.FindGroup(GroupName);
            if (group == null)
            {
                group = settings.CreateGroup(GroupName, false, false, true, null);
                Debug.Log($"[AddressablesMarker] 创建 Addressables Group: {GroupName}");
            }

            var marked = 0;

            // 标记角色立绘
            marked += MarkFolder(settings, group, "Assets/Resources/HeroQuest/Characters", "HeroQuest/Characters");
            marked += MarkFolder(settings, group, "Assets/Resources/HeroQuest/Anime/Characters", "HeroQuest/Anime/Characters");
            marked += MarkFolder(settings, group, "Assets/Resources/HeroQuest/Anime/Playable", "HeroQuest/Anime/Playable");
            marked += MarkFolder(settings, group, "Assets/Resources/HeroQuest/Anime/Enemies", "HeroQuest/Anime/Enemies");
            marked += MarkFolder(settings, group, "Assets/Resources/HeroQuest/Anime/World", "HeroQuest/Anime/World");

            // 标记音频
            marked += MarkFolder(settings, group, "Assets/TJGenerators/History", "HeroQuest/Audio/History");

            // 标记 UI Prefab
            marked += MarkFile(settings, group, "Assets/Prefabs/UI/LoginPanel.prefab", "UI/LoginPanel");
            marked += MarkFile(settings, group, "Assets/Prefabs/UI/GameplayHUD.prefab", "UI/GameplayHUD");

            // 标记后处理预设
            marked += MarkFile(settings, group, "Assets/Settings/PP_Map.asset", "PostProcessing/PP_Map");
            marked += MarkFile(settings, group, "Assets/Settings/PP_Combat.asset", "PostProcessing/PP_Combat");
            marked += MarkFile(settings, group, "Assets/Settings/PP_Login.asset", "PostProcessing/PP_Login");
            marked += MarkFile(settings, group, "Assets/Settings/PP_Raid.asset", "PostProcessing/PP_Raid");

            // 标记主题
            marked += MarkFile(settings, group, "Assets/Resources/HeroQuest/Themes/AnimeTheme.asset", "HeroQuest/Themes/AnimeTheme");

            AssetDatabase.SaveAssets();
            Debug.Log($"[AddressablesMarker] 完成！共标记 {marked} 个资源到 Group '{GroupName}'。");
        }

        private static int MarkFolder(AddressableAssetSettings settings, AddressableAssetGroup group, string folderPath, string addressPrefix)
        {
            if (!AssetDatabase.IsValidFolder(folderPath)) return 0;

            var count = 0;
            var guids = AssetDatabase.FindAssets("", new[] { folderPath });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path)) continue; // 跳过子文件夹
                if (path.EndsWith(".meta")) continue;

                count += MarkFile(settings, group, path, null);
            }

            Debug.Log($"[AddressablesMarker] {folderPath} — 标记 {count} 个资源。");
            return count;
        }

        private static int MarkFile(AddressableAssetSettings settings, AddressableAssetGroup group, string assetPath, string customAddress)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
            {
                return 0;
            }

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid)) return 0;

            var entry = settings.CreateOrMoveEntry(guid, group, false);
            if (!string.IsNullOrEmpty(customAddress))
            {
                entry.address = customAddress;
            }
            else
            {
                // 使用相对于 Assets/ 的路径作为地址，去掉扩展名
                entry.address = assetPath;
            }

            return 1;
        }
    }
}
