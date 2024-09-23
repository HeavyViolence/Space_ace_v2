using Cysharp.Threading.Tasks;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Auxiliary
{
    public static class AuxAsync
    {
        public static async UniTask DelayAsync(Func<float> delayProvider,
                                               Func<bool> pauseCondition,
                                               CancellationToken token = default)
        {
            float timer = 0f;
            float delay = delayProvider();

            while (timer < delay)
            {
                if (token.IsCancellationRequested == true) return;

                timer += Time.deltaTime;

                await UniTask.WaitUntil(() => pauseCondition() == false);
                await UniTask.Yield();
            }
        }

        public static async UniTask DelayAsync(Func<bool> delayCondition,
                                               Func<bool> pauseCondition,
                                               CancellationToken token = default)
        {
            while (delayCondition() == true && token.IsCancellationRequested == false)
            {
                await UniTask.WaitUntil(() => pauseCondition() == false);
                await UniTask.Yield();
            }
        }

        public static async UniTask DelayThenDoAsync(Action action,
                                                     Func<float> delayProvider,
                                                     Func<bool> pauseCondition,
                                                     CancellationToken token = default)
        {
            await DelayAsync(delayProvider, pauseCondition, token);
            action();
        }

        public static async UniTask DoThenDelayAsync(Action action,
                                                     Func<float> delayProvider,
                                                     Func<bool> pauseCondition,
                                                     CancellationToken token = default)
        {
            action();
            await DelayAsync(delayProvider, pauseCondition, token);
        }

        public static async UniTask DoForeverAsync(Action action,
                                                   Func<float> firstDelayProvider,
                                                   Func<float> regularDelayProvider,
                                                   Func<bool> pauseCondition,
                                                   CancellationToken token = default)
        {
            await DelayAsync(firstDelayProvider, pauseCondition, token);

            while (token.IsCancellationRequested == false)
                await DoThenDelayAsync(action, regularDelayProvider, pauseCondition, token);
        }
    }
}