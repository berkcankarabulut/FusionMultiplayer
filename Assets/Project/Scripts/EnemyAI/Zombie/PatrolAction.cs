using BehaviourSystem;
using UnityEngine;

namespace FPSGame.AI.Actions
{
    public class PatrolAction : Leaf
    {
        private readonly IMovement _movement;
        private readonly PatrolSystem _patrolSystem;
        private bool _isMoving;
        private Vector3 _targetPosition;
        private float _stuckTimer;
        private Vector3 _lastPosition;
        private readonly float _stuckThreshold = 2f; 
        private readonly float _stuckDistance = 0.1f;  
        
        public PatrolAction(IMovement movement, PatrolSystem patrolSystem) 
            : base("Patrol")
        { 
            _movement = movement;
            _patrolSystem = patrolSystem;
        }
        
        public override Status Process()
        {  
            if (!_isMoving)
            {
                if (!_patrolSystem.HasValidPatrolPoints)
                { 
                    return Status.FAILURE;
                }
                
                _targetPosition = _patrolSystem.GetRandomPatrolPoint();
                _movement.MoveTo(_targetPosition);
                _isMoving = true;
                _stuckTimer = 0f;
                _lastPosition = ((MonoBehaviour)_movement).transform.position;
                 
                return Status.RUNNING;
            }
             
            Vector3 currentPosition = ((MonoBehaviour)_movement).transform.position;
            if (Vector3.Distance(currentPosition, _lastPosition) < _stuckDistance)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= _stuckThreshold)
                { 
                    _movement.Stop();
                    _isMoving = false;
                    return Status.FAILURE; 
                }
            }
            else
            {
                _stuckTimer = 0f;
                _lastPosition = currentPosition;
            }
             
            if (_movement.HasReachedDestination())
            { 
                _patrolSystem.ClearTarget();
                _isMoving = false;
                return Status.SUCCESS;
            }
              
            return Status.RUNNING;
        }
        
        public override void Reset()
        {
            _movement.Stop();
            _isMoving = false;
            _stuckTimer = 0f;
            base.Reset();
        }
    }
}