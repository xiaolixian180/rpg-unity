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
