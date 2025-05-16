using UnityEngine;
using Components.Player;

namespace Components.Enemies
{
    public class BasicEnemy : EnemyBase
    {
        [Header("Debug Actions")]
        [SerializeField] private Animator _animator;

        [Header("Attack Settings")]
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private int _damageAmount = 10;

        [Header("Chase Settings")]
        [SerializeField] private float _chaseTimeout = 3f;

        private float _chaseTimer;
        private bool _isPlayerVisible;

        protected override void Start()
        {
            base.Start();
            OnSeePlayer += HandleSeePlayer;
            OnLosePlayer += HandleLosePlayer;
            OnIdle += HandleIdle;
            OnAttack += HandleAttack;
            OnWalk += HandleWalk;
        }

        private void OnDestroy()
        {
            OnSeePlayer -= HandleSeePlayer;
            OnLosePlayer -= HandleLosePlayer;
            OnIdle -= HandleIdle;
            OnAttack -= HandleAttack;
            OnWalk -= HandleWalk;
        }

        private void Update()
        {
            if (_isPlayerVisible)
            {
                _chaseTimer = _chaseTimeout;
            }
            else
            {
                if (_chaseTimer > 0)
                {
                    _chaseTimer -= Time.deltaTime;
                    if (_chaseTimer <= 0f)
                    {
                        base.LosePlayer(); // вручную вызвать смену состояния
                    }
                }
            }
        }

        private void HandleSeePlayer()
        {
            _isPlayerVisible = true;
            _animator?.Play("Chase");
        }

        private void HandleLosePlayer()
        {
            _isPlayerVisible = false;
            // не вызываем анимацию, ждём таймер
        }

        private void HandleIdle() => _animator?.Play("Idle");
        private void HandleWalk() => _animator?.Play("Walk");
        private void HandleAttack() => _animator?.Play("Attack");

        public override EnemyType GetEnemyType() => EnemyType.Basic;

        public void PerformAttack()
        {
            Vector3 origin = _viewOrigin != null ? _viewOrigin.position : transform.position;

            Vector3 targetPoint = _player.position + Vector3.up * 1.2f; // прицельная точка (грудь)
            Vector3 direction = (targetPoint - origin).normalized;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, _attackRange, ~0);

            foreach (var hit in hits)
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_damageAmount);
                    Debug.Log($"Ударили сквозь препятствия: {hit.collider.name}");
                    break;
                }
            }

#if UNITY_EDITOR
            Debug.DrawRay(origin, direction * _attackRange, Color.red, 0.5f);
#endif
        }
    }
}