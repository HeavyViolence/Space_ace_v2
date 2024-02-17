namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public readonly struct ShotResult
    {
        public int ProjectilesSent { get; }
        public float Heat { get; }

        public ShotResult(int projectilesSent, float heat)
        {
            ProjectilesSent = projectilesSent;
            Heat = heat;
        }
    }
}