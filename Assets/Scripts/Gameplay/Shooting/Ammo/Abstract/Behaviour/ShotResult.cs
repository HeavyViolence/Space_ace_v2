namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public readonly struct ShotResult
    {
        public static ShotResult None => new(0, 0f);

        public int ProjectilesSent { get; }
        public float Heat { get; }

        public ShotResult(int projectilesSent, float heat)
        {
            ProjectilesSent = projectilesSent;
            Heat = heat;
        }
    }
}