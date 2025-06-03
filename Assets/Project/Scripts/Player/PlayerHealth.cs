using System.Threading;
using Cysharp.Threading.Tasks;
using FPSGame.Health;
using FPSGame.Networking;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace FPSGame.Player
{
    public class PlayerHealth : BaseHealth
    {
        [Header("Player UI")]
        [SerializeField] private TextMeshProUGUI healthText;
        
        [Header("Player Specific Settings")]
        [SerializeField] private bool enableHealthRegeneration = false;
        [SerializeField] private float regenDelay = 5f;
        [SerializeField] private int regenAmount = 1;
        [SerializeField] private float regenInterval = 1f;

        private bool isLocalPlayer;
        private PhotonView photonView;
        private float lastDamageTime;
        private CancellationTokenSource regenCancellationToken;

        public bool IsLocalPlayer => isLocalPlayer;
        public bool IsAlive => !isDead;
        public bool IsHealthFull => currentHealth >= maxHealth;

        // Events
        public event System.Action OnKilled;

        public void Init(bool localPlayer, PhotonView pv)
        {
            isLocalPlayer = localPlayer;
            photonView = pv;
            UpdateHealthUI();
        }

        protected override void TakeDamage(float damage, GameObject attacker = null)
        {
            float previousHealth = currentHealth;
            base.TakeDamage(damage, attacker);
            
            UpdateHealthUI();
            OnDamageTakenInternal((int)damage, (int)previousHealth, attacker);
            
            if (enableHealthRegeneration)
            {
                lastDamageTime = Time.time;
                StopRegeneration();
            }
        }

        public override void Heal(float amount)
        {
            float previousHealth = currentHealth;
            base.Heal(amount);
            
            UpdateHealthUI();
            OnHealed((int)amount, (int)previousHealth);
        }

        public override void ResetHealth()
        {
            base.ResetHealth();
            StopRegeneration();
            UpdateHealthUI();
        }

        protected override void HandleDeath(GameObject attacker = null)
        {
            if (isLocalPlayer)
            {
                HandleLocalPlayerDeath(attacker);
            }
            
            if (photonView != null)
            {
                string attackerName = attacker != null ? attacker.name : "";
                photonView.RPC(nameof(OnPlayerKilled), RpcTarget.All, attackerName);
            }
            
            Destroy(gameObject);
        }

        private void HandleLocalPlayerDeath(GameObject attacker = null)
        {
            StopRegeneration();
            
            if (RoomManager.instance != null)
            {
                RoomManager.instance.PlayerSpawn();
                RoomManager.instance.AddDeath();
            }
            
            if (attacker != null)
            {
                Debug.Log($"Player was killed by: {attacker.name}");
            }
        }

        [PunRPC]
        private void OnPlayerKilled(string attackerName = "")
        {
            OnKilled?.Invoke();
            
            if (RoomManager.instance != null)
            {
                RoomManager.instance.AddKill(1);
            }
            
            if (!string.IsNullOrEmpty(attackerName))
            {
                Debug.Log($"Player killed by: {attackerName}");
            }
        }

        private void OnDamageTakenInternal(int damage, int previousHealth, GameObject attacker = null)
        {
            if (isLocalPlayer)
            {
                string attackerName = attacker != null ? attacker.name : "Unknown";
                Debug.Log($"Player took {damage} damage from {attackerName}! Health: {CurrentHealth}/{MaxHealth}");
            }
        }

        private void OnHealed(int amount, int previousHealth)
        {
            if (isLocalPlayer)
            {
                Debug.Log($"Player healed {amount} HP! Health: {CurrentHealth}/{MaxHealth}");
            }
        }

        private void Update()
        {
            HandleHealthRegeneration();
        }

        private void HandleHealthRegeneration()
        {
            if (!enableHealthRegeneration || isDead || IsHealthFull) return;

            if (Time.time - lastDamageTime >= regenDelay && regenCancellationToken == null)
            {
                StartRegeneration();
            }
        }

        private void StartRegeneration()
        {
            if (regenCancellationToken == null)
            {
                regenCancellationToken = new CancellationTokenSource();
                HealthRegenerationAsync(regenCancellationToken.Token).Forget();
            }
        }

        private void StopRegeneration()
        {
            if (regenCancellationToken != null)
            {
                regenCancellationToken.Cancel();
                regenCancellationToken.Dispose();
                regenCancellationToken = null;
            }
        }

        private async UniTaskVoid HealthRegenerationAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (IsAlive && !IsHealthFull && !cancellationToken.IsCancellationRequested)
                {
                    await UniTask.Delay((int)(regenInterval * 1000), cancellationToken: cancellationToken);
                    
                    if (Time.time - lastDamageTime >= regenDelay)
                    {
                        Heal(regenAmount);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (System.OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            finally
            {
                regenCancellationToken?.Dispose();
                regenCancellationToken = null;
            }
        }

        private void UpdateHealthUI()
        {
            if (healthText != null)
                healthText.text = currentHealth.ToString("F0");
        }

        private void OnDestroy()
        {
            StopRegeneration();
        }
    }
}