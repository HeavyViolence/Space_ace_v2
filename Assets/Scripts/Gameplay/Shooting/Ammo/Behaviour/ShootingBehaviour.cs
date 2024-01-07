using Cysharp.Threading.Tasks;

using System.Threading;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public delegate UniTask ShootingBehaviour(CancellationToken token, params object[] args);
}