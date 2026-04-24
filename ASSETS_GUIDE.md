# Assets Folder Guide

This guide defines how files and folders should be organized under `Assets`.

## Top-Level Rules

- `Assets/Game` contains project-owned content.
- `Assets/Plugins` is only for real plugins, native libraries, and assets that expect to live in `Plugins`.
- `Assets/ThirdParty` is mostly for samples or demos that come from Unity asset packages imported via `.unitypackage`.
- Do not place new loose files directly under `Assets`.
- If a folder or file does not clearly belong somewhere, stop and choose a proper home before committing it.

## Current Target Structure

```text
Assets
  Game
    Scenes
      Gameplay
      Prototype
      Lighting
    Content
      Pullman
        Prefabs
        Audio
          Music
          SFX
        Art
          Models
          Materials
          Textures
          Animations
        UI
      Shared
        Art
        Audio
        Prefabs
        Shaders
    Scripts
      Features
      Systems
      Shared
    Editor
    Runtime
      Resources
    Settings
  Plugins
  ThirdParty
    Samples
    ImportedUnityPackages
```

## What Goes Where

### `Assets/Game`

Use this for all project-owned assets.

- `Scenes`: game scenes, prototype scenes, and lighting-related assets.
- `Content`: prefabs, models, materials, textures, audio, animations, and UI assets.
- `Scripts`: temporary home for project code until custom packages are introduced.
- `Editor`: editor-only tooling for the project.
- `Runtime/Resources`: only assets that must be loaded through `Resources.Load`.
- `Settings`: project-owned settings assets such as input, rendering, or configuration assets.

### `Assets/Plugins`

Use this only for plugin-style content.

- Native libraries.
- Third-party runtime/editor plugins that rely on Unity plugin import settings.
- Assets that explicitly expect the `Plugins` location.

Do not put normal game prefabs, art, scenes, or gameplay scripts here.

### `Assets/ThirdParty`

Use this mostly for content imported from `.unitypackage` files when the imported package includes:

- sample scenes
- demo content
- reference assets
- temporary evaluation content

This folder is not meant to absorb every external dependency.

## Package Manager Note

Unity assets that are brought in through Package Manager do not need to be moved into `Assets/ThirdParty` just because they are third-party.

- If something is managed through Package Manager, leave it in its managed location.
- `Assets/ThirdParty` is mainly for imported `.unitypackage` content, especially samples and demos.
- It is acceptable for some Unity/Asset Store content to exist outside `Assets/ThirdParty` when that content is package-managed.

## File and Naming Rules

- Use PascalCase for folder names and project-owned asset names.
- Avoid spaces in new file names unless a third-party import already uses them.
- Do not commit files named like `New Material`, `New Scene`, or similar placeholders.
- Group assets by feature first, then by asset type.
- Keep project content separate from sample or vendor content.

## Scene Rules

- Put production scenes in `Assets/Game/Scenes/Gameplay`.
- Put experiments and throwaway work in `Assets/Game/Scenes/Prototype`.
- Keep lighting assets with `Scenes/Lighting` when they are scene-level support assets.

## Script Rules

Until custom packages are introduced, keep project scripts under `Assets/Game/Scripts`.

- `Features`: feature-specific scripts.
- `Systems`: reusable gameplay systems.
- `Shared`: common helpers and cross-feature utilities.
- `Assets/Game/Editor`: editor-only code.

When custom packages are added later, runtime/editor code should gradually move out of `Assets/Game/Scripts` and into those packages.

## Do Not Do This

- Do not add new root-level files directly under `Assets`.
- Do not mix scripts, audio, materials, and prefabs in one flat feature folder.
- Do not move plugin folders into `ThirdParty` without checking import settings.
- Do not keep demo/sample scenes next to production scenes.

## Migration Direction

When reorganizing existing content:

- move project-owned scenes and assets into `Assets/Game`
- keep real plugins in `Assets/Plugins`
- move imported `.unitypackage` samples and demos into `Assets/ThirdParty`
- keep package-managed dependencies in their package-managed location