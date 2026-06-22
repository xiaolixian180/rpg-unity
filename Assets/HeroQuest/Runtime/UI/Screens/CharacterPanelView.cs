using System;
using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.Screens
{
    public sealed class CharacterPanelView : MonoBehaviour
    {
        private static readonly Color Bg = new(0.03f, 0.04f, 0.03f, 0.97f);
        private static readonly Color PanelDark = new(0.06f, 0.08f, 0.06f, 0.95f);
        private static readonly Color Border = new(0.26f, 0.42f, 0.24f, 1f);
        private static readonly Color TextLight = new(0.92f, 0.90f, 0.82f, 1f);
        private static readonly Color TextGold = new(0.78f, 0.58f, 0.22f, 1f);

        public event Action OnCloseRequested;
        public event Action<int> OnStrengthenRequested;
        public event Action<int> OnEnchantRequested;
        public event Action<int> OnUnequipRequested;

        private Text nameText;
        private Text classText;
        private Text levelText;
        private Text expText;
        private Text hpText;
        private Text mpText;
        private Text atkText;
        private Text defText;
        private Text spdText;
        private Text crtText;
        private Text powerText;
        private Text goldText;
        private Text honorText;
        private Text equipInfoText;

        public static CharacterPanelView Create(Canvas parent)
        {
            var go = new GameObject("CharacterPanelView", typeof(RectTransform), typeof(Image));
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
            SetAnchor(panelRect, new Vector2(0.5f, 0.5f), new Vector2(900f, 680f));
            panel.GetComponent<Image>().color = Bg;

            var view = go.AddComponent<CharacterPanelView>();
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

        public void ApplyCharacter(
            string name, string classLabel, int level,
            long exp, long expNext,
            int hp, int maxHp, int mp, int maxMp,
            int atkMin, int atkMax, int def, int spd, int crt, int power,
            int gold, int honor)
        {
            if (nameText != null) nameText.text = name;
            if (classText != null) classText.text = classLabel;
            if (levelText != null) levelText.text = $"Lv.{level}";
            if (expText != null) expText.text = $"经验: {exp} / {expNext}";
            if (hpText != null) hpText.text = $"生命: {hp} / {maxHp}";
            if (mpText != null) mpText.text = $"魔法: {mp} / {maxMp}";
            if (atkText != null) atkText.text = $"攻击: {atkMin}~{atkMax}";
            if (defText != null) defText.text = $"防御: {def}";
            if (spdText != null) spdText.text = $"速度: {spd}";
            if (crtText != null) crtText.text = $"暴击: {crt / 10f:F1}%";
            if (powerText != null) powerText.text = $"战力: {power}";
            if (goldText != null) goldText.text = $"金币: {gold}";
            if (honorText != null) honorText.text = $"荣誉: {honor}";
        }

        public void ApplyEquipment(object data)
        {
            if (equipInfoText != null)
            {
                equipInfoText.text = data == null ? "装备数据暂未下发" : data.ToString();
            }
        }

        private void Build(Transform root)
        {
            // Title
            CreateText(root, "角色 · 装备", 32, new Vector2(0.5f, 0.94f), new Vector2(400f, 48f), TextLight);

            // Close button
            var closeBtn = CreateButton(root, "X", new Vector2(0.96f, 0.94f), new Vector2(44f, 44f));
            closeBtn.onClick.AddListener(() => OnCloseRequested?.Invoke());

            // Left column: character info
            var leftBg = new GameObject("LeftBg", typeof(RectTransform), typeof(Image));
            leftBg.transform.SetParent(root, false);
            SetAnchor(leftBg.GetComponent<RectTransform>(), new Vector2(0.27f, 0.50f), new Vector2(400f, 540f));
            leftBg.GetComponent<Image>().color = PanelDark;

            var left = leftBg.transform;
            nameText = CreateText(left, "勇者", 26, new Vector2(0.5f, 0.92f), new Vector2(360f, 36f), TextGold);
            classText = CreateText(left, "战士", 20, new Vector2(0.5f, 0.86f), new Vector2(360f, 30f), TextLight);
            levelText = CreateText(left, "Lv.1", 22, new Vector2(0.5f, 0.80f), new Vector2(360f, 32f), TextLight);
            expText = CreateText(left, "经验: 0 / 100", 18, new Vector2(0.5f, 0.74f), new Vector2(360f, 28f), TextLight);

            CreateText(left, "— 基础属性 —", 18, new Vector2(0.5f, 0.67f), new Vector2(360f, 28f), Border);
            hpText = CreateText(left, "生命: 100 / 100", 18, new Vector2(0.5f, 0.62f), new Vector2(360f, 28f), TextLight);
            mpText = CreateText(left, "魔法: 50 / 50", 18, new Vector2(0.5f, 0.57f), new Vector2(360f, 28f), TextLight);
            atkText = CreateText(left, "攻击: 10~20", 18, new Vector2(0.5f, 0.52f), new Vector2(360f, 28f), TextLight);
            defText = CreateText(left, "防御: 10", 18, new Vector2(0.5f, 0.47f), new Vector2(360f, 28f), TextLight);
            spdText = CreateText(left, "速度: 10", 18, new Vector2(0.5f, 0.42f), new Vector2(360f, 28f), TextLight);
            crtText = CreateText(left, "暴击: 1.0%", 18, new Vector2(0.5f, 0.37f), new Vector2(360f, 28f), TextLight);
            powerText = CreateText(left, "战力: 500", 18, new Vector2(0.5f, 0.32f), new Vector2(360f, 28f), TextGold);

            CreateText(left, "— 货币 —", 18, new Vector2(0.5f, 0.25f), new Vector2(360f, 28f), Border);
            goldText = CreateText(left, "金币: 0", 18, new Vector2(0.5f, 0.20f), new Vector2(360f, 28f), TextLight);
            honorText = CreateText(left, "荣誉: 0", 18, new Vector2(0.5f, 0.15f), new Vector2(360f, 28f), TextLight);

            // Right column: equipment
            var rightBg = new GameObject("RightBg", typeof(RectTransform), typeof(Image));
            rightBg.transform.SetParent(root, false);
            SetAnchor(rightBg.GetComponent<RectTransform>(), new Vector2(0.73f, 0.50f), new Vector2(400f, 540f));
            rightBg.GetComponent<Image>().color = PanelDark;

            var right = rightBg.transform;
            CreateText(right, "— 装备栏 —", 20, new Vector2(0.5f, 0.92f), new Vector2(360f, 32f), Border);

            var slotNames = new[] { "武器", "头盔", "铠甲", "护腿", "鞋子", "饰品" };
            for (int i = 0; i < slotNames.Length; i++)
            {
                var y = 0.84f - i * 0.11f;

                CreateText(right, $"{slotNames[i]}: 空", 18, new Vector2(0.35f, y), new Vector2(220f, 28f), TextLight);

                var strengthenBtn = CreateButton(right, "强化", new Vector2(0.72f, y), new Vector2(60f, 28f));
                var slotIdx = i;
                strengthenBtn.onClick.AddListener(() => OnStrengthenRequested?.Invoke(slotIdx));

                var enchantBtn = CreateButton(right, "附魔", new Vector2(0.85f, y), new Vector2(60f, 28f));
                enchantBtn.onClick.AddListener(() => OnEnchantRequested?.Invoke(slotIdx));

                var unequipBtn = CreateButton(right, "卸下", new Vector2(0.98f, y), new Vector2(60f, 28f));
                unequipBtn.onClick.AddListener(() => OnUnequipRequested?.Invoke(slotIdx));
            }

            equipInfoText = CreateText(right, "装备数据暂未下发", 16, new Vector2(0.5f, 0.10f), new Vector2(360f, 48f), new Color(0.62f, 0.66f, 0.62f, 1f));
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
