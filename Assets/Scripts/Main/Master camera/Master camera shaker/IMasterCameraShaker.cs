namespace SpaceAce.Main
{
    public interface IMasterCameraShaker
    {
        MasterCameraShakerSettings Settings { get; set; }

        void ShakeOnShotFired();
        void ShakeOnDefeat();
        void ShakeOnCollision();
        void ShakeOnHit();
    }
}