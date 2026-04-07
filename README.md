# DefaultGameTemplate

A 3D game by Liminal Arcane Studio.

## Project Info

| Field | Value |
|---|---|
| Game Name | DefaultGameTemplate |
| Type | 3D |
| Render Pipeline | URP |
| Unity Version | [check Unity Hub for current LTS] |
| Studio | Liminal Arcane Studio |

## Getting Started

1. Clone this repository
2. Open Unity Hub
3. Click **Add project from disk** and select this folder
4. Select the correct Unity version and open
5. Run the following to enable git hooks:
   ```bash
   git config core.hooksPath hooks
   ```

## Project Structure

```
Assets/
  Art/
    3D/         # 3D models, animations, textures
    2D/         # Sprites, tilemaps
    UI/         # UI graphics, icons, buttons
    Fonts/
  Audio/
    Music/
    SFX/
  Data/
    ScriptableObjects/
  Materials/
  Prefabs/
  Resources/
  Scenes/
    MainMenu/
    Levels/
    Testing/
  Scripts/
    Core/
    Player/
    Enemies/
    Managers/
    UI/
    Utilities/
    Data/
  Settings/     # URP/HDRP render pipeline assets
  Shaders/
  VFX/
  Plugins/
  Editor/
  Tests/
    EditMode/
    PlayMode/
```

## Branch Strategy

- `main` — protected, production-ready
- Feature branches: `feature/description`
- Bug fixes: `fix/description`
- All changes go through Pull Requests

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).
