using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

namespace HeroQuest.Systems.AssetManagement
{
    /// <summary>
    /// 资源预加载器：在进入游戏前批量加载关键资源并显示进度条。
    /// 挂载到场景中的 Canvas 上，调用 PreloadAll() 启动。
    /// </summary>
    public sealed class LoadingScreenController : MonoBehaviour
    {
        [Header("UI 引用")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private Text statusText;
        [SerializeField] private Text percentText;

        private Canvas _canvas;

        public static LoadingScreenController Ensure()
        {
            var existing = FindFirstObjectByType<LoadingScreenController>();
            if (existing != null) return existing;

            var go = new GameObject("Loading Screen", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // 背景
            var bgGO = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(go.transform, false);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
            bgGO.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.06f, 0.95f);

            // 进度条
            var barGO = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image));
            barGO.transform.SetParent(go.transform, false);
            var barRect = barGO.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.5f, 0.3f);
            barRect.anchorMax = new Vector2(0.5f, 0.3f);
            barRect.sizeDelta = new Vector2(600, 12);
            barGO.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.16f, 1f);

            var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(barGO.transform, false);
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero; fillRect.offsetMax = Vector2.zero;
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.color = new Color(0.20f, 0.45f, 0.85f, 1f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;

            var slider = barGO.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.minValue = 0; slider.maxValue = 1;
            slider.value = 0;

            // 文本
            var statusGO = new GameObject("StatusText", typeof(RectTransform), typeof(Text));
            statusGO.transform.SetParent(go.transform, false);
            var statusRect = statusGO.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0.38f);
            statusRect.anchorMax = new Vector2(0.5f, 0.38f);
            statusRect.sizeDelta = new Vector2(600, 40);
            var statusTxt = statusGO.GetComponent<Text>();
            statusTxt.font = HeroQuest.UI.Core.ChineseFontProvider.GetFont();
            statusTxt.fontSize = 24;
            statusTxt.alignment = TextAnchor.MiddleCenter;
            statusTxt.color = new Color(0.92f, 0.93f, 0.96f, 1f);

            var pctGO = new GameObject("PercentText", typeof(RectTransform), typeof(Text));
            pctGO.transform.SetParent(go.transform, false);
            var pctRect = pctGO.GetComponent<RectTransform>();
            pctRect.anchorMin = new Vector2(0.5f, 0.25f);
            pctRect.anchorMax = new Vector2(0.5f, 0.25f);
            pctRect.sizeDelta = new Vector2(200, 30);
            var pctTxt = pctGO.GetComponent<Text>();
            pctTxt.font = HeroQuest.UI.Core.ChineseFontProvider.GetFont();
            pctTxt.fontSize = 20;
            pctTxt.alignment = TextAnchor.MiddleCenter;
            pctTxt.color = new Color(0.65f, 0.68f, 0.75f, 1f);

            var ctrl = go.AddComponent<LoadingScreenController>();
            ctrl._canvas = canvas;
            ctrl.progressBar = slider;
            ctrl.statusText = statusTxt;
            ctrl.percentText = pctTxt;

            return ctrl;
        }

        public async Task PreloadAll()
        {
            _canvas.enabled = true;

            var keys = new List<string>
            {
                "HeroQuest/Characters/Warrior_Male",
                "HeroQuest/Characters/Warrior_Female",
                "HeroQuest/Characters/Mage_Male",
                "HeroQuest/Characters/Mage_Female",
                "HeroQuest/Characters/Archer_Male",
                "HeroQuest/Characters/Archer_Female",
                "HeroQuest/Characters/Priest_Male",
                "HeroQuest/Characters/Priest_Female",
                "HeroQuest/World/Grassland_Background",
            };

            float total = keys.Count;
            float done = 0;

            foreach (var key in keys)
            {
                if (statusText != null) statusText.text = $"加载资源: {key}...";
                try
                {
                    await Addressables.LoadAssetAsync<Object>(key).Task;
                }
                catch
                {
                    // Addressables 未配置的资源跳过，不影响运行
                }

                done++;
                float pct = done / total;
                if (progressBar != null) progressBar.value = pct;
                if (percentText != null) percentText.text = $"{Mathf.RoundToInt(pct * 100)}%";
            }

            if (statusText != null) statusText.text = "加载完成！";
            await Task.Delay(300);

            _canvas.enabled = false;
        }
    }
}
