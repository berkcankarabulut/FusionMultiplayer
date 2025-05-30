using UnityEngine;

namespace FPSGame.AI
{
    public interface IDetection
    {
        GameObject GetNearestPlayer();
        bool CanSeePlayer(GameObject player);
        float GetDistanceToPlayer(GameObject player);
    }
}