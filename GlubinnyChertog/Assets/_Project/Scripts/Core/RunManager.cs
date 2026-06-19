using System;
using UnityEngine;

namespace GlubinnyChertog.Core
{
    public enum RunPhase
    {
        Intro,      // 0:00 - 1:00
        Rising,     // 1:00 - 2:30
        Peak,       // 2:30 - 3:30
        Finale      // 3:30 - 4:30 (boss)
    }

    /// <summary>
    /// Drives the timeline of a single run (~270 seconds).
    /// Other systems (SpawnManager, SanityManager, UpgradeManager)
    /// subscribe to OnPhaseChanged / OnRunEnded.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [Header("Run Timing (seconds)")]
        [SerializeField] private float introEnd = 60f;
        [SerializeField] private float risingEnd = 150f;
        [SerializeField] private float peakEnd = 210f;
        [SerializeField] private float finaleEnd = 270f;

        public event Action<RunPhase> OnPhaseChanged;
        public event Action OnRunEnded;
        public event Action OnRunSuccess;

        public float ElapsedTime { get; private set; }
        public RunPhase CurrentPhase { get; private set; } = RunPhase.Intro;
        public bool IsRunActive { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartRun()
        {
            ElapsedTime = 0f;
            CurrentPhase = RunPhase.Intro;
            IsRunActive = true;
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        public void EndRun(bool success)
        {
            IsRunActive = false;
            if (success) OnRunSuccess?.Invoke();
            OnRunEnded?.Invoke();
        }

        private void Update()
        {
            if (!IsRunActive) return;

            ElapsedTime += Time.deltaTime;
            RunPhase newPhase = EvaluatePhase(ElapsedTime);

            if (newPhase != CurrentPhase)
            {
                CurrentPhase = newPhase;
                OnPhaseChanged?.Invoke(CurrentPhase);
            }

            if (ElapsedTime >= finaleEnd)
            {
                // Finale phase duration exceeded without boss kill is handled by BossController;
                // RunManager simply stops advancing phases here.
            }
        }

        private RunPhase EvaluatePhase(float t)
        {
            if (t < introEnd) return RunPhase.Intro;
            if (t < risingEnd) return RunPhase.Rising;
            if (t < peakEnd) return RunPhase.Peak;
            return RunPhase.Finale;
        }

        /// <summary> Normalized progress (0-1) within the current phase. Useful for spawn-rate curves. </summary>
        public float GetPhaseProgress()
        {
            float phaseStart, phaseEnd;
            switch (CurrentPhase)
            {
                case RunPhase.Intro: phaseStart = 0; phaseEnd = introEnd; break;
                case RunPhase.Rising: phaseStart = introEnd; phaseEnd = risingEnd; break;
                case RunPhase.Peak: phaseStart = risingEnd; phaseEnd = peakEnd; break;
                default: phaseStart = peakEnd; phaseEnd = finaleEnd; break;
            }
            return Mathf.Clamp01((ElapsedTime - phaseStart) / (phaseEnd - phaseStart));
        }
    }
}
