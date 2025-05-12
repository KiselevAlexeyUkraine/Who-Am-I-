using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Components.Environment
{
    public class ExplosiveBarrel : MonoBehaviour, IDamageable
    {
        public event Action OnExplode;
        
        [Header("Настройки взрыва")]
        [SerializeField] private int _health = 10;
        [SerializeField] private int _explosionDamage = 50;
        [SerializeField] private float _explosionRadius = 5f;
        [SerializeField] private ParticleSystem _explosionEffect;
        [SerializeField] private LayerMask _damageLayers;

        private Collider _collider;
        private Renderer _renderer;
        private CancellationToken _token;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _renderer = GetComponentInChildren<Renderer>();
        }

        public void TakeDamage(int amount)
        {
            _health -= amount;
            
            if (_health <= 0)
            {
                Explode().Forget();
            }
        }

        private async UniTaskVoid Explode()
        {
            OnExplode?.Invoke();

            _renderer.enabled = false;
            _collider.enabled = false;
            _explosionEffect.transform.SetParent(null); 
            _explosionEffect.Play();

            var colliders = Physics.OverlapSphere(transform.position, _explosionRadius, _damageLayers);
            foreach (var nearbyObject in colliders)
            {
                var damageable = nearbyObject.GetComponent<IDamageable>();
                damageable?.TakeDamage(_explosionDamage);
            }
            
            await UniTask.WaitForSeconds(_explosionEffect.main.duration, cancellationToken: _token);
            
            Destroy(_explosionEffect.gameObject);
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawSphere(transform.position, _explosionRadius);
        }
    }
}
