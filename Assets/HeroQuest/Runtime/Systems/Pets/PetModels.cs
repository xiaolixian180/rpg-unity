using System;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Pets
{
    [Serializable]
    public sealed class PetState
    {
        public string petId;
        public string templateId;
        public PetKind kind;
        public Quality quality;
        public int level = 1;
        public bool isSummoned;
        public bool isExploring;
    }

    [Serializable]
    public sealed class PetSnapshot
    {
        public PetState[] pets = Array.Empty<PetState>();
    }
}
