using System.Threading;
using HeroQuest.Domain;
using HeroQuest.Net.Auth;
using HeroQuest.Systems.Character;
using HeroQuest.UI.Core;
using HeroQuest.UI.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeroQuest.Systems.World
{
    public sealed class PrototypeGameplayFlow : MonoBehaviour
    {
        private TopDownPlayerController playerController;
        private GridSpriteSheetAnimator animator;
        private Canvas flowCanvas;
        private GameObject loginPanel;
        private GameObject characterPanel;
        private CharacterClass selectedClass = CharacterClass.Warrior;
        private CharacterGender selectedGender = CharacterGender.Male;
        private Button activeLoginButton;

        private readonly IAuthService authService = new LocalTestAuthService();

        public static PrototypeGameplayFlow Ensure(TopDownPlayerController controller)
        {
            var existing = FindObjectOfType<PrototypeGameplayFlow>();
            if (existing != null)
            {
                existing.Bind(controller);
                return existing;
            }

            var flowObject = new GameObject("Prototype Gameplay Flow");
            var flow = flowObject.AddComponent<PrototypeGameplayFlow>();
            flow.Bind(controller);
            return flow;
        }

        private void Bind(TopDownPlayerController controller)
        {
            playerController = controller;
            animator = controller.GetComponentInChildren<GridSpriteSheetAnimator>(true);
        }

        private void Start()
        {
            if (playerController == null)
            {
                playerController = FindObjectOfType<TopDownPlayerController>();
                animator = playerController != null ? playerController.GetComponentInChildren<GridSpriteSheetAnimator>(true) : null;
            }

            SetGameplayEnabled(false);
            BuildCanvas();
            ShowLogin();
        }

        private void Update()
        {
            if (activeLoginButton != null && Input.GetKeyDown(KeyCode.Return))
            {
                activeLoginButton.onClick.Invoke();
            }
        }

        private void BuildCanvas()
        {
            EnsureEventSystem();

            if (flowCanvas != null)
            {
                return;
            }

            var canvasObject = new GameObject("Prototype Flow Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            flowCanvas = canvasObject.GetComponent<Canvas>();
            flowCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            flowCanvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void ShowLogin()
        {
            ClearPanels();

            loginPanel = CreatePanel("Login Panel", new Color(0.035f, 0.043f, 0.038f, 0.96f));
            CreateText(loginPanel.transform, "勇者远征", 54, new Vector2(0.5f, 0.66f), new Vector2(520f, 72f));

            var account = CreateInput(loginPanel.transform, "账号", LocalTestAuthService.TestAccount, false, new Vector2(0.5f, 0.53f));
            var password = CreateInput(loginPanel.transform, "密码", LocalTestAuthService.TestPassword, true, new Vector2(0.5f, 0.44f));
            var message = CreateText(loginPanel.transform, "测试账号：test / test", 22, new Vector2(0.5f, 0.30f), new Vector2(520f, 44f));
            var loginButton = CreateButton(loginPanel.transform, "登录", new Vector2(0.5f, 0.36f), new Vector2(220f, 58f));
            activeLoginButton = loginButton;
            loginButton.onClick.AddListener(async () =>
            {
                message.text = "登录中...";
                loginButton.interactable = false;
                var result = await authService.LoginAsync(account.text.Trim(), password.text, CancellationToken.None);
                loginButton.interactable = true;
                message.text = result.Message;
                if (result.Success)
                {
                    ShowCharacterSelect();
                }
            });

            EventSystem.current.SetSelectedGameObject(account.gameObject);
            account.ActivateInputField();
        }

        private void ShowCharacterSelect()
        {
            ClearPanels();

            characterPanel = CreatePanel("Character Select Panel", new Color(0.025f, 0.030f, 0.026f, 0.97f));
            CreateText(characterPanel.transform, "选择角色", 44, new Vector2(0.5f, 0.91f), new Vector2(720f, 66f));
            CreateText(characterPanel.transform, "选择职业与性别后进入地图", 20, new Vector2(0.5f, 0.855f), new Vector2(760f, 38f));

            var portraitFrame = new GameObject("Portrait Frame", typeof(RectTransform), typeof(Image));
            portraitFrame.transform.SetParent(characterPanel.transform, false);
            var frameRect = portraitFrame.GetComponent<RectTransform>();
            SetAnchor(frameRect, new Vector2(0.50f, 0.54f), new Vector2(620f, 580f));
            portraitFrame.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.08f, 0.96f);

            var portrait = new GameObject("Character Portrait", typeof(RectTransform), typeof(Image));
            portrait.transform.SetParent(portraitFrame.transform, false);
            var portraitRect = portrait.GetComponent<RectTransform>();
            portraitRect.anchorMin = Vector2.zero;
            portraitRect.anchorMax = Vector2.one;
            portraitRect.offsetMin = new Vector2(28f, 28f);
            portraitRect.offsetMax = new Vector2(-28f, -28f);
            var portraitImage = portrait.GetComponent<Image>();
            portraitImage.preserveAspect = true;

            var className = CreateText(characterPanel.transform, string.Empty, 34, new Vector2(0.82f, 0.70f), new Vector2(380f, 52f));
            var genderLabel = CreateText(characterPanel.transform, string.Empty, 23, new Vector2(0.82f, 0.64f), new Vector2(380f, 40f));
            var description = CreateText(characterPanel.transform, string.Empty, 21, new Vector2(0.82f, 0.53f), new Vector2(390f, 100f));
            var stats = CreateText(characterPanel.transform, string.Empty, 21, new Vector2(0.82f, 0.38f), new Vector2(390f, 150f));

            var classButtons = new Button[CharacterRoster.AvailableClasses.Count];
            for (var i = 0; i < CharacterRoster.AvailableClasses.Count; i++)
            {
                var classIndex = i;
                var characterClass = CharacterRoster.AvailableClasses[i];
                var button = CreateButton(
                    characterPanel.transform,
                    GetClassLabel(characterClass),
                    new Vector2(0.16f, 0.72f - i * 0.13f),
                    new Vector2(260f, 68f));
                classButtons[i] = button;
                button.onClick.AddListener(() =>
                {
                    selectedClass = CharacterRoster.AvailableClasses[classIndex];
                    RefreshCharacterPreview(portraitImage, className, genderLabel, description, stats, classButtons, null, null);
                });
            }

            var maleButton = CreateButton(characterPanel.transform, "男性", new Vector2(0.43f, 0.17f), new Vector2(190f, 58f));
            var femaleButton = CreateButton(characterPanel.transform, "女性", new Vector2(0.57f, 0.17f), new Vector2(190f, 58f));
            maleButton.onClick.AddListener(() =>
            {
                selectedGender = CharacterGender.Male;
                RefreshCharacterPreview(portraitImage, className, genderLabel, description, stats, classButtons, maleButton, femaleButton);
            });
            femaleButton.onClick.AddListener(() =>
            {
                selectedGender = CharacterGender.Female;
                RefreshCharacterPreview(portraitImage, className, genderLabel, description, stats, classButtons, maleButton, femaleButton);
            });

            var enterButton = CreateButton(characterPanel.transform, "进入地图", new Vector2(0.82f, 0.17f), new Vector2(280f, 64f));
            enterButton.onClick.AddListener(EnterMap);

            RefreshCharacterPreview(portraitImage, className, genderLabel, description, stats, classButtons, maleButton, femaleButton);
        }

        private void RefreshCharacterPreview(
            Image portraitImage,
            Text className,
            Text genderLabel,
            Text description,
            Text stats,
            Button[] classButtons,
            Button maleButton,
            Button femaleButton)
        {
            var definition = CharacterRoster.Get(selectedClass, selectedGender);
            portraitImage.sprite = definition.LoadPortrait();
            className.text = GetClassLabel(selectedClass);
            genderLabel.text = selectedGender == CharacterGender.Male ? "男性" : "女性";
            description.text = GetClassDescription(selectedClass);
            stats.text =
                $"力量  {definition.BaseStats.strength}\n" +
                $"敏捷  {definition.BaseStats.agility}\n" +
                $"智力  {definition.BaseStats.intelligence}\n" +
                $"体质  {definition.BaseStats.constitution}\n" +
                $"防御  {definition.BaseStats.defense}";

            for (var i = 0; i < classButtons.Length; i++)
            {
                var selected = CharacterRoster.AvailableClasses[i] == selectedClass;
                classButtons[i].GetComponent<Image>().color = selected
                    ? new Color(0.54f, 0.42f, 0.20f, 1f)
                    : new Color(0.22f, 0.30f, 0.22f, 1f);
            }

            if (maleButton != null && femaleButton != null)
            {
                maleButton.GetComponent<Image>().color = selectedGender == CharacterGender.Male
                    ? new Color(0.54f, 0.42f, 0.20f, 1f)
                    : new Color(0.22f, 0.30f, 0.22f, 1f);
                femaleButton.GetComponent<Image>().color = selectedGender == CharacterGender.Female
                    ? new Color(0.54f, 0.42f, 0.20f, 1f)
                    : new Color(0.22f, 0.30f, 0.22f, 1f);
            }
        }

        private static string GetClassLabel(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => "战士",
                CharacterClass.Mage => "法师",
                CharacterClass.Archer => "弓箭手",
                CharacterClass.Priest => "牧师",
                _ => characterClass.ToString()
            };
        }

        private static string GetClassDescription(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => "近战前排职业，生存能力强，适合新手进入地图测试。",
                CharacterClass.Mage => "远程法术职业，爆发伤害高，后续可扩展元素技能。",
                CharacterClass.Archer => "远程敏捷职业，偏向持续输出和暴击成长。",
                CharacterClass.Priest => "辅助职业，后续可扩展治疗、护盾和团队增益。",
                _ => string.Empty
            };
        }

        private void EnterMap()
        {
            var definition = CharacterRoster.Get(selectedClass, selectedGender);
            var characterRenderer = playerController != null ? playerController.GetComponent<ProceduralCharacterRenderer>() : null;
            if (characterRenderer != null)
            {
                characterRenderer.Configure(
                    definition.ResourcePath,
                    definition.WalkRightResourcePath,
                    definition.WalkLeftResourcePath,
                    definition.WalkColumns,
                    definition.WalkRows);
                animator = playerController.GetComponentInChildren<GridSpriteSheetAnimator>(true);
            }

            if (animator != null)
            {
                animator.Configure(
                    definition.WalkRightResourcePath,
                    definition.WalkLeftResourcePath,
                    definition.WalkColumns,
                    definition.WalkRows);
            }

            ClearPanels();
            SetGameplayEnabled(true);
            PrototypeRuntimeInstaller.EnsureRuntimeObjects(playerController.transform);

            var hud = GameplayHudController.Ensure();
            hud.SetCharacter($"{GetClassLabel(selectedClass)} {(selectedGender == CharacterGender.Male ? "男" : "女")}", definition.LoadPortrait());
            hud.AddLog($"[角色] 已选择{GetClassLabel(selectedClass)}。");
        }

        private void SetGameplayEnabled(bool enabled)
        {
            if (playerController != null)
            {
                playerController.SetInputEnabled(enabled);
            }
        }

        private void ClearPanels()
        {
            activeLoginButton = null;

            if (loginPanel != null)
            {
                Destroy(loginPanel);
            }

            if (characterPanel != null)
            {
                Destroy(characterPanel);
            }
        }

        private GameObject CreatePanel(string name, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(flowCanvas.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private Text CreateText(Transform parent, string text, float size, Vector2 anchor, Vector2 sizeDelta)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<Text>();
            label.text = text;
            label.font = ChineseFontProvider.GetFont();
            label.fontSize = Mathf.RoundToInt(size);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.92f, 0.90f, 0.82f, 1f);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            SetAnchor(label.rectTransform, anchor, sizeDelta);
            return label;
        }

        private InputField CreateInput(Transform parent, string placeholder, string value, bool password, Vector2 anchor)
        {
            var inputObject = new GameObject(placeholder, typeof(RectTransform), typeof(Image), typeof(InputField));
            inputObject.transform.SetParent(parent, false);
            inputObject.GetComponent<Image>().color = new Color(0.15f, 0.17f, 0.16f, 1f);
            SetAnchor(inputObject.GetComponent<RectTransform>(), anchor, new Vector2(420f, 56f));

            var input = inputObject.GetComponent<InputField>();
            input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            input.text = value;

            var text = CreateInputText(inputObject.transform, string.Empty, new Color(0.92f, 0.90f, 0.82f, 1f));
            var placeholderText = CreateInputText(inputObject.transform, placeholder, new Color(0.62f, 0.66f, 0.62f, 1f));
            input.textComponent = text;
            input.placeholder = placeholderText;
            return input;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private Text CreateInputText(Transform parent, string text, Color color)
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

        private Button CreateButton(Transform parent, string text, Vector2 anchor, Vector2 sizeDelta)
        {
            var buttonObject = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.30f, 1f);
            SetAnchor(buttonObject.GetComponent<RectTransform>(), anchor, sizeDelta);
            var label = CreateText(buttonObject.transform, text, 24, new Vector2(0.5f, 0.5f), sizeDelta);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return buttonObject.GetComponent<Button>();
        }

        private static void SetAnchor(RectTransform rectTransform, Vector2 anchor, Vector2 sizeDelta)
        {
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = sizeDelta;
        }
    }
}
