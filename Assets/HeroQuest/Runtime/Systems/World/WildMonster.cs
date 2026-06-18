using UnityEngine;

namespace HeroQuest.Systems.World
{
    public sealed class WildMonster : MonoBehaviour
    {
        [SerializeField] private float wanderRadius = 0.8f;
        [SerializeField] private float wanderSpeed = 0.55f;
        [SerializeField] private float retargetSeconds = 2.8f;

        private Vector3 origin;
        private Vector3 target;
        private float timer;
        private string displayName;
        private Transform healthFill;
        private float healthPercent = 1f;

        public string DisplayName => displayName;

        public void Initialize(string monsterName, Transform healthFillTransform)
        {
            displayName = monsterName;
            healthFill = healthFillTransform;
            SetHealthPercent(1f);
        }

        public void SetHealthPercent(float percent)
        {
            healthPercent = Mathf.Clamp01(percent);
            if (healthFill == null)
            {
                return;
            }

            var scale = healthFill.localScale;
            scale.x = healthPercent;
            healthFill.localScale = scale;
            healthFill.localPosition = new Vector3(-(1f - healthPercent) * 0.59f, 0f, 0f);
        }

        private void Awake()
        {
            origin = transform.position;
            target = origin;
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = retargetSeconds;
                target = origin + new Vector3(Random.Range(-wanderRadius, wanderRadius), Random.Range(-wanderRadius, wanderRadius), 0f);
            }

            transform.position = Vector3.MoveTowards(transform.position, target, wanderSpeed * Time.deltaTime);
        }
    }
}
