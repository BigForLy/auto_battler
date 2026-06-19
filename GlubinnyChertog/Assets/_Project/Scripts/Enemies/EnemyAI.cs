using UnityEngine;

namespace GlubinnyChertog.Enemies
{
    /// <summary>
    /// Simple chase-and-contact-damage AI. Driven by EnemyData ScriptableObject
    /// so balance numbers live in data, not code.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private Data.EnemyData data;

        private Rigidbody2D _rb;
        private Transform _player;
        private float _currentHp;
        private float _moveSpeed;
        private float _damageTickCooldown;
        private const float DamageTickInterval = 0.5f;

        public Data.EnemyData Data => data;

        public void Initialize(Data.EnemyData enemyData, Transform playerTransform, float playerBaseSpeed)
        {
            data = enemyData;
            _player = playerTransform;
            _currentHp = data.maxHp;
            _moveSpeed = playerBaseSpeed * data.moveSpeedMultiplier;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_player == null) return;

            Vector2 direction = ((Vector2)_player.position - _rb.position).normalized;
            _rb.MovePosition(_rb.position + direction * _moveSpeed * Time.fixedDeltaTime);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            _damageTickCooldown -= Time.deltaTime;
            if (_damageTickCooldown > 0f) return;

            if (other.TryGetComponent<Player.PlayerController>(out var player))
            {
                player.TakeDamage(data.damage);
                _damageTickCooldown = DamageTickInterval;
            }
        }

        public void TakeDamage(float amount)
        {
            _currentHp -= amount;
            if (_currentHp <= 0f)
            {
                Sanity.SanityManager.Instance?.AddSanity(data.sanityContribution);
                // TODO: drop resourceDrop pickup, spawn death VFX, return to pool
                Destroy(gameObject);
            }
        }
    }
}
