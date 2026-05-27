using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.Systems.World
{
    public static class PrototypeRuntimeInstaller
    {
        public static void EnsureRuntimeObjects(Transform player)
        {
            if (player == null)
            {
                return;
            }

            EnsureMonsterSpawner(player);
            EnsureMiniMap(player);
        }

        private static void EnsureMonsterSpawner(Transform player)
        {
            var existing = Object.FindObjectOfType<WildMonsterSpawner>();
            if (existing != null)
            {
                existing.SetPlayer(player);
                existing.RefillMonsters();
                return;
            }

            var spawnerObject = new GameObject("Wild Monsters");
            var spawner = spawnerObject.AddComponent<WildMonsterSpawner>();
            spawner.SetPlayer(player);
            spawner.RefillMonsters();
        }

        private static void EnsureMiniMap(Transform player)
        {
            if (GameObject.Find("MiniMap Canvas") != null)
            {
                return;
            }

            var texture = new RenderTexture(384, 384, 16)
            {
                name = "RuntimeMiniMapTexture",
                filterMode = FilterMode.Point
            };

            var cameraObject = new GameObject("MiniMap Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.05f, 0.04f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 18f;
            camera.depth = -5;
            camera.targetTexture = texture;
            cameraObject.transform.position = player.position + new Vector3(0f, 0f, -10f);

            var follow = cameraObject.AddComponent<CameraFollow2D>();
            follow.SetTarget(player);

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

            var view = new GameObject("MiniMap View", typeof(RectTransform), typeof(RawImage));
            view.transform.SetParent(panel.transform, false);
            var viewRect = view.GetComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.offsetMin = new Vector2(8f, 8f);
            viewRect.offsetMax = new Vector2(-8f, -8f);
            view.GetComponent<RawImage>().texture = texture;
        }
    }
}
