using UnityEngine;

namespace FPSGame.Player
{
    public class FaceObjectToCamera : MonoBehaviour
    {
        [SerializeField] private bool _lookAtCamera = true;
        [SerializeField] private bool _reverseDirection = false;
        [SerializeField] private Vector3 _offset = Vector3.zero;

        private Camera _mainCamera;

        void Start()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                _mainCamera = FindObjectOfType<Camera>();
            }
        }

        void Update()
        {
            if (!_lookAtCamera) return;
            if (_mainCamera == null) return;

            Vector3 targetPosition = _mainCamera.transform.position + _offset;

            if (_reverseDirection)
            {
                Vector3 directionAwayFromCamera = transform.position - targetPosition;
                transform.LookAt(transform.position + directionAwayFromCamera);
            }
            else transform.LookAt(targetPosition);
        }

        void LateUpdate()
        {
            if (_mainCamera == null || !_mainCamera.gameObject.activeInHierarchy)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    _mainCamera = FindObjectOfType<Camera>();
                }
            }
        }
    }
}