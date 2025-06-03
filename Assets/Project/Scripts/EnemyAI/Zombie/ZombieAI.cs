using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using BehaviourSystem;
using FPSGame.AI.Actions;

namespace FPSGame.AI
{
    [RequireComponent(typeof(NavMeshAgent), typeof(ZombieHealth))]
    public class ZombieAI : BaseAI
    {
        [Header("Zombie AI Settings")] [SerializeField]
        private ZombieData zombieData;

        [SerializeField] private PatrolSystem patrolSystem;
        [SerializeField] private Animator animator; 
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private ZombieHealth zombieHealth;
        
        private AIMoveController aiMoveController;
        private BehaviourTree behaviorTree;

        private void Awake()
        {
            GetComponents();
            ValidateComponents();
        }

        protected override void OnAIInitialized()
        {
            SetupComponents();
            BuildBehaviorTree();
            Debug.Log("Zombie AI initialized");
        }

        protected override void OnAIStopped()
        {
            if (agent != null && agent.isActiveAndEnabled)
                agent.isStopped = true;

            Debug.Log("Zombie AI stopped");
        }

        protected override void UpdateAI()
        {
            if (behaviorTree != null && !zombieHealth.IsDead)
            {
                behaviorTree.Process();
                UpdateAnimations();
            }
        }

        private void GetComponents()
        {
            agent = GetComponent<NavMeshAgent>();
            zombieHealth = GetComponent<ZombieHealth>();

            if (patrolSystem == null)
                patrolSystem = GetComponent<PatrolSystem>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void ValidateComponents()
        {
            if (agent == null) Debug.LogError($"NavMeshAgent not found on {name}");
            if (zombieHealth == null) Debug.LogError($"ZombieHealth not found on {name}");
            if (patrolSystem == null) Debug.LogError($"PatrolSystem not found on {name}");
            if (zombieData == null) Debug.LogError($"ZombieData not assigned on {name}");
        }

        private void SetupComponents()
        {
            // Agent setup - sadece Master Client
            if (agent != null)
            {
                agent.enabled = PhotonNetwork.IsMasterClient;
                if (agent.enabled)
                {
                    agent.isStopped = false;
                }
            }

            // AI Move Controller
            if (aiMoveController == null)
            {
                aiMoveController = new AIMoveController();
                aiMoveController.Initialize(agent, zombieData.stoppingDistance);
            }

            // Patrol system
            if (patrolSystem != null)
                patrolSystem.Initialize();

            // Health events
            if (zombieHealth != null)
            {
                zombieHealth.OnDeath += OnZombieDeath;
                zombieHealth.OnDamageTaken += OnZombieDamage;
            }
        }

        private void BuildBehaviorTree()
        {
            behaviorTree = new BehaviourTree("Zombie AI");

            Sequence patrolSequence = new Sequence("Patrol Sequence");
            PatrolAction patrolAction = new PatrolAction(aiMoveController, patrolSystem);
            patrolSequence.AddChild(patrolAction);

            IdleAction idleAction = new IdleAction(zombieData);
            patrolSequence.AddChild(idleAction);

            behaviorTree.AddChild(patrolSequence);

            Debug.Log("=== Zombie Behavior Tree Built ===");
            behaviorTree.PrintTree();
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            bool isMoving = agent != null && agent.velocity.magnitude > 0.1f;
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsDead", zombieHealth.IsDead);
        }

        private void OnZombieDeath()
        {
            StopAI();
            UpdateAnimations();
        }

        private void OnZombieDamage(float damage, GameObject attacker)
        {
            // Damage effects burada eklenebilir
        }

        public void ResetAI()
        {
            // Health reset
            if (zombieHealth != null)
                zombieHealth.ResetHealth();

            // Patrol reset
            if (patrolSystem != null)
                patrolSystem.Initialize();

            // Agent reset
            if (agent != null && agent.isActiveAndEnabled)
                agent.Warp(transform.position);

            // Animation reset
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetBool("IsDead", false);
            }

            // AI'ı yeniden başlat
            if (PhotonNetwork.IsMasterClient)
                InitializeAI();
        }

        private void OnDrawGizmosSelected()
        {
            if (zombieData == null) return;

            Gizmos.color = Color.blue;
            Vector3 center = patrolSystem != null ? patrolSystem.transform.position : transform.position;
            Gizmos.DrawWireSphere(center, zombieData.patrolRadius);

            if (Application.isPlaying && patrolSystem != null && patrolSystem.HasCurrentTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(patrolSystem.CurrentTarget, 0.5f);
                Gizmos.DrawLine(transform.position, patrolSystem.CurrentTarget);
            }
        }

        private void OnDestroy()
        {
            if (zombieHealth != null)
            {
                zombieHealth.OnDeath -= OnZombieDeath;
                zombieHealth.OnDamageTaken -= OnZombieDamage;
            }
        }
    }
}