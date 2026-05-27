using UnityEngine;

namespace HeroQuest.Systems.World
{
    public sealed class WildMonsterSpawner : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private int targetMonsterCount = 12;
        [SerializeField] private float spawnRadiusMin = 3f;
        [SerializeField] private float spawnRadiusMax = 8f;
        [SerializeField] private float respawnInterval = 2.5f;
        [SerializeField] private string[] monsterResourcePaths =
        {
            "HeroQuest/Enemies/Archer_Male_Monster",
            "HeroQuest/Enemies/Mage_Male_Monster",
            "HeroQuest/Enemies/Priest_Male_Monster"
        };

        private float timer;

        public void SetPlayer(Transform playerTransform)
        {
            player = playerTransform;
        }

        private void OnEnable()
        {
            timer = 0f;
        }

        private void Start()
        {
            RefillMonsters();
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0f)
            {
                return;
            }

            timer = respawnInterval;
            RefillMonsters();
        }

        public void RefillMonsters()
        {
            var attempts = 0;
            var maxAttempts = targetMonsterCount * 3;
            while (transform.childCount < targetMonsterCount && attempts < maxAttempts)
            {
                attempts++;
                SpawnMonster();
            }
        }

        private void SpawnMonster()
        {
            if (player == null || monsterResourcePaths == null || monsterResourcePaths.Length == 0)
            {
                return;
            }

            var resourcePath = monsterResourcePaths[Random.Range(0, monsterResourcePaths.Length)];
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                sprite = PrototypeSpriteFactory.CreateEllipseSprite(64, 84, new Color(0.55f, 0.14f, 0.12f, 1f), 64);
            }

            var angle = Random.Range(0f, Mathf.PI * 2f);
            var distance = Random.Range(spawnRadiusMin, spawnRadiusMax);
            var position = player.position + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);

            var monster = new GameObject($"WildMonster_{transform.childCount + 1}");
            monster.transform.SetParent(transform, false);
            monster.transform.position = position;

            var shadowObject = new GameObject("Shadow");
            shadowObject.transform.SetParent(monster.transform, false);
            shadowObject.transform.localPosition = new Vector3(0f, -0.18f, 0f);
            shadowObject.transform.localScale = new Vector3(0.66f, 0.24f, 1f);
            var shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = PrototypeSpriteFactory.CreateEllipseSprite(72, 22, new Color(0f, 0f, 0f, 0.22f), 64);
            shadowRenderer.sortingOrder = 1;

            var spriteObject = new GameObject("Visual");
            spriteObject.transform.SetParent(monster.transform, false);
            spriteObject.transform.localPosition = Vector3.zero;
            spriteObject.transform.localScale = new Vector3(0.11f, 0.11f, 1f);
            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 3;

            monster.AddComponent<WildMonster>();
        }
    }
}
