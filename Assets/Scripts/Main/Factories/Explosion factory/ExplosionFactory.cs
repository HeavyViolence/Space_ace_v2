using Cysharp.Threading.Tasks;

using SpaceAce.Main.Audio;

using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public class ExplosionFactory
    {
        private readonly DiContainer _diContainer;
        private readonly GamePauser _gamePauser;
        private readonly AudioPlayer _audioPlayer;
        private readonly Dictionary<ExplosionSize, GameObject> _explosionPrefabs = new();
        private readonly Dictionary<ExplosionSize, AudioCollection> _explosionAudio = new();
        private readonly Dictionary<ExplosionSize, Stack<CachedExplosion>> _explosionPool = new();

        public ExplosionFactory(DiContainer diContainer,
                                GamePauser gamePauser,
                                AudioPlayer audioPlayer,
                                IEnumerable<ExplosionSlot> explosions)
        {
            _diContainer = diContainer ?? throw new ArgumentNullException(nameof(diContainer),
                $"Attempted to pass an empty {typeof(DiContainer)}!");

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");

            _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer),
                $"Attempted to pass an empty {typeof(AudioPlayer)}!");

            if (explosions is null) throw new ArgumentNullException(nameof(explosions),
                $"Attempted to pass an empty explosions collection!");

            foreach (var explosion in explosions)
            {
                _explosionPrefabs.Add(explosion.Size, explosion.Prefab);
                _explosionAudio.Add(explosion.Size, explosion.Audio);
            }
        }

        public async UniTask CreateAsync(ExplosionSize size, Vector3 position, CancellationToken token = default)
        {
            CachedExplosion explosion = InstantiateExplosion(size, position);

            PlayExplosionAudio(size, position, token);
            await AwaitExplosionEffectToPlayAsync(explosion, token);
            ReleaseExplosion(size, explosion);
        }

        private CachedExplosion InstantiateExplosion(ExplosionSize size, Vector3 position)
        {
            CachedExplosion cache;

            if (_explosionPool.TryGetValue(size, out Stack<CachedExplosion> stack) == true &&
                stack.Count > 0)
            {
                cache = stack.Pop();
            }
            else if (_explosionPrefabs.TryGetValue(size, out GameObject value) == true)
            {
                GameObject explosion = _diContainer.InstantiatePrefab(value);

                if (explosion.TryGetComponent(out ParticleSystemPauser pauser) == true)
                    cache = new(explosion, pauser);
                else throw new MissingComponentException($"Explosion prefab is missing {typeof(ParticleSystemPauser)}!");
            }
            else throw new Exception($"Explosion prefab of a requested size ({size}) doesn't exist!");

            cache.Explosion.SetActive(true);
            cache.Explosion.transform.position = position;

            return cache;
        }

        private void PlayExplosionAudio(ExplosionSize size, Vector3 position, CancellationToken token)
        {
            if (_explosionAudio.TryGetValue(size, out AudioCollection audio) == true)
                _audioPlayer.PlayOnceAsync(audio.Random, position, null, token, true).Forget();
            else throw new Exception($"Explosion audio of a requested size ({size}) doesn't exist!");
        }

        private async UniTask AwaitExplosionEffectToPlayAsync(CachedExplosion cache, CancellationToken token)
        {
            float timer = 0f;

            while (timer < cache.Pauser.EffectDuration)
            {
                timer += Time.deltaTime;

                while (_gamePauser.Paused == true) await UniTask.Yield();

                if (token != default && token.IsCancellationRequested == true) break;

                await UniTask.Yield();
            }
        }

        private void ReleaseExplosion(ExplosionSize size, CachedExplosion cache)
        {
            cache.Explosion.SetActive(false);
            cache.Explosion.transform.position = Vector3.zero;

            if (_explosionPool.TryGetValue(size, out Stack<CachedExplosion> stack) == true)
            {
                stack.Push(cache);
            }
            else
            {
                Stack<CachedExplosion> newStack = new();
                newStack.Push(cache);

                _explosionPool.Add(size, newStack);
            }
        }
    }
}