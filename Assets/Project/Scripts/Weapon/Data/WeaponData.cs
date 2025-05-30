using System;
using UnityEngine;

namespace FPSGame.Weapons
{
    [Serializable]
    public class WeaponData
    {
        [Header("Weapon Settings")] [SerializeField]
        private int _damage = 5;

        [SerializeField] private float _fireRate = 10f; // Rounds per second
        [SerializeField] private float _range = 100f;
        [SerializeField] private FireMode _fireMode = FireMode.Automatic;

        [Header("Recoil Settings")] [SerializeField]
        private float _recoilUp = 0.05f;

        [SerializeField] private float _recoilBack = 0.02f;
        [SerializeField] private float _recoverTime = 0.3f;

        public int Damage => _damage;
        public float FireRate => _fireRate;
        public float Range => _range;
        public FireMode FireMode => _fireMode;
        public float RecoilUp => _recoilUp;
        public float RecoilBack => _recoilBack;
        public float RecoverTime => _recoverTime;
    }
}