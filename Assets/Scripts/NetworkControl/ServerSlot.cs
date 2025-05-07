using TMPro;
using UnityEngine;

namespace FPSGame.Networking
{
    public class ServerSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _serverNameText;
        [SerializeField] private TextMeshProUGUI _playerCountText;

        public void Init(string serverName, string playerCount)
        {
            _serverNameText.text = serverName;
            _playerCountText.text = playerCount;
        }

        public void Join()
        {
            RoomList.instance.CreateOrJoinRoom(_serverNameText.text);
        }
    }
}