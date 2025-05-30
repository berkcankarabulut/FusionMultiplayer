using UnityEngine;

namespace FPSGame.Weapons
{
    public class WeaponSwaySystem
    {
        private readonly Transform _weaponTransform;
        private Vector3 _originalPosition;
        private readonly WeaponSwaySettings _settings;
        private readonly bool _enableBreathing;
        private readonly bool _enableWalkingSway;

        private Vector3 _currentSwayVelocity;
        private float _breathingTime;
        private bool _isActive = true;

        public WeaponSwaySystem(Transform weaponTransform, WeaponSwaySettings settings,
            bool enableBreathing = true, bool enableWalkingSway = true)
        {
            _weaponTransform = weaponTransform;
            UpdateOriginalPosition();
            _settings = settings;
            _enableBreathing = enableBreathing;
            _enableWalkingSway = enableWalkingSway;
        }

        private void UpdateOriginalPosition()
        {
            _originalPosition = _weaponTransform.localPosition;
        }

        public void Update(Vector2 lookInput, Vector2 moveInput, bool isAiming = false)
        {
            if (!_isActive) return;

            _breathingTime += Time.deltaTime;

            Vector3 mouseSway = WeaponSway.CalculateSwayOffset(lookInput, _settings, isAiming);
            Vector3 breathingSway = Vector3.zero;
            Vector3 walkingSway = Vector3.zero;

            if (_enableBreathing)
                breathingSway = WeaponSway.CalculateBreathingOffset(_breathingTime, _settings);

            if (_enableWalkingSway)
                walkingSway = WeaponSway.CalculateWalkingSway(moveInput, _breathingTime, _settings);

            Vector3 totalSwayOffset = WeaponSway.CombineSwayEffects(mouseSway, breathingSway, walkingSway);

            ApplySway(totalSwayOffset);
        }

        private void ApplySway(Vector3 swayOffset)
        {
            if (!_isActive) return;

            Vector3 targetPosition = _originalPosition + swayOffset;
 
            _weaponTransform.localPosition = Vector3.SmoothDamp(
                _weaponTransform.localPosition,
                targetPosition,
                ref _currentSwayVelocity,
                _settings.swaySmooth
            );
        }
    }
}