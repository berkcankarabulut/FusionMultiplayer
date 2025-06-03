using System;
using FPSGame.Health;
using FPSGame.Input;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSGame.Weapons
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Player Components")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private BaseHealth _owner;
        [SerializeField] private Transform _weaponHolder;
        
        [Header("Available Weapons")]
        [SerializeField] private Weapon[] _weapons;
        
        [Header("Weapon Sway Settings")]
        [SerializeField] private WeaponSwaySettings _swaySettings = new WeaponSwaySettings();
        [SerializeField] private bool _enableBreathing = true;
        [SerializeField] private bool _enableWalkingSway = true;
        
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _magText;
        [SerializeField] private TextMeshProUGUI _ammoText;
        
        // Current weapon state
        private Weapon _currentWeapon;
        private int _currentWeaponIndex = 0;
        
        // Weapon systems - current weapon'a göre dinamik olarak oluşturulur
        private WeaponFireSystem _fireSystem;
        private WeaponReloadSystem _reloadSystem;
        private WeaponCombatSystem _combatSystem;
        private WeaponSwaySystem _swaySystem;
        private WeaponRecoilSystem _recoilSystem;
        
        // Properties
        public Weapon CurrentWeapon => _currentWeapon;
        public bool HasAmmo => _currentWeapon?.AmmoSystem.HasAmmo ?? false;
        public bool IsReloading => _reloadSystem?.IsReloading ?? false;
        public bool CanFire => _fireSystem?.CanFire == true && !IsReloading;
        public FireMode CurrentFireMode => _fireSystem?.CurrentFireMode ?? FireMode.SemiAutomatic;
        
        // Events
        public event Action<Weapon> OnWeaponChanged;
        public event Action<int> OnDamageDealt;
        
        private void Start()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;
                
            if (_weaponHolder == null)
                _weaponHolder = transform;
                
            ValidateWeapons();
            SwitchToWeapon(0);
        }
        
        private void ValidateWeapons()
        {
            if (_weapons == null || _weapons.Length == 0)
            {
                Debug.LogError("No weapons assigned to WeaponController!");
                return;
            }
            
            // Tüm silahları deaktif et
            foreach (var weapon in _weapons)
            {
                if (weapon != null)
                    weapon.gameObject.SetActive(false);
            }
        }
        
        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.InputActions.Player.Reload.performed += OnReloadInput;
                InputManager.Instance.InputActions.Player.WeaponSlot1.performed += OnWeaponSlot1;
                InputManager.Instance.InputActions.Player.WeaponSlot2.performed += OnWeaponSlot2;
            }
        }
        
        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.InputActions.Player.Reload.performed -= OnReloadInput;
                InputManager.Instance.InputActions.Player.WeaponSlot1.performed -= OnWeaponSlot1;
                InputManager.Instance.InputActions.Player.WeaponSlot2.performed -= OnWeaponSlot2;
            }
        }
        
        private void Update()
        {
            if (InputManager.Instance == null || _currentWeapon == null) return;
            
            HandleWeaponSwitching();
            HandleFiring();
            HandleAutoReload();
            UpdateWeaponEffects();
        }
        
        private void HandleWeaponSwitching()
        {
            int previousIndex = _currentWeaponIndex;
            
            // Scroll wheel input
            float scrollValue = InputManager.Instance.InputActions.Player.ScrollWeapon.ReadValue<float>();
            
            if (scrollValue > 0)
            {
                _currentWeaponIndex = (_currentWeaponIndex + 1) % _weapons.Length;
            }
            else if (scrollValue < 0)
            {
                _currentWeaponIndex = (_currentWeaponIndex - 1 + _weapons.Length) % _weapons.Length;
            }
            
            if (previousIndex != _currentWeaponIndex)
            {
                SwitchToWeapon(_currentWeaponIndex);
            }
        }
        
        private void HandleFiring()
        {
            if (IsReloading) return;
            
            bool isFirePressed = InputManager.Instance.IsFirePressed;
            bool canFire = _fireSystem?.CanFireWithMode(isFirePressed) == true && HasAmmo;
            
            if (canFire)
            {
                ExecuteShot();
            }
        }
        
        private void ExecuteShot()
        {
            _fireSystem?.Fire();
            _currentWeapon?.AmmoSystem.ConsumeAmmo();
            
            // Combat system
            if (_combatSystem?.TryShoot() == true)
            {
                // Visual/Audio effects
                _currentWeapon?.PlayMuzzleFlash();
                _currentWeapon?.PlayFireSound();
            }
            
            // Recoil
            _recoilSystem?.StartRecoil();
            
            UpdateAmmoUI();
        }
        
        private void HandleAutoReload()
        {
            if (_currentWeapon?.AmmoSystem.IsEmpty == true && 
                _currentWeapon?.AmmoSystem.HasMags == true && 
                !IsReloading)
            {
                TryReload();
            }
        }
        
        private void UpdateWeaponEffects()
        {
            if (InputManager.Instance == null || _currentWeapon == null) return;
            
            Vector2 mouseInput = InputManager.Instance.LookInput;
            Vector2 movementInput = InputManager.Instance.MoveInput;
            
            _recoilSystem?.Update();
            
            if (_recoilSystem?.IsActive != true)
            {
                _swaySystem?.Update(mouseInput, movementInput);
            }
        }
        
        public void SwitchToWeapon(int weaponIndex)
        {
            if (!IsValidWeaponIndex(weaponIndex)) return;
            
            // Eski silahı deaktif et
            _currentWeapon?.OnDeselected();
            
            // Yeni silahı aktif et
            _currentWeaponIndex = weaponIndex;
            _currentWeapon = _weapons[_currentWeaponIndex];
            _currentWeapon?.OnSelected();
            
            // Sistemleri güncelle
            InitializeSystemsForCurrentWeapon();
            UpdateAmmoUI();
            
            OnWeaponChanged?.Invoke(_currentWeapon);
            
            Debug.Log($"Switched to weapon: {_currentWeapon?.name}");
        }
        
        private void InitializeSystemsForCurrentWeapon()
        {
            if (_currentWeapon == null) return;
            
            var data = _currentWeapon.Data;
            var weaponModel = _currentWeapon.WeaponModel;
            
            // Fire System
            _fireSystem = new WeaponFireSystem(data.FireRate, data.FireMode);
            
            // Reload System
            _reloadSystem = new WeaponReloadSystem(_currentWeapon.WeaponAnimator);
            _reloadSystem.OnReloadComplete += OnReloadComplete;
            
            // Combat System
            _combatSystem = new WeaponCombatSystem(_mainCamera, data.Damage, data.Range, _owner);
            _combatSystem.OnHit += OnWeaponHit;
            _combatSystem.OnDamageDealt += OnDamageDealtInternal;
            
            // Sway System
            _swaySystem = new WeaponSwaySystem(
                weaponModel,
                _swaySettings,
                _enableBreathing,
                _enableWalkingSway
            );
            
            // Recoil System
            _recoilSystem = new WeaponRecoilSystem(
                weaponModel,
                data.RecoilUp,
                data.RecoilBack,
                data.RecoverTime
            );
            
            // Ammo events
            _currentWeapon.AmmoSystem.OnAmmoChanged += UpdateAmmoUI;
            _currentWeapon.AmmoSystem.OnMagChanged += UpdateMagUI;
        }
        
        private void OnReloadInput(InputAction.CallbackContext context)
        {
            TryReload();
        }
        
        private void OnWeaponSlot1(InputAction.CallbackContext context)
        {
            SwitchToWeapon(0);
        }
        
        private void OnWeaponSlot2(InputAction.CallbackContext context)
        {
            SwitchToWeapon(1);
        }
        
        private void TryReload()
        {
            if (_currentWeapon == null || IsReloading) return;
            
            var ammo = _currentWeapon.AmmoSystem;
            if (ammo.HasMags && !ammo.IsFull)
            {
                _reloadSystem?.StartReload();
                _currentWeapon?.PlayReloadSound();
                _currentWeapon?.TriggerReloadAnimation();
            }
        }
        
        private void OnReloadComplete()
        {
            _currentWeapon?.AmmoSystem.TryReload();
            UpdateAmmoUI();
        }
        
        public void OnWeaponReloadComplete()
        {
            // Animation complete callback
            _reloadSystem?.CompleteReload();
        }
        
        private void OnWeaponHit(Vector3 hitPoint)
        {
            if (_currentWeapon?.HitVFX != null)
            {
                PhotonNetwork.Instantiate(_currentWeapon.HitVFX.name, hitPoint, Quaternion.identity);
            }
        }
        
        private void OnDamageDealtInternal(int damage)
        {
            PhotonNetwork.LocalPlayer.AddScore(damage);
            OnDamageDealt?.Invoke(damage);
        }
        
        private void UpdateAmmoUI(int currentAmmo, int maxAmmo)
        {
            if (_ammoText != null)
                _ammoText.text = $"{currentAmmo}/{maxAmmo}";
        }
        
        private void UpdateAmmoUI()
        {
            if (_currentWeapon?.AmmoSystem != null)
            {
                var ammo = _currentWeapon.AmmoSystem;
                UpdateAmmoUI(ammo.CurrentAmmo, ammo.MagCapacity);
                UpdateMagUI(ammo.TotalMags);
            }
        }
        
        private void UpdateMagUI(int totalMags)
        {
            if (_magText != null)
                _magText.text = totalMags.ToString();
        }
        
        private bool IsValidWeaponIndex(int index)
        {
            return index >= 0 && index < _weapons.Length && _weapons[index] != null;
        }
        
        // Public API
        public void SetFireMode(FireMode fireMode)
        {
            _fireSystem?.SetFireMode(fireMode);
        }
        
        public WeaponData GetCurrentWeaponData()
        {
            return _currentWeapon?.Data;
        }
        
        public void AddAmmo(int amount)
        {
            // Implement ammo pickup logic
        }
        
        public void SetWeaponEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
    }
}