using System;
using FPSGame.Input;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSGame.Player
{
    public class MoveController : MonoBehaviour
    {
        [Header("Movement Settings")] [SerializeField]
        private MoveSettings _moveSettings;

        [Header("References")] [SerializeField]
        private CharacterController _characterController;

        private PhotonView _photonView;

        private Vector2 _input;
        private Vector2 _smoothInput;
        private Vector2 _smoothInputVelocity;

        private bool _sprinting = false;
        private bool _jumping = false;
        private bool _grounded = false;
        private Vector3 _smoothMoveDirection;
        private Vector3 _smoothMoveVelocity;

        private float _verticalVelocity = 0;
        private float _gravity = 20f;

        private Vector3 _networkPosition;
        private Quaternion _networkRotation;
        private float _syncTime = 0;
        private float _syncDelay = 0;
        private float _lastSyncTime = 0;

        public void Init(PhotonView photonView)
        {
            this.enabled = true;
            _photonView = photonView;

            if (!_photonView || _photonView.IsMine) return;
            _networkPosition = transform.position;
            _networkRotation = transform.rotation;
        }

        private void OnEnable()
        {
            InputManager.Instance.InputActions.Player.Jump.performed += OnJumpPerformed;
        }

        private void OnDisable()
        {
            InputManager.Instance.InputActions.Player.Jump.performed -= OnJumpPerformed;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _jumping = true;
        }

        private void Update()
        {
            if (_photonView && !_photonView.IsMine)
            {
                SmoothSyncMovement();
                return;
            }

            _input = InputManager.Instance.MoveInput;
            _sprinting = InputManager.Instance.IsSprintPressed;

            _smoothInput = Vector2.SmoothDamp(
                _smoothInput,
                _input.normalized,
                ref _smoothInputVelocity,
                _moveSettings.smoothTime
            );

            _grounded = _characterController.isGrounded;

            MovePlayer();

            _jumping = false; 
        }

        private void MovePlayer()
        {
            if (_grounded)
            {
                _verticalVelocity = -1f;

                if (_jumping)
                {
                    _verticalVelocity = Mathf.Sqrt(2f * _gravity * _moveSettings.jumpHeight);
                }
            }
            else
            {
                _verticalVelocity -= _gravity * Time.deltaTime;
            }

            float speed = _sprinting ? _moveSettings.sprintSpeed : _moveSettings.walkSpeed;
            Vector3 targetDirection = new Vector3(_smoothInput.x, 0, _smoothInput.y);
            targetDirection = transform.TransformDirection(targetDirection);

            if (!_grounded)
            {
                targetDirection *= _moveSettings.airControl;
            }

            _smoothMoveDirection = Vector3.SmoothDamp(
                _smoothMoveDirection,
                targetDirection * speed,
                ref _smoothMoveVelocity,
                _moveSettings.smoothTime
            );

            Vector3 movement = _smoothMoveDirection;
            movement.y = _verticalVelocity;

            _characterController.Move(movement * Time.deltaTime);

            if (_smoothInput.magnitude > 0.1f)
            {
                Vector3 lookDirection = new Vector3(
                    _smoothMoveDirection.x,
                    0,
                    _smoothMoveDirection.z
                );
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        _moveSettings.rotationSpeed * Time.deltaTime
                    );
                }
            }
        }
 
        private void SmoothSyncMovement()
        {
            _syncTime += Time.deltaTime;
            float interpolation = _syncTime / _syncDelay;
            interpolation = interpolation * interpolation * (3.0f - 2.0f * interpolation);

            transform.position = Vector3.Lerp(transform.position, _networkPosition, interpolation);
            transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, interpolation);
        }

        [Serializable]
        public class MoveSettings
        {
            public float walkSpeed = 4f;
            public float sprintSpeed = 8f;
            public float jumpHeight = 4f;

            [Space] public float airControl = 0.5f;

            [Space] public float smoothTime = 0.15f;
            public float rotationSpeed = 10f;
        }
    }
}