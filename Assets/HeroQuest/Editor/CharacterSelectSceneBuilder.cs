using System.IO;
using HeroQuest.UI.Core;
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
    public static class CharacterSelectSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/CharacterSelectScene.scene";

        [MenuItem("Hero Quest/Build Character Select Scene")]
        public static void BuildCharacterSelectScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();

            var canvas = CreateCanvas();
            var viewRoot = CreatePanel("CharacterSelectView", canvas.transform, new Color(0.08f, 0.09f, 0.11f, 1f));
            Stretch(viewRoot.GetComponent<RectTransform>());

            var title = CreateText("Title", viewRoot.transform, "选择角色", 44, TextAlignmentOptions.Center);
            SetAnchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(420f, 70f));

            var portraitFrame = CreatePanel("PortraitFrame", viewRoot.transform, new Color(0.14f, 0.15f, 0.18f, 1f));
            SetAnchor(portraitFrame.GetComponent<RectTransform>(), new Vector2(0.08f, 0.15f), new Vector2(0.62f, 0.86f), Vector2.zero, Vector2.zero);

            var portrait = CreateImage("Portrait", portraitFrame.transform, Color.white);
            Stretch(portrait.rectTransform, new Vector2(24f, 24f), new Vector2(-24f, -24f));
            portrait.preserveAspect = true;

            var infoPanel = CreatePanel("InfoPanel", viewRoot.transform, new Color(0.11f, 0.12f, 0.15f, 1f));
            SetAnchor(infoPanel.GetComponent<RectTransform>(), new Vector2(0.66f, 0.23f), new Vector2(0.94f, 0.78f), Vector2.zero, Vector2.zero);

            var className = CreateText("ClassName", infoPanel.transform, "战士", 36, TextAlignmentOptions.Left);
            SetAnchor(className.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -42f), new Vector2(-24f, 56f));

            var gender = CreateText("Gender", infoPanel.transform, "男性", 24, TextAlignmentOptions.Left);
            SetAnchor(gender.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -96f), new Vector2(-24f, 40f));

            var description = CreateText("Description", infoPanel.transform, "近战物理，高生存，适合作为队伍前排。", 22, TextAlignmentOptions.TopLeft);
            SetAnchor(description.rectTransform, new Vector2(0f, 0.52f), new Vector2(1f, 0.82f), new Vector2(24f, 0f), new Vector2(-24f, 0f));

            var stats = CreateText("Stats", infoPanel.transform, "力量 8\n敏捷 4\n智力 2\n体质 7\n防御 5", 22, TextAlignmentOptions.TopLeft);
            SetAnchor(stats.rectTransform, new Vector2(0f, 0.15f), new Vector2(1f, 0.48f), new Vector2(24f, 0f), new Vector2(-24f, 0f));

            var previousButton = CreateButton("PreviousClassButton", viewRoot.transform, "上一职业");
            SetAnchor(previousButton.GetComponent<RectTransform>(), new Vector2(0.12f, 0.05f), new Vector2(0.27f, 0.13f), Vector2.zero, Vector2.zero);

            var nextButton = CreateButton("NextClassButton", viewRoot.transform, "下一职业");
            SetAnchor(nextButton.GetComponent<RectTransform>(), new Vector2(0.31f, 0.05f), new Vector2(0.46f, 0.13f), Vector2.zero, Vector2.zero);

            var genderButton = CreateButton("ToggleGenderButton", viewRoot.transform, "切换性别");
            SetAnchor(genderButton.GetComponent<RectTransform>(), new Vector2(0.50f, 0.05f), new Vector2(0.65f, 0.13f), Vector2.zero, Vector2.zero);

            var createButton = CreateButton("CreateButton", viewRoot.transform, "创建角色");
            SetAnchor(createButton.GetComponent<RectTransform>(), new Vector2(0.75f, 0.05f), new Vector2(0.90f, 0.13f), Vector2.zero, Vector2.zero);

            var manager = canvas.gameObject.AddComponent<UIManager>();
            _ = manager;

            var characterView = viewRoot.AddComponent<CharacterSelectView>();
            AssignSerializedField(characterView, "portraitImage", portrait);
            AssignSerializedField(characterView, "classNameText", className);
            AssignSerializedField(characterView, "genderText", gender);
            AssignSerializedField(characterView, "descriptionText", description);
            AssignSerializedField(characterView, "statsText", stats);
            AssignSerializedField(characterView, "previousClassButton", previousButton);
            AssignSerializedField(characterView, "nextClassButton", nextButton);
            AssignSerializedField(characterView, "toggleGenderButton", genderButton);
            AssignSerializedField(characterView, "createButton", createButton);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"Character select scene generated: {ScenePath}");
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.07f, 0.09f, 1f);
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

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = new Color(0.94f, 0.92f, 0.86f, 1f);
            label.enableWordWrapping = true;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, string text)
        {
            var buttonObject = CreatePanel(name, parent, new Color(0.22f, 0.30f, 0.42f, 1f));
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();

            var label = CreateText("Label", buttonObject.transform, text, 24, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            return button;
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
            rectTransform.offsetMin = anchorMin == anchorMax ? rectTransform.offsetMin : Vector2.zero;
            rectTransform.offsetMax = anchorMin == anchorMax ? rectTransform.offsetMax : Vector2.zero;
        }

        private static void AssignSerializedField(Object target, string fieldName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(fieldName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
