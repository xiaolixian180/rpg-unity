using UnityEngine;
using UnityEngine.UIElements;

namespace HeroQuest.UI.Toolkit
{
    /// <summary>
    /// 运行时 UI Toolkit 面板加载器。
    /// 原型阶段使用 AssetDatabase 加载 UXML，后续迁移到 Addressables。
    /// </summary>
    public static class ToolkitPanelLoader
    {
        private static UIDocument _activeDocument;
        private static PanelSettings _panelSettings;

        private const string PanelSettingsPath = "Assets/Settings/UI/PanelSettings.asset";
        private const string PanelBasePath = "Assets/HeroQuest/Runtime/UI/Toolkit/";

        public static PanelSettings GetPanelSettings()
        {
            if (_panelSettings != null) return _panelSettings;
#if UNITY_EDITOR
            _panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
#endif
            if (_panelSettings == null)
            {
                Debug.LogError($"[ToolkitPanelLoader] 未找到 PanelSettings: {PanelSettingsPath}");
            }
            return _panelSettings;
        }

        /// <summary>
        /// 显示指定 UI Toolkit 面板。panelName 不含扩展名。
        /// </summary>
        public static T ShowPanel<T>(string panelName) where T : Component
        {
            HidePanel();

#if UNITY_EDITOR
            var uxmlPath = $"{PanelBasePath}{panelName}.uxml";
            var uxmlAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if (uxmlAsset == null)
            {
                Debug.LogError($"[ToolkitPanelLoader] 未找到 UXML: {uxmlPath}");
                return null;
            }

            var go = new GameObject($"Toolkit Panel: {panelName}");
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = GetPanelSettings();
            if (doc.panelSettings == null) return null;

            uxmlAsset.CloneTree(doc.rootVisualElement);

            var controller = go.AddComponent<T>();
            _activeDocument = doc;
            return controller;
#else
            Debug.LogError("[ToolkitPanelLoader] 运行时加载 UXML 需要 Addressables，当前仅支持 Editor 模式。");
            return null;
#endif
        }

        public static void HidePanel()
        {
            if (_activeDocument != null)
            {
                Object.Destroy(_activeDocument.gameObject);
                _activeDocument = null;
            }
        }
    }
}
