using UnityEngine;
using UnityEngine.UI;
using HeroQuest.UI.Core;
using HeroQuest.UI.HUD;

namespace HeroQuest.Systems.World
{
    public static class PrototypeRuntimeInstaller
    {
        private const int MinimumMapWidth = 128;
        private const int MinimumMapHeight = 88;

        public static void EnsureRuntimeObjects(Transform player)
        {
            if (player == null)
            {
                return;
            }

            var mapBounds = EnsureMapCoverage();
            EnsureMainCameraBounds(mapBounds);
            EnsureMonsterSpawner(player);
            GameplayHudController.Ensure();
            EnsureMiniMap(player, mapBounds);
        }

        private static Bounds EnsureMapCoverage()
        {
            var map = Object.FindObjectOfType<ProceduralMapRenderer>();
            if (map == null)
            {
                var mapObject = new GameObject("Prototype Map");
                map = mapObject.AddComponent<ProceduralMapRenderer>();
            }

            map.EnsureMinimumSize(MinimumMapWidth, MinimumMapHeight);
            return map.GetWorldBounds();
        }

        private static void EnsureMainCameraBounds(Bounds mapBounds)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            var follow = mainCamera.GetComponent<CameraFollow2D>();
            if (follow != null)
            {
                follow.SetBounds(mapBounds);
            }
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

        private static void EnsureMiniMap(Transform player, Bounds mapBounds)
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
            camera.orthographicSize = 22f;
            camera.depth = -5;
            camera.targetTexture = texture;
            cameraObject.transform.position = player.position + new Vector3(0f, 0f, -10f);

            var follow = cameraObject.AddComponent<CameraFollow2D>();
            follow.SetTarget(player);
            follow.SetBounds(mapBounds);

            var canvasObject = new GameObject("MiniMap Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 75;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = new GameObject("MiniMap Root", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.zero;
            panelRect.pivot = Vector2.zero;
            panelRect.anchoredPosition = new Vector2(102f, 30f);
            panelRect.sizeDelta = new Vector2(150f, 150f);
            panel.GetComponent<Image>().color = new Color(0.01f, 0.012f, 0.01f, 0.78f);

            var mask = new GameObject("MiniMap Circle Mask", typeof(RectTransform), typeof(CircleMaskGraphic), typeof(Mask));
            mask.transform.SetParent(panel.transform, false);
            var maskRect = mask.GetComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = new Vector2(18f, 18f);
            maskRect.offsetMax = new Vector2(-18f, -18f);
            mask.GetComponent<CircleMaskGraphic>().color = Color.white;
            var maskComponent = mask.GetComponent<Mask>();
            maskComponent.showMaskGraphic = false;

            var view = new GameObject("MiniMap View", typeof(RectTransform), typeof(RawImage));
            view.transform.SetParent(mask.transform, false);
            var viewRect = view.GetComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.offsetMin = Vector2.zero;
            viewRect.offsetMax = Vector2.zero;
            var rawImage = view.GetComponent<RawImage>();
            rawImage.texture = texture;
            rawImage.color = Color.white;

            var playerDot = new GameObject("MiniMap Player Dot", typeof(RectTransform), typeof(Image));
            playerDot.transform.SetParent(panel.transform, false);
            var dotRect = playerDot.GetComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.5f, 0.5f);
            dotRect.anchorMax = new Vector2(0.5f, 0.5f);
            dotRect.pivot = new Vector2(0.5f, 0.5f);
            dotRect.anchoredPosition = Vector2.zero;
            dotRect.sizeDelta = new Vector2(11f, 11f);
            playerDot.GetComponent<Image>().sprite = CreateCircleSprite(32, new Color(0.86f, 0.18f, 0.12f, 1f), new Color(0.08f, 0.02f, 0.02f, 1f));

            var frame = new GameObject("MiniMap Frame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(panel.transform, false);
            var frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            var frameImage = frame.GetComponent<Image>();
            frameImage.sprite = CreateMiniMapFrameSprite(256);
            frameImage.color = Color.white;
            frameImage.raycastTarget = false;
        }

        private static Sprite CreateMiniMapFrameSprite(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.5f;
            var outer = new Color(0.015f, 0.014f, 0.011f, 1f);
            var paper = new Color(0.50f, 0.39f, 0.22f, 1f);
            var paperLight = new Color(0.68f, 0.55f, 0.32f, 1f);
            var innerInk = new Color(0.05f, 0.035f, 0.02f, 1f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                    if (distance > 1f || distance < 0.68f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    var noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    if (distance > 0.92f || distance < 0.73f)
                    {
                        texture.SetPixel(x, y, outer);
                    }
                    else if (distance < 0.78f)
                    {
                        texture.SetPixel(x, y, innerInk);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.Lerp(paper, paperLight, noise));
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateCircleSprite(int size, Color fill, Color outline)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                    texture.SetPixel(x, y, distance > 1f ? Color.clear : distance > 0.72f ? outline : fill);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
