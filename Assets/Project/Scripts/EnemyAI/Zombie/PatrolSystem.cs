using UnityEngine;
using System.Collections.Generic;

namespace FPSGame.AI
{
    public class PatrolSystem : MonoBehaviour
    {
        [Header("Patrol Settings")] 
        [SerializeField] private Transform[] _patrolPoints;
        [SerializeField] private bool _useRandomPatrol = true;
        [SerializeField] private bool _loopPatrol = true;
        
        [Header("Fallback Settings")]
        [SerializeField] private bool _useFallbackRadius = true;
        [SerializeField] private float _fallbackRadius = 10f;
        
        private Vector3 _spawnPoint;
        private Vector3 _currentTarget;
        private bool _hasTarget;
        private int _currentPatrolIndex = 0;
        private List<Vector3> _validPatrolPoints = new List<Vector3>();
        
        [Header("Debug")]
        [SerializeField] private bool _showGizmos = true;
        [SerializeField] private Color _gizmosColor = Color.yellow;
        
        private void Awake()
        {
            _spawnPoint = transform.position;
            ValidatePatrolPoints();
        }
        
        public void Initialize()
        { 
            _spawnPoint = transform.position;
            ValidatePatrolPoints();
        }
        
        private void ValidatePatrolPoints()
        {
            _validPatrolPoints.Clear();
            
            if (_patrolPoints != null && _patrolPoints.Length > 0)
            {
                foreach (Transform point in _patrolPoints)
                {
                    if (point == null) continue;
                    if (UnityEngine.AI.NavMesh.SamplePosition(point.position, out UnityEngine.AI.NavMeshHit hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        _validPatrolPoints.Add(hit.position);
                    }
                    else
                    {
                        Debug.LogWarning($"Patrol point {point.name} is not on NavMesh!", point);
                    }
                }
            } 
        }
        
        public Vector3 GetRandomPatrolPoint()
        {
            if (_validPatrolPoints.Count <= 0) return GetFallbackPatrolPoint();
            
            if (_useRandomPatrol)
            { 
                int randomIndex = Random.Range(0, _validPatrolPoints.Count);
                _currentTarget = _validPatrolPoints[randomIndex];
            }
            else
            { 
                _currentTarget = _validPatrolPoints[_currentPatrolIndex];
                    
                _currentPatrolIndex++;
                if (_currentPatrolIndex >= _validPatrolPoints.Count)
                {
                    _currentPatrolIndex = _loopPatrol ? 0 : _validPatrolPoints.Count - 1;
                }
            }
                
            _hasTarget = true;
            return _currentTarget; 
        }
        
        private Vector3 GetFallbackPatrolPoint()
        {
            if (!_useFallbackRadius)
            {
                _currentTarget = _spawnPoint;
                _hasTarget = true;
                return _spawnPoint;
            }
             
            Vector2 randomDirection = Random.insideUnitCircle * _fallbackRadius;
            Vector3 targetPosition = _spawnPoint + new Vector3(randomDirection.x, 0, randomDirection.y);
             
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, _fallbackRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                _currentTarget = hit.position;
                _hasTarget = true;
                return hit.position;
            } 
            
            _currentTarget = _spawnPoint;
            _hasTarget = true;
            return _spawnPoint;
        }
        
        public Vector3 GetNextSequentialPatrolPoint()
        {
            if (_validPatrolPoints.Count == 0)
                return GetFallbackPatrolPoint();
                
            _currentTarget = _validPatrolPoints[_currentPatrolIndex];
            
            _currentPatrolIndex++;
            if (_currentPatrolIndex >= _validPatrolPoints.Count)
            {
                _currentPatrolIndex = _loopPatrol ? 0 : _validPatrolPoints.Count - 1;
            }
            
            _hasTarget = true;
            return _currentTarget;
        }
        
        public bool HasCurrentTarget => _hasTarget;
        public Vector3 CurrentTarget => _currentTarget;
        public int PatrolPointCount => _validPatrolPoints.Count;
        public bool HasValidPatrolPoints => _validPatrolPoints.Count > 0;
        
        public void ClearTarget()
        {
            _hasTarget = false;
        }
        
        public void ResetPatrolIndex()
        {
            _currentPatrolIndex = 0;
        }
        
        public Vector3 GetClosestPatrolPoint(Vector3 position)
        {
            if (_validPatrolPoints.Count == 0)
                return _spawnPoint;
                
            Vector3 closest = _validPatrolPoints[0];
            float closestDistance = Vector3.Distance(position, closest);
            
            for (int i = 1; i < _validPatrolPoints.Count; i++)
            {
                float distance = Vector3.Distance(position, _validPatrolPoints[i]);
                if (distance < closestDistance)
                {
                    closest = _validPatrolPoints[i];
                    closestDistance = distance;
                }
            }
            
            return closest;
        }
         
        [ContextMenu("Validate Patrol Points")]
        public void ValidatePatrolPointsEditor()
        {
            ValidatePatrolPoints();
        }
        
        private void OnDrawGizmos()
        {
            if (!_showGizmos) return;
             
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
             
            if (_patrolPoints != null)
            {
                Gizmos.color = _gizmosColor;
                for (int i = 0; i < _patrolPoints.Length; i++)
                {
                    if (_patrolPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(_patrolPoints[i].position, 0.5f);
                         
                        #if UNITY_EDITOR
                        UnityEditor.Handles.Label(_patrolPoints[i].position, i.ToString());
                        #endif
                         
                        if (!_useRandomPatrol && i < _patrolPoints.Length - 1 && _patrolPoints[i + 1] != null)
                        {
                            Gizmos.DrawLine(_patrolPoints[i].position, _patrolPoints[i + 1].position);
                        }
                         
                        if (!_useRandomPatrol && _loopPatrol && i == _patrolPoints.Length - 1 && _patrolPoints[0] != null)
                        {
                            Gizmos.DrawLine(_patrolPoints[i].position, _patrolPoints[0].position);
                        }
                    }
                }
            }
             
            if (_useFallbackRadius)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, _fallbackRadius);
            }
             
            if (Application.isPlaying && _hasTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_currentTarget, 0.3f);
                Gizmos.DrawLine(transform.position, _currentTarget);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Seçili olduğunda daha detaylı gizmos
            if (!_showGizmos) return;
            
            // Tüm patrol noktalarından spawn'a çizgiler
            if (_patrolPoints != null)
            {
                Gizmos.color = Color.cyan;
                foreach (Transform point in _patrolPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawLine(transform.position, point.position);
                    }
                }
            }
        }
    }
}