namespace HeroQuest.Systems.Combat
{
    public readonly struct DamagePreview
    {
        public DamagePreview(int damage, bool dodged, bool critical)
        {
            Damage = damage;
            Dodged = dodged;
            Critical = critical;
        }

        public int Damage { get; }
        public bool Dodged { get; }
        public bool Critical { get; }
    }
}
