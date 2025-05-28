using System;
using FPSGame.Input;
using FPSGame.Weapons;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace FPSGame.Player
{
    public class PlayerSetup : MonoBehaviour
    {
        [Header("Player Components")]
        [SerializeField] private Health _health;
        [SerializeField] private MoveController _moveController;
        [SerializeField] private MouseLook _mouseLook;
        [SerializeField] private GameObject _playerCamera;
        [SerializeField] private TextMeshPro __nicknameText;
        [SerializeField] private PhotonView _photonView;
        [SerializeField] private GameObject _playerUI;
        [SerializeField] private Transform _tpWeaponHolder;
        
        [Header("Weapon Components")]
        [SerializeField] private WeaponController[] _weaponControllers;
        [SerializeField] private WeaponSwitcher _weaponSwitcher;

        private string _nickName;
        private bool _isLocalPlayer = false;

        public void IsLocalPlayer()
        {
            _isLocalPlayer = _photonView.IsMine; 
            _health.Init(_isLocalPlayer, _photonView);
            _weaponSwitcher.Init(_photonView);
            
            if (!_isLocalPlayer) SetupRemotePlayer();
            else SetupLocalPlayer();
        }

        private void SetupRemotePlayer()
        {
            __nicknameText.gameObject.SetActive(true);
            _tpWeaponHolder.gameObject.SetActive(true); 
            Debug.Log($"Remote player setup complete: {_nickName}");
        }

        private void SetupLocalPlayer()
        { 
            _moveController.Init(_photonView);
            _playerUI.SetActive(true);
            _playerCamera.SetActive(true); 
            _tpWeaponHolder.gameObject.SetActive(false); 
            
            if (InputManager.Instance != null)
            {
                InputManager.Instance.EnablePlayerInput();
                Debug.Log($"Local player setup complete: {_nickName}");
            }
            else
            {
                Debug.LogWarning("InputManager not found! Make sure InputManager exists in scene.");
            }
        } 
        
        [PunRPC]
        public void SetTPWeapon(int weaponIndex)
        {
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
            __nicknameText.text = nickName;
        } 
    }
}