namespace SpaceAce.Main.Audio
{
    public interface IMusicPlayer
    {
        bool IsPlaying { get; }
        MusicPlayerSettings Settings { get; }

        void Play();
        void Stop();
    }
}