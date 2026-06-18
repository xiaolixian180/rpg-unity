using System;
using HeroQuest.Domain;
using HeroQuest.Systems.Character;
using HeroQuest.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.Screens
{
    public sealed class CharacterSelectView : UIView
    {
        [Header("角色立绘")]
        [SerializeField] private Image portraitImage;

        [Header("文本")]
        [SerializeField] private TMP_Text classNameText;
        [SerializeField] private TMP_Text genderText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text statsText;

        [Header("按钮")]
        [SerializeField] private Button previousClassButton;
        [SerializeField] private Button nextClassButton;
        [SerializeField] private Button toggleGenderButton;
        [SerializeField] private Button createButton;

        private int classIndex;
        private CharacterGender gender;

        public event Action<CharacterSelection> CharacterCreated;

        private void OnEnable()
        {
            previousClassButton?.onClick.AddListener(SelectPreviousClass);
            nextClassButton?.onClick.AddListener(SelectNextClass);
            toggleGenderButton?.onClick.AddListener(ToggleGender);
            createButton?.onClick.AddListener(CreateCharacter);
            Refresh();
        }

        private void OnDisable()
        {
            previousClassButton?.onClick.RemoveListener(SelectPreviousClass);
            nextClassButton?.onClick.RemoveListener(SelectNextClass);
            toggleGenderButton?.onClick.RemoveListener(ToggleGender);
            createButton?.onClick.RemoveListener(CreateCharacter);
        }

        public void SelectClass(CharacterClass characterClass)
        {
            var classes = CharacterRoster.AvailableClasses;
            for (var i = 0; i < classes.Count; i++)
            {
                if (classes[i] == characterClass)
                {
                    classIndex = i;
                    Refresh();
                    return;
                }
            }
        }

        private void SelectPreviousClass()
        {
            var count = CharacterRoster.AvailableClasses.Count;
            classIndex = (classIndex - 1 + count) % count;
            Refresh();
        }

        private void SelectNextClass()
        {
            var count = CharacterRoster.AvailableClasses.Count;
            classIndex = (classIndex + 1) % count;
            Refresh();
        }

        private void ToggleGender()
        {
            gender = gender == CharacterGender.Male ? CharacterGender.Female : CharacterGender.Male;
            Refresh();
        }

        private void CreateCharacter()
        {
            CharacterCreated?.Invoke(new CharacterSelection(CurrentClass, gender));
        }

        private CharacterClass CurrentClass => CharacterRoster.AvailableClasses[classIndex];

        private void Refresh()
        {
            var definition = CharacterRoster.Get(CurrentClass, gender);
            var portrait = definition.LoadPortrait();
            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.preserveAspect = true;
            }

            if (classNameText != null)
            {
                classNameText.text = definition.DisplayName;
            }

            if (genderText != null)
            {
                genderText.text = gender == CharacterGender.Male ? "男性" : "女性";
            }

            if (descriptionText != null)
            {
                descriptionText.text = definition.RoleDescription;
            }

            if (statsText != null)
            {
                statsText.text = FormatStats(definition.BaseStats);
            }
        }

        private static string FormatStats(StatBlock stats)
        {
            return $"力量 {stats.strength}\n敏捷 {stats.agility}\n智力 {stats.intelligence}\n体质 {stats.constitution}\n防御 {stats.defense}";
        }
    }
}
