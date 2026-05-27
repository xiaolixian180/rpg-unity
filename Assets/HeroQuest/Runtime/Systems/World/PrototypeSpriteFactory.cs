using UnityEngine;

namespace HeroQuest.Systems.World
{
    public static class PrototypeSpriteFactory
    {
        public static Sprite CreateEllipseSprite(int width, int height, Color color, int pixelsPerUnit)
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
    }
}
