# AfterAsh

A 2D game for CIS 2991.

## Project Info

| Field | Value |
|---|---|
| Game Name | AfterAsh |
| Type | 2D |
| Render Pipeline | URP |
| Unity Version | [6000.41f1] |
| Studio | Student Project |

## Getting Started

1. Clone this repository
2. Open Unity Hub
3. Click **Add project from disk** and select this folder
4. Select the correct Unity version (6000.41f1) and open
5. Run the following to enable git hooks:
   if you are using github desktop, click repository at the top, then open in command line/prompt, then just copy and paste:
   "git config core.hooksPath hooks"
   

## Project Structure

```
Assets/
  Art/
    3D/         # 2D models, animations, textures
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
