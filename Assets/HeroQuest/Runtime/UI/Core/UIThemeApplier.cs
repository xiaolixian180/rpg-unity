using UnityEngine;
using TMPro;

namespace HeroQuest.UI.Core
{
    /// <summary>
    /// 运行时主题应用器，提供全局访问点。
    /// 首次访问时从 Resources 加载 "HeroQuest/Themes/AnimeTheme"。
    /// 若资产缺失则使用代码默认值，保证不阻塞运行。
    /// </summary>
    public static class UIThemeApplier
    {
        private const string DefaultThemePath = "HeroQuest/Themes/AnimeTheme";

        private static UIThemeData _current;
        private static bool _loaded;

        /// <summary>
        /// 当前生效的主题。首次访问时自动加载。
        /// </summary>
        public static UIThemeData Current
        {
            get
            {
                if (!_loaded)
                {
                    _current = Resources.Load<UIThemeData>(DefaultThemePath);
                    _loaded = true;

                    if (_current == null)
                    {
                        Debug.LogWarning($"[UIThemeApplier] 未找到主题资产 {DefaultThemePath}，使用代码默认值。");
                        _current = CreateFallback();
                    }
                }

                return _current;
            }
        }

        /// <summary>
        /// 手动设置当前主题（覆盖自动加载）。
        /// </summary>
        public static void SetTheme(UIThemeData theme)
        {
            _current = theme;
            _loaded = true;
        }

        /// <summary>
        /// 获取 TMP 字体资产，若主题未配置则返回 null（由调用方回退到 ChineseFontProvider）。
        /// </summary>
        public static TMP_FontAsset GetTmpFont()
        {
            return Current != null ? Current.tmpFont : null;
        }

        /// <summary>
        /// 获取 OS 动态字体（兼容旧版 UGUI Text 组件）。
        /// </summary>
        public static Font GetLegacyFont()
        {
            return ChineseFontProvider.GetFont();
        }

        private static UIThemeData CreateFallback()
        {
            var theme = ScriptableObject.CreateInstance<UIThemeData>();
            return theme;
        }
    }
}
