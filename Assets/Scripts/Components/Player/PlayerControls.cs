using Services;
using System;
using UnityEngine;
using Zenject;

namespace Components.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerControls : MonoBehaviour, IStunnable, ISlowable
    {
        public event Action OnJump;
        public event Action OnMove;
        public event Action<float> OnStaminaChanged;

        [Header("Camera Settings")]
        [SerializeField] private Camera _camera;
        [SerializeField] private float _sprintFovMultiplier = 1.15f;
        [SerializeField] private float _fovTransitionSpeed = 10f;

        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _sprintMultiplier = 1.5f;
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private float _gravity = -9.81f;

        [Header("Mouse Look Settings")]
        [SerializeField] private float _mouseSmoothTime = 0.1f;
        [SerializeField] private float _maxLookAngle = 90f;

        [Header("Crouch Settings")]
        [SerializeField] private float _crouchHeight = 1f;
        [SerializeField] private float _standHeight = 2f;
        [SerializeField] private float _crouchSpeed = 2.5f;
        [SerializeField] private float _crouchTransitionSpeed = 6f;

        [Header("Stamina Settings")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _staminaRecoveryStanding = 20f;
        [SerializeField] private float _staminaRecoveryWalking = 5f;
        [SerializeField] private float _staminaDrainSprinting = 10f;
        [SerializeField] private float _staminaSprintThreshold = 10f;

        [Header("Ceiling Check Settings")]
        [SerializeField] private Transform _ceilCheckerTransform;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _checkerRadius = 0.2f;

        private float _currentStamina;
        private bool _isStaminaExhausted;
        private CharacterController _controller;
        private SettingsService _settings;
        private InputService _input;
        private PauseService _pause;

        private Vector2 _currentMouseDeltaVelocity;
        private Vector2 _currentMouseDelta;
        private float _xRotation;
        private bool _isCrouching;
        private Vector3 _motion;

        private bool _isStunned;
        private float _slowMultiplier = 1f;
        private float _slowEndTime = 0f;
        private float _verticalVelocity;
        private bool IsGrounded => _controller.isGrounded;

        [Inject]
        private void Construct(InputService inputService, SettingsService settingsService, PauseService pauseService)
        {
            _input = inputService;
            _settings = settingsService;
            _pause = pauseService;
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _camera.fieldOfView = _settings.SavedFov;
            _currentStamina = _maxStamina;
            _isStaminaExhausted = false;
            OnStaminaChanged?.Invoke(_currentStamina / _maxStamina);
        }

        private void Update()
        {
            if (_pause.IsPaused)
            {
                _camera.fieldOfView = _settings.SavedFov;
                return;
            }

            MouseLook();
            if (_isStunned) return;

            if (Time.time >= _slowEndTime)
                _slowMultiplier = 1f;

            Crouch();
            ChangeFov();
            UpdateStamina();
            Move();
        }

        private void Move()
        {
            bool canSprint = !_isStaminaExhausted;
            bool isSprinting = _input.Sprint && !_isCrouching && canSprint;
            float speed = (_isCrouching ? _crouchSpeed : _moveSpeed) * (isSprinting ? _sprintMultiplier : 1f);
            speed *= _slowMultiplier;

            _motion = transform.right * _input.Horizontal + transform.forward * _input.Vertical;

            if (_motion.magnitude > 1f)
                _motion.Normalize();

            if (_motion.magnitude > 0.1f)
                OnMove?.Invoke();

            if (IsGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            if (IsGrounded && _input.Jump && !_isCrouching)
            {
                _verticalVelocity = _jumpForce;
                OnJump?.Invoke();
            }

            _verticalVelocity += _gravity * Time.deltaTime;
            Vector3 velocity = _motion * speed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        private void UpdateStamina()
        {
            bool isMoving = _motion.magnitude > 0.1f;
            bool isSprinting = _input.Sprint && isMoving && !_isCrouching && !_isStaminaExhausted;

            if (isSprinting)
            {
                _currentStamina -= _staminaDrainSprinting * Time.deltaTime;
                if (_currentStamina <= 0f)
                {
                    _currentStamina = 0f;
                    _isStaminaExhausted = true;
                }
            }
            else if (!isMoving)
            {
                _currentStamina += _staminaRecoveryStanding * Time.deltaTime;
            }
            else
            {
                _currentStamina += _staminaRecoveryWalking * Time.deltaTime;
            }

            if (_isStaminaExhausted && _currentStamina >= _staminaSprintThreshold)
            {
                _isStaminaExhausted = false;
            }

            _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina);
            OnStaminaChanged?.Invoke(_currentStamina / _maxStamina);
        }

        private void ChangeFov()
        {
            var fov = _input.Sprint && _motion.magnitude > 0.1f && !_isStaminaExhausted
                ? _settings.SavedFov * _sprintFovMultiplier
                : _settings.SavedFov;
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, fov, Time.deltaTime * _fovTransitionSpeed);
        }

        private void MouseLook()
        {
            var mouseX = _input.MouseX * _settings.SavedMouseSensitivity;
            var mouseY = _input.MouseY * _settings.SavedMouseSensitivity;
            var targetMouseDelta = new Vector2(mouseX, mouseY);

            _currentMouseDelta = Vector2.SmoothDamp(
                _currentMouseDelta, targetMouseDelta, ref _currentMouseDeltaVelocity, _mouseSmoothTime
            );

            _xRotation -= _currentMouseDelta.y;
            _xRotation = Mathf.Clamp(_xRotation, -_maxLookAngle, _maxLookAngle);

            _camera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up, _currentMouseDelta.x);
        }

        private void Crouch()
        {
            if (_input.Crouch)
            {
                _isCrouching = true;
            }
            else if (_isCrouching)
            {
                if (!IsCeilingBlocked())
                {
                    _isCrouching = false;
                }
            }

            var targetHeight = _isCrouching ? _crouchHeight : _standHeight;
            var scale = transform.localScale;
            scale.y = Mathf.Lerp(scale.y, targetHeight / _standHeight, Time.deltaTime * _crouchTransitionSpeed);
            transform.localScale = scale;

            var cameraPosition = _camera.transform.localPosition;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetHeight, Time.deltaTime * _crouchTransitionSpeed);
            _camera.transform.localPosition = cameraPosition;
        }

        private bool IsCeilingBlocked()
        {
            return Physics.CheckSphere(_ceilCheckerTransform.position, _checkerRadius, _groundLayer);
        }

        public void Stun(float duration)
        {
            if (_isStunned) return;
            _isStunned = true;
            Invoke(nameof(EndStun), duration);
        }

        private void EndStun()
        {
            _isStunned = false;
        }

        public void ApplySlow(float speedMultiplier, float duration)
        {
            _slowMultiplier = Mathf.Clamp(speedMultiplier, 0f, 1f);
            _slowEndTime = Time.time + duration;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_ceilCheckerTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_ceilCheckerTransform.position, _checkerRadius);
            }
        }
#endif
    }
}
