using UnityEngine;

namespace HeroQuest.Systems.World
{
    /// <summary>
    /// 命中特效动画：缩放放大 + 淡出，0.4秒后自动销毁。
    /// </summary>
    public sealed class HitEffectAnimator : MonoBehaviour
    {
        private float elapsed;
        private const float Duration = 0.4f;

        public void StartAnim()
        {
            elapsed = 0f;
            transform.localScale = Vector3.one * 0.3f;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / Duration);

            // 扩散
            transform.localScale = Vector3.Lerp(Vector3.one * 0.3f, Vector3.one * 1.5f, t);

            // 淡出
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var c = sr.color;
                c.a = Mathf.Lerp(0.9f, 0f, t);
                sr.color = c;
            }
        }
    }
}
