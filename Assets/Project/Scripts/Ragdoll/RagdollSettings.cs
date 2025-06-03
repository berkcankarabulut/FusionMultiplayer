using UnityEngine;

namespace FPSGame.Ragdoll
{
    [System.Serializable]
    public class RagdollSettings
    {
        [Header("Force Settings")]
        [SerializeField] public float baseForce = 500f;
        [SerializeField] public float randomForceMultiplier = 0.5f;
        [SerializeField] public Vector3 forceDirection = Vector3.up;
        
        [Header("Physics Settings")]
        [SerializeField] public float mass = 1f;
        [SerializeField] public float drag = 0.1f;
        [SerializeField] public float angularDrag = 0.05f;
        
        [Header("Auto Cleanup")]
        [SerializeField] public bool autoCleanup = true;
        [SerializeField] public float cleanupDelay = 10f;
    }
}