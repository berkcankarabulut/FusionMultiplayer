using System;
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
        [SerializeField] private WeaponData _weaponData = new WeaponData();
        public WeaponData WeaponData => _weaponData;

        [Header("Components")] [SerializeField]
        private Camera _mainCamera;

        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _hitVFX;
        [SerializeField] private Transform _weaponModel;

        [Header("Weapon Sway Settings")] [SerializeField]
        private WeaponSwaySettings _swaySettings = new WeaponSwaySettings();

        [SerializeField] private bool _enableBreathing = true;
        [SerializeField] private bool _enableWalkingSway = true;

        [Header("Ammo Configuration")] [SerializeField]
        private WeaponAmmo _ammoSystem = new WeaponAmmo();

        [Header("UI")] [SerializeField] private TextMeshProUGUI _magText;
        [SerializeField] private TextMeshProUGUI _ammoText;


        public bool HasAmmo => _ammoSystem.HasAmmo;
        public bool IsReloading => _reloadSystem.IsReloading;
        public bool CanFire => _fireSystem.CanFire && !_reloadSystem.IsReloading;
        public FireMode CurrentFireMode => _fireSystem.CurrentFireMode;
        private bool _isAiming = false;

        public bool IsAiming
        {
            get => _isAiming;
            set => _isAiming = value;
        }

        private WeaponFireSystem _fireSystem;
        private WeaponReloadSystem _reloadSystem;
        private WeaponCombatSystem _combatSystem;
        private WeaponSwaySystem _swaySystem;
        private WeaponRecoilSystem _recoilSystem;

        private Vector3 _originalWeaponPosition;

        private void Start()
        {
            if (_weaponModel == null)
                _weaponModel = this.transform;

            if (_mainCamera == null)
                _mainCamera = Camera.main;

            _originalWeaponPosition = _weaponModel.localPosition;

            InitializeSystems();
            SetupEvents();
            UpdateAmmoUI();
        }

        private void InitializeSystems()
        {
            _fireSystem = new WeaponFireSystem(WeaponData.FireRate, WeaponData.FireMode);
            _reloadSystem = new WeaponReloadSystem(_animator);
            _combatSystem = new WeaponCombatSystem(_mainCamera, WeaponData.Damage, WeaponData.Range);

            _swaySystem = new WeaponSwaySystem(
                _weaponModel,
                _swaySettings,
                _enableBreathing,
                _enableWalkingSway
            );

            _recoilSystem = new WeaponRecoilSystem(
                _weaponModel,
                WeaponData.RecoilUp,
                WeaponData.RecoilBack,
                WeaponData.RecoverTime
            );
        }

        private void SetupEvents()
        {
            _ammoSystem.OnAmmoChanged += UpdateAmmoUI;
            _ammoSystem.OnMagChanged += UpdateMagUI;

            _reloadSystem.OnReloadComplete += OnReloadComplete;

            _combatSystem.OnHit += OnWeaponHit;
            _combatSystem.OnDamageDealt += OnDamageDealt;
        }

        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.InputActions.Player.Reload.performed += OnReloadInput;
            }
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.InputActions.Player.Reload.performed -= OnReloadInput;
            }
        }

        private void Update()
        {
            if (InputManager.Instance == null) return;

            HandleFiring();
            HandleAutoReload();
            UpdateWeaponEffects();
        }

        private void HandleFiring()
        {
            if (_reloadSystem.IsReloading) return;

            bool isFirePressed = InputManager.Instance.IsFirePressed;
            bool canFire = _fireSystem.CanFireWithMode(isFirePressed) && _ammoSystem.HasAmmo;

            if (canFire)
            {
                ExecuteShot();
            }
        }

        private void ExecuteShot()
        {
            _fireSystem.Fire();
            _ammoSystem.ConsumeAmmo();
            _combatSystem.TryShoot();

            _recoilSystem.StartRecoil();
        }

        private void HandleAutoReload()
        {
            if (_ammoSystem.IsEmpty && _ammoSystem.HasMags && !_reloadSystem.IsReloading)
            {
                _reloadSystem.StartReload();
            }
        }

        private void UpdateWeaponEffects()
        {
            if (InputManager.Instance == null) return;

            Vector2 mouseInput = InputManager.Instance.LookInput;
            Vector2 movementInput = InputManager.Instance.MoveInput;

            _recoilSystem?.Update();

            if (!_recoilSystem.IsActive)
            {
                _swaySystem?.Update(mouseInput, movementInput);
            }
        }

        private void OnReloadInput(InputAction.CallbackContext context)
        {
            TryReload();
        }

        private void TryReload()
        {
            if (!_reloadSystem.IsReloading && _ammoSystem.HasMags && !_ammoSystem.IsFull)
            {
                _reloadSystem.StartReload();
            }
        }

        private void OnReloadComplete()
        {
            _ammoSystem.TryReload();
        }

        private void OnWeaponHit(Vector3 hitPoint)
        {
            if (_hitVFX != null)
            {
                PhotonNetwork.Instantiate(_hitVFX.name, hitPoint, Quaternion.identity);
            }
        }

        private void OnDamageDealt(int damage)
        {
            PhotonNetwork.LocalPlayer.AddScore(damage);
        }

        private void UpdateAmmoUI(int currentAmmo, int maxAmmo)
        {
            if (_ammoText != null)
                _ammoText.text = $"{currentAmmo}/{maxAmmo}";
        }

        private void UpdateAmmoUI()
        {
            UpdateAmmoUI(_ammoSystem.CurrentAmmo, _ammoSystem.MagCapacity);
        }

        private void UpdateMagUI(int totalMags)
        {
            if (_magText != null)
                _magText.text = totalMags.ToString();
        }

        public void OnReloadAnimationComplete()
        {
            _reloadSystem.CompleteReload();
        } 
 
    }
}