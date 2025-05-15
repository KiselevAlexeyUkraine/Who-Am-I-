using Components.Player;
using UnityEngine;
using Zenject;

namespace Components.Enemies
{
    public class BasicEnemy : EnemyBase
    {
        [Header("Debug Actions")]
        [SerializeField] private Animator _animator;

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

        private void HandleSeePlayer()
        {
            _animator?.Play("Chase");
        }

        private void HandleLosePlayer()
        {
            _animator?.Play("Idle");
        }

        private void HandleIdle()
        {
            _animator?.Play("Idle");
        }

        private void HandleWalk()
        {
            _animator?.Play("Walk");
        }

        private void HandleAttack()
        {
            _animator?.Play("Attack");
        }

        public override EnemyType GetEnemyType() => EnemyType.Basic;
    }
}
