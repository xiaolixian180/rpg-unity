using System.IO;
using HeroQuest.Systems.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HeroQuest.Editor
{
    public static class GameplayPrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/GameplayPrototypeScene.scene";
        private const string GeneratedAssetFolder = "Assets/HeroQuest/Generated";
        private const string MiniMapTexturePath = GeneratedAssetFolder + "/MiniMapRenderTexture.renderTexture";

        [MenuItem("Hero Quest/Build Gameplay Prototype Scene")]
        public static void BuildGameplayPrototypeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.05f, 0.045f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 10.5f;
            camera.depth = 0;
            cameraObject.tag = "MainCamera";

            var map = new GameObject("Prototype Map");
            var mapRenderer = map.AddComponent<ProceduralMapRenderer>();
            mapRenderer.Build();

            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            player.AddComponent<TopDownPlayerController>();
            player.AddComponent<ProceduralCharacterRenderer>().Build();

            var monsters = new GameObject("Wild Monsters");
            var spawner = monsters.AddComponent<WildMonsterSpawner>();
            spawner.SetPlayer(player.transform);
            spawner.RefillMonsters();

            var network = new GameObject("Prototype Network");
            var networkClient = network.AddComponent<PrototypeNetworkClient>();
            networkClient.SetPlayerController(player.GetComponent<TopDownPlayerController>());

            var follow = cameraObject.AddComponent<CameraFollow2D>();
            follow.SetTarget(player.transform);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var minimapCameraObject = new GameObject("MiniMap Camera");
            var minimapCamera = minimapCameraObject.AddComponent<Camera>();
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = new Color(0.04f, 0.05f, 0.04f, 1f);
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = 18f;
            minimapCamera.depth = -5;
            minimapCamera.targetTexture = CreateMiniMapTexture();
            minimapCameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var minimapFollow = minimapCameraObject.AddComponent<CameraFollow2D>();
            minimapFollow.SetTarget(player.transform);

            CreateMinimapOverlay(minimapCamera.targetTexture);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"Gameplay prototype scene generated: {ScenePath}");
        }

        private static void CreateMinimapOverlay(RenderTexture minimapTexture)
        {
            var canvasObject = new GameObject("MiniMap Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = new GameObject("MiniMap Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(24f, -24f);
            panelRect.sizeDelta = new Vector2(230f, 230f);
            panel.GetComponent<Image>().color = new Color(0.02f, 0.025f, 0.02f, 0.86f);

            var rawImageObject = new GameObject("MiniMap View", typeof(RectTransform), typeof(RawImage));
            rawImageObject.transform.SetParent(panel.transform, false);
            var rawRect = rawImageObject.GetComponent<RectTransform>();
            rawRect.anchorMin = Vector2.zero;
            rawRect.anchorMax = Vector2.one;
            rawRect.offsetMin = new Vector2(8f, 8f);
            rawRect.offsetMax = new Vector2(-8f, -8f);
            var rawImage = rawImageObject.GetComponent<RawImage>();
            rawImage.texture = minimapTexture;
            rawImage.color = Color.white;
        }

        private static RenderTexture CreateMiniMapTexture()
        {
            if (!AssetDatabase.IsValidFolder(GeneratedAssetFolder))
            {
                AssetDatabase.CreateFolder("Assets/HeroQuest", "Generated");
            }

            var existing = AssetDatabase.LoadAssetAtPath<RenderTexture>(MiniMapTexturePath);
            if (existing != null)
            {
                existing.Release();
                existing.width = 384;
                existing.height = 384;
                existing.depth = 16;
                existing.filterMode = FilterMode.Point;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var texture = new RenderTexture(384, 384, 16)
            {
                name = "MiniMapRenderTexture",
                filterMode = FilterMode.Point
            };
            AssetDatabase.CreateAsset(texture, MiniMapTexturePath);
            return texture;
        }
    }
}
