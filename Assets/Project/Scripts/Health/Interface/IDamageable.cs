using UnityEngine;

namespace FPSGame.Health
{
    public interface IDamageable
    {
        void TakeDamage(int damage, GameObject attacker = null);
        bool IsAlive { get; }
        Vector3 HitPoint { get; }
    }
}