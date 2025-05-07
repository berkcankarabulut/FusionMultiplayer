using FPSGame.Networking;
using FPSGame.Player;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using TMPro;
using UnityEngine;

namespace FPSGame.Weapons
{
    public class WeaponController : MonoBehaviour
    {
        public int Damage = 5;
        public float fireRate = 1;
        public bool isReloading = false;
        [Space] 
        [Header("Components")]
        public Camera mainCamera;
        public Animator animator;
        
        [Header("VFX")] public GameObject hitVFX;
        private float nextFire;

        [Header("Ammo")] public int mag = 5;
        public int ammo = 30;
        public int magAmmo = 30;
        [Space]
        
        [Header("Recoil Settings")] 
        [Range(0,2)]
        public float recoverPercent = 0.7f;
        public float recoilUp = 0.2f;
        public float recoilBack = 0.2f; 
        
        private Vector3 originPosition;
        private Vector3 recoilVelocity = Vector3.zero;

        private float recoilLenght;
        private float recoverLenght;
        
        private bool isRecoiling = false;
        private bool isRecovering = false;
        
        [Header("UI")]
        public TextMeshProUGUI magText;
        public TextMeshProUGUI ammoText;

        private void Start()
        {
            ReloadAmmoUI();
            originPosition = transform.localPosition;
            recoilLenght = 0;
            recoverLenght = 1 / fireRate * recoverPercent;
        }

        private void Update()
        { 
            
            if(isReloading) return;
            if (nextFire > 0) nextFire -= Time.deltaTime;

            if (Input.GetButton("Fire1") && nextFire <= 0 && ammo > 0)
            {
                nextFire = 1 / fireRate;
                Fire();
                ammo--;
                ReloadAmmoUI();
            }

            if (Input.GetKeyDown(KeyCode.R) || ammo <= 0) Reload();
             
            if(isRecoiling) Recoil();
            else if(isRecovering) Recover();
        }

        private void Recoil()
        {
            Vector3 finalPosition = new Vector3(originPosition.x, originPosition.y + recoilUp, originPosition.z - recoilBack);
            
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoilLenght);
            
            if (transform.localPosition != finalPosition) return;
            isRecoiling = false;
            isRecovering = true;
        }
        
        private void Recover()
        {
            Vector3 finalPosition = originPosition;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoverLenght);
            if (transform.localPosition != finalPosition) return;
            isRecoiling = false;
            isRecovering = false;
        }
        
        void Reload()
        {
            if (mag <= 0) return;
            animator.SetTrigger("onReload");
            isReloading = true;
        }

        //Trigger on Animator Event
        public void Reloaded()
        { 
            isReloading = false;  
            mag--;
            ammo = magAmmo;
            ReloadAmmoUI();
        }

        private void ReloadAmmoUI()
        {
            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;
        }

        private void Fire()
        { 
            isRecoiling = true;
            isRecovering = false;
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition); 
            RaycastHit hit; 
            if (!Physics.Raycast(ray.origin, ray.direction, out hit, 100)) return;
            PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);
            
            Health health = hit.transform.GetComponent<Health>();
            if (health == null) return;
            PhotonNetwork.LocalPlayer.AddScore(Damage);
            if(Damage >= health._health) RoomManager.instance.AddKill(1); 
            
            hit.transform.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, Damage);
        }
    }
}