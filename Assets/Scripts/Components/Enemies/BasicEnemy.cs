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
        }

        private void OnDestroy()
        {
            OnSeePlayer -= HandleSeePlayer;
            OnLosePlayer -= HandleLosePlayer;
        }

        private void HandleSeePlayer()
        {
            if (_animator != null)
            {
                _animator.SetBool("IsChasing", true);
            }
        }

        private void HandleLosePlayer()
        {
            if (_animator != null)
            {
                _animator.SetBool("IsChasing", false);
            }
        }

        public override EnemyType GetEnemyType() => EnemyType.Basic;
    }
}
