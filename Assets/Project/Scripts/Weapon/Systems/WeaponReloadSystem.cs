using System;
using UnityEngine;

namespace FPSGame.Weapons
{
    public class WeaponReloadSystem
    {
        private readonly Animator _animator;
        private bool _isReloading;
        
        public WeaponReloadSystem(Animator animator)
        {
            _animator = animator;
        }
        
        public bool IsReloading => _isReloading;
        
        public event Action OnReloadComplete;
        
        public void StartReload()
        {
            if (_isReloading) return;
            
            _isReloading = true;
            _animator.SetTrigger("onReload");
        }
         
        public void CompleteReload()
        {
            _isReloading = false;
            OnReloadComplete?.Invoke();
        }
    }
}