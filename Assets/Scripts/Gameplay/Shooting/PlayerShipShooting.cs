using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Players;

using System.Threading;

namespace SpaceAce.Gameplay.Shooting
{
    public sealed class PlayerShipShooting : Shooting, IShootingController
    {
        private CancellationTokenSource _shootingCancellation;

        public void Shoot()
        {
            _shootingCancellation = new();
            FireAsync(_shootingCancellation.Token).Forget();
        }

        public void StopShooting()
        {
            _shootingCancellation?.Cancel();
            _shootingCancellation.Dispose();
            _shootingCancellation = null;
        }

        protected override async UniTask FireAsync(CancellationToken token = default)
        {
            await base.FireAsync(token);

            while (CurrentHeat < HeatCapacity)
            {
                while (GamePauser.Paused == true) await UniTask.Yield();

                if (CurrentHeat == HeatCapacity) await PerformOverheatAsync();

                if (token != default && token.IsCancellationRequested == true)
                {
                    Ceasefire();
                    return;
                }

                await UniTask.Yield();
            }
        }
    }
}