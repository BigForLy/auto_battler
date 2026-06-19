using System;
using UnityEngine;

namespace GlubinnyChertog.Core
{
    /// <summary>
    /// Implements the "soft death" design decision:
    /// 1 free automatic revive per run, 1 additional revive via rewarded video.
    /// Reduces frustration and increases run-completion rate for casual audience.
    /// </summary>
    public class ReviveManager : MonoBehaviour
    {
        public static ReviveManager Instance { get; private set; }

        public event Action OnFreeReviveUsed;
        public event Action OnAdRevivePrompt;   // UI should show rewarded-video offer
        public event Action OnFinalDeath;       // No revives left -> real game over

        private bool _freeReviveAvailable;
        private bool _adReviveAvailable;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void ResetForNewRun()
        {
            _freeReviveAvailable = true;
            _adReviveAvailable = true;
        }

        /// <summary> Call when player HP reaches 0. Returns true if the run continues. </summary>
        public bool TryRevive()
        {
            if (_freeReviveAvailable)
            {
                _freeReviveAvailable = false;
                OnFreeReviveUsed?.Invoke();
                return true;
            }

            if (_adReviveAvailable)
            {
                _adReviveAvailable = false;
                OnAdRevivePrompt?.Invoke();
                // Calling code (UI) should show rewarded video via
                // Monetization/AdsManager, then call ConfirmAdRevive() on success
                // or ConfirmFinalDeath() if the ad is skipped/unavailable.
                return true; // optimistic; UI flow resolves actual outcome
            }

            ConfirmFinalDeath();
            return false;
        }

        public void ConfirmFinalDeath()
        {
            OnFinalDeath?.Invoke();
        }
    }
}
