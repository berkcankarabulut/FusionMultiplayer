using UnityEngine;

namespace FPSGame.Weapons
{
    [System.Serializable]
    public class WeaponSwaySettings
    {
        [Header("Basic Sway Settings")]
        [Range(0f, 1f)]
        [Tooltip("Maximum sway amount")]
        public float swayClamp = 0.09f;
        
        [Range(0f, 1f)]
        [Tooltip("How smooth the sway movement is")]
        public float swaySmooth = 0.05f;
        
        [Range(0f, 3f)]
        [Tooltip("Overall sway intensity multiplier")]
        public float swayIntensity = 1f;
        
        [Header("ADS (Aim Down Sights) Settings")]
        [Range(0f, 1f)]
        [Tooltip("Sway multiplier when aiming")]
        public float aimingMultiplier = 0.3f;
        
        [Header("Breathing Settings")]
        [Range(0f, 0.01f)]
        [Tooltip("Breathing effect intensity")]
        public float breathingIntensity = 0.001f;
        
        [Range(0.5f, 3f)]
        [Tooltip("Breathing speed")]
        public float breathingSpeed = 1.2f;
        
        [Header("Walking Sway Settings")]
        [Range(0f, 0.05f)]
        [Tooltip("Walking sway intensity")]
        public float walkingSwayIntensity = 0.01f;
        
        [Range(1f, 8f)]
        [Tooltip("Walking sway speed")]
        public float walkingSwaySpeed = 4f;
        
        [Header("Advanced Settings")]
        [Range(0f, 2f)]
        [Tooltip("Vertical sway multiplier")]
        public float verticalMultiplier = 0.5f;
        
        [Range(0f, 2f)]
        [Tooltip("Horizontal sway multiplier")]
        public float horizontalMultiplier = 1f;
        
        [Tooltip("Invert horizontal sway")]
        public bool invertHorizontal = false;
        
        [Tooltip("Invert vertical sway")]
        public bool invertVertical = false;
    }
    
    public static class WeaponSway
    { 
        public static Vector3 CalculateSwayOffset(
            Vector2 lookInput, 
            WeaponSwaySettings settings, 
            bool isAiming = false)
        { 
            float currentIntensity = isAiming ? 
                settings.swayIntensity * settings.aimingMultiplier : 
                settings.swayIntensity;
             
            Vector2 clampedInput = new Vector2(
                Mathf.Clamp(lookInput.x, -settings.swayClamp, settings.swayClamp),
                Mathf.Clamp(lookInput.y, -settings.swayClamp, settings.swayClamp)
            );
             
            clampedInput.x *= settings.horizontalMultiplier;
            clampedInput.y *= settings.verticalMultiplier;
             
            if (settings.invertHorizontal) clampedInput.x = -clampedInput.x;
            if (settings.invertVertical) clampedInput.y = -clampedInput.y;
             
            return new Vector3(
                -clampedInput.x * currentIntensity,
                -clampedInput.y * currentIntensity,
                0f
            );
        }
          
        public static Vector3 CalculateBreathingOffset(float time, WeaponSwaySettings settings)
        {
            float breathingX = Mathf.Sin(time * settings.breathingSpeed) * settings.breathingIntensity;
            float breathingY = Mathf.Cos(time * settings.breathingSpeed * 0.7f) * settings.breathingIntensity * 0.5f;
            
            return new Vector3(breathingX, breathingY, 0f);
        }
          
        public static Vector3 CalculateWalkingSway(
            Vector2 movementInput, 
            float time, 
            WeaponSwaySettings settings)
        {
            if (movementInput.magnitude < 0.1f) return Vector3.zero;
            
            float walkCycle = time * settings.walkingSwaySpeed;
            float movementMagnitude = movementInput.magnitude;
            
            float swayX = Mathf.Sin(walkCycle) * settings.walkingSwayIntensity * movementMagnitude;
            float swayY = Mathf.Abs(Mathf.Sin(walkCycle * 2f)) * settings.walkingSwayIntensity * 0.5f * movementMagnitude;
            
            return new Vector3(swayX, swayY, 0f);
        } 
         
        public static Vector3 CombineSwayEffects(params Vector3[] swayEffects)
        {
            Vector3 combinedSway = Vector3.zero;
            
            foreach (var sway in swayEffects)
            {
                combinedSway += sway;
            }
            
            return combinedSway;
        }
          
    }
}