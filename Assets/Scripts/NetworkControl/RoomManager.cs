using System;
using FPSGame.Player;
using Photon.Pun; 
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace FPSGame.Networking
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        public static RoomManager instance;

        public GameObject player;
        public LeaderBoard leaderBoard;
        [Space] [Header("Room Settings")] public GameObject cameraGO;
        public GameObject nameUI;
        public GameObject connectionUI;
        [Space] public Transform[] playerSpawn;

        private string nickName = "unnamed";
        private string roomName = "Room";
        private int kills = 0;
        private int deaths = 0;

        public int Kills => kills;

        public int Deaths => deaths;

        private void Awake()
        {
            instance = this;
        }

        public void InitRoom(string lobbyName)
        {
            roomName = lobbyName;
            cameraGO.SetActive(true);
            nameUI.SetActive(true);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            PlayerSpawn();
            leaderBoard.Init();
        }

        public void JoinRoomButtonPressed()
        {
            PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
            nameUI.SetActive(false);
            cameraGO.SetActive(false);
            connectionUI.SetActive(true);
        }

        public void ChangeNickName(string nickName)
        {
            this.nickName = nickName;
        }


        public void PlayerSpawn()
        {
            Transform spawn = playerSpawn[UnityEngine.Random.Range(0, playerSpawn.Length)];
            GameObject _player = PhotonNetwork.Instantiate(player.name, spawn.position, Quaternion.identity);
            PlayerSetup playerSetup = _player.GetComponent<PlayerSetup>();
            playerSetup.IsLocalPlayer();
            _player.GetComponent<PhotonView>().RPC("SetupName", RpcTarget.AllBuffered, this.nickName);
            _player.GetComponent<Health>().isLocalPlayer = true;
            PhotonNetwork.LocalPlayer.NickName = nickName;
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
            kills += killCount;
            SetHashes();
        }

        public void AddDeath()
        {
            deaths++;
            SetHashes();
        }
    }
}