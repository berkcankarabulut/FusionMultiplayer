using System; 
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace FPSGame.Player
{
    public class PlayerSetup : MonoBehaviour
    {
        public MoveController moveController;
        public GameObject playerCamera;
        public TextMeshPro _nicknameText;
        public PhotonView photonView;
        public GameObject playerUI;
        public Transform tpWeaponHolder;
        private string _nickName;

        public void IsLocalPlayer()
        {
            if (!photonView.IsMine)
            { 
                _nicknameText.gameObject.SetActive(true);
                tpWeaponHolder.gameObject.SetActive(true);
            }
            else
            { 
                playerUI.SetActive(true);
                tpWeaponHolder.gameObject.SetActive(false);
                moveController.enabled = true;
                playerCamera.SetActive(true);
            }
        }

        [PunRPC]
        public void SetTPWeapon(int weaponIndex)
        {
            foreach (Transform weapon in tpWeaponHolder)
            {
                weapon.gameObject.SetActive(false);
            }
            tpWeaponHolder.GetChild(weaponIndex).gameObject.SetActive(true);
        }

        [PunRPC]
        public void SetupName(string nickName)
        {
            _nickName = nickName;
            _nicknameText.text = nickName; 
        } 
       
    }
}