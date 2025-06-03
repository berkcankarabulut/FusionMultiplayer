using FPSGame.Health;
using FPSGame.Input;
using FPSGame.Weapons;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace FPSGame.Player
{
    public class PlayerSetup : MonoBehaviour
    {
        [Header("Player Components")] 
        [SerializeField] private PlayerHealth _health;
        [SerializeField] private MoveController _moveController;
        [SerializeField] private MouseLook _mouseLook;
        [SerializeField] private GameObject _playerCamera;
        [SerializeField] private TextMeshPro _nicknameText;  
        [SerializeField] private PhotonView _photonView;
        [SerializeField] private GameObject _playerUI;
        [SerializeField] private Transform _tpWeaponHolder;

        [Header("Weapon Components")] 
        [SerializeField] private WeaponController _weaponController;
        [SerializeField] private WeaponSwitcher _weaponSwitcher;

        private string _nickName;
        private bool _isLocalPlayer = false;

        private void Start()
        {
            IsLocalPlayer();
        }

        public void IsLocalPlayer()
        {
            _isLocalPlayer = _photonView.IsMine;

            if (_health != null)
                _health.Init(_isLocalPlayer, _photonView);

            if (_weaponSwitcher != null)
                _weaponSwitcher.Init(_photonView);

            if (_mouseLook != null)
                _mouseLook.Init();

            if (!_isLocalPlayer)
            {
                SetupRemotePlayer();
            }
            else
            {
                SetupLocalPlayer();
            }
        }

        private void SetupRemotePlayer()
        {
            if (_nicknameText != null) 
                _nicknameText.gameObject.SetActive(true); 

            if (_tpWeaponHolder != null)
                _tpWeaponHolder.gameObject.SetActive(true);

            if (_playerCamera != null)
                _playerCamera.SetActive(false);

            if (_playerUI != null)
                _playerUI.SetActive(false);
 
            if (_moveController != null)
                _moveController.enabled = false;

            if (_mouseLook != null)
                _mouseLook.enabled = false;

            // Remote player için weapon controller'ı deaktif et
            if (_weaponController != null)
                _weaponController.enabled = false;

            Debug.Log($"Remote player setup complete: {_nickName}");
        }

        private void SetupLocalPlayer()
        {
            if (_moveController != null)
                _moveController.Init(_photonView);

            if (_playerUI != null)
                _playerUI.SetActive(true);

            if (_playerCamera != null)
                _playerCamera.SetActive(true);

            if (_tpWeaponHolder != null)
                _tpWeaponHolder.gameObject.SetActive(false);
 
            if (_nicknameText != null)
                _nicknameText.gameObject.SetActive(false);

            // Local player için weapon controller'ı aktif et
            if (_weaponController != null)
                _weaponController.enabled = true;

            EnablePlayerInput();
            SetCursorState(true);

            Debug.Log($"Local player setup complete: {_nickName}");
        }

        private void EnablePlayerInput()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.EnablePlayerInput();
                Debug.Log("Player input enabled");
            }
            else
            {
                Debug.LogError("Cannot enable player input - InputManager is null!");
            }
        }

        private void SetCursorState(bool locked)
        {
            if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        [PunRPC]
        public void SetTPWeapon(int weaponIndex)
        {
            if (_tpWeaponHolder == null) return;

            foreach (Transform weapon in _tpWeaponHolder)
            {
                weapon.gameObject.SetActive(false);
            }

            if (weaponIndex >= 0 && weaponIndex < _tpWeaponHolder.childCount)
            {
                _tpWeaponHolder.GetChild(weaponIndex).gameObject.SetActive(true);
            }
        }

        [PunRPC]
        public void SetupName(string nickName)
        {
            _nickName = nickName;
            if (_nicknameText != null)
            {
                _nicknameText.text = nickName;
                Debug.Log($"Nickname set to: {nickName} for player: {gameObject.name}");
            }
            else
            {
                Debug.LogError("Nickname text component is null!");
            }
        }
 
        public void ReactivateComponents()
        {
            if (!_isLocalPlayer) return; 
            
            if (_mouseLook != null)
            {
                _mouseLook.enabled = true;
            }
 
            if (_moveController != null)
            {
                _moveController.enabled = true;
                _moveController.Init(_photonView);
            }

            if (_weaponController != null)
            {
                _weaponController.enabled = true;
            }

            EnablePlayerInput();
            SetCursorState(true);
        }
    }
}