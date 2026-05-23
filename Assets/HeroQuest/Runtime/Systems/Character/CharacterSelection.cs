using HeroQuest.Domain;

namespace HeroQuest.Systems.Character
{
    public readonly struct CharacterSelection
    {
        public CharacterSelection(CharacterClass characterClass, CharacterGender gender)
        {
            CharacterClass = characterClass;
            Gender = gender;
        }

        public CharacterClass CharacterClass { get; }
        public CharacterGender Gender { get; }
    }
}
