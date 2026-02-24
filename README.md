# Squishies

A cute character-collecting 2D puzzle game built with Unity 2022.3 LTS. Draw paths to connect matching squishy characters on a grid, squeezing them together into bigger, more powerful versions.

## Quick Start

### Requirements
- **Unity 2022.3 LTS** (any 2022.3.x patch)
- Windows for editor playtesting (iOS build target supported)

### Setup

1. Open **Unity Hub**
2. Click **Open** > navigate to this project folder (`Squishies/`)
3. Select Unity 2022.3 LTS as the editor version
4. Wait for Unity to import all scripts and packages
5. When prompted to import **TextMeshPro Essentials**, click **Import TMP Essentials**
6. Go to **Squishies > Setup Project (Create Scenes)** in the menu bar
7. This creates the Game, MainMenu, and Workshop scenes with all GameObjects pre-configured
8. Open **Assets/Scenes/Game.unity**
9. Press **Play**

### How to Play

- **Draw paths** by clicking/tapping and dragging across matching colored squishies (3+ in a row)
- **Diagonal connections** are allowed (8-directional adjacency)
- **Release** to pop the chain and score points
- **5-7 match** creates a **Chonky** (2x2 super squishy)
- **8+ match** creates a **Mega Chonk** (3x3) with a special ability
- **Two adjacent Chonkies/Mega Chonks** of any color can be squeezed together for a massive combo

### Game Modes

| Mode | Description |
|------|-------------|
| **Zen** | Endless play, no timer. New squishy types unlock as your score climbs. |
| **Rush** | 90-second countdown. Combos add bonus time. |
| **Puzzle** | Coming soon (infrastructure stubbed). |

## Squishy Characters

| Name | Color | Mega Chonk Ability |
|------|-------|--------------------|
| Bloop | Blue | Radial burst (clears 2-tile radius) |
| Rosie | Pink | Row clear |
| Limbo | Green | Column clear |
| Sunny | Yellow | Color drain (removes random color) |
| Plum | Purple | Shuffle (randomizes board) |
| Tangy | Orange | Happiness burst (all squishies happy) |
| Mochi | White | Wildcard (next match connects any colors) |

## Mood System

- Squishies have moods: **Happy** (1.5x score), **Neutral** (1x), **Sad** (0.75x)
- Matches spread happiness to nearby squishies
- Squishies unmatched for 8+ turns become sad
- Strategy: spread the joy across the board, don't just focus on one corner

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # GameManager, GridManager, MatchEngine, ComboSystem, ScoreManager
│   ├── Squishies/      # Squishy, SquishyData, MoodSystem, Abilities, Animations
│   ├── Input/          # InputHandler, PathDrawer, GridInputMapper
│   ├── VFX/            # JuiceManager, ParticleManager, CameraEffects
│   ├── UI/             # HUDController, MainMenuController, GameOverPanel, UIAnimations
│   ├── Audio/          # AudioManager (procedural placeholder sounds)
│   └── Utility/        # ObjectPool, SpriteGenerator
├── Editor/             # ProjectSetup (scene/asset generator)
└── Scenes/             # Game, MainMenu, Workshop (created by Setup)
```

## Technical Notes

- **No external dependencies** beyond Unity built-in packages and TextMeshPro
- **Placeholder art**: colored circles with dot-face sprites generated procedurally at runtime
- **Placeholder audio**: synthesized pop/merge/combo sounds generated at runtime
- All squishies are **object-pooled** for 60fps performance
- Input works with both **mouse** (Windows) and **touch** (iOS)
- Portrait orientation (9:16 aspect ratio)
- Personal bests saved via PlayerPrefs
