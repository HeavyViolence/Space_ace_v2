using Cysharp.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class HitFactory
    {
        private readonly DiContainer _diContainer;
        private readonly GamePauser _gamePauser;
        private readonly Dictionary<HitStrength, GameObject> _hitPrefabs = new();
        private readonly Dictionary<HitStrength, Stack<CachedParticleSystem>> _hitPool = new();

        public HitFactory(DiContainer diContainer,
                          GamePauser gamePauser,
                          IEnumerable<HitSlot> hitEffects)
        {
            _diContainer = diContainer ?? throw new ArgumentNullException(nameof(diContainer),
                $"Attempted to pass an empty {typeof(DiContainer)}!");

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");

            if (hitEffects is null)
                throw new ArgumentNullException(nameof(hitEffects),
                    "Attempted to pass an empty hit effects collection!");

            foreach (var hitEffect in hitEffects)
                _hitPrefabs.Add(hitEffect.Strength, hitEffect.Prefab);
        }

        public async UniTask CreateAsync(HitStrength strength, Vector3 position, CancellationToken token = default)
        {
            CachedParticleSystem hitEffect = InstantiateHitEffect(strength, position);
            await AwaitHitEffectToPlayAsync(hitEffect, token);
            ReleaseHitEffect(strength, hitEffect);
        }

        private CachedParticleSystem InstantiateHitEffect(HitStrength strength, Vector3 position)
        {
            CachedParticleSystem hitEffect;

            if (_hitPool.TryGetValue(strength, out Stack<CachedParticleSystem> stack) == true && stack.Count > 0)
            {
                hitEffect = stack.Pop();
            }
            else if (_hitPrefabs.TryGetValue(strength, out GameObject hitPrefab) == true)
            {
                GameObject instance = _diContainer.InstantiatePrefab(hitPrefab);

                if (hitPrefab.TryGetComponent(out ParticleSystemPauser pauser) == true)
                    hitEffect = new(instance, pauser);
                else throw new MissingComponentException($"Hit prefab is missing {typeof(ParticleSystemPauser)}!");
            }
            else throw new Exception($"Hit effect of a requested strength ({strength}) doesn't exist!");

            hitEffect.Instance.SetActive(true);
            hitEffect.Instance.transform.position = position;

            return hitEffect;
        }

        private async UniTask AwaitHitEffectToPlayAsync(CachedParticleSystem instance, CancellationToken token)
        {
            float timer = 0f;

            while (timer < instance.Pauser.EffectDuration)
            {
                timer += Time.deltaTime;

                while (_gamePauser.Paused == true) await UniTask.Yield();

                if (token != default && token.IsCancellationRequested == true) break;

                await UniTask.Yield();
            }
        }

        private void ReleaseHitEffect(HitStrength strength, CachedParticleSystem instance)
        {
            instance.Instance.SetActive(false);
            instance.Instance.transform.position = Vector3.zero;

            if (_hitPool.TryGetValue(strength, out Stack<CachedParticleSystem> stack) == true)
            {
                stack.Push(instance);
            }
            else
            {
                Stack<CachedParticleSystem> newStack = new();
                newStack.Push(instance);

                _hitPool.Add(strength, newStack);
            }
        }
    }
}