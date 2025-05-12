using Services;
using System;
using UnityEngine;
using Zenject;

namespace Components.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerControls : MonoBehaviour
    {
        public event Action OnJump;
        public event Action OnMove;

        [Header("Camera Settings")]
        [SerializeField] private Camera _camera;
        [SerializeField] private float _tiltAngle = 3f;
        [SerializeField] private float _tiltSmoothTime = 0.1f;
        [SerializeField] private float _sprintFovMultiplier = 1.15f;
        [SerializeField] private float _fovTransitionSpeed = 10f;
        
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _sprintMultiplier = 1.5f;
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private float _gravity = 9.81f;
        [SerializeField] private float _fallMultiplier = 2.5f;

        [Header("Mouse Look Settings")]
        [SerializeField] private float _mouseSmoothTime = 0.1f;
        [SerializeField] private float _maxLookAngle = 90f;

        [Header("Crouch Settings")]
        [SerializeField] private float _crouchHeight = 1f;
        [SerializeField] private float _standHeight = 2f;
        [SerializeField] private float _crouchSpeed = 2.5f;
        [SerializeField] private float _crouchTransitionSpeed = 6f;

        [Header("Ground and Ceil Check")]
        [SerializeField] private Transform _groundCheckerTransform;
        [SerializeField] private Transform _ceilCheckerTransform;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _checkerRadius;

        private CharacterController _character;
        private SettingsService _settings;
        private InputService _input;
        private PauseService _pause;
        
        private Vector2 _currentMouseDeltaVelocity;
        private Vector2 _currentMouseDelta;
        private Vector3 _velocity;
        private float _xRotation;
        private float _zRotation;
        private float _currentZRotationVelocity;
        private bool _isCrouching;
        private Vector3 _motion;

        [Inject]
        private void Construct(InputService inputService, SettingsService settingsService, PauseService pauseService)
        {
            _input = inputService;
            _settings = settingsService;
            _pause = pauseService;
        }
        
        private void Start()
        {
            _character = GetComponent<CharacterController>();
            _camera.fieldOfView = _settings.SavedFov;
        }

        private void Update()
        {
            if (_pause.IsPaused)
            {
                _camera.fieldOfView = _settings.SavedFov;
                return;
            }
            
            Move();
            ApplyGravity();
            MouseLook();
            Crouch();
            ChangeFov();
        }

        private void Move()
        {
            var speed = (_isCrouching ? _crouchSpeed : _moveSpeed) * (_input.Sprint && !_isCrouching ? _sprintMultiplier : 1f);
            _motion = transform.right * _input.Horizontal + transform.forward * _input.Vertical;

            if (_motion.magnitude > 1f)
            {
                _motion.Normalize();
            }

            if (_motion.magnitude > 0.5f)
            {
				OnMove?.Invoke();
			}

			_character.Move(_motion * (speed * Time.deltaTime));
        }

        private void ChangeFov()
        {
			var fov = _input.Sprint && !Mathf.Approximately(_motion.magnitude, 0f) ? _settings.SavedFov * _sprintFovMultiplier : _settings.SavedFov;
			_camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, fov, Time.deltaTime * _fovTransitionSpeed);
		}

        private void ApplyGravity()
        {
            if (IsGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            if (IsGrounded && _input.Jump && !_isCrouching)
            {
                _velocity.y = Mathf.Sqrt(_jumpForce * 2f * _gravity);
                OnJump?.Invoke();
            }

            if (_velocity.y < 0)
            {
                _velocity.y -= _gravity * _fallMultiplier * Time.deltaTime;
            }
            else
            {
                _velocity.y -= _gravity * Time.deltaTime;
            }

            _character.Move(_velocity * Time.deltaTime);
        }

        private void MouseLook()
        {
            var mouseX = _input.MouseX * _settings.SavedMouseSensitivity;
            var mouseY = _input.MouseY * _settings.SavedMouseSensitivity;
            var targetMouseDelta = new Vector2(mouseX, mouseY);
            var targetZRotation = -_input.Horizontal * _tiltAngle;
            
            _currentMouseDelta = Vector2.SmoothDamp(
                _currentMouseDelta, targetMouseDelta, ref _currentMouseDeltaVelocity, _mouseSmoothTime
            );
            
            _zRotation = Mathf.SmoothDamp(
                _zRotation, targetZRotation, ref _currentZRotationVelocity, _tiltSmoothTime
            );

            _xRotation -= _currentMouseDelta.y;
            _xRotation = Mathf.Clamp(_xRotation, -_maxLookAngle, _maxLookAngle);
            
            _camera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, _zRotation);
            transform.Rotate(Vector3.up, _currentMouseDelta.x);
        }

        private void Crouch()
        {
            _isCrouching = _input.Crouch || IsFloored;

            var targetHeight = _isCrouching ? _crouchHeight : _standHeight;
            var center = _character.center;
            center.y = Mathf.Lerp(center.y, targetHeight * 0.5f, Time.deltaTime * _crouchTransitionSpeed);
            _character.height = Mathf.Lerp(_character.height, targetHeight, Time.deltaTime * _crouchTransitionSpeed);
            _character.center = center;

            var cameraPosition = _camera.transform.localPosition;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetHeight, Time.deltaTime * _crouchTransitionSpeed);
            _camera.transform.localPosition = cameraPosition;
        }

        private bool IsGrounded =>
            Physics.CheckSphere(_groundCheckerTransform.position, _checkerRadius, _groundLayer.value);
        
        private bool IsFloored => 
            Physics.CheckSphere(_ceilCheckerTransform.position, _checkerRadius, _groundLayer.value);
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_groundCheckerTransform.position, _checkerRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_ceilCheckerTransform.position, _checkerRadius);
        }
#endif
    }
}
