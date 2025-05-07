using System;
using TMPro;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

namespace FPSGame.Networking
{
    public class LeaderBoard : MonoBehaviour
    {
        public GameObject playersHolder;
        [Header("Options")] public float refreshRate = 1;

        [Header("UI Elements")] [SerializeField]
        private LeaderboardSlot[] _leaderboardSlots;

        public void Init()
        {
            Refresh();
            InvokeRepeating("Refresh", 1f, refreshRate);
        }

        private void Refresh()
        {
            foreach (var slot in _leaderboardSlots)
            {
                slot.slot.SetActive(false);
            }

            var sortedPlayers =
                (from player in PhotonNetwork.PlayerList orderby player.GetScore() descending select player)
                .ToList();
            int i = 0;

            foreach (var player in sortedPlayers)
            {
                _leaderboardSlots[i].slot.SetActive(true);

                if (player.NickName == "") player.NickName = "Player" + i;
                _leaderboardSlots[i].playerNameText.text = player.NickName;
                _leaderboardSlots[i].scoreText.text = player.GetScore().ToString();

                if (player.CustomProperties["kills"] != null)
                    _leaderboardSlots[i].killText.text = player.CustomProperties["kills"].ToString();
                else _leaderboardSlots[i].killText.text = "0";

                if (player.CustomProperties["deaths"] != null)
                    _leaderboardSlots[i].deathText.text = player.CustomProperties["deaths"].ToString();
                else _leaderboardSlots[i].killText.text = "0";

                i++;
            }
        }

        private void Update()
        {
            playersHolder.SetActive(Input.GetKey(KeyCode.Tab));
        }
    }

    [Serializable]
    public class LeaderboardSlot
    {
        public GameObject slot;
        public TextMeshProUGUI killText;
        public TextMeshProUGUI deathText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI playerNameText;
    }
}