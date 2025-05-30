using BehaviourSystem;
using UnityEngine;

namespace FPSGame.AI.Actions
{
    public class IdleAction : Leaf
    {
        private readonly ZombieData _data;
        private float _idleStartTime;
        private bool _isIdle;
        
        public IdleAction(ZombieData data) : base("Idle")
        { 
            _data = data;
        }
        
        public override Status Process()
        { 
            if (!_isIdle)
            {
                _idleStartTime = Time.time;
                _isIdle = true;
                Debug.Log($"Zombie idle moduna geçti - {_data.idleTime} saniye bekleyecek");
                return Status.RUNNING;
            }
             
            if (Time.time >= _idleStartTime + _data.idleTime)
            {
                Debug.Log("Zombie idle süresini tamamladı");
                _isIdle = false;
                return Status.SUCCESS;
            }
             
            return Status.RUNNING;
        }
        
        public override void Reset()
        {
            _isIdle = false;
            base.Reset();
        }
    }
}