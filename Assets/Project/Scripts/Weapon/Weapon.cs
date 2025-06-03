using UnityEngine;

namespace FPSGame.Weapons
{
    public class Weapon : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        [SerializeField] private WeaponData _weaponData = new WeaponData();
        [SerializeField] private WeaponAmmo _ammoSystem = new WeaponAmmo();
        
        [Header("Weapon References")]
        [SerializeField] private Transform _weaponModel;
        [SerializeField] private Transform _muzzlePoint;
        [SerializeField] private GameObject _hitVFX;
        [SerializeField] private Animator _animator;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _fireSound;
        [SerializeField] private AudioClip _reloadSound;
        
        // Properties
        public WeaponData Data => _weaponData;
        public WeaponAmmo AmmoSystem => _ammoSystem;
        public Transform WeaponModel => _weaponModel;
        public Transform MuzzlePoint => _muzzlePoint;
        public GameObject HitVFX => _hitVFX;
        public Animator WeaponAnimator => _animator;
        
        // Events
        public event System.Action OnWeaponSelected;
        public event System.Action OnWeaponDeselected;
        
        private void Awake()
        {
            if (_weaponModel == null)
                _weaponModel = transform;
                
            if (_muzzlePoint == null)
            {
                // Muzzle point bulunamazsa weapon model'in önüne koy
                GameObject muzzleGO = new GameObject("MuzzlePoint");
                muzzleGO.transform.SetParent(_weaponModel);
                muzzleGO.transform.localPosition = Vector3.forward;
                _muzzlePoint = muzzleGO.transform;
            }
        }
        
        public void OnSelected()
        {
            gameObject.SetActive(true);
            OnWeaponSelected?.Invoke();
        }
        
        public void OnDeselected()
        {
            gameObject.SetActive(false);
            OnWeaponDeselected?.Invoke();
        }
        
        public void PlayMuzzleFlash()
        {
            if (_muzzleFlash != null)
                _muzzleFlash.Play();
        }
        
        public void PlayFireSound()
        {
            if (_audioSource != null && _fireSound != null)
                _audioSource.PlayOneShot(_fireSound);
        }
        
        public void PlayReloadSound()
        {
            if (_audioSource != null && _reloadSound != null)
                _audioSource.PlayOneShot(_reloadSound);
        }
        
        public void TriggerReloadAnimation()
        {
            if (_animator != null)
                _animator.SetTrigger("onReload");
        }
        
        // Animation Events'ler için
        public void OnReloadAnimationComplete()
        {
            // WeaponController'a bildir
            var weaponController = GetComponentInParent<WeaponController>();
            weaponController?.OnWeaponReloadComplete();
        }
    }
}