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

        private void HandleSeePlayer() => _animator?.Play("Chase");
        private void HandleLosePlayer() => _animator?.Play("Idle");
        private void HandleIdle() => _animator?.Play("Idle");
        private void HandleWalk() => _animator?.Play("Walk");
        private void HandleAttack() => _animator?.Play("Attack");

        public override EnemyType GetEnemyType() => EnemyType.Basic;

        // Call this from animation event
        public void PerformAttack()
        {
            Vector3 origin = _viewOrigin != null ? _viewOrigin.position : transform.position;
            Vector3 direction = transform.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, _attackRange, _playerLayer))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_damageAmount);
                    Debug.Log("Попали по игроку");
                }
            }

#if UNITY_EDITOR
            Debug.DrawRay(origin, direction * _attackRange, Color.red, 0.5f);
#endif
        }
    }
}
