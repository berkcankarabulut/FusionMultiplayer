using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine; 

public class WeaponSwitcher : MonoBehaviour
{
     public PhotonView view;
     private int selectedWeapon = 0;
     private int previousSelectedWeapon = 0;
     public GameObject[] weapons;
     private void Start()
     {
          SelectWeapon();
     }

     private void Update()
     {
          previousSelectedWeapon = selectedWeapon;
          if (Input.GetKeyDown(KeyCode.Alpha1)) selectedWeapon = 0;
          else if (Input.GetKeyDown(KeyCode.Alpha2)) selectedWeapon = 1;
          else if (Input.GetAxis("Mouse ScrollWheel") > 0)
          {
               if (selectedWeapon >= weapons.Length - 1) selectedWeapon = 0;
               else selectedWeapon += 1;
          } 
          else  if (Input.GetAxis("Mouse ScrollWheel") < 0)
          {
               if (selectedWeapon <= 0) selectedWeapon = weapons.Length - 1;
               else selectedWeapon -= 1;
          }
          
          if(previousSelectedWeapon != selectedWeapon) SelectWeapon();
     }

     void SelectWeapon()
     { 
          view.RPC("SetTPWeapon",RpcTarget.All, selectedWeapon);

          for (int i = 0; i < weapons.Length; i++)
          {
               if (i == selectedWeapon)
               {
                    weapons[i].gameObject.SetActive(true);
               }
               else weapons[i].gameObject.SetActive(false);
          }
     }
}
