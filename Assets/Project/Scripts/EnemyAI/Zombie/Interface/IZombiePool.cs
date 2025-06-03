using UnityEngine;

namespace FPSGame.AI
{
    public interface IZombiePool
    {
        GameObject GetZombie();
        void ReturnZombie(GameObject zombie);
        int ActiveCount { get; }
        int PooledCount { get; }
    }
}