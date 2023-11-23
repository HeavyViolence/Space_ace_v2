using UnityEngine;

namespace SpaceAce.Main
{
    public interface IMasterAudioListenerHolder
    {
        AudioListener MasterAudioListener { get; }
    }
}