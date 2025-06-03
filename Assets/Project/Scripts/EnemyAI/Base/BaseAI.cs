using UnityEngine;

namespace FPSGame.AI
{
    public abstract class BaseAI : MonoBehaviour 
    {
        [Header("AI Settings")]
        [SerializeField] protected bool startActiveOnEnable = false;
        
        protected bool isAIActive = false;

        public bool IsActive => isAIActive;

        protected virtual void OnEnable()
        {
            if (startActiveOnEnable)
            {
                InitializeAI();
            }
        }

        protected virtual void OnDisable()
        {
            StopAI();
        }

        public virtual void InitializeAI()
        {
            isAIActive = true;
            OnAIInitialized();
        }

        public virtual void StopAI()
        {
            isAIActive = false;
            OnAIStopped();
        }

        protected abstract void OnAIInitialized();
        protected abstract void OnAIStopped();

        protected virtual void Update()
        {
            if (isAIActive)
            {
                UpdateAI();
            }
        }

        protected abstract void UpdateAI();
    }
}