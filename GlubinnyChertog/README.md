# Глубинный Чертог (Glubinny Chertog)

Auto-shooter / Bullet Heaven + Roguelike + Base & Sortie structure, Android, Unity 6.3 LTS.

Dark fantasy in the spirit of Darkest Dungeon: a cursed estate's catacombs,
a sanity meter, and a fortified surface camp (the Chertog) you rebuild between
sorties.

**Core loop — Base & Sortie (inspired by Alien Invasion: RPG Idle Space):**
- **Chertog (base)** — persistent hub: heal, upgrade gear, build/upgrade
  structures, NPC quests, resource exchange. Always safe.
- **Sortie (вылазка)** — a single 4–5 min run into the depths. The deeper
  you go, the tougher the enemies and the richer the loot.
- **Extraction risk** — loot gathered during a sortie is only banked if you
  make it back to the Chertog alive. Die before extracting and unbanked
  loot is lost — this is the core risk/reward decision every run ("push
  deeper or extract now").
- **Revive** (free + rewarded-video) now reads narratively as "a desperate
  chance to drag your loot back to the surface."
- *(Post-MVP, optional)* idle/passive layer: send recovered allies/servants
  to auto-farm previously cleared zones while you're on another sortie.

## Tech Stack

- **Engine:** Unity 6.3 LTS (or latest stable LTS at time of setup)
- **Language:** C#
- **Rendering:** Universal Render Pipeline (URP), 2D
- **Monetization:** Unity Ads / LevelPlay mediation, Unity IAP, Google Play Billing
- **Analytics:** Firebase Analytics, GameAnalytics
- **Backend (post-MVP):** PlayFab or Firebase Realtime Database
- **Version Control:** Git + Git LFS for binary assets

## Run Design (current balance target)

- Run length: **4–5 minutes** (270s reference) per sortie
- Phases: Intro (0–60s) → Rising (60–150s) → Peak (150–210s) → Finale/Boss (210–270s)
- Upgrade picks: ~5 per run (every 45–60s)
- Sanity meter: 0–50% no effect, 50–80% cosmetic only, 80–100% guaranteed
  "Mad Fury" buff (no debuffs — casual-tuned)
- Soft death: 1 free auto-revive + 1 rewarded-video revive per sortie —
  framed as a last chance to extract with loot intact
- **Extraction**: loot is provisional during the sortie; only banked to the
  Chertog on successful return (boss kill / timer end / manual extract).
  Death before extraction forfeits unbanked loot.
- Target run completion rate: 75–80%
- Target boss death rate: 15–20%

## Project Structure

```
Assets/_Project/
  Scripts/
    Core/          - RunManager, ReviveManager, GameStateManager
    Player/        - PlayerController, PlayerCombat, PlayerStats
    Enemies/       - EnemyAI, EnemySpawner, BossController
    Weapons/       - Weapon base classes, projectile behavior
    Sanity/        - SanityManager (Mad Fury buff system)
    Camp/          - Camp progression, building upgrades, between-run hub
    UI/            - HUD, upgrade-choice screen, camp UI, results screen
    Monetization/  - AdsManager, IAPManager, BattlePassManager
    Analytics/     - AnalyticsManager (Firebase + GameAnalytics wrappers)
    SaveSystem/    - Local save/load (JSON, pre-backend MVP)
    Utils/         - ObjectPool<T> and shared helpers
  Prefabs/         - Player, Enemies, UI, VFX
  Art/             - Sprites, Animations
  Audio/           - Music, SFX
  Data/
    ScriptableObjects/
      Weapons/     - WeaponData definitions
      Enemies/     - EnemyData definitions
      Waves/       - Wave/phase configuration assets
  Scenes/          - MainMenu, Run, Camp
  Settings/        - URP settings, input action assets
```

## Setup Instructions

### 1. Prerequisites
- Unity Hub installed
- Unity 6.3 LTS (or latest stable LTS) installed via Hub
- Git + [Git LFS](https://git-lfs.com/) installed locally (`git lfs install`)
- Android Build Support module installed in Unity Hub (incl. SDK, NDK, OpenJDK)

### 2. Clone and open
```bash
git clone <your-repo-url> GlubinnyChertog
cd GlubinnyChertog
git lfs pull
```
Open the folder as a project from Unity Hub. Unity will regenerate the
`Library/` folder and resolve packages from `Packages/manifest.json`
automatically on first open (may take several minutes).

### 3. Switch platform
`File → Build Settings → Android → Switch Platform`

### 4. Player Settings checklist
- Set Package Name (`com.yourstudio.glubinnychertog`)
- Set Minimum API Level (Android 8.0 / API 26 recommended baseline)
- Set Scripting Backend to IL2CPP, Target Architecture ARM64
- Enable URP in Graphics settings if not auto-applied

## Git Workflow

- `main` — stable, always buildable
- `develop` — integration branch
- `feature/<name>` — individual features, PR into `develop`

Binary assets (sprites, audio, prefabs, scenes, materials) are tracked via
Git LFS — see `.gitattributes`. Run `git lfs install` once per machine before
your first clone/commit.

## Roadmap

See design docs (MVP scope, balance tables) for the 14–16 week MVP plan:
fixed 5 dungeon layouts, 1 playable hero (Crusader), simplified single-stat
sanity meter, 3 camp buildings, basic monetization (rewarded video, single
season Battle Pass, banner/interstitial).
