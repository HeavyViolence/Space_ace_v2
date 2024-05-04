using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Shooting.Guns;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public delegate UniTask<ShotResult> ShotBehaviourAsync(object user, IGun gun);
}