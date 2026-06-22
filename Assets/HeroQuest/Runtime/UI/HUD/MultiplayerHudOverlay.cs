using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.HUD
{
    public sealed class MultiplayerHudOverlay : MonoBehaviour
    {
        private static readonly Color Bg = new(0.03f, 0.04f, 0.03f, 0.95f);
        private static readonly Color PanelDark = new(0.06f, 0.08f, 0.06f, 0.92f);
        private static readonly Color TextLight = new(0.92f, 0.90f, 0.82f, 1f);
        private static readonly Color Border = new(0.26f, 0.42f, 0.24f, 1f);

        private GameObject teamPanel;
        private bool isTeamPanelVisible;

        public void ToggleTeamPanel()
        {
            if (teamPanel == null)
            {
                BuildTeamPanel();
            }

            isTeamPanelVisible = !isTeamPanelVisible;
            teamPanel.SetActive(isTeamPanelVisible);
        }

        private void BuildTeamPanel()
        {
            var canvasObject = new GameObject("TeamPanelCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 70;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Dimmed overlay
            var overlay = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(canvasObject.transform, false);
            var overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            // Panel frame
            teamPanel = new GameObject("TeamPanel", typeof(RectTransform), typeof(Image));
            teamPanel.transform.SetParent(canvasObject.transform, false);
            SetAnchor(teamPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(520f, 420f));
            teamPanel.GetComponent<Image>().color = Bg;

            var root = teamPanel.transform;

            // Title
            CreateText(root, "队伍", 28, new Vector2(0.5f, 0.92f), new Vector2(300f, 42f), Border);

            // Close button
            var closeBtn = CreateButton(root, "X", new Vector2(0.94f, 0.92f), new Vector2(40f, 40f));
            closeBtn.onClick.AddListener(() =>
            {
                isTeamPanelVisible = false;
                teamPanel.SetActive(false);
            });

            // Content area
            var contentBg = new GameObject("ContentBg", typeof(RectTransform), typeof(Image));
            contentBg.transform.SetParent(root, false);
            SetAnchor(contentBg.GetComponent<RectTransform>(), new Vector2(0.5f, 0.48f), new Vector2(460f, 300f));
            contentBg.GetComponent<Image>().color = PanelDark;

            CreateText(contentBg.transform, "队伍功能开发中", 22, new Vector2(0.5f, 0.55f), new Vector2(400f, 36f), TextLight);
            CreateText(contentBg.transform, "敬请期待...", 18, new Vector2(0.5f, 0.40f), new Vector2(400f, 30f), new Color(0.62f, 0.66f, 0.62f, 1f));

            teamPanel.SetActive(false);
        }

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

            var text = CreateText(go.transform, label, 16, new Vector2(0.5f, 0.5f), sizeDelta, TextLight);
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
