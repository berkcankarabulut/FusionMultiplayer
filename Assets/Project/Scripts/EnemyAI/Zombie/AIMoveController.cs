using UnityEngine;


namespace FPSGame.AI
{
    public class AIMoveController  
    {
        private UnityEngine.AI.NavMeshAgent _agent;
        private float _stoppingDistance;
        public UnityEngine.AI.NavMeshAgent Agent => _agent;

        public void Initialize(UnityEngine.AI.NavMeshAgent agent, float stoppingDistance)
        {
            _agent = agent;
            _stoppingDistance = stoppingDistance;
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
            return _agent.remainingDistance <= _stoppingDistance && !_agent.pathPending;
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