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
        /// <summary>
        /// Calculate basic mouse sway offset
        /// </summary>
        public static Vector3 CalculateSwayOffset(
            Vector2 lookInput, 
            WeaponSwaySettings settings, 
            bool isAiming = false)
        {
            // Apply ADS multiplier if aiming
            float currentIntensity = isAiming ? 
                settings.swayIntensity * settings.aimingMultiplier : 
                settings.swayIntensity;
            
            // Clamp input to prevent excessive sway
            Vector2 clampedInput = new Vector2(
                Mathf.Clamp(lookInput.x, -settings.swayClamp, settings.swayClamp),
                Mathf.Clamp(lookInput.y, -settings.swayClamp, settings.swayClamp)
            );
            
            // Apply multipliers for different axes
            clampedInput.x *= settings.horizontalMultiplier;
            clampedInput.y *= settings.verticalMultiplier;
            
            // Apply inversion if needed
            if (settings.invertHorizontal) clampedInput.x = -clampedInput.x;
            if (settings.invertVertical) clampedInput.y = -clampedInput.y;
            
            // Calculate sway offset (opposite direction of mouse movement)
            return new Vector3(
                -clampedInput.x * currentIntensity,
                -clampedInput.y * currentIntensity,
                0f
            );
        }
        
        /// <summary>
        /// Apply smooth sway movement to weapon position
        /// </summary>
        public static Vector3 ApplySwaySmoothing(
            Vector3 currentPosition,
            Vector3 swayOffset,
            Vector3 originPosition,
            WeaponSwaySettings settings)
        {
            Vector3 targetPosition = originPosition + swayOffset;
            
            return Vector3.Lerp(
                currentPosition,
                targetPosition,
                settings.swaySmooth * Time.deltaTime
            );
        }
        
        /// <summary>
        /// Calculate breathing effect for idle weapon movement
        /// </summary>
        public static Vector3 CalculateBreathingOffset(float time, WeaponSwaySettings settings)
        {
            float breathingX = Mathf.Sin(time * settings.breathingSpeed) * settings.breathingIntensity;
            float breathingY = Mathf.Cos(time * settings.breathingSpeed * 0.7f) * settings.breathingIntensity * 0.5f;
            
            return new Vector3(breathingX, breathingY, 0f);
        }
        
        /// <summary>
        /// Calculate breathing effect with default settings
        /// </summary>
        public static Vector3 CalculateBreathingOffset(float time, float intensity = 0.001f)
        {
            float breathingX = Mathf.Sin(time * 1.2f) * intensity;
            float breathingY = Mathf.Cos(time * 0.8f) * intensity * 0.5f;
            
            return new Vector3(breathingX, breathingY, 0f);
        }
        
        /// <summary>
        /// Calculate walking sway based on movement input
        /// </summary>
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
        
        /// <summary>
        /// Calculate walking sway with default settings
        /// </summary>
        public static Vector3 CalculateWalkingSway(
            Vector2 movementInput, 
            float time, 
            float intensity = 0.01f)
        {
            if (movementInput.magnitude < 0.1f) return Vector3.zero;
            
            float walkCycle = time * 4f;
            float movementMagnitude = movementInput.magnitude;
            
            return new Vector3(
                Mathf.Sin(walkCycle) * intensity * movementMagnitude,
                Mathf.Abs(Mathf.Sin(walkCycle * 2f)) * intensity * 0.5f * movementMagnitude,
                0f
            );
        }
        
        /// <summary>
        /// Calculate running bounce effect
        /// </summary>
        public static Vector3 CalculateRunningBounce(
            Vector2 movementInput, 
            float time, 
            bool isSprinting,
            float intensity = 0.02f)
        {
            if (movementInput.magnitude < 0.1f || !isSprinting) return Vector3.zero;
            
            float runCycle = time * 6f; // Faster than walking
            float movementMagnitude = movementInput.magnitude;
            
            return new Vector3(
                Mathf.Sin(runCycle) * intensity * movementMagnitude * 1.5f,
                Mathf.Abs(Mathf.Sin(runCycle * 1.5f)) * intensity * movementMagnitude,
                Mathf.Sin(runCycle * 0.5f) * intensity * 0.3f * movementMagnitude // Z-axis bounce
            );
        }
        
        /// <summary>
        /// Calculate recoil sway (different from weapon recoil)
        /// </summary>
        public static Vector3 CalculateRecoilSway(
            Vector3 recoilDirection, 
            float intensity = 0.05f)
        {
            return new Vector3(
                recoilDirection.x * intensity,
                recoilDirection.y * intensity * 0.3f, // Less vertical sway
                0f
            );
        }
        
        /// <summary>
        /// Calculate landing impact sway
        /// </summary>
        public static Vector3 CalculateLandingSway(
            float landingForce,
            float intensity = 0.03f)
        {
            float normalizedForce = Mathf.Clamp01(landingForce / 10f);
            
            return new Vector3(
                Random.Range(-1f, 1f) * intensity * normalizedForce,
                -intensity * normalizedForce, // Downward movement
                Random.Range(-0.5f, 0.5f) * intensity * normalizedForce * 0.5f
            );
        }
        
        /// <summary>
        /// Combine multiple sway effects
        /// </summary>
        public static Vector3 CombineSwayEffects(params Vector3[] swayEffects)
        {
            Vector3 combinedSway = Vector3.zero;
            
            foreach (var sway in swayEffects)
            {
                combinedSway += sway;
            }
            
            return combinedSway;
        }
        
        /// <summary>
        /// Apply sway with custom easing
        /// </summary>
        public static Vector3 ApplySwayWithEasing(
            Vector3 currentPosition,
            Vector3 targetPosition,
            float smoothTime,
            AnimationCurve easingCurve = null)
        {
            float t = smoothTime * Time.deltaTime;
            
            if (easingCurve != null)
            {
                t = easingCurve.Evaluate(t);
            }
            
            return Vector3.Lerp(currentPosition, targetPosition, t);
        }
    }
}