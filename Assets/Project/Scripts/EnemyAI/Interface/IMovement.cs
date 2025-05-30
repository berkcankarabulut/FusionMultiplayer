using UnityEngine;

namespace FPSGame.AI
{
    public interface IMovement
    {
        void MoveTo(Vector3 position);
        void Stop();
        bool HasReachedDestination();
        void SetSpeed(float speed);
    }
}