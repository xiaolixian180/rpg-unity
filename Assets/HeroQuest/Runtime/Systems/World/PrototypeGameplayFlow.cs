using System.Threading;
using HeroQuest.Net.Auth;
using HeroQuest.Systems.Character;
using TMPro;
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
        private GameObject warriorPanel;
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
            CreateText(loginPanel.transform, "Hero Quest", 54, new Vector2(0.5f, 0.66f), new Vector2(520f, 72f));

            var account = CreateInput(loginPanel.transform, "Account", LocalTestAuthService.TestAccount, false, new Vector2(0.5f, 0.53f));
            var password = CreateInput(loginPanel.transform, "Password", LocalTestAuthService.TestPassword, true, new Vector2(0.5f, 0.44f));
            var message = CreateText(loginPanel.transform, "test / test", 22, new Vector2(0.5f, 0.30f), new Vector2(520f, 44f));
            var loginButton = CreateButton(loginPanel.transform, "Login", new Vector2(0.5f, 0.36f), new Vector2(220f, 58f));
            activeLoginButton = loginButton;
            loginButton.onClick.AddListener(async () =>
            {
                message.text = "Logging in...";
                loginButton.interactable = false;
                var result = await authService.LoginAsync(account.text.Trim(), password.text, CancellationToken.None);
                loginButton.interactable = true;
                message.text = result.Message;
                if (result.Success)
                {
                    ShowWarriorSelect();
                }
            });

            EventSystem.current.SetSelectedGameObject(account.gameObject);
            account.ActivateInputField();
        }

        private void ShowWarriorSelect()
        {
            ClearPanels();

            warriorPanel = CreatePanel("Warrior Select Panel", new Color(0.035f, 0.043f, 0.038f, 0.96f));
            CreateText(warriorPanel.transform, "Choose Warrior", 46, new Vector2(0.5f, 0.78f), new Vector2(560f, 70f));

            var portrait = new GameObject("Warrior Portrait", typeof(RectTransform), typeof(Image));
            portrait.transform.SetParent(warriorPanel.transform, false);
            var portraitRect = portrait.GetComponent<RectTransform>();
            SetAnchor(portraitRect, new Vector2(0.5f, 0.56f), new Vector2(460f, 300f));
            var portraitImage = portrait.GetComponent<Image>();
            portraitImage.preserveAspect = true;

            var genderLabel = CreateText(warriorPanel.transform, "Male", 28, new Vector2(0.5f, 0.33f), new Vector2(320f, 44f));
            RefreshWarriorPreview(portraitImage, genderLabel);

            var toggleButton = CreateButton(warriorPanel.transform, "Toggle Gender", new Vector2(0.42f, 0.24f), new Vector2(220f, 58f));
            toggleButton.onClick.AddListener(() =>
            {
                selectedGender = selectedGender == CharacterGender.Male ? CharacterGender.Female : CharacterGender.Male;
                RefreshWarriorPreview(portraitImage, genderLabel);
            });

            var enterButton = CreateButton(warriorPanel.transform, "Enter Map", new Vector2(0.58f, 0.24f), new Vector2(220f, 58f));
            enterButton.onClick.AddListener(EnterMap);
        }

        private void RefreshWarriorPreview(Image portraitImage, TMP_Text genderLabel)
        {
            var isMale = selectedGender == CharacterGender.Male;
            portraitImage.sprite = Resources.Load<Sprite>(isMale ? "HeroQuest/Characters/Warrior_Male" : "HeroQuest/Characters/Warrior_Female");
            genderLabel.text = isMale ? "Male Warrior" : "Female Warrior";
        }

        private void EnterMap()
        {
            if (animator != null)
            {
                if (selectedGender == CharacterGender.Male)
                {
                    animator.Configure("HeroQuest/Playable/Warrior_Male_Walk_Right", "HeroQuest/Playable/Warrior_Male_Walk_Left");
                }
                else
                {
                    animator.Configure("HeroQuest/Playable/Warrior_Female_Walk_Right", "HeroQuest/Playable/Warrior_Female_Walk_Left");
                }
            }

            ClearPanels();
            SetGameplayEnabled(true);
            PrototypeRuntimeInstaller.EnsureRuntimeObjects(playerController.transform);
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

            if (warriorPanel != null)
            {
                Destroy(warriorPanel);
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

        private TMP_Text CreateText(Transform parent, string text, float size, Vector2 anchor, Vector2 sizeDelta)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.92f, 0.90f, 0.82f, 1f);
            SetAnchor(label.rectTransform, anchor, sizeDelta);
            return label;
        }

        private TMP_InputField CreateInput(Transform parent, string placeholder, string value, bool password, Vector2 anchor)
        {
            var inputObject = new GameObject(placeholder, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputObject.transform.SetParent(parent, false);
            inputObject.GetComponent<Image>().color = new Color(0.15f, 0.17f, 0.16f, 1f);
            SetAnchor(inputObject.GetComponent<RectTransform>(), anchor, new Vector2(420f, 56f));

            var input = inputObject.GetComponent<TMP_InputField>();
            input.contentType = password ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
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

        private TextMeshProUGUI CreateInputText(Transform parent, string text, Color color)
        {
            var textObject = new GameObject("Input Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 24;
            label.alignment = TextAlignmentOptions.Left;
            label.color = color;
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
