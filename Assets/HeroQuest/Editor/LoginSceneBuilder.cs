using System.IO;
using HeroQuest.UI.Screens;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HeroQuest.Editor
{
    public static class LoginSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/LoginScene.scene";

        [MenuItem("Hero Quest/Build Login Scene")]
        public static void BuildLoginScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();

            var canvas = CreateCanvas();
            var root = CreatePanel("LoginView", canvas.transform, new Color(0.07f, 0.08f, 0.075f, 1f));
            Stretch(root.GetComponent<RectTransform>());

            var title = CreateText("Title", root.transform, "Hero Quest", 56, TextAlignmentOptions.Center);
            SetAnchor(title.rectTransform, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(520f, 80f));

            var account = CreateInput("AccountInput", root.transform, "Account", false);
            SetAnchor(account.GetComponent<RectTransform>(), new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), Vector2.zero, new Vector2(420f, 56f));

            var password = CreateInput("PasswordInput", root.transform, "Password", true);
            SetAnchor(password.GetComponent<RectTransform>(), new Vector2(0.5f, 0.47f), new Vector2(0.5f, 0.47f), Vector2.zero, new Vector2(420f, 56f));

            var button = CreateButton("LoginButton", root.transform, "Login");
            SetAnchor(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), Vector2.zero, new Vector2(220f, 58f));

            var message = CreateText("MessageText", root.transform, "test / test", 22, TextAlignmentOptions.Center);
            SetAnchor(message.rectTransform, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), Vector2.zero, new Vector2(520f, 46f));

            var loginView = root.AddComponent<LoginView>();
            AssignSerializedField(loginView, "accountInput", account);
            AssignSerializedField(loginView, "passwordInput", password);
            AssignSerializedField(loginView, "messageText", message);
            AssignSerializedField(loginView, "loginButton", button);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AddSceneToBuildSettings("Assets/Scenes/CharacterSelectScene.scene");
            AddSceneToBuildSettings("Assets/Scenes/GameplayPrototypeScene.scene");
            AssetDatabase.Refresh();

            Debug.Log($"Login scene generated: {ScenePath}");
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.05f, 0.048f, 1f);
            camera.orthographic = true;
            cameraObject.tag = "MainCamera";
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static TMP_InputField CreateInput(string name, Transform parent, string placeholder, bool password)
        {
            var inputObject = CreatePanel(name, parent, new Color(0.15f, 0.17f, 0.16f, 1f));
            var input = inputObject.AddComponent<TMP_InputField>();
            input.contentType = password ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;

            var text = CreateText("Text", inputObject.transform, string.Empty, 24, TextAlignmentOptions.Left);
            Stretch(text.rectTransform, new Vector2(18f, 0f), new Vector2(-18f, 0f));

            var placeholderText = CreateText("Placeholder", inputObject.transform, placeholder, 24, TextAlignmentOptions.Left);
            Stretch(placeholderText.rectTransform, new Vector2(18f, 0f), new Vector2(-18f, 0f));
            placeholderText.color = new Color(0.62f, 0.66f, 0.62f, 1f);

            input.textComponent = text;
            input.placeholder = placeholderText;
            return input;
        }

        private static Button CreateButton(string name, Transform parent, string text)
        {
            var buttonObject = CreatePanel(name, parent, new Color(0.30f, 0.42f, 0.30f, 1f));
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();

            var label = CreateText("Label", buttonObject.transform, text, 26, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            return button;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = new Color(0.92f, 0.90f, 0.82f, 1f);
            return label;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            Stretch(rectTransform, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static void SetAnchor(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
        }

        private static void AssignSerializedField(Object target, string fieldName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(fieldName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(scene => scene.path == scenePath))
            {
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
