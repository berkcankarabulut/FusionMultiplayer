using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int _health = 100;
    public TextMeshProUGUI healthText;
    public bool isLocalPlayer;
    [PunRPC]
    public void TakeDamage(int damage)
    {
        _health -= damage;
        healthText.text = _health.ToString();

        if (_health > 0) return;
        Destroy(gameObject); 
        if (isLocalPlayer)
        {
            RoomManager.instance.PlayerSpawn();
            RoomManager.instance.AddDeath(); 
        }
    }
}
