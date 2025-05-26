using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace FPSGame.Networking
{
    public class RoomList : MonoBehaviourPunCallbacks
    {
        public static RoomList instance;

        [Header("UI")]
        [SerializeField]
        private GameObject _roomListCamera;

        [SerializeField]
        private Transform _roomListParent;

        [SerializeField]
        private ServerSlot _serverSlot;

        [SerializeField]
        private TMP_InputField _roomName;
        private List<RoomInfo> cacheroomList = new List<RoomInfo>();

        private void Awake()
        {
            instance = this;
        }

        private IEnumerator Start()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.Disconnect();
            }

            yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            PhotonNetwork.JoinLobby();
        }

        public void CreateOrJoinRoom(string roomName)
        {
            if (roomName.Trim() == "")
                roomName = _roomName.text;
           RoomManager.instance.InitRoom(roomName);
            _roomListCamera.SetActive(false);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            if (cacheroomList.Count == 0)
                cacheroomList = roomList;
            else
            {
                foreach (var room in roomList)
                {
                    RefreshCacheList(room);
                }
            }

            UpdateUI();
        }

        private void RefreshCacheList(RoomInfo room)
        {
            for (int i = 0; i < cacheroomList.Count; i++)
            {
                if (cacheroomList[i].Name == room.Name)
                {
                    List<RoomInfo> newList = cacheroomList;
                    if (room.RemovedFromList)
                    {
                        newList.Remove(newList[i]);
                    }
                    else
                    {
                        newList[i] = room;
                    }

                    cacheroomList = newList;
                }
            }
        }

        private void UpdateUI()
        {
            foreach (Transform roomItem in _roomListParent)
                Destroy(roomItem.gameObject);
            foreach (var room in cacheroomList)
            {
                ServerSlot serverSlot = Instantiate(_serverSlot, _roomListParent);
                serverSlot.Init(room.Name, room.PlayerCount + "/16");
                serverSlot.transform.SetParent(_roomListParent);
            }
        }
    }
}
