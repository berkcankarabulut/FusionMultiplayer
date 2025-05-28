using FPSGame.Networking;
using FPSGame.Weapons;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace FPSGame.Player
{ 
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }
     
    public class Health : MonoBehaviour, IDamageable, IWeaponTarget
    {
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _health = 100;
        [SerializeField] private TextMeshProUGUI _healthText;
        private bool _isLocalPlayer; 
        private PhotonView _photonView;
        public int CurrentHealth => _health;
        public int MaxHealth => _maxHealth;
        public bool IsAlive => _health > 0;
        public bool IsLocalPlayer => _isLocalPlayer;
         
        public Vector3 HitPoint => transform.position;
        
        public event System.Action<int> OnHealthChanged;
        public event System.Action OnDeath;
        public event System.Action OnKilled;
        
        private void Start()
        {
            UpdateHealthUI();
        }

        public void Init(bool isLocalPlayer, PhotonView photonView)
        {
            _isLocalPlayer = isLocalPlayer;
            _photonView = photonView;
        }
        
        [PunRPC]
        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;
            
            _health = Mathf.Max(0, _health - damage);
            UpdateHealthUI();
            OnHealthChanged?.Invoke(_health);
            
            if (!IsAlive)
            {
                HandleDeath();
            }
        }
         
        public bool TryDamage(int damage)
        {
            if (!IsAlive) return false;
            
            bool wasAlive = IsAlive;
            TakeDamage(damage);
             
            return wasAlive && !IsAlive;
        }
        
        private void HandleDeath()
        {
            OnDeath?.Invoke();
            
            if (_isLocalPlayer)
            {
                HandleLocalPlayerDeath();
            }
             
            if (_photonView != null)
            {
                _photonView.RPC("OnPlayerKilled", RpcTarget.All);
            }
            
            Destroy(gameObject);
        }
        
        private void HandleLocalPlayerDeath()
        {
            if (RoomManager.instance != null)
            {
                RoomManager.instance.PlayerSpawn();
                RoomManager.instance.AddDeath();
            }
        }
        
        [PunRPC]
        private void OnPlayerKilled()
        {
            OnKilled?.Invoke();
             
            if (RoomManager.instance != null)
            {
                RoomManager.instance.AddKill(1);
            }
        }
        
        private void UpdateHealthUI()
        {
            if (_healthText != null)
                _healthText.text = _health.ToString();
        }
         
        public void Heal(int amount)
        {
            if (!IsAlive) return;
            
            _health = Mathf.Min(_maxHealth, _health + amount);
            UpdateHealthUI();
            OnHealthChanged?.Invoke(_health);
        }
         
        public void FullHeal()
        {
            Heal(_maxHealth - _health);
        }
    }
}