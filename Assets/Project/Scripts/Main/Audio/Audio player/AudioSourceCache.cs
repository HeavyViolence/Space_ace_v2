using System;

using UnityEngine;

namespace SpaceAce.Main.Audio
{
    public sealed record AudioSourceCache
    {
        public AudioSource AudioSource { get; }
        public Transform Transform { get; }

        public AudioSourceCache(AudioSource audioSource, Transform transform)
        {
            if (audioSource == null) throw new ArgumentNullException();
            AudioSource = audioSource;

            if (transform == null) throw new ArgumentNullException();
            Transform = transform;
        }
    }
}