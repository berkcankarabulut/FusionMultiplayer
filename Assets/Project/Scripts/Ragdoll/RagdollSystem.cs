using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace FPSGame.Ragdoll
{
    public class RagdollSystem : MonoBehaviour
    {
        [Header("Ragdoll Configuration")] [SerializeField]
        private RagdollSettings _settings = new RagdollSettings(); 
        [SerializeField] private bool _excludeMainCollider = true;

        [Header("Components")] [SerializeField]
        private Rigidbody[] _ragdollRigidbodies;

        [SerializeField] private Collider[] _ragdollColliders;
        [SerializeField] private Animator _animator;
        [SerializeField] private Collider _mainCollider;

        [Header("Debug")] [SerializeField] private bool _showDebugInfo = false;

        private bool _isRagdollActive = false;
        private CancellationTokenSource _cleanupCancellationToken;

        // Backup original physics settings
        private Dictionary<Rigidbody, RigidbodyState> _originalStates = new Dictionary<Rigidbody, RigidbodyState>();

        public bool IsRagdollActive => _isRagdollActive;
        public RagdollSettings Settings => _settings;

        // Events
        public event System.Action OnRagdollEnabled;
        public event System.Action OnRagdollDisabled;
        public event System.Action OnRagdollCleanedUp;

        private struct RigidbodyState
        {
            public bool isKinematic;
            public float mass;
            public float drag;
            public float angularDrag;
        }


        public void EnableRagdoll(Vector3? forcePoint = null, Vector3? forceDirection = null,
            float? forceMultiplier = null)
        {
            if (_isRagdollActive) return;

            _isRagdollActive = true;

            // Disable animator
            if (_animator != null)
            {
                _animator.enabled = false;
            }

            // Enable ragdoll physics
            foreach (var rb in _ragdollRigidbodies)
            {
                rb.isKinematic = false;
                rb.mass = _settings.mass;
                rb.drag = _settings.drag;
                rb.angularDrag = _settings.angularDrag;
            }

            // Enable ragdoll colliders
            foreach (var col in _ragdollColliders)
            {
                if (_excludeMainCollider && col == _mainCollider) continue;
                col.enabled = true;
            }

            // Disable main collider
            if (_mainCollider != null)
            {
                _mainCollider.enabled = false;
            }

            // Apply forces
            ApplyRagdollForces(forcePoint, forceDirection, forceMultiplier);

            OnRagdollEnabled?.Invoke();

            // Start auto cleanup if enabled
            if (_settings.autoCleanup)
            {
                StartCleanupTimer();
            }

            if (_showDebugInfo)
            {
                Debug.Log("Ragdoll enabled");
            }
        }

        public void DisableRagdoll()
        {
            if (!_isRagdollActive) return;

            _isRagdollActive = false;

            // Stop cleanup timer
            StopCleanupTimer();

            // Restore original states
            foreach (var rb in _ragdollRigidbodies)
            {
                if (_originalStates.TryGetValue(rb, out RigidbodyState originalState))
                {
                    rb.isKinematic = originalState.isKinematic;
                    rb.mass = originalState.mass;
                    rb.drag = originalState.drag;
                    rb.angularDrag = originalState.angularDrag;
                }
            }

            // Disable ragdoll colliders
            foreach (var col in _ragdollColliders)
            {
                if (_excludeMainCollider && col == _mainCollider) continue;
                col.enabled = false;
            }

            // Enable main collider
            if (_mainCollider != null)
            {
                _mainCollider.enabled = true;
            }

            // Enable animator
            if (_animator != null)
            {
                _animator.enabled = true;
            }

            OnRagdollDisabled?.Invoke();

            if (_showDebugInfo)
            {
                Debug.Log("Ragdoll disabled");
            }
        }

        private void ApplyRagdollForces(Vector3? forcePoint = null, Vector3? forceDirection = null,
            float? forceMultiplier = null)
        {
            Vector3 finalForceDirection = forceDirection ?? _settings.forceDirection;
            float finalForceMultiplier = forceMultiplier ?? 1f;

            foreach (var rb in _ragdollRigidbodies)
            {
                Vector3 force = finalForceDirection * _settings.baseForce * finalForceMultiplier;
                Vector3 randomForce = Random.insideUnitSphere * _settings.baseForce * _settings.randomForceMultiplier;
                Vector3 totalForce = force + randomForce;

                if (forcePoint.HasValue)
                {
                    rb.AddForceAtPosition(totalForce, forcePoint.Value);
                }
                else
                {
                    rb.AddForce(totalForce);
                }
            }
        }

        public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius)
        {
            if (!_isRagdollActive) return;

            foreach (var rb in _ragdollRigidbodies)
            {
                rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
            }
        }

        public void SetRagdollSettings(RagdollSettings newSettings)
        {
            _settings = newSettings;

            if (_isRagdollActive)
            {
                // Apply new settings to active ragdoll
                foreach (var rb in _ragdollRigidbodies)
                {
                    rb.mass = _settings.mass;
                    rb.drag = _settings.drag;
                    rb.angularDrag = _settings.angularDrag;
                }
            }
        }

        private void StartCleanupTimer()
        {
            if (_cleanupCancellationToken != null) return;

            _cleanupCancellationToken = new CancellationTokenSource();
            CleanupAsync(_cleanupCancellationToken.Token).Forget();
        }

        private void StopCleanupTimer()
        {
            if (_cleanupCancellationToken != null)
            {
                _cleanupCancellationToken.Cancel();
                _cleanupCancellationToken.Dispose();
                _cleanupCancellationToken = null;
            }
        }

        private async UniTaskVoid CleanupAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay((int)(_settings.cleanupDelay * 1000), cancellationToken: cancellationToken);

                if (!cancellationToken.IsCancellationRequested && gameObject != null)
                {
                    OnRagdollCleanedUp?.Invoke();

                    if (_showDebugInfo)
                    {
                        Debug.Log("Ragdoll auto-cleanup triggered");
                    }

                    Destroy(gameObject);
                }
            }
            catch (System.OperationCanceledException)
            { 
            }
            finally
            {
                _cleanupCancellationToken?.Dispose();
                _cleanupCancellationToken = null;
            }
        }

        public void ForceCleanup()
        {
            StopCleanupTimer();
            OnRagdollCleanedUp?.Invoke();

            if (_showDebugInfo)
            {
                Debug.Log("Ragdoll force cleanup");
            }

            Destroy(gameObject);
        }

        // Utility methods
        public Vector3 GetCenterOfMass()
        {
            if (_ragdollRigidbodies == null || _ragdollRigidbodies.Length == 0)
                return transform.position;

            Vector3 centerOfMass = Vector3.zero;
            float totalMass = 0f;

            foreach (var rb in _ragdollRigidbodies)
            {
                centerOfMass += rb.worldCenterOfMass * rb.mass;
                totalMass += rb.mass;
            }

            return totalMass > 0 ? centerOfMass / totalMass : transform.position;
        }

        public float GetTotalMass()
        {
            if (_ragdollRigidbodies == null) return 0f;

            float totalMass = 0f;
            foreach (var rb in _ragdollRigidbodies)
            {
                totalMass += rb.mass;
            }

            return totalMass;
        }

        private void OnDestroy()
        {
            StopCleanupTimer();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showDebugInfo || !_isRagdollActive) return;

            // Draw center of mass
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetCenterOfMass(), 0.1f);

            // Draw ragdoll rigidbodies
            if (_ragdollRigidbodies != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var rb in _ragdollRigidbodies)
                {
                    if (rb != null)
                    {
                        Gizmos.DrawWireCube(rb.transform.position, Vector3.one * 0.05f);
                    }
                }
            }
        }
    }
}