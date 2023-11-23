namespace SpaceAce.Main.Audio
{
    public interface IAudioCollection
    {
        int AudioClipsAmount { get; }

        AudioProperties Next { get; }
        AudioProperties Random { get; }
        AudioProperties NonRepeatingRandom { get; }
    }
}