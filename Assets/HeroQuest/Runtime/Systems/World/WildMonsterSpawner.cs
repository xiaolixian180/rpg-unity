using HeroQuest.UI.Core;
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
            "HeroQuest/Enemies/WildMonster_01",
            "HeroQuest/Enemies/WildMonster_02",
            "HeroQuest/Enemies/WildMonster_03"
        };
        [SerializeField] private string[] monsterDisplayNames =
        {
            "荒原角兽",
            "沼泽掠夺者",
            "林地蜥蜴"
        };
        [SerializeField] private float monsterWorldHeight = 1.45f;

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

            var monsterIndex = Random.Range(0, monsterResourcePaths.Length);
            var resourcePath = monsterResourcePaths[monsterIndex];
            var displayName = monsterIndex < monsterDisplayNames.Length
                ? monsterDisplayNames[monsterIndex]
                : "野怪";
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                sprite = PrototypeSpriteFactory.CreateEllipseSprite(64, 84, new Color(0.55f, 0.14f, 0.12f, 1f), 64);
            }

            var angle = Random.Range(0f, Mathf.PI * 2f);
            var distance = Random.Range(spawnRadiusMin, spawnRadiusMax);
            var position = player.position + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);

            var monster = new GameObject($"{displayName}_{transform.childCount + 1}");
            monster.transform.SetParent(transform, false);
            monster.transform.position = position;

            var shadowObject = new GameObject("Shadow");
            shadowObject.transform.SetParent(monster.transform, false);
            shadowObject.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            shadowObject.transform.localScale = new Vector3(0.66f, 0.24f, 1f);
            var shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = PrototypeSpriteFactory.CreateEllipseSprite(72, 22, new Color(0f, 0f, 0f, 0.22f), 64);
            shadowRenderer.sortingOrder = 1;

            var spriteObject = new GameObject("Visual");
            spriteObject.transform.SetParent(monster.transform, false);
            spriteObject.transform.localPosition = Vector3.zero;
            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 3;
            var spriteHeight = Mathf.Max(0.001f, sprite.bounds.size.y);
            spriteObject.transform.localScale = Vector3.one * (monsterWorldHeight / spriteHeight);

            var healthFill = CreateMonsterUi(monster.transform, displayName);
            var wildMonster = monster.AddComponent<WildMonster>();
            wildMonster.Initialize(displayName, healthFill);
        }

        private Transform CreateMonsterUi(Transform parent, string displayName)
        {
            var barY = monsterWorldHeight + 0.13f;
            var background = new GameObject("生命条背景");
            background.transform.SetParent(parent, false);
            background.transform.localPosition = new Vector3(0f, barY, 0f);
            background.transform.localScale = new Vector3(0.68f, 0.42f, 1f);
            var backgroundRenderer = background.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = PrototypeSpriteFactory.CreateRectangleSprite(80, 10, new Color(0.10f, 0.08f, 0.07f, 0.95f), 64);
            backgroundRenderer.sortingOrder = 10;

            var fill = new GameObject("生命值");
            fill.transform.SetParent(background.transform, false);
            fill.transform.localPosition = Vector3.zero;
            fill.transform.localScale = Vector3.one;
            var fillRenderer = fill.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = PrototypeSpriteFactory.CreateRectangleSprite(76, 6, new Color(0.72f, 0.13f, 0.10f, 1f), 64);
            fillRenderer.sortingOrder = 11;

            var nameObject = new GameObject("野怪名称");
            nameObject.transform.SetParent(parent, false);
            nameObject.transform.localPosition = new Vector3(0f, barY + 0.20f, 0f);
            var text = nameObject.AddComponent<TextMesh>();
            text.text = displayName;
            text.font = ChineseFontProvider.GetFont();
            text.fontSize = 32;
            text.characterSize = 0.065f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(0.96f, 0.92f, 0.78f, 1f);
            text.GetComponent<MeshRenderer>().sortingOrder = 12;
            return fill.transform;
        }
    }
}
