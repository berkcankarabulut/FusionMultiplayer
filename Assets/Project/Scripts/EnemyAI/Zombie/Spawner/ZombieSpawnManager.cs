using UnityEngine; 
using Photon.Pun;

namespace FPSGame.AI
{
    public class ZombieSpawnManager : MonoBehaviourPun 
    {
        [Header("Spawn Settings")]
        [SerializeField] private string zombiePrefabPath = "Zombies"; 
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform poolParent;

        [Header("Pool Settings")]
        [SerializeField] private int poolSize = 10;

        [Header("Controls")] 
        [SerializeField] private bool canSpawn = true;

        private ZombiePool zombiePool;

        public bool CanSpawn => PhotonNetwork.IsMasterClient && canSpawn;

        private void Awake()
        {
            zombiePool = new ZombiePool(zombiePrefabPath, poolSize, poolParent);
        }    
        public void SpawnZombie()
        {
            if (!CanSpawn || spawnPoints.Length == 0) return;

            int randomIndex = Random.Range(0, spawnPoints.Length);
            SpawnZombieAtPoint(randomIndex);
        }

        public void SpawnZombieAtPoint(int pointIndex)
        {
            if (!CanSpawn || !IsValidSpawnPoint(pointIndex)) return;

            Transform spawnPoint = spawnPoints[pointIndex];
            GameObject zombie = zombiePool.GetZombie();

            if (zombie != null)
            {
                SetupZombie(zombie, spawnPoint); 
            }
        }

        public void ReturnZombieToPool(GameObject zombie)
        {
            if (zombie == null) return;

            var zombieAI = zombie.GetComponent<ZombieAI>();
            zombieAI?.StopAI(); 
            zombiePool.DestroyZombie(zombie);
        }

        [PunRPC]
        private void OnZombieSpawned(int spawnPointIndex)
        {
            if (!IsValidSpawnPoint(spawnPointIndex)) return;

            Transform spawnPoint = spawnPoints[spawnPointIndex];
            GameObject zombie = zombiePool.GetZombie();

            if (zombie != null)
            {
                SetupZombie(zombie, spawnPoint);
            }
        }

        private void SetupZombie(GameObject zombie, Transform spawnPoint)
        {
            SetupTransform(zombie, spawnPoint);
            SetupController(zombie);
            zombie.SetActive(true);
            
            PhotonView zombiePhotonView = zombie.GetComponent<PhotonView>();
            Debug.Log($"Photon zombie spawned at {spawnPoint.name} - ViewID: {zombiePhotonView?.ViewID ?? -1}");
        }

        private void SetupTransform(GameObject zombie, Transform spawnPoint)
        {
            zombie.transform.position = spawnPoint.position;
            zombie.transform.rotation = spawnPoint.rotation;
        }

        private void SetupController(GameObject zombie)
        {
            var zombieAI = zombie.GetComponent<ZombieAI>();
            if (zombieAI != null)
            {
                zombieAI.ResetAI();
            }
        } 

        private bool IsValidSpawnPoint(int index)
        {
            return index >= 0 && index < spawnPoints.Length && spawnPoints[index] != null;
        }

        // Public API
        public void TriggerSpawn() => SpawnZombie();
        public void TriggerSpawnAtPoint(int pointIndex) => SpawnZombieAtPoint(pointIndex);
        public void SetSpawnEnabled(bool enabled) => canSpawn = enabled;
        public void ClearAllZombies() => zombiePool.ClearAll();

        // Debug Info
        public int ActiveZombieCount => zombiePool.ActiveCount;
        public int PooledZombieCount => zombiePool.PooledCount;

        private void OnDrawGizmosSelected()
        {
            if (spawnPoints == null) return;

            Gizmos.color = Color.red;
            foreach (var point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 1f);
                    Gizmos.DrawRay(point.position, point.forward * 2f);
                }
            }
        }
    }
}