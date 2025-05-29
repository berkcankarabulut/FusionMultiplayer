using System; 
using FPSGame.Player;
using FPSGame.PlayFab;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;

namespace FPSGame.Networking
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        public static RoomManager instance;

        [SerializeField] private GameObject _player;
        [SerializeField] private LeaderBoard _leaderBoard;
 
        [Space]
        [Header("Room Settings")]
        [SerializeField] private GameObject _connectionUI; 
        [Space]
        [SerializeField] private Transform[] _playerSpawn;

        private string _nickName = "unnamed";
        private string _roomName = "Room";
        private int _kills = 0;
        private int _deaths = 0;

        public int Kills => _kills;
        public int Deaths => _deaths;

        private void Awake()
        {
            instance = this;
        }

        public void InitRoom(string lobbyName)
        {
            _roomName = lobbyName;
            _connectionUI.SetActive(true);
            _ = PreparePlayerData();
        }

        private async UniTaskVoid PreparePlayerData()
        {
            string result = await PlayfabManager.Instance.GetUsernameAsync();
            if (string.IsNullOrEmpty(result))
            {
                Debug.LogError("Username not FOUND!");
                // Fallback nickname
                _nickName = "Player" + UnityEngine.Random.Range(1000, 9999);
            }
            else
            {
                _nickName = result;
            }
            
            Debug.Log($"Prepared nickname: {_nickName}");
            OnReadyToJoin();
        }

        private void OnReadyToJoin()
        {
            // PhotonNetwork nickname'ini ayarla
            PhotonNetwork.NickName = _nickName;
            PhotonNetwork.JoinOrCreateRoom(_roomName, null, null);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            Debug.Log($"Joined room with nickname: {PhotonNetwork.NickName}");
            PlayerSpawn();
            _leaderBoard.Init();
        }

        public void PlayerSpawn()
        {
            Debug.Log("PlayerSpawn called");
    
            Transform spawn = _playerSpawn[UnityEngine.Random.Range(0, _playerSpawn.Length)];
            GameObject playerObj = PhotonNetwork.Instantiate(
                _player.name,
                spawn.position,
                Quaternion.identity
            );
    
            PlayerSetup playerSetup = playerObj.GetComponent<PlayerSetup>();
            if (playerSetup != null)
            {
                // Setup'ı başlat
                playerSetup.IsLocalPlayer();
        
                // Nickname'i hemen ayarla (AllBuffered ile tüm oyunculara gönder)
                PhotonView playerPhotonView = playerObj.GetComponent<PhotonView>();
                if (playerPhotonView != null)
                {
                    Debug.Log($"Sending SetupName RPC with nickname: {_nickName}");
                    playerPhotonView.RPC("SetupName", RpcTarget.AllBuffered, _nickName);
                }
                else
                {
                    Debug.LogError("PhotonView component not found on player!");
                }
        
                Debug.Log($"Player spawned and setup completed for: {_nickName}");
            }
            else
            {
                Debug.LogError("PlayerSetup component not found on spawned player!");
            }
        }

        private void SetHashes()
        {
            try
            {
                Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
                hash["kills"] = Kills;
                hash["deaths"] = Deaths;
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting hashes: {e.Message}");
            }
        }

        public void AddKill(int killCount)
        {
            _kills += killCount;
            SetHashes();
        }

        public void AddDeath()
        {
            _deaths++;
            SetHashes();
        }
    }
}