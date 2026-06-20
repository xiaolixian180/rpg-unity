using System.Collections.Generic;
using UnityEngine;

namespace HeroQuest.Systems.World
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GridSpriteSheetAnimator : MonoBehaviour
    {
        private static readonly Dictionary<string, Sprite[]> FrameCache = new();

        [SerializeField] private string rightTextureResourcePath = "HeroQuest/Playable/Warrior_Male_Walk_Right";
        [SerializeField] private string leftTextureResourcePath = "HeroQuest/Playable/Warrior_Male_Walk_Left";
        [SerializeField] private int columns = 8;
        [SerializeField] private int rows = 8;
        [SerializeField] private float framesPerSecond = 10f;
        [SerializeField] private int idleFrame = 0;
        [SerializeField] private int animationRow = 0;
        [SerializeField] private int sortingOrder = 3;
        [SerializeField] private int pixelsPerUnit = 300;
        [SerializeField] private Vector3 localScale = Vector3.one;
        [SerializeField] private float targetWorldHeight = 1.5f;
        [SerializeField] private bool mirrorRightFramesForLeft = true;
        [SerializeField] private bool trimTransparentFrameBounds = true;
        [SerializeField] private byte alphaTrimThreshold = 8;
        [SerializeField] private int framePadding = 6;

        private SpriteRenderer spriteRenderer;
        private Sprite[] rightFrames;
        private Sprite[] leftFrames;
        private Sprite[] activeFrames;
        private TopDownPlayerController controller;
        private float frameTimer;
        private int currentFrame;
        private bool facingLeft;

        public void Configure(string rightResourcePath, string leftResourcePath)
        {
            Configure(rightResourcePath, leftResourcePath, columns, rows);
        }

        public void Configure(string rightResourcePath, string leftResourcePath, int frameColumns, int frameRows)
        {
            EnsureInitialized();
            rightTextureResourcePath = rightResourcePath;
            leftTextureResourcePath = leftResourcePath;
            columns = Mathf.Max(1, frameColumns);
            rows = Mathf.Max(1, frameRows);
            currentFrame = 0;
            rightFrames = GetFrames(
                rightTextureResourcePath,
                columns,
                rows,
                pixelsPerUnit,
                trimTransparentFrameBounds,
                alphaTrimThreshold,
                framePadding);
            leftFrames = GetFrames(
                leftTextureResourcePath,
                columns,
                rows,
                pixelsPerUnit,
                trimTransparentFrameBounds,
                alphaTrimThreshold,
                framePadding);
            activeFrames = SelectFramesForFacing();
            ApplyFacingFlip();
            ApplyIdle();
        }

        private void Awake()
        {
            EnsureInitialized();
            Configure(rightTextureResourcePath, leftTextureResourcePath);
        }

        private void EnsureInitialized()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sortingOrder = sortingOrder;
                }
                transform.localScale = localScale;
                controller = GetComponentInParent<TopDownPlayerController>();
            }
        }

        private void Update()
        {
            if (activeFrames == null || activeFrames.Length == 0)
            {
                return;
            }

            var isMoving = controller != null && controller.IsMoving;
            UpdateFacing();
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
            SetSprite(GetFrame(animationRow, currentFrame));
        }

        private void ApplyIdle()
        {
            if (activeFrames == null || activeFrames.Length == 0)
            {
                return;
            }

            frameTimer = 0f;
            currentFrame = idleFrame;
            SetSprite(GetFrame(animationRow, idleFrame));
        }

        private Sprite GetFrame(int row, int column)
        {
            var index = Mathf.Clamp(row, 0, rows - 1) * columns + Mathf.Clamp(column, 0, columns - 1);
            return activeFrames[index];
        }

        private void UpdateFacing()
        {
            if (controller == null || Mathf.Abs(controller.MoveInput.x) <= 0.01f)
            {
                return;
            }

            var nextFacingLeft = controller.MoveInput.x < 0f;
            if (nextFacingLeft == facingLeft)
            {
                return;
            }

            facingLeft = nextFacingLeft;
            activeFrames = SelectFramesForFacing();
            ApplyFacingFlip();
            SetSprite(GetFrame(animationRow, currentFrame));
        }

        private Sprite[] SelectFramesForFacing()
        {
            if (mirrorRightFramesForLeft && rightFrames != null)
            {
                return rightFrames;
            }

            return facingLeft ? leftFrames ?? rightFrames : rightFrames ?? leftFrames;
        }

        private void ApplyFacingFlip()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.flipX = mirrorRightFramesForLeft && rightFrames != null && facingLeft;
        }

        private void SetSprite(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            spriteRenderer.sprite = sprite;
            ApplyConsistentWorldHeight(sprite);
        }

        private void ApplyConsistentWorldHeight(Sprite sprite)
        {
            var spriteHeight = sprite.bounds.size.y;
            if (targetWorldHeight <= 0f || spriteHeight <= 0f)
            {
                transform.localScale = localScale;
                return;
            }

            var heightScale = targetWorldHeight / spriteHeight;
            transform.localScale = new Vector3(
                localScale.x * heightScale,
                localScale.y * heightScale,
                localScale.z);
        }

        private static Sprite[] GetFrames(
            string resourcePath,
            int columns,
            int rows,
            int pixelsPerUnit,
            bool trimTransparentFrameBounds,
            byte alphaTrimThreshold,
            int framePadding)
        {
            var cacheKey = $"{resourcePath}:{columns}:{rows}:{pixelsPerUnit}:{trimTransparentFrameBounds}:{alphaTrimThreshold}:{framePadding}";
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
            var readablePixels = TryReadPixels(texture);
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var index = row * columns + column;
                    if (trimTransparentFrameBounds && readablePixels != null)
                    {
                        if (!TryGetTrimmedCellRect(
                                texture,
                                readablePixels,
                                row,
                                column,
                                cellWidth,
                                cellHeight,
                                alphaTrimThreshold,
                                framePadding,
                                out var trimmedRect))
                        {
                            continue;
                        }

                        frames[index] = Sprite.Create(texture, trimmedRect, new Vector2(0.5f, 0.08f), pixelsPerUnit);
                    }
                    else
                    {
                        var rect = GetFullCellRect(texture, row, column, cellWidth, cellHeight);
                        frames[index] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.08f), pixelsPerUnit);
                    }
                }
            }

            FillMissingFrames(frames, columns, rows);
            FrameCache[cacheKey] = frames;
            return frames;
        }

        private static Color32[] TryReadPixels(Texture2D texture)
        {
            try
            {
                return texture.GetPixels32();
            }
            catch (UnityException)
            {
                return null;
            }
        }

        private static Rect GetFullCellRect(Texture2D texture, int row, int column, int cellWidth, int cellHeight)
        {
            return new Rect(
                column * cellWidth,
                texture.height - (row + 1) * cellHeight,
                cellWidth,
                cellHeight);
        }

        private static bool TryGetTrimmedCellRect(
            Texture2D texture,
            Color32[] pixels,
            int row,
            int column,
            int cellWidth,
            int cellHeight,
            byte alphaTrimThreshold,
            int padding,
            out Rect rect)
        {
            rect = default;
            var minX = cellWidth;
            var minY = cellHeight;
            var maxX = -1;
            var maxY = -1;
            var originX = column * cellWidth;
            var originY = texture.height - (row + 1) * cellHeight;

            for (var y = 0; y < cellHeight; y++)
            {
                var textureY = originY + y;
                if (textureY < 0 || textureY >= texture.height)
                {
                    continue;
                }

                for (var x = 0; x < cellWidth; x++)
                {
                    var textureX = originX + x;
                    if (textureX < 0 || textureX >= texture.width)
                    {
                        continue;
                    }

                    if (pixels[textureY * texture.width + textureX].a <= alphaTrimThreshold)
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            if (maxX < minX || maxY < minY)
            {
                return false;
            }

            minX = Mathf.Max(0, minX - padding);
            minY = Mathf.Max(0, minY - padding);
            maxX = Mathf.Min(cellWidth - 1, maxX + padding);
            maxY = Mathf.Min(cellHeight - 1, maxY + padding);

            rect = new Rect(
                originX + minX,
                originY + minY,
                Mathf.Max(1, maxX - minX + 1),
                Mathf.Max(1, maxY - minY + 1));
            return true;
        }

        private static void FillMissingFrames(Sprite[] frames, int columns, int rows)
        {
            for (var row = 0; row < rows; row++)
            {
                Sprite first = null;
                Sprite previous = null;
                var rowStart = row * columns;

                for (var column = 0; column < columns; column++)
                {
                    var frame = frames[rowStart + column];
                    if (frame == null)
                    {
                        continue;
                    }

                    first ??= frame;
                    previous = frame;
                }

                if (first == null)
                {
                    continue;
                }

                previous = first;
                for (var column = 0; column < columns; column++)
                {
                    var index = rowStart + column;
                    if (frames[index] == null)
                    {
                        frames[index] = previous;
                        continue;
                    }

                    previous = frames[index];
                }
            }
        }
    }
}
