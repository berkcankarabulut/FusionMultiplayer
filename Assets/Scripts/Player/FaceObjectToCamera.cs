using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSGame.Player
{
    public class FaceObjectToCamera : MonoBehaviour
    {
        void Update()
        {
            transform.LookAt(Camera.main.transform);
        }
    }
}