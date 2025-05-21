// File: Components/Enemies/EnemyBase.cs

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Components.Player;
using Zenject;
using Components;

namespace Components.Enemies
{
    public abstract class EnemyBase : MonoBehaviour, IStunnable, ISlowable
    {
        public event Action OnSeePlayer;
        public event Action OnLosePlayer;
        public event Action OnIdle;
        public event Action OnAttack;
        public event Action OnWalk;
        public event Action OnDeath;

        [Header("Stats")]
        [SerializeField] protected int _maxHealth = 100;

        [Header("Movement")]
        [SerializeField] protected Transform[] _patrolPoints;
        [SerializeField] protected float _moveSpeed = 2f;
        [SerializeField] protected float _chaseSpeed = 4f;
        [SerializeField] protected float _rotationSpeed = 5f;

        [Header("Detection")]
        [SerializeField] protected float _chaseDistance = 15f;
        [SerializeField] private float DefaultViewAngle = 90f;
        [SerializeField] protected float _stopChaseDistance = 2f;
        [SerializeField] protected float _postLoseWaitTime = 2f;
        [SerializeField] protected float _waitBeforeNextPatrolPoint = 1f;
        [SerializeField] protected float _chaseMemoryDuration = 3f;
        [SerializeField] protected LayerMask _playerLayer;
        [SerializeField] protected LayerMask _obstacleLayers;
        [SerializeField] private LayerMask _ignoreWhileChasing;
        [SerializeField] protected Transform _viewOrigin;
        [SerializeField] protected bool _debug = true;

        protected float _viewAngle;
        private const float ChaseViewAngle = 360f;

        protected Transform _player;
        protected NavMeshAgent _agent;
        protected CancellationToken _token;
        protected bool _isChasing;
        protected bool _waitingAfterLostPlayer;
        protected bool _isDead;
        protected bool _isStunned;

        private float _chaseMemoryTimer = 0f;
        private int _currentPatrolIndex;
        private bool _playerVisible;
        private bool _isAttacking;
        private bool _isWaitingAtPatrol;

        private bool _isSlowed;
        private float _originalMoveSpeed;
        private float _originalChaseSpeed;

        [Inject]
        private void Construct(PlayerComponent player)
        {
            _player = player.transform;
        }

        protected virtual void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _originalMoveSpeed = _moveSpeed;
            _originalChaseSpeed = _chaseSpeed;
            _agent.speed = _moveSpeed;
            _token = gameObject.GetCancellationTokenOnDestroy();
            _viewAngle = DefaultViewAngle;
            PatrolLoop().Forget();
        }

        public void ApplySlow(float speedMultiplier, float duration)
        {
            if (_debug) Debug.Log($"[Enemy] Slowed by x{speedMultiplier} for {duration} sec");
            if (_isDead || _isStunned || _isSlowed) return;

            _isSlowed = true;

            _moveSpeed *= speedMultiplier;
            _chaseSpeed *= speedMultiplier;

            _agent.speed = _isChasing ? _chaseSpeed : _moveSpeed;

            HandleSlow(duration).Forget();
        }

