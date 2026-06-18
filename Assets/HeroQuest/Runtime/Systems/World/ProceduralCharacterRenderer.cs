using UnityEngine;

namespace HeroQuest.Systems.World
{
    [ExecuteAlways]
    public sealed class ProceduralCharacterRenderer : MonoBehaviour
    {
        [SerializeField] private int pixelsPerUnit = 64;
        [SerializeField] private string playableSpriteResourcePath = "HeroQuest/Playable/Warrior_Male_Player";
        [SerializeField] private string walkRightResourcePath = "HeroQuest/Playable/Warrior_Male_Walk_Right";
        [SerializeField] private string walkLeftResourcePath = "HeroQuest/Playable/Warrior_Male_Walk_Left";
        [SerializeField] private int walkColumns = 8;
        [SerializeField] private int walkRows = 8;

        public void Configure(
            string playableResourcePath,
            string walkRightResourcePath,
            string walkLeftResourcePath,
            int columns,
            int rows)
        {
            playableSpriteResourcePath = playableResourcePath;
            this.walkRightResourcePath = walkRightResourcePath;
            this.walkLeftResourcePath = walkLeftResourcePath;
            walkColumns = Mathf.Max(1, columns);
            walkRows = Mathf.Max(1, rows);
            Build();
        }

        private void OnEnable()
        {
            Build();
        }

        [ContextMenu("Rebuild Character")]
        public void Build()
        {
            ClearChildren();

            var root = new GameObject("VisualRoot");
            root.transform.SetParent(transform, false);

            AddSprite(root.transform, "Shadow", CreateEllipseSprite(96, 28, new Color(0f, 0f, 0f, 0.28f)), new Vector3(0f, -0.10f, 0f), new Vector3(0.72f, 0.24f, 1f), -2);

            var walkRight = Resources.Load<Texture2D>(walkRightResourcePath);
            if (walkRight != null)
            {
                var animatedObject = new GameObject("AnimatedSprite");
                animatedObject.transform.SetParent(root.transform, false);
                animatedObject.transform.localPosition = Vector3.zero;
                var spriteRenderer = animatedObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = 3;
                var animator = animatedObject.AddComponent<GridSpriteSheetAnimator>();
                animator.Configure(walkRightResourcePath, walkLeftResourcePath, walkColumns, walkRows);
                return;
            }

            var playableSprite = Resources.Load<Sprite>(playableSpriteResourcePath);
            if (playableSprite != null)
            {
                AddSprite(root.transform, "PlayableSprite", playableSprite, new Vector3(0f, 0.04f, 0f), Vector3.one * 0.16f, 3);
                return;
            }

            AddSprite(root.transform, "Body", CreateEllipseSprite(72, 96, new Color(0.28f, 0.34f, 0.27f, 1f)), new Vector3(0f, -0.18f, 0f), Vector3.one, 1);
            AddSprite(root.transform, "Head", CreateEllipseSprite(68, 68, new Color(0.93f, 0.78f, 0.60f, 1f)), new Vector3(0f, 0.62f, 0f), Vector3.one, 3);
            AddSprite(root.transform, "Hair", CreateEllipseSprite(76, 38, new Color(0.12f, 0.10f, 0.09f, 1f)), new Vector3(0f, 0.88f, 0f), Vector3.one, 4);
            AddSprite(root.transform, "LeftEye", CreateRectangleSprite(8, 12, new Color(0.07f, 0.06f, 0.06f, 1f)), new Vector3(-0.18f, 0.64f, 0f), Vector3.one, 5);
            AddSprite(root.transform, "RightEye", CreateRectangleSprite(8, 12, new Color(0.07f, 0.06f, 0.06f, 1f)), new Vector3(0.18f, 0.64f, 0f), Vector3.one, 5);
            AddSprite(root.transform, "Legs", CreateRectangleSprite(56, 38, new Color(0.18f, 0.16f, 0.18f, 1f)), new Vector3(0f, -0.76f, 0f), Vector3.one, 0);
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

        private void AddSprite(Transform parent, string name, Sprite sprite, Vector3 localPosition, Vector3 localScale, int sortingOrder)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localScale = localScale;

            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }

        private Sprite CreateEllipseSprite(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
            var radius = new Vector2(width * 0.5f, height * 0.5f);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var point = new Vector2((x - center.x) / radius.x, (y - center.y) / radius.y);
                    texture.SetPixel(x, y, point.sqrMagnitude <= 1f ? color : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        private Sprite CreateRectangleSprite(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
    }
}
