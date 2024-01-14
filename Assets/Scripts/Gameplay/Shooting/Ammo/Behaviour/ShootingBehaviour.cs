using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Shooting.Guns;

using System.Threading;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public delegate UniTask ShootingBehaviour(Gun gun, CancellationToken token, params object[] args);
}