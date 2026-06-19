using UnityEngine;

namespace GlubinnyChertog.Player
{
    /// <summary>
    /// Handles touch/drag movement and automatic attacks toward the nearest enemy.
    /// Auto-shooter convention: player never manually aims or fires.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private Camera mainCamera;

        [Header("Stats (casual baseline)")]
        [SerializeField] private float maxHp = 120f;
        [SerializeField] private float baseDamage = 12f;
        [SerializeField] private float attacksPerSecond = 1f;
        [SerializeField] private float attackRange = 3f;

        [Header("Weapon")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private float _attackCooldown;
        private float _currentHp;

        // Mad Fury multipliers, applied/reverted by SanityManager events
        private float _damageMultiplier = 1f;
        private float _attackSpeedMultiplier = 1f;

        public float CurrentHp => _currentHp;
        public float MaxHp => maxHp;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (mainCamera == null) mainCamera = Camera.main;
            _currentHp = maxHp;
        }

        private void OnEnable()
        {
            if (Sanity.SanityManager.Instance != null)
                Sanity.SanityManager.Instance.OnMadFuryTriggered += ApplyMadFury;
        }

        private void OnDisable()
        {
            if (Sanity.SanityManager.Instance != null)
                Sanity.SanityManager.Instance.OnMadFuryTriggered -= ApplyMadFury;
        }

        private void Update()
        {
            ReadInput();
            HandleAttack();
        }

        private void FixedUpdate()
        {
            _rb.MovePosition(_rb.position + _moveInput * moveSpeed * Time.fixedDeltaTime);
        }

        private void ReadInput()
        {
            // Simple drag-to-move: works with mouse in editor and touch on device.
            _moveInput = Vector2.zero;

            if (Input.GetMouseButton(0))
            {
                Vector3 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = (Vector2)worldPoint - _rb.position;
                _moveInput = direction.sqrMagnitude > 0.04f ? direction.normalized : Vector2.zero;
            }
        }

        private void HandleAttack()
        {
            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown > 0f) return;

            Transform target = FindNearestEnemy();
            if (target == null) return;

            Fire(target);
            _attackCooldown = 1f / (attacksPerSecond * _attackSpeedMultiplier);
        }

        private Transform FindNearestEnemy()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemy"));
            if (hits.Length == 0) return null;

            Transform nearest = null;
            float closestSqr = float.MaxValue;

            foreach (var hit in hits)
            {
                float sqrDist = ((Vector2)hit.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (sqrDist < closestSqr)
                {
                    closestSqr = sqrDist;
                    nearest = hit.transform;
                }
            }
            return nearest;
        }

        private void Fire(Transform target)
        {
            if (projectilePrefab == null || firePoint == null) return;

            Vector2 direction = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            if (proj.TryGetComponent<Weapons.Projectile>(out var projectile))
            {
                projectile.Initialize(direction, baseDamage * _damageMultiplier);
            }
        }

        public void TakeDamage(float amount)
        {
            _currentHp = Mathf.Max(0f, _currentHp - amount);
            if (_currentHp <= 0f)
            {
                bool revived = Core.ReviveManager.Instance != null && Core.ReviveManager.Instance.TryRevive();
                if (revived) _currentHp = maxHp * 0.5f; // revive with half HP
            }
        }

        public void ResetForNewRun()
        {
            _currentHp = maxHp;
            _damageMultiplier = 1f;
            _attackSpeedMultiplier = 1f;
        }

        private void ApplyMadFury()
        {
            if (Sanity.SanityManager.Instance == null) return;

            _damageMultiplier = Sanity.SanityManager.Instance.GetMadFuryDamageMultiplier();
            _attackSpeedMultiplier = Sanity.SanityManager.Instance.GetMadFuryAttackSpeedMultiplier();

            Invoke(nameof(RevertMadFury), Sanity.SanityManager.Instance.GetMadFuryDuration());
        }

        private void RevertMadFury()
        {
            _damageMultiplier = 1f;
            _attackSpeedMultiplier = 1f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
