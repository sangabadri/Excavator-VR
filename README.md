# Excavator

Unity VR project built with **Unity 6000.4.0f1**.

## Requirements

- [Unity 6000.4.0f1](https://unity.com/releases/editor/whats-new/6000.4.0)
- Git with [Git LFS](https://git-lfs.com/) installed

## Getting started

1. Clone the repository:
   ```powershell
   git clone https://github.com/sangabadri/Excavator-VR.git
   cd Excavator
   git lfs pull
   ```
2. Open the project folder in Unity Hub.
3. Open a scene from `Assets/Scenes/`.

## Project structure

| Folder | Purpose |
|--------|---------|
| `Assets/Scenes/` | Game scenes |
| `Assets/Scripts/` | Project scripts |
| `Assets/Prefabs/` | Prefabs and UI |
| `Packages/` | Unity package manifest |
| `ProjectSettings/` | Unity project settings |

## Git LFS

Binary assets (textures, models, audio, etc.) are tracked with Git LFS. Make sure LFS is installed before cloning:

```powershell
git lfs install
```

## Builds

Build output is ignored by git. Local Windows builds are written to `Windows_build/`.
