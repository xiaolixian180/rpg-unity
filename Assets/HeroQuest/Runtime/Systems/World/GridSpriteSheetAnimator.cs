using System.Collections.Generic;
using UnityEngine;

namespace HeroQuest.Systems.World
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GridSpriteSheetAnimator : MonoBehaviour
    {
        private static readonly Dictionary<string, Sprite[]> FrameCache = new();

        [SerializeField] private string textureResourcePath = "HeroQuest/Playable/Warrior_Male_Walksheet";
        [SerializeField] private int columns = 8;
        [SerializeField] private int rows = 8;
        [SerializeField] private float framesPerSecond = 10f;
        [SerializeField] private int idleFrame = 0;
        [SerializeField] private int animationRow = 0;
        [SerializeField] private int sortingOrder = 3;
        [SerializeField] private int pixelsPerUnit = 300;
        [SerializeField] private Vector3 localScale = Vector3.one;
        [SerializeField] private int frameInsetLeft = 230;
        [SerializeField] private int frameInsetRight = 230;
        [SerializeField] private int frameInsetTop = 35;
        [SerializeField] private int frameInsetBottom = 45;

        private SpriteRenderer spriteRenderer;
        private Sprite[] frames;
        private TopDownPlayerController controller;
        private float frameTimer;
        private int currentFrame;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = sortingOrder;
            transform.localScale = localScale;
            controller = GetComponentInParent<TopDownPlayerController>();
            frames = GetFrames(
                textureResourcePath,
                columns,
                rows,
                pixelsPerUnit,
                frameInsetLeft,
                frameInsetRight,
                frameInsetTop,
                frameInsetBottom);
            ApplyIdle();
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            var isMoving = controller != null && controller.IsMoving;
            if (!isMoving)
            {
                ApplyIdle();
                return;
            }

            frameTimer += Time.deltaTime * Mathf.Max(1f, framesPerSecond);
            if (frameTimer < 1f)
            {
                return;
            }

            frameTimer = 0f;
            currentFrame = (currentFrame + 1) % columns;
            spriteRenderer.sprite = GetFrame(animationRow, currentFrame);
        }

        private void ApplyIdle()
        {
            frameTimer = 0f;
            currentFrame = idleFrame;
            spriteRenderer.sprite = GetFrame(animationRow, idleFrame);
        }

        private Sprite GetFrame(int row, int column)
        {
            var index = Mathf.Clamp(row, 0, rows - 1) * columns + Mathf.Clamp(column, 0, columns - 1);
            return frames[index];
        }

        private static Sprite[] GetFrames(
            string resourcePath,
            int columns,
            int rows,
            int pixelsPerUnit,
            int insetLeft,
            int insetRight,
            int insetTop,
            int insetBottom)
        {
            var cacheKey = $"{resourcePath}:{columns}:{rows}:{pixelsPerUnit}:{insetLeft}:{insetRight}:{insetTop}:{insetBottom}";
            if (FrameCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                return null;
            }

            var cellWidth = texture.width / columns;
            var cellHeight = texture.height / rows;
            var frames = new Sprite[columns * rows];
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var index = row * columns + column;
                    var x = column * cellWidth + insetLeft;
                    var y = texture.height - (row + 1) * cellHeight + insetBottom;
                    var width = Mathf.Max(1, cellWidth - insetLeft - insetRight);
                    var height = Mathf.Max(1, cellHeight - insetTop - insetBottom);
                    var rect = new Rect(x, y, width, height);
                    frames[index] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.08f), pixelsPerUnit);
                }
            }

            FrameCache[cacheKey] = frames;
            return frames;
        }
    }
}
