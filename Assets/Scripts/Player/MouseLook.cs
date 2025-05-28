using FPSGame.Input;
using UnityEngine;

namespace FPSGame.Player
{
    public class MouseLook : MonoBehaviour
    {
        [Header("Settings")] [Space] [SerializeField]
        private Vector2 _clampInDegrees = new Vector2(360, 180);

        [Space] [SerializeField] private bool _lockCursor = true;
        [Space] [SerializeField] private Vector2 _smoothing = new Vector2(3, 3);

        [Header("First Person")] public GameObject characterBody;
        
        private GameplayInputActions _inputActions;
        private Vector2 _targetDirection;
        private Vector2 _targetCharacterDirection;

        private Vector2 _mouseAbsolute;
        private Vector2 _smoothMouse;

        private Vector2 _mouseDelta;
        

        void Start()
        {
            _inputActions = InputManager.Instance.InputActions;
            _inputActions.Player.Enable();

            _targetDirection = transform.localRotation.eulerAngles;

            if (characterBody)
                _targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;

            if (_lockCursor)
                LockCursor();
        }

        private void OnDestroy()
        {
            if (_inputActions == null) return;
            _inputActions.Player.Disable();
            _inputActions?.Dispose();
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            var targetOrientation = Quaternion.Euler(_targetDirection);
            var targetCharacterOrientation = Quaternion.Euler(_targetCharacterDirection);

            Vector2 mouseInput = _inputActions.Player.Look.ReadValue<Vector2>();

            _mouseDelta = mouseInput * Time.deltaTime;

            _mouseDelta = Vector2.Scale(_mouseDelta,
                new Vector2(_smoothing.x, _smoothing.y));

            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, _mouseDelta.x, 1f / _smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, _mouseDelta.y, 1f / _smoothing.y);

            _mouseAbsolute += _smoothMouse;
            if (_clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -_clampInDegrees.x * 0.5f, _clampInDegrees.x * 0.5f);

            if (_clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -_clampInDegrees.y * 0.5f, _clampInDegrees.y * 0.5f);

            transform.localRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right) *
                                      targetOrientation;

            if (characterBody)
            {
                var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up);
                characterBody.transform.localRotation = yRotation * targetCharacterOrientation;
            }
            else
            {
                var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
                transform.localRotation *= yRotation;
            }
        }
    }
}