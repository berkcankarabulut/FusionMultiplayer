using UnityEngine;

namespace FPSGame.Weapons
{
    public class WeaponRecoilSystem
    {
        private readonly Transform _weaponTransform;
        private Vector3 _originalPosition;
        private readonly float _recoilUp;
        private readonly float _recoilBack;
        private readonly float _recoverTime;
        
        private Vector3 _recoilVelocity;
        private bool _isRecoiling;
        private bool _isRecovering;
        private float _recoilStartTime;
        
        public WeaponRecoilSystem(Transform weaponTransform, float recoilUp, float recoilBack, float recoverTime)
        {
            _weaponTransform = weaponTransform;
            UpdateOriginalPosition(); 
            _recoilUp = recoilUp;
            _recoilBack = recoilBack;
            _recoverTime = recoverTime;
        }
        
        public bool IsActive => _isRecoiling || _isRecovering;

        private void UpdateOriginalPosition()
        {
            _originalPosition = _weaponTransform.localPosition;
        } 
        
        public void StartRecoil()
        { 
            if (!_isRecoiling)
            {
                _isRecoiling = true;
                _isRecovering = false;
                _recoilStartTime = Time.time;
            }
        } 
        
        public void Update()
        {
            if (_isRecoiling)
                HandleRecoil();
            else if (_isRecovering)
                HandleRecover();
        }
        
        private void HandleRecoil()
        { 
            float randomFactorX = Random.Range(-0.3f, 0.3f);
            float randomFactorY = Random.Range(0.8f, 1.2f);
            
            Vector3 targetPosition = _originalPosition + new Vector3(
                randomFactorX * _recoilUp * 0.5f, 
                _recoilUp * randomFactorY,        
                -_recoilBack                       
            );
             
            _weaponTransform.localPosition = Vector3.SmoothDamp(
                _weaponTransform.localPosition,
                targetPosition,
                ref _recoilVelocity,
                0.05f 
            );
             
            bool positionReached = Vector3.Distance(_weaponTransform.localPosition, targetPosition) < 0.001f;
            bool timeElapsed = Time.time - _recoilStartTime > 0.1f; 

            if (!positionReached && !timeElapsed) return;
            _isRecoiling = false;
            _isRecovering = true;
        }
        
        private void HandleRecover()
        {
            _weaponTransform.localPosition = Vector3.SmoothDamp(
                _weaponTransform.localPosition,
                _originalPosition,
                ref _recoilVelocity,
                _recoverTime
            );

            if (!(Vector3.Distance(_weaponTransform.localPosition, _originalPosition) < 0.001f)) return;
            _isRecovering = false;
            _weaponTransform.localPosition = _originalPosition; // Ensure exact position
        } 
    }
}