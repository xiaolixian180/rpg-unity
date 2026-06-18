using UnityEngine;

namespace HeroQuest.UI.Core
{
    public static class ChineseFontProvider
    {
        private static Font cachedFont;

        public static Font GetFont()
        {
            if (cachedFont != null)
            {
                return cachedFont;
            }

            cachedFont = Font.CreateDynamicFontFromOSFont(
                new[] { "Microsoft YaHei", "SimHei", "SimSun", "Arial Unicode MS", "Arial" },
                18);

            return cachedFont;
        }
    }
}
