using UnityEngine;

namespace Components.Enemies
{
    public class BasicEnemy : EnemyBase, IDamageable
    {
        [Header("Debug Actions")]
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _source;

        [Header("Attack Settings")]
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private int _damageAmount = 10;

        [Header("Chase Settings")]
        [SerializeField] private float _chaseTimeout = 3f;

        [Header("Ragdoll Settings")]
        [SerializeField] private Rigidbody[] _ragdollBodies;
        [SerializeField] private Collider[] _ragdollColliders;

        private float _chaseTimer;
        private bool _isPlayerVisible;
        private int _currentHealth;

        private readonly int HashIdle = Animator.StringToHash("Idle");
        private readonly int HashWalk = Animator.StringToHash("Walk");
        private readonly int HashAttack = Animator.StringToHash("Attack");
        private readonly int HashChase = Animator.StringToHash("Chase");

        protected override void Start()
        {
            base.Start();
            _currentHealth = _maxHealth;
            OnSeePlayer += HandleSeePlayer;
            OnLosePlayer += HandleLosePlayer;
            OnIdle += HandleIdle;
            OnAttack += HandleAttack;
            OnWalk += HandleWalk;
            OnDeath += HandleDeath;

            _playerLayer = LayerMask.GetMask("Player");

            SetRagdollState(false);
        }

        private void OnDestroy()
        {
            OnSeePlayer -= HandleSeePlayer;
            OnLosePlayer -= HandleLosePlayer;
            OnIdle -= HandleIdle;
            OnAttack -= HandleAttack;
            OnWalk -= HandleWalk;
            OnDeath -= HandleDeath;
        }

        private void Update()
        {
            if (_isPlayerVisible)
            {
                _chaseTimer = _chaseTimeout;
            }
            else if (_chaseTimer > 0)
            {
                _chaseTimer -= Time.deltaTime;
                if (_chaseTimer <= 0f)
                {
                    base.LosePlayer();
                }
            }
        }

        public void TakeDamage(int amount)
        {
            if (_isDead) return;

            _currentHealth -= amount;
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            MarkDead();
        }

        private void HandleSeePlayer()
        {
            _isPlayerVisible = true;
            _animator?.Play(HashChase);
        }

        private void HandleLosePlayer() => _isPlayerVisible = false;
        private void HandleIdle() => _animator?.Play(HashIdle);
        private void HandleWalk() => _animator?.Play(HashWalk);
        private void HandleAttack() => _animator?.Play(HashAttack);

        private void HandleDeath()
        {
            _source.enabled = false;
            SetRagdollState(true);
        }

        private void SetRagdollState(bool state)
        {
            _animator.enabled = !state;

            foreach (var rb in _ragdollBodies)
            {
                rb.isKinematic = !state;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            foreach (var col in _ragdollColliders)
            {
                col.enabled = state;
            }

            if (TryGetComponent<Collider>(out var mainCollider))
                mainCollider.enabled = !state;
            if (TryGetComponent<Rigidbody>(out var mainRb))
                mainRb.isKinematic = state;
        }

        public override EnemyType GetEnemyType() => EnemyType.Basic;

        public void PerformAttack()
        {
            Vector3 origin = _viewOrigin != null ? _viewOrigin.position : transform.position;
            Vector3 targetPoint = _player.position + Vector3.up * 1.2f;
            Vector3 direction = (targetPoint - origin).normalized;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, _attackRange, _playerLayer);

            foreach (var hit in hits)
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_damageAmount);
                    Debug.Log($"Ударили игрока: {hit.collider.name}");
                    break;
                }
            }

#if UNITY_EDITOR
            Debug.DrawRay(origin, direction * _attackRange, Color.red, 0.5f);
#endif
        }
    }
}
