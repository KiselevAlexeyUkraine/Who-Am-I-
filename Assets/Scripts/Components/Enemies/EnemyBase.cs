using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Components.Player;
using Zenject;

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

        [Header("Detection")]
        [SerializeField] protected float _chaseDistance = 15f;
        [SerializeField] private float DefaultViewAngle = 90f;
        [SerializeField] protected float _stopChaseDistance = 2f;
        [SerializeField] private float _attackEndDistance = 3f;
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
        protected bool _isSlowed;
        protected bool _isAttacking;
        protected bool _isWaitingAtPatrol;
        private bool _playerVisible;

        private float _chaseMemoryTimer;
        private int _currentPatrolIndex;
        private float _originalMoveSpeed;
        private float _originalChaseSpeed;

        [Inject]
        private void Construct(PlayerComponent player) => _player = player.transform;

        protected virtual void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _originalMoveSpeed = _moveSpeed;
            _originalChaseSpeed = _chaseSpeed;
            _agent.speed = _moveSpeed;
            _agent.updateRotation = true;
            _token = gameObject.GetCancellationTokenOnDestroy();
            _viewAngle = DefaultViewAngle;

            InvokeRepeating(nameof(AITick), 0f, 0.1f);
        }

        private void AITick()
        {
            if (_isDead || _isStunned || _agent == null || !_agent.isOnNavMesh || !_agent.enabled) return;

            _playerVisible = CanSeePlayer(_isChasing);
            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            if (_playerVisible)
            {
                _chaseMemoryTimer = _chaseMemoryDuration;

                if (distanceToPlayer <= _stopChaseDistance)
                {
                    if (!_isAttacking)
                    {
                        _isAttacking = true;
                        _isChasing = true;
                        _agent.isStopped = true;
                        OnAttack?.Invoke();
                        if (_debug) Debug.Log("[Enemy] Attack started");
                    }
                }
                else if (distanceToPlayer > _attackEndDistance)
                {
                    if (_isAttacking)
                    {
                        _isAttacking = false;
                        _agent.isStopped = false;
                        OnSeePlayer?.Invoke();
                        if (_debug) Debug.Log("[Enemy] Back to chase from attack");
                    }
                    if (!_isChasing)
                    {
                        _isChasing = true;
                        OnSeePlayer?.Invoke();
                        if (_debug) Debug.Log("[Enemy] Immediate chase");
                    }
                }
            }
            else
            {
                _chaseMemoryTimer -= 0.1f;
            }

            if (_isChasing)
            {
                _agent.speed = _chaseSpeed;
                _viewAngle = ChaseViewAngle;

                if (_chaseMemoryTimer <= 0f)
                {
                    _isChasing = false;
                    _isAttacking = false;
                    _agent.ResetPath();
                    _agent.speed = _moveSpeed;
                    _viewAngle = DefaultViewAngle;
                    LosePlayer();
                    OnIdle?.Invoke();
                    if (_debug) Debug.Log("[Enemy] Lost player");
                }
                else if (!_isAttacking && distanceToPlayer > _stopChaseDistance)
                {
                    _agent.isStopped = false;
                    if (!_agent.hasPath || _agent.remainingDistance > 0.5f)
                    {
                        _agent.SetDestination(_player.position);
                    }
                }
            }
            else if (!_waitingAfterLostPlayer && !_isWaitingAtPatrol)
            {
                _agent.speed = _moveSpeed;
                if (_playerVisible) return;
                Patrol().Forget();
            }
        }

        protected virtual async UniTask Patrol()
        {
            if (_isDead || _isStunned || _agent == null || !_agent.isOnNavMesh || !_agent.enabled) return;
            if (_patrolPoints.Length == 0) return;
            if (_agent.hasPath && _agent.remainingDistance > _agent.stoppingDistance) return;

            _isWaitingAtPatrol = true;
            OnIdle?.Invoke();

            float elapsed = 0f;
            while (elapsed < _waitBeforeNextPatrolPoint && !_isDead && !_isStunned && _agent != null && _agent.isOnNavMesh && _agent.enabled)
            {
                if (CanSeePlayer(false)) return;
                await UniTask.Yield(PlayerLoopTiming.Update);
                elapsed += Time.deltaTime;
            }

            if (_isDead || _isStunned || _agent == null || !_agent.isOnNavMesh || !_agent.enabled) return;

            OnWalk?.Invoke();
            Vector3 targetPos = _patrolPoints[_currentPatrolIndex].position;
            _agent.SetDestination(targetPos);
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;

            while (!_isDead && !_isStunned && !_playerVisible && _agent != null && _agent.isOnNavMesh && _agent.enabled)
            {
                if (_agent.pathPending)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    continue;
                }

                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                        break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            _isWaitingAtPatrol = false;
        }

        public void ApplySlow(float speedMultiplier, float duration)
        {
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
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            _moveSpeed = _originalMoveSpeed;
            _chaseSpeed = _originalChaseSpeed;
            _agent.speed = _isChasing ? _chaseSpeed : _moveSpeed;
            _isSlowed = false;
        }

        public void Stun(float duration)
        {
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
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            _isStunned = false;
            _agent.isStopped = false;

            if (_isChasing)
            {
                OnSeePlayer?.Invoke();
                if (_debug) Debug.Log("[Enemy] Resume chase after stun complete");
            }
        }

        protected void MarkDead()
        {
            _isDead = true;
            OnDeath?.Invoke();
            CancelInvoke(nameof(AITick));
            if (_agent != null)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.enabled = false;
            }
            gameObject.layer = LayerMask.NameToLayer("IgnorePlayer");
            enabled = false;
        }

        protected bool CanSeePlayer(bool ignoreObstacles)
        {
            if (_player == null) return false;

            Vector3 origin = _viewOrigin ? _viewOrigin.position : transform.position;
            Vector3 directionToPlayer = _player.position - origin;
            float distance = directionToPlayer.magnitude;
            if (distance > _chaseDistance) return false;

            float angle = Vector3.Angle(_viewOrigin.forward, directionToPlayer);
            if (angle > _viewAngle / 2f) return false;

            LayerMask mask = _playerLayer | _obstacleLayers;
            if (ignoreObstacles) mask &= ~_ignoreWhileChasing;

            if (Physics.Raycast(origin, directionToPlayer.normalized, out var hit, _chaseDistance, mask))
                return hit.transform == _player;

            return false;
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
            Quaternion left = Quaternion.Euler(0, -halfFOV, 0);
            Quaternion right = Quaternion.Euler(0, halfFOV, 0);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_viewOrigin.position, left * forward * _chaseDistance);
            Gizmos.DrawRay(_viewOrigin.position, right * forward * _chaseDistance);

            if (_player != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_viewOrigin.position, _player.position);
            }
        }

        public virtual void LosePlayer()
        {
            _isAttacking = false;
            OnLosePlayer?.Invoke();
        }

        public abstract EnemyType GetEnemyType();
    }
}
