 
using UnityEngine;
 

namespace FPSGame.AI
{
    public class ZombieMovementController : MonoBehaviour, IMovement
    {
        private UnityEngine.AI.NavMeshAgent _agent;
        private ZombieData _data;
        
        public void Initialize(UnityEngine.AI.NavMeshAgent agent, ZombieData data)
        {
            _agent = agent;
            _data = data;
        }
        
        public void MoveTo(Vector3 position)
        {
            if (!_agent.isActiveAndEnabled) return;
            _agent.SetDestination(position);
            _agent.isStopped = false;
        }
        
        public void Stop()
        {
            if (!_agent.isActiveAndEnabled) return;
            _agent.isStopped = true;
        }
        
        public bool HasReachedDestination()
        {
            return _agent.remainingDistance <= _data.stoppingDistance && !_agent.pathPending;
        }
        
        public float GetDistanceToTarget()
        {
            return _agent.remainingDistance;
        }
        
        public void SetSpeed(float speed)
        {
            if (_agent.isActiveAndEnabled)
            {
                _agent.speed = speed;
            }
        }
    }
}
  