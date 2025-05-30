using FPSGame.Input;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSGame.Weapons
{
    public class WeaponSwitcher : MonoBehaviour
    { 
        private PhotonView _photonView;
        private int _selectedWeapon = 0;
        private int _previousSelectedWeapon = 0;
        public GameObject[] weapons;

        public void Init(PhotonView photonView)
        {
            _photonView = photonView;
            SelectWeapon();
        }
        private void OnEnable()
        { 
            if (InputManager.Instance == null) return;
            InputManager.Instance.InputActions.Player.WeaponSlot1.performed += OnWeaponSlot1;
            InputManager.Instance.InputActions.Player.WeaponSlot2.performed += OnWeaponSlot2;
        }

        private void OnDisable()
        { 
            if (InputManager.Instance != null)
            {
                InputManager.Instance.InputActions.Player.WeaponSlot1.performed -= OnWeaponSlot1;
                InputManager.Instance.InputActions.Player.WeaponSlot2.performed -= OnWeaponSlot2;
            }
        }

        private void OnWeaponSlot1(InputAction.CallbackContext context)
        {
            _selectedWeapon = 0;
        }

        private void OnWeaponSlot2(InputAction.CallbackContext context)
        {
            _selectedWeapon = 1;
        }

        private void Update()
        {
            if (InputManager.Instance == null) return;
            
            _previousSelectedWeapon = _selectedWeapon;
             
            float scrollValue = InputManager.Instance.InputActions.Player.ScrollWeapon.ReadValue<float>();
            switch (scrollValue)
            {
                case > 0 when _selectedWeapon >= weapons.Length - 1:
                    _selectedWeapon = 0;
                    break;
                case > 0:
                    _selectedWeapon += 1;
                    break;
                case < 0 when _selectedWeapon <= 0:
                    _selectedWeapon = weapons.Length - 1;
                    break;
                case < 0:
                    _selectedWeapon -= 1;
                    break;
            }

            if (_previousSelectedWeapon != _selectedWeapon)
                SelectWeapon();
        }

        void SelectWeapon()
        { 
            for (int i = 0; i < weapons.Length; i++)
            {
                if (i == _selectedWeapon)
                {
                    weapons[i].gameObject.SetActive(true);
                }
                else
                    weapons[i].gameObject.SetActive(false);
            }
            _photonView.RPC("SetTPWeapon", RpcTarget.AllBuffered, _selectedWeapon);
        }
    }
}