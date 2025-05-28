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
        [SerializeField] private  GameObject _connectionUI; 
        [Space]
        [SerializeField] private  Transform[] _playerSpawn;

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

        public async UniTask PreparePlayerData()
        {
            string result = await PlayfabManager.Instance.GetUsernameAsync();
            if (string.IsNullOrEmpty(result))
            {
                Debug.LogError("Username not FOUND!");
            }
            this._nickName = result;
            OnReadyToJoin();
        }

        public void OnReadyToJoin()
        {
            PhotonNetwork.JoinOrCreateRoom(_roomName, null, null);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            PlayerSpawn();
            _leaderBoard.Init();
        }

        public void PlayerSpawn()
        {
            Transform spawn = _playerSpawn[UnityEngine.Random.Range(0, _playerSpawn.Length)];
            GameObject _player = PhotonNetwork.Instantiate(
                this._player.name,
                spawn.position,
                Quaternion.identity
            );
            PlayerSetup playerSetup = _player.GetComponent<PlayerSetup>();
            playerSetup.IsLocalPlayer();
            _player
                .GetComponent<PhotonView>()
                .RPC("SetupName", RpcTarget.AllBuffered, this._nickName); 
            PhotonNetwork.LocalPlayer.NickName = _nickName;
        }

        public void SetHashes()
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
                Console.WriteLine(e);
                throw;
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
