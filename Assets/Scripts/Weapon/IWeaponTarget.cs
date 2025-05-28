using UnityEngine;

namespace FPSGame.Weapons
{
    public interface IWeaponTarget
    {
        bool TryDamage(int damage);
        Vector3 HitPoint { get; }
    }
}