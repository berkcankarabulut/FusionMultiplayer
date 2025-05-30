using System;
using Photon.Pun;
using UnityEngine;

namespace FPSGame.Weapons
{
    public class WeaponCombatSystem
    {
        private readonly Camera _camera;
        private readonly int _damage;
        private readonly float _range;
        
        public WeaponCombatSystem(Camera camera, int damage, float range = 100f)
        {
            _camera = camera;
            _damage = damage;
            _range = range;
        }
        
        public event Action<Vector3> OnHit;
        public event Action<int> OnDamageDealt;
        
        public bool TryShoot()
        {
            Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);

            if (!Physics.Raycast(ray, out RaycastHit hit, _range)) return false;
            
            OnHit?.Invoke(hit.point);
 
            var photonView = hit.transform.GetComponent<PhotonView>();
            if (photonView == null) return false;
            photonView.RPC("TakeDamage", RpcTarget.All, _damage);
            OnDamageDealt?.Invoke(_damage);
            return true; 
        }
    }
}