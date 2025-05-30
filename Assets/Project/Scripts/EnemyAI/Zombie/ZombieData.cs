using UnityEngine;

namespace FPSGame.AI
{
    [System.Serializable]
    public class ZombieData
    {
        [Header("Movement Settings")]
        public float walkSpeed = 2f;
        public float stoppingDistance = 1f;
        
        [Header("Patrol Settings")]
        public float patrolRadius = 10f;
        public float idleTime = 3f;
    }
}