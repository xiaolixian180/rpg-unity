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
        private Text clockText;
        private Text targetText;

        public event Action<int> SkillSlotClicked;
        public event Action<string> CommandClicked;

        public static GameplayHudController Ensure()
        {
            var existing = FindObjectOfType<GameplayHudController>();
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
            }

            if (mpFill != null)
            {
                mpFill.fillAmount = Mathf.Clamp01(mpPercent);
            }
        }

        public void SetTarget(string targetName)
        {
            if (targetText != null)
            {
                targetText.text = string.IsNullOrWhiteSpace(targetName) ? "无目标" : targetName;
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
            BuildChatLog(root);
            BuildBottomBar(root);

            SetVitals(1f, 0.82f);
            SetTarget("无目标");
            AddLog("[系统] 欢迎进入勇者远征。");
            AddLog("[任务] 探索荒野区域。");
            AddLog("[提示] 技能栏已预留绑定入口。");
        }

        private void BuildTopBar(Transform root)
        {
            var bar = CreatePanel("HUD Top Bar", root, PanelDark, Border);
            SetStretch(bar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), Vector2.zero);

            CreateLabel("Top Title", bar.transform, "勇者远征", 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(260f, 28f));
            clockText = CreateLabel("Clock", bar.transform, "00:00", 18, TextAnchor.MiddleCenter, new Vector2(0.62f, 0.5f), new Vector2(120f, 28f));

            var commands = new[] { "背包", "任务", "队伍", "设置" };
            for (var i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                var button = CreateButton($"Top {command}", bar.transform, command, new Vector2(0.78f + i * 0.055f, 0.5f), new Vector2(92f, 26f));
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
            hpFill = CreateBar(frame.transform, "HP", new Vector2(0.5f, 0.16f), new Color(0.76f, 0.10f, 0.08f, 1f));
            mpFill = CreateBar(frame.transform, "MP", new Vector2(0.5f, 0.06f), new Color(0.13f, 0.24f, 0.86f, 1f));
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

        private void BuildBottomBar(Transform root)
        {
            var deck = CreatePanel("HUD Bottom Deck", root, PanelDark, Border);
            SetStretch(deck.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 206f));

            var miniFrame = CreatePanel("MiniMap Dock", deck.transform, Panel, Border);
            SetAnchor(miniFrame.rectTransform, new Vector2(0f, 0f), new Vector2(360f, 184f), new Vector2(16f, 12f), Vector2.zero);
            CreateLabel("MiniMap Dock Label", miniFrame.transform, "小地图", 16, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.95f), new Vector2(120f, 22f));

            var info = CreatePanel("Character Info Dock", deck.transform, Panel, Border);
            SetAnchor(info.rectTransform, new Vector2(0f, 0f), new Vector2(470f, 184f), new Vector2(392f, 12f), Vector2.zero);
            targetText = CreateLabel("Target Text", info.transform, "无目标", 22, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.77f), new Vector2(410f, 30f));
            CreateLabel("Stats Text", info.transform, "攻击  397-419\n防御  62\n速度  34\n状态  就绪", 18, TextAnchor.MiddleLeft, new Vector2(0.50f, 0.42f), new Vector2(410f, 96f));

            var quick = CreatePanel("Quick Bar", deck.transform, Panel, Border);
            SetAnchor(quick.rectTransform, new Vector2(0.5f, 0f), new Vector2(510f, 70f), new Vector2(0f, 122f), new Vector2(0.5f, 0f));
            for (var i = 0; i < 8; i++)
            {
                var index = i;
                var button = CreateIconButton($"Quick {i + 1}", quick.transform, i + 1, new Vector2(0.08f + i * 0.12f, 0.5f), new Vector2(48f, 48f));
                button.onClick.AddListener(() =>
                {
                    SkillSlotClicked?.Invoke(index);
                    AddLog($"[技能] 已选择快捷栏 {index + 1}。");
                });
            }

            var skills = CreatePanel("Skill Panel", deck.transform, Panel, Border);
            SetAnchor(skills.rectTransform, new Vector2(1f, 0f), new Vector2(420f, 184f), new Vector2(-436f, 12f), new Vector2(1f, 0f));
            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    var index = row * 4 + col;
                    var button = CreateIconButton($"Skill {index + 1}", skills.transform, index + 1, new Vector2(0.14f + col * 0.24f, 0.78f - row * 0.31f), new Vector2(68f, 48f));
                    button.onClick.AddListener(() =>
                    {
                        SkillSlotClicked?.Invoke(index);
                        AddLog($"[技能] 技能槽 {index + 1} 已预留。");
                    });
                }
            }

            var commands = CreatePanel("Command Panel", deck.transform, Panel, Border);
            SetAnchor(commands.rectTransform, new Vector2(1f, 0f), new Vector2(344f, 184f), new Vector2(-84f, 12f), new Vector2(1f, 0f));
            CreateCommandGrid(commands.transform);
        }

        private void CreateCommandGrid(Transform parent)
        {
            var commands = new[] { "移动", "攻击", "技能", "宠物", "背包", "锻造", "商店", "交易", "排行" };
            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    var command = commands[row * 3 + col];
                    var button = CreateButton(command, parent, command, new Vector2(0.18f + col * 0.32f, 0.78f - row * 0.31f), new Vector2(86f, 48f));
                    button.onClick.AddListener(() =>
                    {
                        CommandClicked?.Invoke(command);
                        AddLog($"[界面] {command}指令已预留。");
                    });
                }
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

        private Button CreateIconButton(string name, Transform parent, int index, Vector2 anchor, Vector2 size)
        {
            var button = CreateButton(name, parent, index.ToString(), anchor, size);
            button.GetComponent<Image>().color = new Color(0.09f, 0.12f, 0.10f, 1f);
            return button;
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
}