        private async UniTaskVoid HandleSlow(float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, _token);
            }

            _moveSpeed = _originalMoveSpeed;
            _chaseSpeed = _originalChaseSpeed;
            _agent.speed = _isChasing ? _chaseSpeed : _moveSpeed;

            _isSlowed = false;
            if (_debug) Debug.Log("[Enemy] Recovered from slow");
        }

        public void Stun(float duration)
        {
            if (_debug) Debug.Log("[Enemy] Stunned for " + duration);
            if (_isDead || _isStunned) return;

            _isStunned = true;
            _agent.isStopped = true;
            _agent.ResetPath();
            OnIdle?.Invoke();

            HandleStun(duration).Forget();
        }

        private async UniTaskVoid HandleStun(float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, _token);
            }

            _isStunned = false;
            _agent.isStopped = false;
            if (_debug) Debug.Log("[Enemy] Recovered from stun");
        }

        protected void MarkDead()
        {
            if (_debug) Debug.Log("[Enemy] Died");

            _isDead = true;
            OnDeath?.Invoke();

            if (_agent != null)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.enabled = false;
            }

            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            gameObject.layer = LayerMask.NameToLayer("Default");
            enabled = false;
        }

        // Remaining code (PatrolLoop, CanSeePlayer, Rotate methods, OnDrawGizmosSelected, etc.) stays unchanged
        private async UniTaskVoid PatrolLoop()
        {
            while (true)
            {
                if (_isDead || _isStunned) { await UniTask.Yield(PlayerLoopTiming.Update, _token); continue; }

                _playerVisible = CanSeePlayer(_isChasing);
                if (_isDead || _isStunned) break;

                if (_playerVisible)
                {
                    float dist = Vector3.Distance(transform.position, _player.position);

                    if (dist <= _stopChaseDistance)
                    {
                        _isAttacking = true;
                        _isChasing = true;
                        _chaseMemoryTimer = _chaseMemoryDuration;
                        _agent.isStopped = true;
                        OnAttack?.Invoke();
                        if (_debug) Debug.Log("[Enemy] Immediate Attack");
                    }
                    else
                    {
                        _isChasing = true;
                        _chaseMemoryTimer = _chaseMemoryDuration;
                        _agent.isStopped = false;
                        OnSeePlayer?.Invoke();
                        if (_debug) Debug.Log("[Enemy] Immediate Chase");
                    }
                }

                if (_isChasing)
                {
                    if (_isStunned) continue;
                    _agent.speed = _chaseSpeed;
                    _viewAngle = ChaseViewAngle;

                    float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

                    if (_playerVisible)
                        _chaseMemoryTimer = _chaseMemoryDuration;
                    else
                        _chaseMemoryTimer -= Time.deltaTime;

                    RotateTowardsPlayer();
                    if (_isDead || _isStunned) break;

                    if (_chaseMemoryTimer <= 0f)
                    {
                        _isChasing = false;
                        _agent.ResetPath();
                        _agent.speed = _moveSpeed;
                        _viewAngle = DefaultViewAngle;
                        LosePlayer();
                        if (_debug) Debug.Log("[Enemy] Lost player after memory");

                        _waitingAfterLostPlayer = true;
                        OnIdle?.Invoke();

                        float rotateTime = 0f;
                        while (rotateTime < _postLoseWaitTime)
                        {
                            if (_isDead || _isStunned) break;

                            transform.Rotate(Vector3.up * _rotationSpeed);
                            rotateTime += Time.deltaTime;
                            await UniTask.Yield(PlayerLoopTiming.Update, _token);

                            if (_isDead || _isStunned) break;

                            _playerVisible = CanSeePlayer(false);
                            if (_playerVisible)
                            {
                                float dist = Vector3.Distance(transform.position, _player.position);
                                if (dist <= _stopChaseDistance)
                                {
                                    _isAttacking = true;
                                    _isChasing = true;
                                    _chaseMemoryTimer = _chaseMemoryDuration;
                                    _agent.isStopped = true;
                                    OnAttack?.Invoke();
                                    if (_debug) Debug.Log("[Enemy] Reacquired and Attacking while rotating");
                                }
                                else
                                {
                                    _isChasing = true;
                                    _chaseMemoryTimer = _chaseMemoryDuration;
                                    _agent.isStopped = false;
                                    OnSeePlayer?.Invoke();
                                    if (_debug) Debug.Log("[Enemy] Reacquired and Chasing while rotating");
                                }
                                break;
                            }
                        }

                        _waitingAfterLostPlayer = false;
                        _isAttacking = false;
                        continue;
                    }
                    else if (distanceToPlayer > _stopChaseDistance)
                    {
                        if (_isAttacking)
                        {
                            _isAttacking = false;
                            OnSeePlayer?.Invoke();
                            if (_debug) Debug.Log("[Enemy] Back to Chase from Attack");
                        }

                        _agent.isStopped = false;
                        _agent.SetDestination(_player.position);
                    }
                    else
                    {
                        if (!_isAttacking && _playerVisible)
                        {
                            _isAttacking = true;
                            _agent.isStopped = true;
                            OnAttack?.Invoke();
                            if (_debug) Debug.Log("[Enemy] Attack!");
                        }
                    }
                }
                else
                {
                    _viewAngle = DefaultViewAngle;

                    if (!_waitingAfterLostPlayer && !_isWaitingAtPatrol)
                    {
                        _agent.speed = _moveSpeed;

                        if (CanSeePlayer(false))
                        {
                            continue;
                        }

                        await Patrol();
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.Update, _token);
            }
        }

        protected virtual async UniTask Patrol()
        {
            if (_isDead || _isStunned) return;
            if (_patrolPoints.Length == 0) return;

            if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
            {
                if (_isDead || _isStunned) return;

                _isWaitingAtPatrol = true;
                OnIdle?.Invoke();

                float elapsed = 0f;
                while (elapsed < _waitBeforeNextPatrolPoint)
                {
                    if (_isDead || _isStunned) return;

                    if (CanSeePlayer(false)) return;

                    RotateTowardsNextPatrolPoint();
                    await UniTask.Yield(PlayerLoopTiming.Update, _token);
                    elapsed += Time.deltaTime;
                }

                if (_isDead || _isStunned) return;

                OnWalk?.Invoke();

                Vector3 direction = (_patrolPoints[_currentPatrolIndex].position - transform.position).normalized;
                direction.y = 0f;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime * 100f);
                }

                _agent.SetDestination(_patrolPoints[_currentPatrolIndex].position);
                _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
                _isWaitingAtPatrol = false;
            }
        }

        protected bool CanSeePlayer(bool ignoreObstacles)
        {
            if (_player == null) return false;

            Vector3 origin = _viewOrigin ? _viewOrigin.position : transform.position;
            Vector3 directionToPlayer = _player.position - origin;
            float distanceToPlayer = directionToPlayer.magnitude;
            if (distanceToPlayer > _chaseDistance) return false;

            float angle = Vector3.Angle(_viewOrigin.forward, directionToPlayer);
            if (angle > _viewAngle / 2f) return false;

            LayerMask checkMask = _playerLayer | _obstacleLayers;
            if (ignoreObstacles)
                checkMask &= ~_ignoreWhileChasing;

            if (Physics.Raycast(origin, directionToPlayer.normalized, out var hit, _chaseDistance, checkMask))
            {
                if (hit.transform == _player) return true;
            }

            return false;
        }

        private void RotateTowardsPlayer()
        {
            Vector3 direction = (_player.position - transform.position).normalized;
            direction.y = 0f;
            if (direction == Vector3.zero) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime * 100f);
        }

        private void RotateTowardsNextPatrolPoint()
        {
            if (_patrolPoints.Length == 0) return;

            Vector3 direction = (_patrolPoints[_currentPatrolIndex].position - transform.position).normalized;
            direction.y = 0f;
            if (direction == Vector3.zero) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime * 100f);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (_viewOrigin == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_viewOrigin.position, _chaseDistance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_viewOrigin.position, _stopChaseDistance);

            Vector3 forward = _viewOrigin.forward;
            float halfFOV = _viewAngle * 0.5f;

            Quaternion leftRayRotation = Quaternion.Euler(0, -halfFOV, 0);
            Quaternion rightRayRotation = Quaternion.Euler(0, halfFOV, 0);

            Vector3 leftRayDirection = leftRayRotation * forward;
            Vector3 rightRayDirection = rightRayRotation * forward;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(_viewOrigin.position, leftRayDirection * _chaseDistance);
            Gizmos.DrawRay(_viewOrigin.position, rightRayDirection * _chaseDistance);

            if (_player != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_viewOrigin.position, _player.position);
            }
        }

        public virtual void LosePlayer()
        {
            OnLosePlayer?.Invoke();
        }

        public abstract EnemyType GetEnemyType();
    }
}
