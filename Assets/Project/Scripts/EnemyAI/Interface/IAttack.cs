using UnityEngine;

namespace FPSGame.AI
{
    public interface IAttack
    {
        bool CanAttack();
        void Attack(GameObject target);
        bool IsInAttackRange(GameObject target);
    }
}