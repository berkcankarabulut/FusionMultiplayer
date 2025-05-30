using UnityEngine;

namespace FPSGame.Input
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        
        private GameplayInputActions _inputActions;
        public GameplayInputActions InputActions => _inputActions;
        
        [Header("Settings")]
        [SerializeField] private bool enableOnStart = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        
        public bool IsPlayerInputEnabled => _inputActions?.Player.enabled ?? false;
        public bool IsUIInputEnabled => _inputActions?.UI.enabled ?? false;
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            // Initialize input actions
            _inputActions = new GameplayInputActions();
            
            Debug.Log("InputManager initialized");
        }
        
        private void Start()
        {
            if (enableOnStart)
            {
                EnablePlayerInput();
            }
        }
        
        private void OnDestroy()
        { 
            _inputActions?.Dispose();
            
            if (Instance == this)
            {
                Instance = null; 
            }
        }
        
        public void EnablePlayerInput()
        {
            _inputActions?.Player.Enable(); 
        } 
         
        public bool IsFirePressed => _inputActions?.Player.Fire.IsPressed() ?? false;
        public bool IsJumpPressed => _inputActions?.Player.Jump.IsPressed() ?? false;
        public bool IsSprintPressed => _inputActions?.Player.Sprint.IsPressed() ?? false;
        public bool IsScoreboardPressed => _inputActions?.Player.Scoreboard.IsPressed() ?? false;
        
        public Vector2 MoveInput => _inputActions?.Player.Move.ReadValue<Vector2>() ?? Vector2.zero;
        public Vector2 LookInput => _inputActions?.Player.Look.ReadValue<Vector2>() ?? Vector2.zero;
         
    }
}