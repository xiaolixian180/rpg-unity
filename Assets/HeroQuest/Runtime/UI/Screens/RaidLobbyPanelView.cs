using System;
using HeroQuest.Net.Go;
using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.Screens
{
    /// <summary>
    /// 战局大厅面板 — 展示可用地图列表，选择地图查看详情，点击进入战局。
    /// </summary>
    public sealed class RaidLobbyPanelView : MonoBehaviour
    {
        private static readonly Color Bg = new(0.03f, 0.04f, 0.03f, 0.97f);
        private static readonly Color PanelDark = new(0.06f, 0.08f, 0.06f, 0.95f);
        private static readonly Color PanelMid = new(0.08f, 0.10f, 0.08f, 0.90f);
        private static readonly Color Border = new(0.26f, 0.42f, 0.24f, 1f);
        private static readonly Color TextLight = new(0.92f, 0.90f, 0.82f, 1f);
        private static readonly Color TextGold = new(0.78f, 0.58f, 0.22f, 1f);
        private static readonly Color TextDim = new(0.60f, 0.58f, 0.52f, 1f);

        // --- Events ---
        public event Action<int> OnRaidEnterRequested;
        public event Action OnCloseRequested;
        public event Action OnStashRequested;

        // --- References ---
        private ScrollRect _mapScroll;
        private Transform _mapListContent;
        private Text _detailNameText;
        private Text _detailDurationText;
        private Text _detailZoneText;
        private Text _detailLootText;
        private Button _enterBtn;

        private GoRaidMapInfo[] _maps;
        private GoRaidMapInfo _selectedMap;

        public static RaidLobbyPanelView Create(Canvas parent)
        {
            var go = new GameObject("RaidLobbyPanelView", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(go.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            SetAnchor(panelRect, new Vector2(0.5f, 0.5f), new Vector2(750f, 520f));
            panel.GetComponent<Image>().color = Bg;

            var view = go.AddComponent<RaidLobbyPanelView>();
            view.Build(panel.transform);
            return view;
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
        /// 设置可用地图列表。
        /// </summary>
        public void SetMaps(GoRaidMapInfo[] maps)
        {
            _maps = maps;
            _selectedMap = null;
            RebuildMapList();
            ClearDetail();
        }

        // -------------------------------------------------------------------
        // Build
        // -------------------------------------------------------------------

        private void Build(Transform root)
        {
            // Title
            CreateText(root, "战局大厅", 28, new Vector2(0.5f, 0.95f), new Vector2(200f, 40f), TextLight);

            // Close button
            var closeBtn = CreateButton(root, "X", new Vector2(0.96f, 0.95f), new Vector2(40f, 40f));
            closeBtn.onClick.AddListener(() => OnCloseRequested?.Invoke());

            // --- Left: map list ---
            var leftBg = new GameObject("LeftBg", typeof(RectTransform), typeof(Image));
            leftBg.transform.SetParent(root, false);
            leftBg.GetComponent<Image>().color = PanelMid;
            SetAnchor(leftBg.GetComponent<RectTransform>(), new Vector2(0.28f, 0.48f), new Vector2(380f, 400f));

            var left = leftBg.transform;
            CreateText(left, "地图列表", 18, new Vector2(0.5f, 0.95f), new Vector2(200f, 28f), Border);

            // ScrollRect for map list
            var scrollGo = new GameObject("MapScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(Mask));
            scrollGo.transform.SetParent(left, false);
            scrollGo.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.08f, 1f);
            SetAnchor(scrollGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.45f), new Vector2(350f, 340f));

            var mask = scrollGo.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            _mapScroll = scrollGo.GetComponent<ScrollRect>();

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            viewport.GetComponent<Image>().color = Color.white;
            var vpRt = viewport.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = new Vector2(5, 5);
            vpRt.offsetMax = new Vector2(-5, -5);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 0);

            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 4;
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _mapListContent = content.transform;
            _mapScroll.content = contentRt;

            // --- Right: detail area ---
            var rightBg = new GameObject("RightBg", typeof(RectTransform), typeof(Image));
            rightBg.transform.SetParent(root, false);
            rightBg.GetComponent<Image>().color = PanelMid;
            SetAnchor(rightBg.GetComponent<RectTransform>(), new Vector2(0.72f, 0.55f), new Vector2(340f, 340f));

            var right = rightBg.transform;
            CreateText(right, "地图详情", 18, new Vector2(0.5f, 0.95f), new Vector2(200f, 28f), Border);

            _detailNameText = CreateText(right, "", 22, new Vector2(0.5f, 0.82f), new Vector2(300f, 32f), TextGold);
            _detailDurationText = CreateText(right, "", 16, new Vector2(0.5f, 0.70f), new Vector2(300f, 24f), TextLight);
            _detailZoneText = CreateText(right, "", 16, new Vector2(0.5f, 0.60f), new Vector2(300f, 24f), TextLight);
            _detailLootText = CreateText(right, "", 16, new Vector2(0.5f, 0.50f), new Vector2(300f, 24f), TextLight);

            // --- Bottom buttons ---
            _enterBtn = CreateButton(root, "进入战局", new Vector2(0.60f, 0.08f), new Vector2(160f, 44f));
            _enterBtn.onClick.AddListener(() =>
            {
                if (_selectedMap != null)
                    OnRaidEnterRequested?.Invoke(_selectedMap.template_id);
            });

            var stashBtn = CreateButton(root, "仓库", new Vector2(0.82f, 0.08f), new Vector2(100f, 44f));
            stashBtn.onClick.AddListener(() => OnStashRequested?.Invoke());
        }

        // -------------------------------------------------------------------
        // Map list helpers
        // -------------------------------------------------------------------

        private void RebuildMapList()
        {
            // Clear existing children
            for (int i = _mapListContent.childCount - 1; i >= 0; i--)
                Destroy(_mapListContent.GetChild(i).gameObject);

            if (_maps == null) return;

            for (int i = 0; i < _maps.Length; i++)
            {
                var map = _maps[i];
                var rowGo = new GameObject("MapRow", typeof(RectTransform), typeof(Image), typeof(Button));
                rowGo.transform.SetParent(_mapListContent, false);
                var rowRt = rowGo.GetComponent<RectTransform>();
                rowRt.sizeDelta = new Vector2(0, 44);
                rowGo.GetComponent<Image>().color = new Color(0.10f, 0.14f, 0.10f, 0.8f);

                var btn = rowGo.GetComponent<Button>();
                var captured = map;
                btn.onClick.AddListener(() => SelectMap(captured));

                CreateText(rowGo.transform, map.name, 16, new Vector2(0.30f, 0.5f), new Vector2(160f, 28f), TextLight);
                string tierLabel = GetLootTierLabel(map.loot_tier);
                CreateText(rowGo.transform, tierLabel, 13, new Vector2(0.75f, 0.5f), new Vector2(80f, 24f), TextDim);
            }
        }

        private void SelectMap(GoRaidMapInfo map)
        {
            _selectedMap = map;
            if (_detailNameText != null) _detailNameText.text = map.name;
            if (_detailDurationText != null) _detailDurationText.text = $"时长: {map.duration / 60} 分钟";
            if (_detailZoneText != null) _detailZoneText.text = $"区域数: {map.zone_count}";
            if (_detailLootText != null) _detailLootText.text = $"战利品等级: {GetLootTierLabel(map.loot_tier)}";
        }

        private void ClearDetail()
        {
            if (_detailNameText != null) _detailNameText.text = "";
            if (_detailDurationText != null) _detailDurationText.text = "";
            if (_detailZoneText != null) _detailZoneText.text = "";
            if (_detailLootText != null) _detailLootText.text = "";
        }

        private static string GetLootTierLabel(int tier)
        {
            return tier switch
            {
                0 => "普通",
                1 => "精良",
                2 => "稀有",
                3 => "史诗",
                4 => "传说",
                _ => "未知"
            };
        }

        // -------------------------------------------------------------------
        // Helpers (same pattern as TeamPanelView / ForgePanelView)
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
