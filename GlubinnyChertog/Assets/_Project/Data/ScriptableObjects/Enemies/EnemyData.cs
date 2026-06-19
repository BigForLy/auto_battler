using UnityEngine;

namespace GlubinnyChertog.Data
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "GlubinnyChertog/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName = "Gnilostny Sluga";
        public Sprite icon;

        [Header("Stats (casual baseline)")]
        public float maxHp = 15f;
        public float damage = 3f;
        public float moveSpeedMultiplier = 0.6f; // relative to player speed

        [Header("Spawn Tuning")]
        [Tooltip("Base seconds between spawns of this enemy type at phase start")]
        public float baseSpawnInterval = 1.2f;

        [Tooltip("Minimum seconds between spawns at phase peak intensity")]
        public float minSpawnInterval = 0.5f;

        [Header("Rewards")]
        public int sanityContribution = 1; // how much this enemy adds to sanity accrual when killed nearby
        public int resourceDrop = 1;
    }
}
