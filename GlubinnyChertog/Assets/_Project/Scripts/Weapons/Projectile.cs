using UnityEngine;

namespace GlubinnyChertog.Weapons
{
    /// <summary>
    /// Basic straight-line projectile. Pooled in later iteration via ObjectPool&lt;Projectile&gt;;
    /// MVP uses Instantiate/Destroy for simplicity, swap to pooling once profiling shows need.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float lifetime = 2f;

        private Vector2 _direction;
        private float _damage;

        public void Initialize(Vector2 direction, float damage)
        {
            _direction = direction;
            _damage = damage;
            Destroy(gameObject, lifetime);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void Update()
        {
            transform.Translate(_direction * speed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Enemies.EnemyAI>(out var enemy))
            {
                enemy.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}
