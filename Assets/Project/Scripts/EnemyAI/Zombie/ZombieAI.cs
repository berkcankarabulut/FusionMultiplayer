using BehaviourSystem;
using FPSGame.AI.Actions;
using UnityEngine;
using UnityEngine.AI;

namespace FPSGame.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class ZombieAI : MonoBehaviour
    {
        [SerializeField] private ZombieData _zombieData;

        [Header("Components")] 
        [SerializeField]
        private PatrolSystem _patrolSystem; 
        [SerializeField] 
        private NavMeshAgent _agent;

        private IMovement _movement;
        private BehaviourTree _behaviorTree;

        private void Start()
        {
            InitializeComponents();
            BuildBehaviorTree();
        }

        private void InitializeComponents()
        {
            var movementController = gameObject.AddComponent<ZombieMovementController>();
            movementController.Initialize(_agent, _zombieData);
            _movement = movementController;
            _patrolSystem.Initialize();
        }

        private void BuildBehaviorTree()
        {
            _behaviorTree = new BehaviourTree("Simple Zombie AI"); 
            Sequence patrolSequence = new Sequence("Patrol Sequence"); 
            PatrolAction patrolAction = new PatrolAction(_movement, _patrolSystem);
            patrolSequence.AddChild(patrolAction);

            // Sonra bekle
            IdleAction idleAction = new IdleAction(_zombieData);
            patrolSequence.AddChild(idleAction);

            // Ana tree'ye ekle
            _behaviorTree.AddChild(patrolSequence);

            // Debug: Tree yapısını yazdır
            Debug.Log("=== Zombie Behavior Tree ===");
            _behaviorTree.PrintTree();
        }

        private void Update()
        {
            if (_behaviorTree != null)
            {
                _behaviorTree.Process();
            }
        }

        // Debug için Gizmos
        private void OnDrawGizmosSelected()
        {
            if (_zombieData == null) return;

            // Patrol radius
            Gizmos.color = Color.blue;
            Vector3 center = Application.isPlaying ? _patrolSystem.transform.position : transform.position;
            Gizmos.DrawWireSphere(center, _zombieData.patrolRadius);

            // Current target
            if (Application.isPlaying && _patrolSystem != null && _patrolSystem.HasCurrentTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_patrolSystem.CurrentTarget, 0.5f);
                Gizmos.DrawLine(transform.position, _patrolSystem.CurrentTarget);
            }

            // Spawn point
            Gizmos.color = Color.green;
            Vector3 spawn = Application.isPlaying ? _patrolSystem.transform.position : transform.position;
            Gizmos.DrawWireCube(spawn, Vector3.one);
        }
    }
}