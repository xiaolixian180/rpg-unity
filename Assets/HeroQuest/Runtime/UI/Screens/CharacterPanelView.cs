using System;
using HeroQuest.Net.Go;
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
        private static readonly Color BonusGreen = new(0.2f, 0.8f, 0.2f, 1f);

        // 品质颜色映射
        private static readonly Color[] QualityColors = new Color[]
        {
            new(0.80f, 0.80f, 0.80f, 1f), // 0=白 #CCCCCC
            new(0.12f, 1.00f, 0.00f, 1f), // 1=绿 #1EFF00
            new(0.00f, 0.44f, 0.87f, 1f), // 2=蓝 #0070DD
            new(0.64f, 0.21f, 0.93f, 1f), // 3=紫 #A335EE
            new(1.00f, 0.50f, 0.00f, 1f), // 4=橙 #FF8000
            new(1.00f, 0.00f, 0.00f, 1f), // 5=红 #FF0000
        };

        public event Action OnCloseRequested;
        public event Action<int> OnStrengthenRequested;
        public event Action<int> OnEnchantRequested;
        public event Action<int> OnUnequipRequested;
        public event Action<string> OnAttrAssignRequested;

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
        private Text attrPointsText;

        // 属性分配按钮（str/agi/int/con）
        private Button strPlusBtn;
        private Button agiPlusBtn;
        private Button intPlusBtn;
        private Button conPlusBtn;

        // 8个装备槽位的文本引用
        private Text[] slotTexts = new Text[8];
        private Button[] strengthenBtns = new Button[8];
        private Button[] enchantBtns = new Button[8];
        private Button[] unequipBtns = new Button[8];

        private static readonly string[] SlotNames = { "武器", "头盔", "铠甲", "手套", "靴子", "项链", "戒指1", "戒指2" };

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
            SetAnchor(panelRect, new Vector2(0.5f, 0.5f), new Vector2(960f, 720f));
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

        /// <summary>
        /// 填充角色面板数据。
        /// baseAtk/baseDef 为基础属性（不含装备），equipAtk/equipDef/equipHp 为装备加成。
        /// </summary>
        public void ApplyCharacter(
            string name, string classLabel, int level,
            long exp, long expNext,
            int hp, int maxHp, int mp, int maxMp,
            int baseAtk, int equipAtk,
            int baseDef, int equipDef,
            int equipHp,
            int str, int agi, int intVal, int con, int defVal,
            int spd, float crtRate, int power,
            int gold, int honor, int attrPoints)
        {
            if (nameText != null) nameText.text = name;
            if (classText != null) classText.text = classLabel;
            if (levelText != null) levelText.text = $"Lv.{level}";
            if (expText != null) expText.text = $"经验: {exp} / {expNext}";
            if (hpText != null) hpText.text = $"生命: {hp} / {maxHp}";
            if (mpText != null) mpText.text = $"魔法: {mp} / {maxMp}";

            // 攻击力：基础（灰）+ 装备加成（绿）
            if (atkText != null)
            {
                atkText.text = equipAtk > 0
                    ? $"攻击: <color=#999999>{baseAtk}</color> <color=#33CC33>+{equipAtk}</color>"
                    : $"攻击: {baseAtk}";
                atkText.supportRichText = true;
            }

            // 防御力：基础（灰）+ 装备加成（绿）
            if (defText != null)
            {
                defText.text = equipDef > 0
                    ? $"防御: <color=#999999>{baseDef}</color> <color=#33CC33>+{equipDef}</color>"
                    : $"防御: {baseDef}";
                defText.supportRichText = true;
            }

            if (spdText != null) spdText.text = $"速度: {spd}";
            if (crtText != null) crtText.text = $"暴击: {crtRate:F1}%";
            if (powerText != null) powerText.text = $"战力: {power}";
            if (goldText != null) goldText.text = $"金币: {gold}";
            if (honorText != null) honorText.text = $"荣誉: {honor}";

            // 属性点显示 + 按钮启用/禁用
            if (attrPointsText != null)
                attrPointsText.text = $"可用属性点: {attrPoints}";

            bool canAssign = attrPoints > 0;
            if (strPlusBtn != null) strPlusBtn.interactable = canAssign;
            if (agiPlusBtn != null) agiPlusBtn.interactable = canAssign;
            if (intPlusBtn != null) intPlusBtn.interactable = canAssign;
            if (conPlusBtn != null) conPlusBtn.interactable = canAssign;
        }

        /// <summary>
        /// 填充装备栏数据，根据品质着色，显示强化等级、附魔属性和技能特效。
        /// </summary>
        public void ApplyEquipment(GoEquipmentData[] equipment)
        {
            // 先清空所有槽位
            for (int i = 0; i < 8; i++)
            {
                if (slotTexts[i] != null)
                {
                    slotTexts[i].text = $"{SlotNames[i]}: 空";
                    slotTexts[i].color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    slotTexts[i].supportRichText = false;
                }
                if (strengthenBtns[i] != null) strengthenBtns[i].interactable = false;
                if (enchantBtns[i] != null) enchantBtns[i].interactable = false;
                if (unequipBtns[i] != null) unequipBtns[i].interactable = false;
            }

            if (equipment == null) return;

            foreach (var eq in equipment)
            {
                int slot = eq.slot;
                if (slot < 0 || slot >= 8) continue;

                string label = $"{SlotNames[slot]}: {eq.name}";
                if (eq.strengthen_level > 0)
                    label += $" +{eq.strengthen_level}";

                // 技能特效摘要
                if (eq.skill_effects != null && eq.skill_effects.Length > 0)
                {
                    label += " <color=#FFD700>★(";
                    for (int i = 0; i < eq.skill_effects.Length; i++)
                    {
                        if (i > 0) label += " ";
                        label += eq.skill_effects[i].desc;
                    }
                    label += ")</color>";

                    if (slotTexts[slot] != null)
                        slotTexts[slot].supportRichText = true;
                }

                if (slotTexts[slot] != null)
                {
                    slotTexts[slot].text = label;
                    int qi = Mathf.Clamp(eq.quality, 0, QualityColors.Length - 1);
                    slotTexts[slot].color = QualityColors[qi];
                }

                // 有装备时启用操作按钮
                if (strengthenBtns[slot] != null) strengthenBtns[slot].interactable = true;
                if (enchantBtns[slot] != null) enchantBtns[slot].interactable = true;
                if (unequipBtns[slot] != null) unequipBtns[slot].interactable = true;
            }
        }

        private void Build(Transform root)
        {
            // Title
            CreateText(root, "角色 · 装备", 32, new Vector2(0.5f, 0.95f), new Vector2(400f, 48f), TextLight);

            // Close button
            var closeBtn = CreateButton(root, "X", new Vector2(0.96f, 0.95f), new Vector2(44f, 44f));
            closeBtn.onClick.AddListener(() => OnCloseRequested?.Invoke());

            // Left column: character info
            var leftBg = new GameObject("LeftBg", typeof(RectTransform), typeof(Image));
            leftBg.transform.SetParent(root, false);
            SetAnchor(leftBg.GetComponent<RectTransform>(), new Vector2(0.27f, 0.50f), new Vector2(440f, 580f));
            leftBg.GetComponent<Image>().color = PanelDark;

            var left = leftBg.transform;
            nameText = CreateText(left, "勇者", 26, new Vector2(0.5f, 0.93f), new Vector2(400f, 36f), TextGold);
            classText = CreateText(left, "战士", 20, new Vector2(0.5f, 0.87f), new Vector2(400f, 30f), TextLight);
            levelText = CreateText(left, "Lv.1", 22, new Vector2(0.5f, 0.81f), new Vector2(400f, 32f), TextLight);
            expText = CreateText(left, "经验: 0 / 100", 18, new Vector2(0.5f, 0.75f), new Vector2(400f, 28f), TextLight);

            CreateText(left, "— 基础属性 —", 18, new Vector2(0.5f, 0.69f), new Vector2(400f, 28f), Border);
            hpText = CreateText(left, "生命: 100 / 100", 18, new Vector2(0.5f, 0.64f), new Vector2(400f, 28f), TextLight);
            mpText = CreateText(left, "魔法: 50 / 50", 18, new Vector2(0.5f, 0.59f), new Vector2(400f, 28f), TextLight);
            atkText = CreateText(left, "攻击: 10", 18, new Vector2(0.5f, 0.54f), new Vector2(400f, 28f), TextLight);
            defText = CreateText(left, "防御: 10", 18, new Vector2(0.5f, 0.49f), new Vector2(400f, 28f), TextLight);
            spdText = CreateText(left, "速度: 10", 18, new Vector2(0.5f, 0.44f), new Vector2(400f, 28f), TextLight);
            crtText = CreateText(left, "暴击: 1.0%", 18, new Vector2(0.5f, 0.39f), new Vector2(400f, 28f), TextLight);
            powerText = CreateText(left, "战力: 500", 18, new Vector2(0.5f, 0.34f), new Vector2(400f, 28f), TextGold);

            // 属性分配区域
            CreateText(left, "— 属性分配 —", 18, new Vector2(0.5f, 0.28f), new Vector2(400f, 28f), Border);
            attrPointsText = CreateText(left, "可用属性点: 0", 16, new Vector2(0.5f, 0.23f), new Vector2(400f, 24f), TextGold);

            // 四维属性 +1 按钮
            float attrStartY = 0.18f;
            float attrSpacing = 0.05f;
            strPlusBtn = CreateAttrRow(left, "力量", attrStartY, () => OnAttrAssignRequested?.Invoke("str"));
            agiPlusBtn = CreateAttrRow(left, "敏捷", attrStartY - attrSpacing, () => OnAttrAssignRequested?.Invoke("agi"));
            intPlusBtn = CreateAttrRow(left, "智力", attrStartY - attrSpacing * 2, () => OnAttrAssignRequested?.Invoke("int"));
            conPlusBtn = CreateAttrRow(left, "体质", attrStartY - attrSpacing * 3, () => OnAttrAssignRequested?.Invoke("con"));

            // Right column: equipment
            var rightBg = new GameObject("RightBg", typeof(RectTransform), typeof(Image));
            rightBg.transform.SetParent(root, false);
            SetAnchor(rightBg.GetComponent<RectTransform>(), new Vector2(0.73f, 0.50f), new Vector2(440f, 580f));
            rightBg.GetComponent<Image>().color = PanelDark;

            var right = rightBg.transform;
            CreateText(right, "— 装备栏 —", 20, new Vector2(0.5f, 0.93f), new Vector2(400f, 32f), Border);

            for (int i = 0; i < 8; i++)
            {
                float y = 0.87f - i * 0.10f;

                slotTexts[i] = CreateText(right, $"{SlotNames[i]}: 空", 17, new Vector2(0.30f, y), new Vector2(240f, 26f), new Color(0.5f, 0.5f, 0.5f, 1f));

                var slotIdx = i;
                strengthenBtns[i] = CreateButton(right, "强化", new Vector2(0.62f, y), new Vector2(52f, 26f));
                strengthenBtns[i].onClick.AddListener(() => OnStrengthenRequested?.Invoke(slotIdx));
                strengthenBtns[i].interactable = false;

                enchantBtns[i] = CreateButton(right, "附魔", new Vector2(0.76f, y), new Vector2(52f, 26f));
                enchantBtns[i].onClick.AddListener(() => OnEnchantRequested?.Invoke(slotIdx));
                enchantBtns[i].interactable = false;

                unequipBtns[i] = CreateButton(right, "卸下", new Vector2(0.90f, y), new Vector2(52f, 26f));
                unequipBtns[i].onClick.AddListener(() => OnUnequipRequested?.Invoke(slotIdx));
                unequipBtns[i].interactable = false;
            }
        }

        private static Button CreateAttrRow(Transform parent, string label, float y, Action onPlus)
        {
            CreateText(parent, label, 16, new Vector2(0.25f, y), new Vector2(120f, 24f), TextLight);
            var btn = CreateButton(parent, "+1", new Vector2(0.45f, y), new Vector2(48f, 24f));
            btn.onClick.AddListener(() => onPlus?.Invoke());
            btn.interactable = false;
            return btn;
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
