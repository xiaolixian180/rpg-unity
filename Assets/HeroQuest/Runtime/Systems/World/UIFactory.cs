using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeroQuest.Systems.World
{
    /// <summary>
    /// UI 创建工具，提取自 PrototypeGameplayFlow 中重复的 panel/text/input/button 工厂方法。
    /// 使用 UIThemeApplier 主题色替代硬编码颜色。
    /// </summary>
    public static class UIFactory
    {
        public static GameObject CreatePanel(string name, Color color, Transform parent)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        public static Text CreateText(Transform parent, string text, float size, Vector2 anchor, Vector2 sizeDelta)
        {
            var theme = UIThemeApplier.Current;
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<Text>();
            label.text = text;
            label.font = ChineseFontProvider.GetFont();
            label.fontSize = Mathf.RoundToInt(size);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = theme.textPrimary;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            SetAnchor(label.rectTransform, anchor, sizeDelta);
            return label;
        }

        public static InputField CreateInput(Transform parent, string placeholder, string value, bool password, Vector2 anchor)
        {
            var theme = UIThemeApplier.Current;
            var inputObject = new GameObject(placeholder, typeof(RectTransform), typeof(Image), typeof(InputField));
            inputObject.transform.SetParent(parent, false);
            inputObject.GetComponent<Image>().color = theme.slotBackground;
            SetAnchor(inputObject.GetComponent<RectTransform>(), anchor, new Vector2(420f, 56f));

            var input = inputObject.GetComponent<InputField>();
            input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            input.text = value;

            var text = CreateInputText(inputObject.transform, string.Empty, theme.textPrimary);
            var placeholderText = CreateInputText(inputObject.transform, placeholder, theme.textPlaceholder);
            input.textComponent = text;
            input.placeholder = placeholderText;
            return input;
        }

        public static Button CreateButton(Transform parent, string text, Vector2 anchor, Vector2 sizeDelta)
        {
            var theme = UIThemeApplier.Current;
            var buttonObject = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = theme.secondary;
            SetAnchor(buttonObject.GetComponent<RectTransform>(), anchor, sizeDelta);
            var label = CreateText(buttonObject.transform, text, 24, new Vector2(0.5f, 0.5f), sizeDelta);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return buttonObject.GetComponent<Button>();
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public static void SetAnchor(RectTransform rectTransform, Vector2 anchor, Vector2 sizeDelta)
        {
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = sizeDelta;
        }

        private static Text CreateInputText(Transform parent, string text, Color color)
        {
            var textObject = new GameObject("Input Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<Text>();
            label.text = text;
            label.font = ChineseFontProvider.GetFont();
            label.fontSize = 24;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(18f, 0f);
            label.rectTransform.offsetMax = new Vector2(-18f, 0f);
            return label;
        }
    }
}
