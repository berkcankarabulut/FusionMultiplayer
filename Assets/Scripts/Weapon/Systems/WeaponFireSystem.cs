using UnityEngine;

namespace FPSGame.Weapons
{
    public enum FireMode
    {
        SemiAutomatic,  
        Automatic,      
        Burst            
    }

    public class WeaponFireSystem
    {
        private readonly float _fireRate;
        private float _nextFireTime;
        private FireMode _fireMode;
         
        private int _burstCount = 0;
        private int _maxBurstCount = 3;
        private bool _isBursting = false;
        private float _burstCooldown = 0.5f;  
        private float _nextBurstTime = 0f;
         
        private bool _lastFrameFirePressed = false;
        
        public WeaponFireSystem(float fireRate, FireMode fireMode = FireMode.SemiAutomatic)
        {
            _fireRate = fireRate;
            _fireMode = fireMode;
        }
        
        public bool CanFire => Time.time >= _nextFireTime;
        public FireMode CurrentFireMode => _fireMode;
        public bool IsBursting => _isBursting;
        public int CurrentBurstCount => _burstCount;
         
        public bool CanFireWithMode(bool isFirePressed)
        {
            if (!CanFire) return false;
            
            switch (_fireMode)
            {
                case FireMode.SemiAutomatic:
                    return HandleSemiAutoFire(isFirePressed);
                    
                case FireMode.Automatic:
                    return isFirePressed; 
                    
                case FireMode.Burst:
                    return HandleBurstFire(isFirePressed);
                    
                default:
                    return false;
            }
        }
        
        private bool HandleSemiAutoFire(bool isFirePressed)
        { 
            bool canFire = isFirePressed && !_lastFrameFirePressed;
            _lastFrameFirePressed = isFirePressed;
            return canFire;
        }
        
        private bool HandleBurstFire(bool isFirePressed)
        { 
            if (_isBursting)
            {
                return _burstCount < _maxBurstCount;
            } 
            bool buttonJustPressed = isFirePressed && !_lastFrameFirePressed;
            bool cooldownReady = Time.time >= _nextBurstTime;
            
            _lastFrameFirePressed = isFirePressed;
            
            if (buttonJustPressed && cooldownReady)
            {
                StartNewBurst();
                return true;
            }
            
            return false;
        }
        
        private void StartNewBurst()
        {
            _isBursting = true;
            _burstCount = 0; 
        }
        
        public void Fire()
        {
            _nextFireTime = Time.time + (1f / _fireRate);

            if (_fireMode != FireMode.Burst || !_isBursting) return;
            _burstCount++;
            if (_burstCount < _maxBurstCount) return;
            _isBursting = false;
            _nextBurstTime = Time.time + _burstCooldown;
        } 
        
        public void SetFireMode(FireMode newMode)
        {
            _fireMode = newMode;
            Reset();
        } 
        
        public void Reset()
        {
            _nextFireTime = 0f;
            _isBursting = false;
            _burstCount = 0;
            _nextBurstTime = 0f;
            _lastFrameFirePressed = false;
        } 
    
        public float GetTimeUntilNextShot()
        {
            return Mathf.Max(0f, _nextFireTime - Time.time);
        } 
        
        public float GetTimeUntilNextBurst()
        {
            return Mathf.Max(0f, _nextBurstTime - Time.time);
        }
    }
}