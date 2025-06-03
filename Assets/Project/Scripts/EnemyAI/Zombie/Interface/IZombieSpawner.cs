using UnityEngine;

namespace FPSGame.AI
{
    public interface IZombieSpawner
    {
        void Spawn();
        void SpawnZombieAtPoint(int pointIndex);
        void ReturnZombieToPool(GameObject zombie);
        bool CanSpawn { get; }
    }
}