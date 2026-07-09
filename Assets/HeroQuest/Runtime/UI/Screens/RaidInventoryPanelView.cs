using System;
using HeroQuest.Net.Go;
using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.Screens
{
    /// <summary>
    /// 战利品面板 — 展示当前战局拾取的物品列表，可丢弃物品。
    /// </summary>
    public sealed class RaidInventoryPanelView : MonoBehaviour
    {
        private static readonly Color Bg = new(0.03f, 0.04f, 0.03f, 0.97f);
        private static readonly Color PanelDark = new(0.06f, 0.08f, 0.06f, 0.95f);
        private static readonly Color Border = new(0.26f, 0.42f, 0.24f, 1f);
        private static readonly Color TextLight = new(0.92f, 0.90f, 0.82f, 1f);
        private static readonly Color TextDim = new(0.60f, 0.58f, 0.52f, 1f);

        // Quality colors matching typical RPG convention
        private static readonly Color QualityCommon = new(0.75f, 0.75f, 0.75f, 1f);
        private static readonly Color QualityFine = new(0.30f, 0.78f, 0.30f, 1f);
        private static readonly Color QualityRare = new(0.25f, 0.50f, 0.95f, 1f);
        private static readonly Color QualityEpic = new(0.65f, 0.30f, 0.85f, 1f);
        private static readonly Color QualityLegendary = new(0.95f, 0.65f, 0.10f, 1f);

        // --- Events ---
        public event Action<int> OnDiscardRequested;
        public event Action OnCloseRequested;

        // --- References ---
        private ScrollRect _itemScroll;
        private Transform _itemListContent;

        public static RaidInventoryPanelView Create(Canvas parent)
        {
            var go = new GameObject("RaidInventoryPanelView", typeof(RectTransform), typeof(Image));
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
            SetAnchor(panelRect, new Vector2(0.5f, 0.5f), new Vector2(500f, 480f));
            panel.GetComponent<Image>().color = Bg;

            var view = go.AddComponent<RaidInventoryPanelView>();
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
        /// 填充物品列表。
        /// </summary>
        public void SetItems(GoRaidLootData[] items)
        {
            Clear();
            if (items == null) return;

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var rowGo = new GameObject("ItemRow", typeof(RectTransform), typeof(Image));
                rowGo.transform.SetParent(_itemListContent, false);
                var rowRt = rowGo.GetComponent<RectTransform>();
                rowRt.sizeDelta = new Vector2(0, 48);
                rowGo.GetComponent<Image>().color = new Color(0.10f, 0.14f, 0.10f, 0.7f);

                // Item name with quality color
                Color nameColor = GetQualityColor(item.quality);
                CreateText(rowGo.transform, item.name, 16, new Vector2(0.25f, 0.5f), new Vector2(180f, 28f), nameColor);

                // Count
                CreateText(rowGo.transform, $"x{item.count}", 14, new Vector2(0.55f, 0.5f), new Vector2(60f, 24f), TextDim);

                // Quality label
                CreateText(rowGo.transform, GetQualityLabel(item.quality), 12, new Vector2(0.70f, 0.5f), new Vector2(60f, 22f), nameColor);

                // Discard button
                var discardBtn = CreateButton(rowGo.transform, "丢弃", new Vector2(0.90f, 0.5f), new Vector2(52f, 28f));
                int capturedIndex = item.index;
                discardBtn.onClick.AddListener(() => OnDiscardRequested?.Invoke(capturedIndex));
            }
        }

        /// <summary>
        /// 清空物品列表。
        /// </summary>
        public void Clear()
        {
            if (_itemListContent == null) return;
            for (int i = _itemListContent.childCount - 1; i >= 0; i--)
                Destroy(_itemListContent.GetChild(i).gameObject);
        }

        // -------------------------------------------------------------------
        // Quality helpers
        // -------------------------------------------------------------------

        private static Color GetQualityColor(int quality)
        {
            return quality switch
            {
                0 => QualityCommon,
                1 => QualityFine,
                2 => QualityRare,
                3 => QualityEpic,
                4 => QualityLegendary,
                _ => QualityCommon
            };
        }

        private static string GetQualityLabel(int quality)
        {
            return quality switch
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
        // Build
        // -------------------------------------------------------------------

        private void Build(Transform root)
        {
            // Title
            CreateText(root, "战利品", 28, new Vector2(0.5f, 0.95f), new Vector2(200f, 40f), TextLight);

            // Close button
            var closeBtn = CreateButton(root, "X", new Vector2(0.96f, 0.95f), new Vector2(40f, 40f));
            closeBtn.onClick.AddListener(() => OnCloseRequested?.Invoke());

            // Item list background
            var listBg = new GameObject("ListBg", typeof(RectTransform), typeof(Image));
            listBg.transform.SetParent(root, false);
            listBg.GetComponent<Image>().color = PanelDark;
            SetAnchor(listBg.GetComponent<RectTransform>(), new Vector2(0.5f, 0.46f), new Vector2(460f, 380f));

            // ScrollRect for items
            var scrollGo = new GameObject("ItemScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(Mask));
            scrollGo.transform.SetParent(listBg.transform, false);
            scrollGo.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.08f, 1f);
            SetAnchor(scrollGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(440f, 360f));

            var mask = scrollGo.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            _itemScroll = scrollGo.GetComponent<ScrollRect>();

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

            _itemListContent = content.transform;
            _itemScroll.content = contentRt;
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
