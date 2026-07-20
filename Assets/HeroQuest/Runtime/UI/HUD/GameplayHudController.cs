using System;
using System.Collections.Generic;
using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.HUD
{
    public sealed class GameplayHudController : MonoBehaviour
    {
        private static readonly Color Panel = new(0.035f, 0.075f, 0.045f, 0.94f);
        private static readonly Color PanelDark = new(0.012f, 0.022f, 0.018f, 0.96f);
        private static readonly Color Border = new(0.26f, 0.42f, 0.24f, 1f);
        private static readonly Color Gold = new(0.78f, 0.58f, 0.22f, 1f);
        private static readonly Color TextColor = new(0.86f, 0.90f, 0.72f, 1f);

        private readonly List<Text> logLines = new();
        private Text characterNameText;
        private Image portraitImage;
        private Image hpFill;
        private Image mpFill;
        private Text hpText;
        private Text mpText;
        private Text clockText;
        private Text targetText;
        private Text targetLevelText;
        private Image targetHpFill;
        private Text targetHpText;
        private readonly List<Button> actionBarButtons = new();
        private readonly List<Image> actionBarCooldowns = new();
        private readonly List<Text> actionBarKeybinds = new();
        private readonly List<Text> actionBarSlotLabels = new();
        private readonly List<Text> consumableCounts = new();
        private readonly List<Text> consumableNames = new();

        public event Action<int> SkillSlotClicked;
        public event Action<string> CommandClicked;

        public static GameplayHudController Ensure()
        {
            var existing = FindFirstObjectByType<GameplayHudController>();
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                return existing;
            }

            var canvasObject = new GameObject("Gameplay HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var hud = canvasObject.AddComponent<GameplayHudController>();
            hud.Build(canvasObject.transform);
            return hud;
        }

        public void SetCharacter(string displayName, Sprite portrait)
        {
            if (characterNameText != null)
            {
                characterNameText.text = displayName;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.preserveAspect = true;
            }
        }

        public void SetVitals(float hpPercent, float mpPercent)
        {
            if (hpFill != null)
            {
                hpFill.fillAmount = Mathf.Clamp01(hpPercent);
                // HP 颜色：绿 > 60%，黄 30-60%，红 < 30%
                hpFill.color = hpPercent > 0.6f
                    ? new Color(0.20f, 0.72f, 0.20f, 1f)
                    : hpPercent > 0.3f
                        ? new Color(0.90f, 0.80f, 0.15f, 1f)
                        : new Color(0.85f, 0.12f, 0.10f, 1f);
            }

            if (mpFill != null)
            {
                mpFill.fillAmount = Mathf.Clamp01(mpPercent);
            }
        }

        public void SetVitalsText(long hp, long maxHp, long mp, long maxMp)
        {
            if (hpText != null)
            {
                hpText.text = $"{hp}/{maxHp}";
            }

            if (mpText != null)
            {
                mpText.text = $"{mp}/{maxMp}";
            }
        }

        public void SetTarget(string targetName, int level, float hpPercent, long hp, long maxHp)
        {
            if (targetText != null)
            {
                targetText.text = string.IsNullOrWhiteSpace(targetName) ? "无目标" : targetName;
            }

            if (targetLevelText != null)
            {
                targetLevelText.text = level > 0 ? $"Lv.{level}" : "";
            }

            if (targetHpFill != null)
            {
                targetHpFill.fillAmount = Mathf.Clamp01(hpPercent);
            }

            if (targetHpText != null)
            {
                targetHpText.text = level > 0 ? $"{hp}/{maxHp}" : "";
            }
        }

        public void AddLog(string message)
        {
            if (logLines.Count == 0)
            {
                return;
            }

            for (var i = 0; i < logLines.Count - 1; i++)
            {
                logLines[i].text = logLines[i + 1].text;
            }

            logLines[^1].text = message;
        }

        public void SetActionBarCooldown(int slotIndex, float fraction)
        {
            if (slotIndex < 0 || slotIndex >= actionBarCooldowns.Count)
            {
                return;
            }
            actionBarCooldowns[slotIndex].fillAmount = Mathf.Clamp01(fraction);
        }

        public void UpdateActionBarSlot(int slotIndex, string label)
        {
            if (slotIndex < 0 || slotIndex >= actionBarSlotLabels.Count) return;
            actionBarSlotLabels[slotIndex].text = label ?? "";
        }

        public void UpdateInventorySlot(int slot, string itemName, int count)
        {
            if (slot < 0 || slot >= consumableCounts.Count) return;
            consumableCounts[slot].text = count > 0 ? $"x{count}" : "";
            consumableNames[slot].text = count > 0 ? itemName : "";
        }

        public void ShowFloatingText(Vector3 worldPos, string text, Color color)
        {
            var go = new GameObject("FloatingText", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform, screenPos, null, out var localPos);
            rect.anchoredPosition = localPos;
            var label = CreateLabel("Dmg", go.transform, text, 28, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(200f, 40f));
            label.color = color;
            var anim = go.AddComponent<FloatingTextAnimator>();
            anim.label = label;
            Destroy(go, 1.2f);
        }

        private void Update()
        {
            if (clockText != null)
            {
                var totalSeconds = Mathf.FloorToInt(Time.time);
                clockText.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
            }
        }

        private void Build(Transform root)
        {
            BuildTopBar(root);
            BuildPlayerFrame(root);
            BuildTargetFrame(root);
            BuildActionBar(root);
            BuildChatLog(root);
            BuildConsumableBar(root);

            SetVitals(1f, 0.82f);
            SetTarget("", 0, 0, 0, 0);
            AddLog("[系统] 欢迎进入勇者远征。");
            AddLog("[任务] 探索荒野区域。");
        }

        private void BuildTopBar(Transform root)
        {
            var bar = CreatePanel("HUD Top Bar", root, PanelDark, Border);
            SetStretch(bar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), Vector2.zero);

            CreateLabel("Top Title", bar.transform, "勇者远征", 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(260f, 28f));
            clockText = CreateLabel("Clock", bar.transform, "00:00", 18, TextAnchor.MiddleCenter, new Vector2(0.62f, 0.5f), new Vector2(120f, 28f));

            var commands = new[] { "战局", "背包", "任务", "队伍", "设置" };
            for (var i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                var button = CreateButton($"Top {command}", bar.transform, command, new Vector2(0.62f + i * 0.055f, 0.5f), new Vector2(92f, 26f));
                button.onClick.AddListener(() =>
                {
                    CommandClicked?.Invoke(command);
                    AddLog($"[界面] {command}功能已预留。");
                });
            }
        }

        private void BuildPlayerFrame(Transform root)
        {
            var frame = CreatePanel("Player Frame", root, PanelDark, Border);
            SetAnchor(frame.rectTransform, new Vector2(0f, 1f), new Vector2(112f, 112f), new Vector2(8f, -44f), new Vector2(0f, 1f));

            portraitImage = CreateImage("Portrait", frame.transform, new Color(0.09f, 0.07f, 0.08f, 1f), new Vector2(0.5f, 0.66f), new Vector2(78f, 64f));
            characterNameText = CreateLabel("Character Name", frame.transform, "角色", 16, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.31f), new Vector2(100f, 20f));
            hpFill = CreateBar(frame.transform, "HP", new Vector2(0.5f, 0.16f), new Color(0.20f, 0.72f, 0.20f, 1f));
            hpText = CreateLabel("HP Text", frame.transform, "0/0", 12, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.16f), new Vector2(84f, 10f));
            mpFill = CreateBar(frame.transform, "MP", new Vector2(0.5f, 0.06f), new Color(0.13f, 0.24f, 0.86f, 1f));
            mpText = CreateLabel("MP Text", frame.transform, "0/0", 12, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.06f), new Vector2(84f, 10f));
        }

        private void BuildTargetFrame(Transform root)
        {
            var frame = CreatePanel("Target Frame", root, PanelDark, Border);
            SetAnchor(frame.rectTransform, new Vector2(0.5f, 1f), new Vector2(320f, 64f), new Vector2(0f, -40f), new Vector2(0.5f, 1f));

            targetText = CreateLabel("Target Name", frame.transform, "无目标", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.7f), new Vector2(280f, 24f));
            targetLevelText = CreateLabel("Target Level", frame.transform, "", 16, TextAnchor.MiddleCenter, new Vector2(0.08f, 0.7f), new Vector2(60f, 20f));
            targetHpFill = CreateBar(frame.transform, "Target HP", new Vector2(0.5f, 0.25f), new Color(0.76f, 0.10f, 0.08f, 1f));
            targetHpText = CreateLabel("Target HP Text", frame.transform, "", 14, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.25f), new Vector2(260f, 14f));
        }

        private void BuildActionBar(Transform root)
        {
            // WoW风格动作条：底部居中，12个槽位 (1-9, 0, -, =)
            var actionBar = CreatePanel("Action Bar", root, PanelDark, Border);
            SetAnchor(actionBar.rectTransform, new Vector2(0.5f, 0f), new Vector2(680f, 70f), new Vector2(0f, 16f), new Vector2(0.5f, 0f));

            var keys = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=" };

            for (var i = 0; i < 12; i++)
            {
                var index = i;
                var slot = CreatePanel($"Slot {keys[i]}", actionBar.transform, new Color(0.08f, 0.12f, 0.10f, 1f), Border);
                SetAnchor(slot.rectTransform, new Vector2(0.03f + i * 0.08f, 0.5f), new Vector2(48f, 48f), Vector2.zero, new Vector2(0.5f, 0.5f));
                slot.gameObject.AddComponent<Button>();

                // 按键绑定标签（左上角）
                var keybind = CreateLabel($"Key {keys[i]}", slot.transform, keys[i], 11, TextAnchor.UpperLeft, new Vector2(0.12f, 0.88f), new Vector2(20f, 14f));
                keybind.color = new Color(0.78f, 0.58f, 0.22f, 1f);
                actionBarKeybinds.Add(keybind);

                // 技能/物品名称（居中）
                var slotLabel = CreateLabel($"Label {keys[i]}", slot.transform, "", 11, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.4f), new Vector2(42f, 14f));
                actionBarSlotLabels.Add(slotLabel);

                // 冷却遮罩（扇形）
                var cdOverlay = new GameObject($"CD {keys[i]}", typeof(RectTransform), typeof(Image));
                cdOverlay.transform.SetParent(slot.transform, false);
                var cdRect = cdOverlay.GetComponent<RectTransform>();
                cdRect.anchorMin = Vector2.zero;
                cdRect.anchorMax = Vector2.one;
                cdRect.offsetMin = Vector2.zero;
                cdRect.offsetMax = Vector2.zero;
                var cdImage = cdOverlay.GetComponent<Image>();
                cdImage.color = new Color(0.15f, 0.30f, 0.80f, 0.5f);
                cdImage.type = Image.Type.Filled;
                cdImage.fillMethod = Image.FillMethod.Radial360;
                cdImage.fillClockwise = true;
                cdImage.fillAmount = 0f;
                actionBarCooldowns.Add(cdImage);

                var btn = slot.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    SkillSlotClicked?.Invoke(index);
                });
                actionBarButtons.Add(btn);
            }

            // 经验条（动作条下方）
            var xpBar = CreateImage("XP Bar", root, new Color(0.02f, 0.02f, 0.02f, 0.9f), new Vector2(0.5f, 0f), new Vector2(680f, 6f));
            SetAnchor(xpBar.rectTransform, new Vector2(0.5f, 0f), new Vector2(680f, 6f), new Vector2(0f, 4f), new Vector2(0.5f, 0f));
            var xpFill = new GameObject("XP Fill", typeof(RectTransform), typeof(Image));
            xpFill.transform.SetParent(xpBar.transform, false);
            var xpFillRect = xpFill.GetComponent<RectTransform>();
            xpFillRect.anchorMin = Vector2.zero;
            xpFillRect.anchorMax = Vector2.one;
            xpFillRect.offsetMin = Vector2.zero;
            xpFillRect.offsetMax = Vector2.zero;
            var xpFillImage = xpFill.GetComponent<Image>();
            xpFillImage.color = new Color(0.40f, 0.20f, 0.80f, 1f);
            xpFillImage.type = Image.Type.Filled;
            xpFillImage.fillMethod = Image.FillMethod.Horizontal;
            xpFillImage.fillAmount = 0.3f;

            // 微型菜单栏（动作条右侧）
            var microKeys = new[] { "C", "P", "I", "M", "O" };
            var microLabels = new[] { "角色", "技能", "背包", "地图", "社交" };
            var microCommands = new[] { "角色", "技能", "背包", "地图", "社交" };
            for (var i = 0; i < 5; i++)
            {
                var cmd = microCommands[i];
                var btn = CreateButton($"Micro {microKeys[i]}", root, microLabels[i], new Vector2(0.82f + i * 0.035f, 0f), new Vector2(50f, 32f));
                SetAnchor(btn.GetComponent<RectTransform>(), new Vector2(0.82f + i * 0.035f, 0f), new Vector2(50f, 32f), new Vector2(0f, 88f), new Vector2(0.5f, 0f));
                btn.onClick.AddListener(() => CommandClicked?.Invoke(cmd));
            }
        }

        private void BuildChatLog(Transform root)
        {
            var chat = CreatePanel("Battle Log", root, new Color(0.02f, 0.045f, 0.025f, 0.70f), Border);
            SetStretch(chat.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, 220f), new Vector2(392f, 432f));

            for (var i = 0; i < 8; i++)
            {
                var line = CreateLabel($"Log Line {i}", chat.transform, string.Empty, 16, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.88f - i * 0.105f), new Vector2(350f, 22f));
                line.color = new Color(0.28f, 0.95f, 0.25f, 1f);
                logLines.Add(line);
            }
        }

        private void BuildConsumableBar(Transform root)
        {
            var panel = CreatePanel("Consumable Bar", root, PanelDark, Border);
            SetAnchor(panel.rectTransform, new Vector2(1f, 0.42f), new Vector2(176f, 220f), new Vector2(-8f, 0f), new Vector2(1f, 0.5f));

            CreateLabel("Consumable Title", panel.transform, "消耗品", 14, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.92f), new Vector2(160f, 20f));

            for (var i = 0; i < 6; i++)
            {
                var slot = CreatePanel($"Item Slot {i}", panel.transform, new Color(0.06f, 0.10f, 0.08f, 1f), Border);
                SetAnchor(slot.rectTransform, new Vector2(0.5f, 0.80f - i * 0.13f), new Vector2(160f, 22f), Vector2.zero, new Vector2(0.5f, 0.5f));

                var nameLabel = CreateLabel($"Item Name {i}", slot.transform, "", 12, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(120f, 18f));
                consumableNames.Add(nameLabel);

                var countLabel = CreateLabel($"Item Count {i}", slot.transform, "", 12, TextAnchor.MiddleRight, new Vector2(0.5f, 0.5f), new Vector2(36f, 18f));
                consumableCounts.Add(countLabel);
            }
        }

        private Image CreatePanel(string name, Transform parent, Color color, Color borderColor)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
            panel.transform.SetParent(parent, false);
            var image = panel.GetComponent<Image>();
            image.color = color;
            var outline = panel.GetComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(2f, -2f);
            return image;
        }

        private Image CreateImage(string name, Transform parent, Color color, Vector2 anchor, Vector2 size)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            SetAnchor(image.rectTransform, anchor, size, Vector2.zero, new Vector2(0.5f, 0.5f));
            return image;
        }

        private Button CreateButton(string name, Transform parent, string text, Vector2 anchor, Vector2 size)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.10f, 0.19f, 0.12f, 1f);
            var outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = Gold;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            SetAnchor(buttonObject.GetComponent<RectTransform>(), anchor, size, Vector2.zero, new Vector2(0.5f, 0.5f));

            var label = CreateLabel("Label", buttonObject.transform, text, 16, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), size);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return buttonObject.GetComponent<Button>();
        }

        private Image CreateBar(Transform parent, string name, Vector2 anchor, Color fillColor)
        {
            var root = CreateImage($"{name} Bar", parent, new Color(0.02f, 0.02f, 0.02f, 1f), anchor, new Vector2(84f, 8f));
            var fill = new GameObject($"{name} Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(root.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            return fillImage;
        }

        private Text CreateLabel(string name, Transform parent, string text, float size, TextAnchor alignment, Vector2 anchor, Vector2 rectSize)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<Text>();
            label.text = text;
            label.font = ChineseFontProvider.GetFont();
            label.fontSize = Mathf.RoundToInt(size);
            label.alignment = alignment;
            label.color = TextColor;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            SetAnchor(label.rectTransform, anchor, rectSize, Vector2.zero, new Vector2(0.5f, 0.5f));
            return label;
        }

        private static void SetAnchor(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 anchoredPosition, Vector2 pivot)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private static void SetStretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }

    internal sealed class FloatingTextAnimator : MonoBehaviour
    {
        internal Text label;
        private float elapsed;
        private Vector2 startPos;
        private RectTransform rect;

        private void Start()
        {
            rect = GetComponent<RectTransform>();
            startPos = rect.anchoredPosition;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            var t = elapsed / 1.2f;
            // 向上漂浮
            rect.anchoredPosition = startPos + Vector2.up * (t * 80f);
            // 淡出
            if (label != null)
            {
                var c = label.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                label.color = c;
            }
        }
    }
}
