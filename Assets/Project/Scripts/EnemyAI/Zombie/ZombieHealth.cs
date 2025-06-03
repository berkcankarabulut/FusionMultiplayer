using UnityEngine;
using FPSGame.Health;
using FPSGame.Ragdoll;
using Photon.Pun;

namespace FPSGame.AI
{
    public class ZombieHealth : BaseHealth
    {
        [Header("Zombie Specific Settings")]
        [SerializeField] private bool ragdollOnDeath = true;
        [SerializeField] private RagdollSystem ragdollSystem;
        [SerializeField] private float deathDelay = 3f;
        
        protected override void HandleDeath(GameObject attacker = null)
        {
            if (ragdollOnDeath && ragdollSystem != null)
            {
                Vector3 deathForce = Vector3.up + Random.insideUnitSphere * 0.5f;
                ragdollSystem.EnableRagdoll(transform.position, deathForce, 1f);
            }

            // Death RPC gönder - sadece Master Client
            if (PhotonNetwork.IsMasterClient)
            {
                int attackerViewID = -1;
                if (attacker != null)
                {
                    PhotonView attackerPV = attacker.GetComponent<PhotonView>();
                    if (attackerPV != null)
                        attackerViewID = attackerPV.ViewID;
                }
                
                GetComponent<PhotonView>().RPC(nameof(OnZombieDeathRPC), RpcTarget.All, attackerViewID);
            }

            // Death delay sonrası pool'a döndür
            Invoke(nameof(ReturnToPool), deathDelay);
        }

        [PunRPC]
        private void OnZombieDeathRPC(int attackerViewID = -1)
        {
            // Tüm clientlarda death effects
            if (ragdollOnDeath && ragdollSystem != null && !ragdollSystem.IsRagdollActive)
            {
                Vector3 deathForce = Vector3.up + Random.insideUnitSphere * 0.5f;
                ragdollSystem.EnableRagdoll(transform.position, deathForce, 1f);
            }
        }

        public override void ResetHealth()
        {
            base.ResetHealth();

            // Ragdoll'u kapat
            if (ragdollSystem != null)
            {
                ragdollSystem.DisableRagdoll();
            }

            // Death invoke'unu iptal et
            CancelInvoke(nameof(ReturnToPool));
        }

        public void EnableRagdoll(Vector3? forceDirection = null, float forceMultiplier = 1f)
        {
            if (ragdollSystem != null)
            {
                ragdollSystem.EnableRagdoll(transform.position, forceDirection, forceMultiplier);
            }
        }

        public void AddExplosionForce(float force, Vector3 explosionPosition, float radius)
        {
            if (ragdollSystem != null && ragdollSystem.IsRagdollActive)
            {
                ragdollSystem.AddExplosionForce(force, explosionPosition, radius);
            }
        }

        private void ReturnToPool()
        {
            // Master Client olup olmadığını kontrol et
            if (!PhotonNetwork.IsMasterClient) return;
            
            // ZombieSpawnManager'a döndür
            var spawnManager = FindObjectOfType<ZombieSpawnManager>();
            if (spawnManager != null)
            {
                spawnManager.ReturnZombieToPool(gameObject);
            }
        }
    }
}