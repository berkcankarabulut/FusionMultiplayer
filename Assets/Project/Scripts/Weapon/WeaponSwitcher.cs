using Photon.Pun;
using UnityEngine;

namespace FPSGame.Weapons
{
    public class WeaponSwitcher : MonoBehaviour
    {
        [Header("Third Person Weapons")] 
        [SerializeField] private GameObject[] _thirdPersonWeapons;
        [Header("Components")]
        [SerializeField] private WeaponController _weaponController;

        private PhotonView _photonView;

        public void Init(PhotonView photonView)
        {
            _photonView = photonView;
            _weaponController.OnWeaponChanged += OnWeaponChanged;

            UpdateThirdPersonWeapon(0);  
        }

        private void OnWeaponChanged(Weapon newWeapon)
        {
            if (_weaponController?.CurrentWeapon == null) return;
            for (int i = 0; i < _weaponController.transform.childCount; i++)
            {
                if (_weaponController.transform.GetChild(i).gameObject != newWeapon.gameObject) continue;
                UpdateThirdPersonWeapon(i);
                break;
            }
        }

        private void UpdateThirdPersonWeapon(int weaponIndex)
        {
            if (_photonView != null)
            {
                _photonView.RPC("SetTPWeapon", RpcTarget.AllBuffered, weaponIndex);
            }
        }

        [PunRPC]
        public void SetTPWeapon(int weaponIndex)
        {
            if (_thirdPersonWeapons == null) return;

            // Tüm TP silahları kapat
            for (int i = 0; i < _thirdPersonWeapons.Length; i++)
            {
                if (_thirdPersonWeapons[i] != null)
                {
                    _thirdPersonWeapons[i].SetActive(i == weaponIndex);
                }
            }
        }

        private void OnDestroy()
        {
            if (_weaponController != null)
            {
                _weaponController.OnWeaponChanged -= OnWeaponChanged;
            }
        }
    }
}