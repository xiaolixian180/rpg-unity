using UnityEngine;
using UnityEngine.UIElements;

namespace HeroQuest.UI.Toolkit
{
    /// <summary>
    /// 设置面板控制器，绑定 UXML 元素事件。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class SettingsPanelController : ToolkitPanelBase
    {
        public event System.Action OnCloseRequested;
        public event System.Action OnLogoutRequested;

        protected override void OnPanelLoaded(VisualElement root)
        {
            RegisterClick(root, "btn-close", () => OnCloseRequested?.Invoke());
            RegisterClick(root, "btn-logout", () => OnLogoutRequested?.Invoke());
            RegisterClick(root, "btn-save", SaveSettings);

            root.style.display = DisplayStyle.Flex;
        }

        private void SaveSettings()
        {
            var root = Root;
            var bgmSlider = Q<Slider>(root, "slider-bgm");
            var sfxSlider = Q<Slider>(root, "slider-sfx");
            var qualityDD = Q<DropdownField>(root, "dd-quality");
            var fullscreenToggle = Q<Toggle>(root, "toggle-fullscreen");
            var autobattleToggle = Q<Toggle>(root, "toggle-autobattle");

            if (bgmSlider != null) PlayerPrefs.SetFloat("Setting_BGM", bgmSlider.value);
            if (sfxSlider != null) PlayerPrefs.SetFloat("Setting_SFX", sfxSlider.value);
            if (qualityDD != null) PlayerPrefs.SetInt("Setting_Quality", qualityDD.index);
            if (fullscreenToggle != null)
            {
                PlayerPrefs.SetInt("Setting_Fullscreen", fullscreenToggle.value ? 1 : 0);
                Screen.fullScreen = fullscreenToggle.value;
            }
            if (autobattleToggle != null) PlayerPrefs.SetInt("Setting_AutoBattle", autobattleToggle.value ? 1 : 0);

            PlayerPrefs.Save();
            Debug.Log("[SettingsPanel] 设置已保存。");
        }

        protected override void OnPanelUnloaded(VisualElement root)
        {
            root.style.display = DisplayStyle.None;
        }
    }
}
