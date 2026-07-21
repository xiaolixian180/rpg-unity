using UnityEngine;
using TMPro;

namespace HeroQuest.UI.Core
{
    /// <summary>
    /// ScriptableObject 主题资产，统一管理 UI 颜色、字体、间距等视觉参数。
    /// 通过 UIThemeApplier 在运行时加载，替代散落在各处的硬编码颜色。
    /// </summary>
    [CreateAssetMenu(fileName = "AnimeTheme", menuName = "HeroQuest/UI Theme")]
    public sealed class UIThemeData : ScriptableObject
    {
        [Header("面板颜色")]
        [Tooltip("标准半透明面板背景")]
        public Color panelBackground = new(0.12f, 0.14f, 0.22f, 0.88f);

        [Tooltip("深色面板背景（角色框、动作条）")]
        public Color panelDark = new(0.06f, 0.08f, 0.14f, 0.92f);

        [Tooltip("浅色面板背景（悬浮提示、子面板）")]
        public Color panelLight = new(0.18f, 0.20f, 0.28f, 0.85f);

        [Tooltip("插槽/格子背景")]
        public Color slotBackground = new(0.08f, 0.10f, 0.16f, 1f);

        [Tooltip("条形背景（血条/蓝条底色）")]
        public Color barBackground = new(0.03f, 0.03f, 0.06f, 1f);

        [Header("边框颜色")]
        [Tooltip("标准面板边框")]
        public Color border = new(0.35f, 0.42f, 0.65f, 1f);

        [Tooltip("金色强调边框（按钮、选中态）")]
        public Color borderAccent = new(0.85f, 0.70f, 0.30f, 1f);

        [Header("强调色")]
        [Tooltip("主色 — 按钮、选中高亮")]
        public Color primary = new(0.20f, 0.45f, 0.85f, 1f);

        [Tooltip("次色 — 次要按钮")]
        public Color secondary = new(0.15f, 0.30f, 0.55f, 1f);

        [Tooltip("金色强调 — 按键绑定、标题")]
        public Color gold = new(0.85f, 0.70f, 0.30f, 1f);

        [Tooltip("危险/错误色")]
        public Color danger = new(0.85f, 0.20f, 0.25f, 1f);

        [Tooltip("成功/确认色")]
        public Color success = new(0.25f, 0.75f, 0.40f, 1f);

        [Header("文字颜色")]
        [Tooltip("主要正文")]
        public Color textPrimary = new(0.92f, 0.93f, 0.96f, 1f);

        [Tooltip("次要文字（说明、提示）")]
        public Color textSecondary = new(0.65f, 0.68f, 0.75f, 1f);

        [Tooltip("占位符文字")]
        public Color textPlaceholder = new(0.45f, 0.48f, 0.55f, 1f);

        [Tooltip("战斗日志/聊天文字")]
        public Color textChat = new(0.55f, 0.85f, 0.55f, 1f);

        [Header("生命/法力条")]
        public Color hpHigh = new(0.20f, 0.72f, 0.30f, 1f);
        public Color hpMid = new(0.90f, 0.75f, 0.15f, 1f);
        public Color hpLow = new(0.85f, 0.15f, 0.12f, 1f);
        public Color mpFill = new(0.15f, 0.35f, 0.90f, 1f);
        public Color xpFill = new(0.45f, 0.25f, 0.85f, 1f);
        public Color cooldownOverlay = new(0.15f, 0.30f, 0.80f, 0.5f);

        [Header("品质颜色")]
        public Color qualityCommon = new(0.70f, 0.70f, 0.70f, 1f);
        public Color qualityUncommon = new(0.30f, 0.75f, 0.30f, 1f);
        public Color qualityRare = new(0.25f, 0.50f, 0.95f, 1f);
        public Color qualityEpic = new(0.65f, 0.30f, 0.85f, 1f);
        public Color qualityLegendary = new(0.95f, 0.65f, 0.15f, 1f);
        public Color qualityMythic = new(0.95f, 0.25f, 0.55f, 1f);

        [Header("间距与尺寸")]
        [Min(0f)] public float spacingSmall = 4f;
        [Min(0f)] public float spacingMedium = 8f;
        [Min(0f)] public float spacingLarge = 16f;
        [Min(0f)] public float panelCornerRadius = 8f;
        [Min(0f)] public float borderWidth = 2f;

        [Header("字体")]
        [Tooltip("TMP 字体资产，为空时回退到 ChineseFontProvider")]
        public TMP_FontAsset tmpFont;

        /// <summary>
        /// 根据 HP 百分比返回对应的血条颜色。
        /// </summary>
        public Color GetHpColor(float percent)
        {
            if (percent > 0.6f) return hpHigh;
            if (percent > 0.3f) return hpMid;
            return hpLow;
        }

        /// <summary>
        /// 根据品质枚举索引返回对应颜色。
        /// </summary>
        public Color GetQualityColor(int qualityIndex)
        {
            return qualityIndex switch
            {
                0 => qualityCommon,
                1 => qualityUncommon,
                2 => qualityRare,
                3 => qualityEpic,
                4 => qualityLegendary,
                5 => qualityMythic,
                _ => qualityCommon
            };
        }
    }
}
