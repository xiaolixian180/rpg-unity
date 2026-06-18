using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.Core
{
    public sealed class CircleMaskGraphic : Graphic
    {
        [SerializeField] private int segments = 64;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var rect = GetPixelAdjustedRect();
            var radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            var center = rect.center;
            var color32 = color;

            vh.AddVert(center, color32, new Vector2(0.5f, 0.5f));

            var segmentCount = Mathf.Max(12, segments);
            for (var i = 0; i <= segmentCount; i++)
            {
                var angle = i / (float)segmentCount * Mathf.PI * 2f;
                var point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                var uv = new Vector2(
                    0.5f + Mathf.Cos(angle) * 0.5f,
                    0.5f + Mathf.Sin(angle) * 0.5f);
                vh.AddVert(point, color32, uv);
            }

            for (var i = 1; i <= segmentCount; i++)
            {
                vh.AddTriangle(0, i, i + 1);
            }
        }
    }
}
