using System.Collections.Generic;
using UnityEngine;
using GlubinnyChertog.Core;
using GlubinnyChertog.Data;

namespace GlubinnyChertog.Enemies
{
    /// <summary>
    /// Spawns enemies around the player based on the current RunPhase.
    /// Spawn intervals follow the casual-tuned balance table:
    /// Intro: 1 enemy/1.2s, Rising: 1/0.7s + elite at 90s, Peak: 1/0.5s, Finale: tapering + boss.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject defaultEnemyPrefab;
        [SerializeField] private EnemyData[] introEnemyPool;
        [SerializeField] private EnemyData[] risingEnemyPool;
        [SerializeField] private EnemyData[] peakEnemyPool;

        [Header("Elite / Boss")]
        [SerializeField] private GameObject elitePrefab;
        [SerializeField] private EnemyData eliteData;
        [SerializeField] private float eliteSpawnTime = 90f;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private EnemyData bossData;

        [Header("Spawn Geometry")]
        [SerializeField] private float spawnRadius = 8f;

        [Header("Player Reference Speed")]
        [SerializeField] private float playerBaseSpeed = 3.5f;

        private float _spawnTimer;
        private bool _eliteSpawned;
        private bool _bossSpawned;
        private readonly List<GameObject> _activeEnemies = new List<GameObject>();

        private void OnEnable()
        {
            if (RunManager.Instance != null)
            {
                RunManager.Instance.OnPhaseChanged += HandlePhaseChanged;
                RunManager.Instance.OnRunEnded += HandleRunEnded;
            }
        }

        private void OnDisable()
        {
            if (RunManager.Instance != null)
            {
                RunManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
                RunManager.Instance.OnRunEnded -= HandleRunEnded;
            }
        }

        private void Update()
        {
            if (RunManager.Instance == null || !RunManager.Instance.IsRunActive) return;

            TrySpawnSpecials();

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnRegularEnemy();
                _spawnTimer = GetCurrentSpawnInterval();
            }
        }

        private void TrySpawnSpecials()
        {
            float t = RunManager.Instance.ElapsedTime;

            if (!_eliteSpawned && t >= eliteSpawnTime && RunManager.Instance.CurrentPhase == RunPhase.Rising)
            {
                _eliteSpawned = true;
                SpawnAt(elitePrefab, eliteData);
            }

            if (!_bossSpawned && RunManager.Instance.CurrentPhase == RunPhase.Finale)
            {
                _bossSpawned = true;
                SpawnAt(bossPrefab, bossData);
            }
        }

        private float GetCurrentSpawnInterval()
        {
            // Casual baseline intervals per phase; interpolate within phase via GetPhaseProgress
            // for a smooth ramp rather than a hard step.
            float progress = RunManager.Instance.GetPhaseProgress();

            switch (RunManager.Instance.CurrentPhase)
            {
                case RunPhase.Intro:
                    return Mathf.Lerp(1.5f, 1.2f, progress);
                case RunPhase.Rising:
                    return Mathf.Lerp(1.0f, 0.7f, progress);
                case RunPhase.Peak:
                    return Mathf.Lerp(0.7f, 0.5f, progress);
                default: // Finale - enemies taper off, focus shifts to boss
                    return Mathf.Lerp(0.6f, 1.2f, progress);
            }
        }

        private EnemyData[] GetCurrentPool()
        {
            switch (RunManager.Instance.CurrentPhase)
            {
                case RunPhase.Intro: return introEnemyPool;
                case RunPhase.Rising: return risingEnemyPool;
                case RunPhase.Peak: return peakEnemyPool;
                default: return risingEnemyPool; // finale reuses rising-tier trash mobs
            }
        }

        private void SpawnRegularEnemy()
        {
            EnemyData[] pool = GetCurrentPool();
            if (pool == null || pool.Length == 0) return;

            EnemyData chosen = pool[Random.Range(0, pool.Length)];
            SpawnAt(defaultEnemyPrefab, chosen);
        }

        private void SpawnAt(GameObject prefab, EnemyData data)
        {
            if (prefab == null || data == null || player == null) return;

            Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = player.position + (Vector3)offset;

            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
            if (instance.TryGetComponent<EnemyAI>(out var ai))
            {
                ai.Initialize(data, player, playerBaseSpeed);
            }
            _activeEnemies.Add(instance);
        }

        private void HandlePhaseChanged(RunPhase phase)
        {
            _spawnTimer = 0f; // spawn immediately on phase entry for readability
        }

        private void HandleRunEnded()
        {
            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null) Destroy(enemy);
            }
            _activeEnemies.Clear();
            _eliteSpawned = false;
            _bossSpawned = false;
        }
    }
}
