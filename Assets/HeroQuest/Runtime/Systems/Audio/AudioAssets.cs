using UnityEngine;

namespace HeroQuest.Systems.Audio
{
    /// <summary>
    /// 音频资产引用 — BGM 和 SFX 的 AudioClip 引用。
    /// 在 Inspector 中配置，或运行时通过 Resources.Load 加载。
    /// </summary>
    public static class AudioAssets
    {
        // BGM clips (assigned at runtime via Resources.Load)
        public static AudioClip BgmLogin;
        public static AudioClip BgmMap;
        public static AudioClip BgmCombat;
        public static AudioClip BgmRaid;

        // SFX clips
        public static AudioClip SfxAttack;
        public static AudioClip SfxHit;
        public static AudioClip SfxSkill;
        public static AudioClip SfxButton;
        public static AudioClip SfxLevelUp;
        public static AudioClip SfxPickup;
        public static AudioClip SfxError;
        public static AudioClip SfxMonsterDeath;

        /// <summary>
        /// 尝试加载所有音频资源（从 Resources/HeroQuest/Audio/ 目录）。
        /// </summary>
        public static void LoadAll()
        {
            BgmLogin = Resources.Load<AudioClip>("HeroQuest/Audio/BGM_Login");
            BgmMap = Resources.Load<AudioClip>("HeroQuest/Audio/BGM_Map");
            BgmCombat = Resources.Load<AudioClip>("HeroQuest/Audio/BGM_Combat");
            BgmRaid = Resources.Load<AudioClip>("HeroQuest/Audio/BGM_Raid");

            SfxAttack = Resources.Load<AudioClip>("HeroQuest/Audio/SFX_Attack");
            SfxHit = Resources.Load<AudioClip>("HeroQuest/Audio/SFX_Hit");
            SfxSkill = Resources.Load<AudioClip>("HeroQuest/Audio/SFX_Skill");
            SfxButton = Resources.Load<AudioClip>("HeroQuest/Audio/SFX_Button");
            SfxLevelUp = Resources.Load<AudioClip>("HeroQuest/Audio/SFX_LevelUp");
            SfxPickup = Resources.Load<AudioClip>("HeroQuest/Audio/SFX_Pickup");
            SfxError = Resources.Load<AudioClip>("HeroQuest/Audio/SFX_Error");
            SfxMonsterDeath = Resources.Load<AudioClip>("HeroQuest/Audio/SFX_MonsterDeath");
        }

        /// <summary>
        /// 从 TJGenerators/History 加载已生成的音频文件。
        /// </summary>
        public static void LoadFromGenerators()
        {
            // AI 生成的音频会保存在 Assets/TJGenerators/History/
            // 我们通过 Resources.Load 加载（需要 .meta 中设置 asset）
            // 或者通过 AssetDatabase.LoadAssetAtPath 加载
#if UNITY_EDITOR
            LoadFromGeneratorsEditor();
#endif
        }

#if UNITY_EDITOR
        private static void LoadFromGeneratorsEditor()
        {
            // 在编辑器模式下从 TJGenerators/History 加载
            BgmLogin = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/BGM_Login.wav");
            BgmMap = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/BGM_Map.wav");
            BgmCombat = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/BGM_Combat.wav");
            BgmRaid = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/BGM_Raid.wav");

            SfxAttack = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/SFX_Attack.mp3");
            SfxHit = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/SFX_Hit.mp3");
            SfxSkill = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/SFX_Skill.mp3");
            SfxButton = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/SFX_Button.mp3");
            SfxLevelUp = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/SFX_LevelUp.mp3");
            SfxPickup = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/SFX_Pickup.mp3");
            SfxError = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/SFX_Error.mp3");
            SfxMonsterDeath = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/SFX_MonsterDeath.mp3");
        }
#endif

        public static bool HasBgm => BgmLogin != null || BgmMap != null;
        public static bool HasSfx => SfxAttack != null || SfxButton != null;
    }
}
