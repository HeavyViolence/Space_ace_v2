using Cysharp.Threading.Tasks;

using SpaceAce.Main.Audio;

using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.ExplosionFactories
{
    public class ExplosionFactory
    {
        private readonly DiContainer _diContainer;
        private readonly GamePauser _gamePauser;
        private readonly AudioPlayer _audioPlayer;
        private readonly MasterCameraShaker _masterCameraShaker;
        private readonly Dictionary<ExplosionSize, GameObject> _explosionPrefabs = new();
        private readonly Dictionary<ExplosionSize, Transform> _explosionAnchors = new();
        private readonly Dictionary<ExplosionSize, AudioCollection> _explosionAudio = new();
        private readonly Dictionary<ExplosionSize, Stack<ParticleSystemCache>> _explosionPool = new();
        private readonly GameObject _masterAnchor = new("Explosion object pools");

        public ExplosionFactory(DiContainer diContainer,
                                GamePauser gamePauser,
                                AudioPlayer audioPlayer,
                                MasterCameraShaker masterCameraShaker,
                                IEnumerable<ExplosionSlot> explosions)
        {
            _diContainer = diContainer ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException();
            _masterCameraShaker = masterCameraShaker ?? throw new ArgumentNullException();

            if (explosions is null) throw new ArgumentNullException();

            foreach (var explosion in explosions)
            {
                _explosionPrefabs.Add(explosion.Size, explosion.Prefab);
                _explosionAudio.Add(explosion.Size, explosion.Audio);

                GameObject anchor = new($"{explosion.Size.ToString().ToLower()} explosions anchor");
                anchor.transform.parent = _masterAnchor.transform;

                _explosionAnchors.Add(explosion.Size, anchor.transform);
            }
        }

        public async UniTask CreateAsync(ExplosionSize size, Vector3 position, CancellationToken token = default)
        {
            ParticleSystemCache explosion = InstantiateExplosion(size, position);

            PlayExplosionEffects(size, position, token);
            await AwaitExplosionEffectToPlayAsync(explosion, token);
            Release(explosion, size);
        }

        private ParticleSystemCache InstantiateExplosion(ExplosionSize size, Vector3 position)
        {
            if (_explosionPool.TryGetValue(size, out Stack<ParticleSystemCache> stack) &&
                stack.TryPop(out ParticleSystemCache system) == true)
            {
                system.Object.SetActive(true);
                system.Transform.parent = null;
                system.Transform.position = position;

                return system;
            }

            if (_explosionPrefabs.TryGetValue(size, out GameObject explosionPrefab) == true)
            {
                GameObject instance = _diContainer.InstantiatePrefab(explosionPrefab);

                if (instance.TryGetComponent(out Transform transform) == false) throw new MissingComponentException($"{typeof(Transform)}");
                if (instance.TryGetComponent(out ParticleSystemPauser pauser) == false) throw new MissingComponentException($"{typeof(ParticleSystemPauser)}");

                transform.position = position;

                return new(instance, pauser);
            }

            throw new Exception($"Explosion prefab of a requested size ({size}) doesn't exist in the config!");
        }

        private void PlayExplosionEffects(ExplosionSize size, Vector3 position, CancellationToken token)
        {
            if (_explosionAudio.TryGetValue(size, out AudioCollection audio) == true)
                _audioPlayer.PlayOnceAsync(audio.Random, position, null, true, token).Forget();
            else throw new Exception($"Explosion audio of a requested size ({size}) doesn't exist!");

            _masterCameraShaker.ShakeOnDefeat();
        }

        private async UniTask AwaitExplosionEffectToPlayAsync(ParticleSystemCache instance, CancellationToken token)
        {
            float timer = 0f;

            while (timer < instance.Pauser.EffectDuration)
            {
                timer += Time.deltaTime;

                while (_gamePauser.Paused == true) await UniTask.Yield();

                if (token != default && token.IsCancellationRequested == true) return;

                await UniTask.Yield();
            }
        }

        private void Release(ParticleSystemCache instance, ExplosionSize size)
        {
            instance.Object.SetActive(false);
            instance.Transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.Transform.parent = _explosionAnchors[size];

            if (_explosionPool.TryGetValue(size, out Stack<ParticleSystemCache> stack) == true)
            {
                stack.Push(instance);
            }
            else
            {
                Stack<ParticleSystemCache> newStack = new();
                newStack.Push(instance);

                _explosionPool.Add(size, newStack);
            }
        }
    }
}