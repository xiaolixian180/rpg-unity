using System;
using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.Screens
{
    /// <summary>
    /// 战局 HUD 覆盖层 — 显示倒计时、区域信息、撤离进度和战利品背包按钮。
    /// 非全屏面板，仅包含屏幕边缘的小型指示器。
    /// </summary>
    public sealed class RaidHudOverlay : MonoBehaviour
    {
        private static readonly Color TextLight = new(0.92f, 0.90f, 0.82f, 1f);
        private static readonly Color TextGold = new(0.78f, 0.58f, 0.22f, 1f);
        private static readonly Color DangerRed = new(0.85f, 0.22f, 0.18f, 1f);
        private static readonly Color SafeGreen = new(0.30f, 0.78f, 0.30f, 1f);
        private static readonly Color PanelBg = new(0.04f, 0.06f, 0.04f, 0.85f);
        private static readonly Color BarFill = new(0.20f, 0.65f, 0.85f, 1f);
        private static readonly Color BarBg = new(0.10f, 0.12f, 0.10f, 0.90f);

        // --- Events ---
        public event Action OnRaidInventoryRequested;

        // --- UI refs ---
        private Text _timerText;
        private Text _zoneText;
        private GameObject _extractRoot;
        private Text _extractTimerText;
        private Image _extractBarFill;
        private Button _inventoryBtn;

        private int _extractDuration;

        public static RaidHudOverlay Create(Canvas parent)
        {
            var go = new GameObject("RaidHudOverlay", typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var overlay = go.AddComponent<RaidHudOverlay>();
            overlay.Build(go.transform);
            return overlay;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新剩余时间显示。
        /// </summary>
        public void SetTimer(long remainingSeconds)
        {
            if (_timerText == null) return;
            long min = remainingSeconds / 60;
            long sec = remainingSeconds % 60;
            _timerText.text = $"剩余: {min:D2}:{sec:D2}";
            _timerText.color = remainingSeconds <= 60 ? DangerRed : TextLight;
        }

        /// <summary>
        /// 更新当前区域名称与 PvP 状态。
        /// </summary>
        public void SetZone(string name, bool pvp)
        {
            if (_zoneText == null) return;
            _zoneText.text = pvp
                ? $"区域: {name} [PvP]"
                : $"区域: {name}";
            _zoneText.color = pvp ? DangerRed : SafeGreen;
        }

        /// <summary>
        /// 显示或隐藏撤离进度条，并更新倒计时。
        /// </summary>
        public void SetExtraction(int timer, bool active)
        {
            if (_extractRoot == null) return;
            _extractRoot.SetActive(active);
            if (!active) return;

            _extractDuration = Mathf.Max(_extractDuration, timer);

            if (_extractTimerText != null)
                _extractTimerText.text = $"撤离中: {timer}s";

            if (_extractBarFill != null && _extractDuration > 0)
                _extractBarFill.fillAmount = 1f - (float)timer / _extractDuration;
        }

        /// <summary>
        /// 设置撤离总时长（用于计算进度条比例），在进入撤离点时调用。
        /// </summary>
        public void SetExtractDuration(int totalSeconds)
        {
            _extractDuration = totalSeconds;
        }

        // -------------------------------------------------------------------
        // Build
        // -------------------------------------------------------------------

        private void Build(Transform root)
        {
            // --- Timer (top-right) ---
            var timerBg = new GameObject("TimerBg", typeof(RectTransform), typeof(Image));
            timerBg.transform.SetParent(root, false);
            timerBg.GetComponent<Image>().color = PanelBg;
            SetAnchor(timerBg.GetComponent<RectTransform>(), new Vector2(0.92f, 0.96f), new Vector2(160f, 36f));
            _timerText = CreateText(timerBg.transform, "剩余: --:--", 18, new Vector2(0.5f, 0.5f), new Vector2(150f, 30f), TextLight);

            // --- Zone indicator (below timer) ---
            var zoneBg = new GameObject("ZoneBg", typeof(RectTransform), typeof(Image));
            zoneBg.transform.SetParent(root, false);
            zoneBg.GetComponent<Image>().color = PanelBg;
            SetAnchor(zoneBg.GetComponent<RectTransform>(), new Vector2(0.92f, 0.91f), new Vector2(180f, 30f));
            _zoneText = CreateText(zoneBg.transform, "区域: --", 15, new Vector2(0.5f, 0.5f), new Vector2(170f, 26f), SafeGreen);

            // --- Extraction progress (center-top, hidden by default) ---
            _extractRoot = new GameObject("ExtractRoot", typeof(RectTransform), typeof(Image));
            _extractRoot.transform.SetParent(root, false);
            _extractRoot.GetComponent<Image>().color = PanelBg;
            SetAnchor(_extractRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 0.92f), new Vector2(260f, 50f));
            _extractRoot.SetActive(false);

            _extractTimerText = CreateText(_extractRoot.transform, "撤离中: 0s", 16, new Vector2(0.5f, 0.75f), new Vector2(240f, 24f), TextGold);

            // Progress bar background
            var barBg = new GameObject("BarBg", typeof(RectTransform), typeof(Image));
            barBg.transform.SetParent(_extractRoot.transform, false);
            barBg.GetComponent<Image>().color = BarBg;
            SetAnchor(barBg.GetComponent<RectTransform>(), new Vector2(0.5f, 0.30f), new Vector2(230f, 12f));

            // Progress bar fill
            var barFillGo = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
            barFillGo.transform.SetParent(barBg.transform, false);
            var fillRt = barFillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            barFillGo.GetComponent<Image>().color = BarFill;
            barFillGo.GetComponent<Image>().type = Image.Type.Filled;
            barFillGo.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
            _extractBarFill = barFillGo.GetComponent<Image>();

            // --- Inventory button (right side) ---
            _inventoryBtn = CreateButton(root, "背包(R)", new Vector2(0.92f, 0.85f), new Vector2(100f, 36f));
            _inventoryBtn.onClick.AddListener(() => OnRaidInventoryRequested?.Invoke());
        }

        // -------------------------------------------------------------------
        // Helpers (same pattern as TeamPanelView)
        // -------------------------------------------------------------------

        private static Text CreateText(Transform parent, string content, float size, Vector2 anchor, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.text = content;
            label.font = ChineseFontProvider.GetFont();
            label.fontSize = Mathf.RoundToInt(size);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            SetAnchor(label.rectTransform, anchor, sizeDelta);
            return label;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchor, Vector2 sizeDelta)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.30f, 1f);
            SetAnchor(go.GetComponent<RectTransform>(), anchor, sizeDelta);

            var text = CreateText(go.transform, label, 14, new Vector2(0.5f, 0.5f), sizeDelta, TextLight);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return go.GetComponent<Button>();
        }

        private static void SetAnchor(RectTransform rt, Vector2 anchor, Vector2 sizeDelta)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = sizeDelta;
        }
    }
}
