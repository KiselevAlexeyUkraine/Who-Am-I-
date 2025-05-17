using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Components.Player;
using Zenject;

namespace Components.Enemies
{
    public abstract class EnemyBase : MonoBehaviour
    {
        public event Action OnSeePlayer;
        public event Action OnLosePlayer;
        public event Action OnIdle;
        public event Action OnAttack;
        public event Action OnWalk;

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

        private float _chaseMemoryTimer = 0f;
        private int _currentPatrolIndex;
        private bool _playerVisible;
        private bool _isAttacking;
        private bool _isWaitingAtPatrol;

        [Inject]
        private void Construct(PlayerComponent player)
        {
            _player = player.transform;
        }

        protected virtual void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = _moveSpeed;
            _token = gameObject.GetCancellationTokenOnDestroy();
            _viewAngle = DefaultViewAngle;
            PatrolLoop().Forget();
        }

        private async UniTaskVoid PatrolLoop()
        {
            while (true)
            {
                _playerVisible = CanSeePlayer(_isChasing);

                if (_isChasing)
                {
                    _agent.speed = _chaseSpeed;
                    _viewAngle = ChaseViewAngle;

                    float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

                    if (_playerVisible)
                        _chaseMemoryTimer = _chaseMemoryDuration;
                    else
                        _chaseMemoryTimer -= Time.deltaTime;

                    RotateTowardsPlayer();

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

                        await UniTask.Delay(TimeSpan.FromSeconds(_postLoseWaitTime), cancellationToken: _token);

                        RotateTowardsLastKnownPlayer();
                        OnWalk?.Invoke();

                        _waitingAfterLostPlayer = false;
                        _isAttacking = false;
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
                        await Patrol();

                        if (_playerVisible)
                        {
                            _isChasing = true;
                            _chaseMemoryTimer = _chaseMemoryDuration;
                            _agent.isStopped = false;
                            OnSeePlayer?.Invoke();
                            if (_debug) Debug.Log("[Enemy] Saw player");
                        }
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.Update, _token);
            }
        }

        protected virtual async UniTask Patrol()
        {
            if (_patrolPoints.Length == 0) return;

            if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
            {
                _isWaitingAtPatrol = true;
                OnIdle?.Invoke();

                float elapsed = 0f;
                while (elapsed < _waitBeforeNextPatrolPoint)
                {
                    RotateTowardsNextPatrolPoint();
                    await UniTask.Yield(PlayerLoopTiming.Update, _token);
                    elapsed += Time.deltaTime;
                }

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

        private void RotateTowardsLastKnownPlayer()
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
