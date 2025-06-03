using Photon.Pun;
using UnityEngine;

namespace FPSGame.Health
{
    public abstract class BaseHealth : MonoBehaviour 
    {
        [Header("Health Settings")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth;

        protected bool isDead = false;
 
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;
 
        public System.Action OnDeath { get; set; }
        public System.Action<float, GameObject> OnDamageTaken { get; set; }

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        protected virtual void TakeDamage(float damage, GameObject attacker = null)
        {
            if (isDead) return;

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            OnDamageTaken?.Invoke(damage, attacker);

            if (currentHealth <= 0)
            {
                Die(attacker);
            }
        }
        
        [PunRPC]
        public void TakeDamageRPC(int damage, int attackerViewID = -1)
        {
            GameObject attacker = null;
            if (attackerViewID != -1)
            {
                PhotonView attackerPV = PhotonView.Find(attackerViewID);
                if (attackerPV != null)
                    attacker = attackerPV.gameObject;
            }
            
            TakeDamage((float)damage, attacker);
        }

        public virtual void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
        }

        public virtual void Heal(float amount)
        {
            if (isDead) return;

            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        protected virtual void Die(GameObject attacker = null)
        {
            if (isDead) return;

            isDead = true;
            OnDeath?.Invoke();
            HandleDeath(attacker);
        }

        protected abstract void HandleDeath(GameObject attacker = null);

        public void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = newMaxHealth;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }

        public float GetHealthPercentage()
        {
            return maxHealth > 0 ? currentHealth / maxHealth : 0f;
        }
    }
}