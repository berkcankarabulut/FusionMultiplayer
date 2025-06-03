using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

namespace FPSGame.AI
{
    [System.Serializable]
    public class ZombiePool : IZombiePool
    {
        private string _prefabPath; 
        private int _poolSize = 10;

        private Queue<GameObject> _pool;
        private List<GameObject> _activeZombies;
        private Transform _poolParent;

        public int ActiveCount => _activeZombies.Count;
        public int PooledCount => _pool.Count;

        public ZombiePool(string prefabPath, int PoolSize, Transform PoolParent = null)
        {
            _prefabPath = prefabPath;
            _poolSize = PoolSize;
            _poolParent = PoolParent;
         
            _pool = new Queue<GameObject>();
            _activeZombies = new List<GameObject>();
            if (PhotonNetwork.IsMasterClient)
            {
                CreateInitialPool();
            }
        } 

        private void CreateInitialPool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                // PhotonNetwork.Instantiate kullan
                GameObject zombie = PhotonNetwork.Instantiate(_prefabPath, Vector3.zero, Quaternion.identity);
                zombie.name = $"Zombie_{i:00}";

                if (_poolParent != null)
                    zombie.transform.SetParent(_poolParent);

                zombie.SetActive(false);
                _pool.Enqueue(zombie);
            }

            Debug.Log($"Photon zombie pool created with {_poolSize} zombies");
        }

        public GameObject GetZombie()
        {
            GameObject zombie = _pool.Count > 0 ? _pool.Dequeue() : CreateNewZombie();
            if (zombie != null)
            {
                _activeZombies.Add(zombie);
            }

            return zombie;
        }

        public void ReturnZombie(GameObject zombie)
        {
            if (zombie == null) return;

            zombie.SetActive(false);
            _activeZombies.Remove(zombie);
            _pool.Enqueue(zombie);
        }

        private GameObject CreateNewZombie()
        {
            // Pool boşsa ve Master Client ise yeni zombie oluştur
            if (PhotonNetwork.IsMasterClient)
            {
                GameObject zombie = PhotonNetwork.Instantiate(_prefabPath, Vector3.zero, Quaternion.identity);
                zombie.name = $"Zombie_Extra_{System.Guid.NewGuid().ToString()[..8]}";

                if (_poolParent != null)
                    zombie.transform.SetParent(_poolParent);

                return zombie;
            }

            return null;
        }

        public void ClearAll()
        {
            // Sadece Master Client temizleyebilir
            if (!PhotonNetwork.IsMasterClient) return;

            foreach (var zombie in _activeZombies.ToArray())
            {
                if (zombie != null)
                {
                    // PhotonNetwork objesini yok et
                    PhotonNetwork.Destroy(zombie);
                }
            }

            // Pool'daki zombileri de temizle
            while (_pool.Count > 0)
            {
                var zombie = _pool.Dequeue();
                if (zombie != null)
                {
                    PhotonNetwork.Destroy(zombie);
                }
            }

            _activeZombies.Clear();
        }

        public void DestroyZombie(GameObject zombie)
        {
            if (zombie == null) return;

            _activeZombies.Remove(zombie);

            // Pool'dan da çıkar
            var tempQueue = new Queue<GameObject>();
            while (_pool.Count > 0)
            {
                var pooledZombie = _pool.Dequeue();
                if (pooledZombie != zombie)
                {
                    tempQueue.Enqueue(pooledZombie);
                }
            }

            _pool = tempQueue;

            // PhotonNetwork objesini yok et
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(zombie);
            }
        }
    }
}