using Services;
using System;
using UnityEngine;
using Zenject;

namespace Components.Player
{
    [RequireComponent(typeof(Rigidbody))]
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

        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheckerTransform;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _checkerRadius;

        private float _currentStamina;
        private bool _isStaminaExhausted;
        private Rigidbody _rigidbody;
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

        [Inject]
        private void Construct(InputService inputService, SettingsService settingsService, PauseService pauseService)
        {
            _input = inputService;
            _settings = settingsService;
            _pause = pauseService;
        }

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
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

        private void FixedUpdate()
        {
            if (_pause.IsPaused || _isStunned) return;
        }

        private void Move()
        {
            bool canSprint = !_isStaminaExhausted;
            var isSprinting = _input.Sprint && !_isCrouching && canSprint;
            var speed = (_isCrouching ? _crouchSpeed : _moveSpeed) * (isSprinting ? _sprintMultiplier : 1f);
            speed *= _slowMultiplier;

            _motion = transform.right * _input.Horizontal + transform.forward * _input.Vertical;

            if (_motion.magnitude > 1f)
                _motion.Normalize();

            if (_motion.magnitude > 0.1f)
                OnMove?.Invoke();

            Vector3 velocity = _motion * speed;
            _rigidbody.linearVelocity = new Vector3(velocity.x, _rigidbody.linearVelocity.y, velocity.z);

            if (IsGrounded && _input.Jump && !_isCrouching)
            {
                _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, _jumpForce, _rigidbody.linearVelocity.z);
                OnJump?.Invoke();
            }
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
            var fov = _input.Sprint && !Mathf.Approximately(_motion.magnitude, 0f) && !_isStaminaExhausted
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
            _isCrouching = _input.Crouch;

            var targetHeight = _isCrouching ? _crouchHeight : _standHeight;
            var scale = transform.localScale;
            scale.y = Mathf.Lerp(scale.y, targetHeight / _standHeight, Time.deltaTime * _crouchTransitionSpeed);
            transform.localScale = scale;

            var cameraPosition = _camera.transform.localPosition;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetHeight, Time.deltaTime * _crouchTransitionSpeed);
            _camera.transform.localPosition = cameraPosition;
        }

        private bool IsGrounded =>
            Physics.CheckSphere(_groundCheckerTransform.position, _checkerRadius, _groundLayer);

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
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_groundCheckerTransform.position, _checkerRadius);
        }
#endif
    }
}
