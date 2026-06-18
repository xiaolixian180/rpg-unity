using UnityEngine;

namespace HeroQuest.Systems.World
{
    [ExecuteAlways]
    public sealed class ProceduralMapRenderer : MonoBehaviour
    {
        private const string BackgroundPath = "HeroQuest/World/Grassland_Background";

        private static readonly string[] TreePaths =
        {
            "HeroQuest/World/Props/Tree_01",
            "HeroQuest/World/Props/Tree_02",
            "HeroQuest/World/Props/Tree_03"
        };

        private static readonly string[] RockPaths =
        {
            "HeroQuest/World/Props/Rock_01",
            "HeroQuest/World/Props/Rock_02"
        };

        private static readonly string[] BushPaths =
        {
            "HeroQuest/World/Props/Bush_01",
            "HeroQuest/World/Props/Bush_02",
            "HeroQuest/World/Props/Bush_03",
            "HeroQuest/World/Props/Bush_04",
            "HeroQuest/World/Props/Bush_05",
            "HeroQuest/World/Props/Bush_06"
        };

        [SerializeField] private int width = 128;
        [SerializeField] private int height = 88;
        [SerializeField] private float tileSize = 1.15f;
        [SerializeField] private int seed = 20260523;
        [SerializeField] private int propCount = 260;
        [SerializeField] private float safeSpawnRadius = 5.5f;

        private Sprite backgroundSprite;
        private Sprite[] treeSprites;
        private Sprite[] rockSprites;
        private Sprite[] bushSprites;

        private void OnEnable()
        {
            Build();
        }

        [ContextMenu("Rebuild Map")]
        public void Build()
        {
            ClearChildren();
            LoadSprites();
            AddBackground();

            var random = new System.Random(seed);
            var origin = new Vector2(-(width - 1) * tileSize * 0.5f, -(height - 1) * tileSize * 0.5f);
            AddProps(random, origin);
        }

        public Bounds GetWorldBounds()
        {
            var size = new Vector3(width * tileSize, height * tileSize, 0f);
            return new Bounds(transform.position, size);
        }

        public void EnsureMinimumSize(int minimumWidth, int minimumHeight)
        {
            var nextWidth = Mathf.Max(width, minimumWidth);
            var nextHeight = Mathf.Max(height, minimumHeight);
            if (nextWidth == width && nextHeight == height)
            {
                return;
            }

            width = nextWidth;
            height = nextHeight;
            propCount = Mathf.Max(propCount, Mathf.RoundToInt(width * height * 0.023f));
            Build();
        }

        private void LoadSprites()
        {
            backgroundSprite = Resources.Load<Sprite>(BackgroundPath);
            treeSprites = LoadSprites(TreePaths);
            rockSprites = LoadSprites(RockPaths);
            bushSprites = LoadSprites(BushPaths);
        }

        private static Sprite[] LoadSprites(string[] resourcePaths)
        {
            var sprites = new Sprite[resourcePaths.Length];
            for (var i = 0; i < resourcePaths.Length; i++)
            {
                sprites[i] = Resources.Load<Sprite>(resourcePaths[i]);
            }

            return sprites;
        }

        private void AddBackground()
        {
            if (backgroundSprite == null)
            {
                return;
            }

            var bounds = backgroundSprite.bounds.size;
            var worldWidth = width * tileSize;
            var worldHeight = height * tileSize;
            var coverScale = Mathf.Max(worldWidth / bounds.x, worldHeight / bounds.y);
            AddSprite("Grassland Background", backgroundSprite, Vector3.zero, Vector3.one * coverScale, -20);
        }

        private void AddProps(System.Random random, Vector2 origin)
        {
            for (var i = 0; i < propCount; i++)
            {
                var position = RandomMapPosition(random, origin);
                if (Vector2.Distance(position, Vector2.zero) < safeSpawnRadius)
                {
                    continue;
                }

                var roll = random.NextDouble();
                if (roll < 0.42)
                {
                    AddTree(random, position);
                }
                else if (roll < 0.67)
                {
                    AddRock(random, position);
                }
                else
                {
                    AddBush(random, position);
                }
            }
        }

        private Vector2 RandomMapPosition(System.Random random, Vector2 origin)
        {
            var x = origin.x + random.NextDouble() * (width - 1) * tileSize;
            var y = origin.y + random.NextDouble() * (height - 1) * tileSize;
            return new Vector2((float)x, (float)y);
        }

        private void AddTree(System.Random random, Vector2 position)
        {
            var sprite = Pick(treeSprites, random);
            var height = Mathf.Lerp(3.1f, 4.0f, (float)random.NextDouble());
            AddWorldSprite("Tree", sprite, position, height, 4);
        }

        private void AddRock(System.Random random, Vector2 position)
        {
            var sprite = Pick(rockSprites, random);
            var height = Mathf.Lerp(0.9f, 1.45f, (float)random.NextDouble());
            AddWorldSprite("Rock", sprite, position, height, 2);
        }

        private void AddBush(System.Random random, Vector2 position)
        {
            var sprite = Pick(bushSprites, random);
            var height = Mathf.Lerp(0.55f, 0.9f, (float)random.NextDouble());
            AddWorldSprite("Bush", sprite, position, height, -6);
        }

        private static Sprite Pick(Sprite[] sprites, System.Random random)
        {
            return sprites == null || sprites.Length == 0 ? null : sprites[random.Next(0, sprites.Length)];
        }

        private void AddWorldSprite(string name, Sprite sprite, Vector2 position, float targetHeight, int sortingOrder)
        {
            if (sprite == null)
            {
                return;
            }

            var spriteHeight = Mathf.Max(0.001f, sprite.bounds.size.y);
            var scale = targetHeight / spriteHeight;
            AddSprite(name, sprite, new Vector3(position.x, position.y, 0f), Vector3.one * scale, sortingOrder);
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
    }
}
