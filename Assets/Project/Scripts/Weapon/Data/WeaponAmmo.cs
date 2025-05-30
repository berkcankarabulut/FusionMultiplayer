using System;
using UnityEngine;

namespace FPSGame.Weapons
{
    [System.Serializable]
    public class WeaponAmmo
    {
        [SerializeField] private int _currentAmmo = 30;
        [SerializeField] private int _magCapacity = 30;
        [SerializeField] private int _totalMags = 5;
        
        public int CurrentAmmo => _currentAmmo;
        public int MagCapacity => _magCapacity;
        public int TotalMags => _totalMags;
        public bool HasAmmo => _currentAmmo > 0;
        public bool HasMags => _totalMags > 0;
        public bool IsEmpty => _currentAmmo == 0;
        public bool IsFull => _currentAmmo == _magCapacity;
        
        public event Action<int, int> OnAmmoChanged;
        public event Action<int> OnMagChanged;
        
        public void ConsumeAmmo()
        {
            if (_currentAmmo > 0)
            {
                _currentAmmo--;
                OnAmmoChanged?.Invoke(_currentAmmo, _magCapacity);
            }
        }
        
        public bool TryReload()
        {
            if (_totalMags <= 0 || IsFull) return false;
            
            _totalMags--;
            _currentAmmo = _magCapacity;
            
            OnAmmoChanged?.Invoke(_currentAmmo, _magCapacity);
            OnMagChanged?.Invoke(_totalMags);
            return true;
        }
    }
}