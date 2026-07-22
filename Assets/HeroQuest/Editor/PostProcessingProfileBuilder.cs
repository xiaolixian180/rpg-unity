using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HeroQuest.Editor
{
    /// <summary>
    /// 构建日系二次元风格的 URP 后处理 Volume Profile 预设。
    /// 通过菜单 Hero Quest > Build Post-Processing Profiles 一键生成。
    /// </summary>
    public static class PostProcessingProfileBuilder
    {
        private const string ProfileDir = "Assets/Settings/";

        [MenuItem("Hero Quest/Build Post-Processing Profiles")]
        public static void BuildAll()
        {
            BuildMapProfile();
            BuildCombatProfile();
            BuildLoginProfile();
            BuildRaidProfile();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PostProcessingProfileBuilder] 所有后处理预设已生成。");
        }

        // === 地图场景：明亮、柔和、清新 ===
        private static void BuildMapProfile()
        {
            var profile = CreateOrLoadProfile($"{ProfileDir}PP_Map.asset");

            var tonemapping = AddOrGet<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.ACES);

            var wb = AddOrGet<WhiteBalance>(profile);
            wb.temperature.Override(15f);
            wb.tint.Override(-3f);

            var colorAdjust = AddOrGet<ColorAdjustments>(profile);
            colorAdjust.postExposure.Override(0.15f);
            colorAdjust.contrast.Override(-8f);
            colorAdjust.saturation.Override(12f);
            colorAdjust.colorFilter.Override(new Color(0.96f, 0.98f, 1.0f, 1f));

            var lift = AddOrGet<LiftGammaGain>(profile);
            lift.lift.Override(new Vector4(0.98f, 0.99f, 1.0f, 0f));
            lift.gamma.Override(new Vector4(0.99f, 0.98f, 1.0f, 0f));
            lift.gain.Override(new Vector4(1.0f, 0.99f, 0.97f, 0f));

            var bloom = AddOrGet<Bloom>(profile);
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(0.5f);
            bloom.scatter.Override(0.7f);
            bloom.tint.Override(new Color(1f, 0.95f, 0.9f, 1f));

            var vignette = AddOrGet<Vignette>(profile);
            vignette.intensity.Override(0.25f);
            vignette.smoothness.Override(0.4f);
            vignette.rounded.Override(false);
            vignette.color.Override(new Color(0f, 0f, 0f, 1f));

            var ca = AddOrGet<ChromaticAberration>(profile);
            ca.intensity.Override(0.15f);

            EditorUtility.SetDirty(profile);
            Debug.Log("[PostProcessing] PP_Map 已配置 — 明亮柔和日系风");
        }

        // === 战斗场景：高对比、暖色、冲击感 ===
        private static void BuildCombatProfile()
        {
            var profile = CreateOrLoadProfile($"{ProfileDir}PP_Combat.asset");

            var tonemapping = AddOrGet<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.ACES);

            var wb = AddOrGet<WhiteBalance>(profile);
            wb.temperature.Override(25f);
            wb.tint.Override(5f);

            var colorAdjust = AddOrGet<ColorAdjustments>(profile);
            colorAdjust.postExposure.Override(-0.1f);
            colorAdjust.contrast.Override(15f);
            colorAdjust.saturation.Override(20f);
            colorAdjust.colorFilter.Override(new Color(1.0f, 0.95f, 0.88f, 1f));

            var lift = AddOrGet<LiftGammaGain>(profile);
            lift.lift.Override(new Vector4(1.0f, 0.97f, 0.93f, 0f));
            lift.gamma.Override(new Vector4(1.0f, 0.96f, 0.92f, 0f));
            lift.gain.Override(new Vector4(1.0f, 0.95f, 0.85f, 0f));

            var bloom = AddOrGet<Bloom>(profile);
            bloom.threshold.Override(0.75f);
            bloom.intensity.Override(0.8f);
            bloom.scatter.Override(0.6f);
            bloom.tint.Override(new Color(1f, 0.85f, 0.7f, 1f));

            var vignette = AddOrGet<Vignette>(profile);
            vignette.intensity.Override(0.4f);
            vignette.smoothness.Override(0.5f);
            vignette.rounded.Override(false);
            vignette.color.Override(new Color(0.1f, 0f, 0f, 1f));

            var ca = AddOrGet<ChromaticAberration>(profile);
            ca.intensity.Override(0.3f);

            var motionBlur = AddOrGet<MotionBlur>(profile);
            motionBlur.intensity.Override(0.3f);
            motionBlur.quality.Override(MotionBlurQuality.Medium);

            EditorUtility.SetDirty(profile);
            Debug.Log("[PostProcessing] PP_Combat 已配置 — 高对比暖色战斗风");
        }

        // === 登录场景：梦幻、柔焦、唯美 ===
        private static void BuildLoginProfile()
        {
            var profile = CreateOrLoadProfile($"{ProfileDir}PP_Login.asset");

            var tonemapping = AddOrGet<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.ACES);

            var wb = AddOrGet<WhiteBalance>(profile);
            wb.temperature.Override(10f);
            wb.tint.Override(-5f);

            var colorAdjust = AddOrGet<ColorAdjustments>(profile);
            colorAdjust.postExposure.Override(0.2f);
            colorAdjust.contrast.Override(-12f);
            colorAdjust.saturation.Override(8f);
            colorAdjust.colorFilter.Override(new Color(0.98f, 0.96f, 1.0f, 1f));

            var lift = AddOrGet<LiftGammaGain>(profile);
            lift.lift.Override(new Vector4(0.97f, 0.98f, 1.02f, 0f));
            lift.gamma.Override(new Vector4(0.98f, 0.99f, 1.02f, 0f));
            lift.gain.Override(new Vector4(0.99f, 0.98f, 1.0f, 0f));

            var bloom = AddOrGet<Bloom>(profile);
            bloom.threshold.Override(0.8f);
            bloom.intensity.Override(0.7f);
            bloom.scatter.Override(0.85f);
            bloom.tint.Override(new Color(0.95f, 0.92f, 1f, 1f));

            var vignette = AddOrGet<Vignette>(profile);
            vignette.intensity.Override(0.35f);
            vignette.smoothness.Override(0.6f);
            vignette.rounded.Override(false);
            vignette.color.Override(new Color(0f, 0f, 0.05f, 1f));

            var ca = AddOrGet<ChromaticAberration>(profile);
            ca.intensity.Override(0.1f);

            var dof = AddOrGet<DepthOfField>(profile);
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.focusDistance.Override(10f);
            dof.focalLength.Override(50f);
            dof.aperture.Override(5.6f);

            EditorUtility.SetDirty(profile);
            Debug.Log("[PostProcessing] PP_Login 已配置 — 梦幻柔焦唯美风");
        }

        // === 副本场景：暗沉、紧张、冷色 ===
        private static void BuildRaidProfile()
        {
            var profile = CreateOrLoadProfile($"{ProfileDir}PP_Raid.asset");

            var tonemapping = AddOrGet<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.ACES);

            var wb = AddOrGet<WhiteBalance>(profile);
            wb.temperature.Override(-15f);
            wb.tint.Override(-8f);

            var colorAdjust = AddOrGet<ColorAdjustments>(profile);
            colorAdjust.postExposure.Override(-0.2f);
            colorAdjust.contrast.Override(10f);
            colorAdjust.saturation.Override(-5f);
            colorAdjust.colorFilter.Override(new Color(0.92f, 0.94f, 1.0f, 1f));

            var lift = AddOrGet<LiftGammaGain>(profile);
            lift.lift.Override(new Vector4(0.95f, 0.97f, 1.05f, 0f));
            lift.gamma.Override(new Vector4(0.97f, 0.98f, 1.03f, 0f));
            lift.gain.Override(new Vector4(0.98f, 0.99f, 1.02f, 0f));

            var bloom = AddOrGet<Bloom>(profile);
            bloom.threshold.Override(0.85f);
            bloom.intensity.Override(0.4f);
            bloom.scatter.Override(0.65f);
            bloom.tint.Override(new Color(0.9f, 0.93f, 1f, 1f));

            var vignette = AddOrGet<Vignette>(profile);
            vignette.intensity.Override(0.5f);
            vignette.smoothness.Override(0.5f);
            vignette.rounded.Override(true);
            vignette.color.Override(new Color(0f, 0.02f, 0.05f, 1f));

            var ca = AddOrGet<ChromaticAberration>(profile);
            ca.intensity.Override(0.2f);

            var dof = AddOrGet<DepthOfField>(profile);
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.focusDistance.Override(8f);
            dof.focalLength.Override(35f);
            dof.aperture.Override(4f);

            EditorUtility.SetDirty(profile);
            Debug.Log("[PostProcessing] PP_Raid 已配置 — 暗沉紧张冷色风");
        }

        // === 工具方法 ===

        private static VolumeProfile CreateOrLoadProfile(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (existing != null) return existing;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            return profile;
        }

        private static T AddOrGet<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet<T>(out var existing))
            {
                existing.SetAllOverridesTo(true);
                return existing;
            }

            var comp = profile.Add<T>(true);
            comp.SetAllOverridesTo(true);
            return comp;
        }
    }
}
