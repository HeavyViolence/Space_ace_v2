using Cysharp.Threading.Tasks;

using System.Threading;

using UnityEngine;

namespace SpaceAce.Main.Audio
{
    public interface IAudioPlayer
    {
        AudioPlayerSettings Settings { get; set; }

        UniTask PlayOnceAsync(AudioProperties properties,
                              Vector3 position,
                              Transform anchor,
                              CancellationToken token);

        UniTask PlayOnLoopAsync(AudioProperties properties,
                                Vector3 position,
                                Transform anchor,
                                CancellationToken token);
    }
}