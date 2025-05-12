using System;
using Components.Player;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Components.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyHealth))]
    public abstract class EnemyBase : MonoBehaviour
    {
        protected event Action OnAttack;

        [Header("Enemy Stats")]
        [SerializeField] protected float _moveSpeed = 3f;
        [SerializeField] protected int _damage = 10;
        [SerializeField] protected float _detectionRadius = 15f;

        protected Transform _target;
        protected NavMeshAgent _agent;
        protected EnemyHealth _health;

        [Inject]
        private void Construct(PlayerComponent player)
        {
            _target = player.transform;
        }

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<EnemyHealth>();
            _agent.speed = _moveSpeed;

            _health.OnDeath += Die;
        }

        protected virtual void OnDestroy()
        {
            _health.OnDeath -= Die;
        }

        protected virtual void Update()
        {
            var targetPosition = _target.position;
            var distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            if (distanceToTarget <= _detectionRadius)
            {
                if (ShouldMoveTowardsTarget(distanceToTarget))
                {
                    _agent.SetDestination(targetPosition);
                }
                else
                {
                    Attack();
                }
            }
        }

        protected abstract bool ShouldMoveTowardsTarget(float distanceToTarget);
        protected abstract void Attack();

        protected virtual void Die()
        {
            
        }

        public void InvokeAttack()
        {
            OnAttack?.Invoke();
        }
    }
}
