namespace SpaceAce.Main
{
    public sealed class CameraShakerSettings
    {
        public static CameraShakerSettings Default => new(true, true, true, true);

        public bool ShakingOnShotFiredEnabled { get; }
        public bool ShakingOnHitEnabled { get; }
        public bool ShakingOnCollisionEnabled { get; }
        public bool ShakingOnDefeatEnabled { get; }

        public CameraShakerSettings(bool shakingOnShotFiredEnabled,
                                    bool shakingOnHitEnabled,
                                    bool shakingOnCollisionEnabled,
                                    bool shakingOnDefeatEnabled)
        {
            ShakingOnShotFiredEnabled = shakingOnShotFiredEnabled;
            ShakingOnHitEnabled = shakingOnHitEnabled;
            ShakingOnCollisionEnabled = shakingOnCollisionEnabled;
            ShakingOnDefeatEnabled = shakingOnDefeatEnabled;
        }
    }
}