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
            mapRenderer.EnsureMinimumSize(128, 88);
            mapRenderer.Build();

            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            player.AddComponent<TopDownPlayerController>();
            player.AddComponent<ProceduralCharacterRenderer>().Build();

            var network = new GameObject("Prototype Network");
            var networkClient = network.AddComponent<PrototypeNetworkClient>();
            networkClient.SetPlayerController(player.GetComponent<TopDownPlayerController>());

            var follow = cameraObject.AddComponent<CameraFollow2D>();
            follow.SetTarget(player.transform);
            follow.SetBounds(mapRenderer.GetWorldBounds());
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"Gameplay prototype scene generated: {ScenePath}");
        }

    }
}
