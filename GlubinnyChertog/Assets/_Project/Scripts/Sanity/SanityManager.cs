using System;
using UnityEngine;
using GlubinnyChertog.Core;

namespace GlubinnyChertog.Sanity
{
    /// <summary>
    /// Casual-tuned sanity meter.
    /// 0-50%: no effect. 50-80%: cosmetic only. 80-100%: guaranteed "Mad Fury" buff
    /// (never a debuff) — reframes the mechanic as a reward rather than a risk,
    /// per design decision for casual audience.
    /// </summary>
    public class SanityManager : MonoBehaviour
    {
        public static SanityManager Instance { get; private set; }

        [Header("Sanity Curve")]
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float cosmeticThreshold = 50f;
        [SerializeField] private float madFuryThreshold = 80f;

        [Header("Mad Fury Buff")]
        [SerializeField] private float madFuryDamageMultiplier = 1.5f;
        [SerializeField] private float madFuryAttackSpeedMultiplier = 1.3f;
        [SerializeField] private float madFuryDuration = 12f;

        public event Action<float> OnSanityChanged;     // normalized 0-1
        public event Action OnCosmeticThresholdReached;
        public event Action OnMadFuryTriggered;

        public float CurrentSanity { get; private set; }
        private bool _madFuryActiveOrUsed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            if (RunManager.Instance != null)
                RunManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            if (RunManager.Instance != null)
                RunManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        public void ResetForNewRun()
        {
            CurrentSanity = 0f;
            _madFuryActiveOrUsed = false;
            OnSanityChanged?.Invoke(0f);
        }

        /// <summary> Call from spawn/combat systems to accumulate sanity over time. </summary>
        public void AddSanity(float amount)
        {
            float previous = CurrentSanity;
            CurrentSanity = Mathf.Clamp(CurrentSanity + amount, 0f, maxSanity);
            OnSanityChanged?.Invoke(CurrentSanity / maxSanity);

            if (previous < cosmeticThreshold && CurrentSanity >= cosmeticThreshold)
                OnCosmeticThresholdReached?.Invoke();

            if (!_madFuryActiveOrUsed && CurrentSanity >= madFuryThreshold)
            {
                _madFuryActiveOrUsed = true;
                TriggerMadFury();
            }
        }

        private void TriggerMadFury()
        {
            OnMadFuryTriggered?.Invoke();
            // PlayerCombat should subscribe to OnMadFuryTriggered and apply:
            // damageMultiplier *= madFuryDamageMultiplier
            // attackSpeedMultiplier *= madFuryAttackSpeedMultiplier
            // for madFuryDuration seconds, then revert.
        }

        private void HandlePhaseChanged(RunPhase phase)
        {
            // Sanity accrual rate scales with phase; SpawnManager or a dedicated
            // SanityAccrualCurve ScriptableObject can drive AddSanity() calls
            // based on this phase to match the design doc's pacing table.
        }

        public float GetMadFuryDamageMultiplier() => madFuryDamageMultiplier;
        public float GetMadFuryAttackSpeedMultiplier() => madFuryAttackSpeedMultiplier;
        public float GetMadFuryDuration() => madFuryDuration;
    }
}
