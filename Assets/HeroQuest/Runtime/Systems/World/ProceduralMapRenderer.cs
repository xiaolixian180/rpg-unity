using UnityEngine;

namespace HeroQuest.Systems.World
{
    [ExecuteAlways]
    public sealed class ProceduralMapRenderer : MonoBehaviour
    {
        [SerializeField] private int width = 72;
        [SerializeField] private int height = 48;
        [SerializeField] private float tileSize = 1.15f;
        [SerializeField] private int seed = 20260523;
        [SerializeField] private int pixelsPerUnit = 64;

        private Sprite grassSprite;
        private Sprite dirtSprite;
        private Sprite darkGrassSprite;
        private Sprite treeTopSprite;
        private Sprite treeTrunkSprite;
        private Sprite rockSprite;
        private Sprite tuftSprite;

        private void OnEnable()
        {
            Build();
        }

        [ContextMenu("Rebuild Map")]
        public void Build()
        {
            ClearChildren();
            CreateSprites();

            var random = new System.Random(seed);
            var origin = new Vector2(-(width - 1) * tileSize * 0.5f, -(height - 1) * tileSize * 0.5f);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var roll = random.NextDouble();
                    var sprite = roll < 0.16 ? dirtSprite : roll < 0.35 ? darkGrassSprite : grassSprite;
                    var position = new Vector3(origin.x + x * tileSize, origin.y + y * tileSize, 0f);
                    AddSprite("Ground", sprite, position, Vector3.one * tileSize, -20);

                    if (roll > 0.78 && random.NextDouble() > 0.55)
                    {
                        AddTuft(position + RandomOffset(random, 0.28f));
                    }
                }
            }

            AddProps(random, origin);
        }

        private void AddProps(System.Random random, Vector2 origin)
        {
            for (var i = 0; i < 110; i++)
            {
                var position = RandomMapPosition(random, origin);
                if (Vector2.Distance(position, Vector2.zero) < 4.5f)
                {
                    continue;
                }

                if (random.NextDouble() < 0.68)
                {
                    AddTree(position);
                }
                else
                {
                    AddRock(position);
                }
            }
        }

        private Vector2 RandomMapPosition(System.Random random, Vector2 origin)
        {
            var x = origin.x + random.NextDouble() * (width - 1) * tileSize;
            var y = origin.y + random.NextDouble() * (height - 1) * tileSize;
            return new Vector2((float)x, (float)y);
        }

        private Vector3 RandomOffset(System.Random random, float radius)
        {
            return new Vector3((float)(random.NextDouble() * 2 - 1) * radius, (float)(random.NextDouble() * 2 - 1) * radius, 0f);
        }

        private void AddTree(Vector2 position)
        {
            AddSprite("TreeTrunk", treeTrunkSprite, new Vector3(position.x, position.y - 0.18f, 0f), Vector3.one, 2);
            AddSprite("TreeTop", treeTopSprite, new Vector3(position.x, position.y + 0.36f, 0f), Vector3.one * 1.25f, 4);
        }

        private void AddRock(Vector2 position)
        {
            AddSprite("Rock", rockSprite, new Vector3(position.x, position.y, 0f), Vector3.one, 3);
        }

        private void AddTuft(Vector3 position)
        {
            AddSprite("GrassTuft", tuftSprite, position, Vector3.one, -8);
        }

        private void AddSprite(string name, Sprite sprite, Vector3 position, Vector3 scale, int sortingOrder)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = position;
            child.transform.localScale = scale;

            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }

        private void ClearChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private void CreateSprites()
        {
            grassSprite = CreateSquareSprite(new Color(0.28f, 0.35f, 0.20f, 1f), new Color(0.20f, 0.25f, 0.15f, 1f));
            darkGrassSprite = CreateSquareSprite(new Color(0.18f, 0.27f, 0.17f, 1f), new Color(0.12f, 0.18f, 0.11f, 1f));
            dirtSprite = CreateSquareSprite(new Color(0.30f, 0.23f, 0.14f, 1f), new Color(0.20f, 0.15f, 0.10f, 1f));
            treeTopSprite = CreateEllipseSprite(82, 92, new Color(0.08f, 0.22f, 0.13f, 1f), new Color(0.04f, 0.09f, 0.06f, 1f));
            treeTrunkSprite = CreateRectangleSprite(24, 58, new Color(0.22f, 0.12f, 0.07f, 1f), new Color(0.10f, 0.06f, 0.04f, 1f));
            rockSprite = CreateEllipseSprite(70, 48, new Color(0.32f, 0.34f, 0.33f, 1f), new Color(0.14f, 0.15f, 0.15f, 1f));
            tuftSprite = CreateEllipseSprite(36, 24, new Color(0.42f, 0.48f, 0.22f, 1f), new Color(0.20f, 0.25f, 0.12f, 1f));
        }

        private Sprite CreateSquareSprite(Color fill, Color outline)
        {
            const int size = 64;
            var texture = NewTexture(size, size);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var edge = x < 2 || y < 2 || x > size - 3 || y > size - 3;
                    var noise = Mathf.PerlinNoise((x + seed) * 0.12f, (y - seed) * 0.12f);
                    var color = edge ? outline : Color.Lerp(fill * 0.85f, fill * 1.16f, noise);
                    color.a = 1f;
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        private Sprite CreateEllipseSprite(int width, int height, Color fill, Color outline)
        {
            var texture = NewTexture(width, height);
            var center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
            var radius = new Vector2(width * 0.5f, height * 0.5f);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var point = new Vector2((x - center.x) / radius.x, (y - center.y) / radius.y);
                    var dist = point.sqrMagnitude;
                    if (dist > 1f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        texture.SetPixel(x, y, dist > 0.78f ? outline : fill);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        private Sprite CreateRectangleSprite(int width, int height, Color fill, Color outline)
        {
            var texture = NewTexture(width, height);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var edge = x < 3 || y < 3 || x > width - 4 || y > height - 4;
                    texture.SetPixel(x, y, edge ? outline : fill);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        private Texture2D NewTexture(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
        }
    }
}
