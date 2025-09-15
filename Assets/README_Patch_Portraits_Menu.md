# Patch: Portraits (BatPony added, Changeling removed) + Character & Pause Buttons fixed

## What changed
- **No Changeling anywhere.** All code paths are scrubbed of "Changeling".
- **BatPony supported.** Folders: `Assets/Resources/Portraits/BatPony/Female/portrait.png` and `.../Male/portrait.png`.
- **Character button works** via `MLPGameUI` — uses `CharacterSheetController` if present, otherwise toggles the UXML container.
- **Menu button opens Pause** via `PauseMenuController` with `Time.timeScale` handling.

## Drop-in
Copy the `Assets/Project/Scripts/UI/` files into your project (replace existing).

## Expected UXML Ids (any of these will work)
- Character button: `btnCharacter`, `button-character`, `characterButton`, `btn-char`, `character` (or class `character-button`).
- Menu/Pause button: `btnMenu`, `menuButton`, `btn-menu`, `pause`, `btnPause` (or class `menu-button`/`pause-button`).
- Character sheet container: `character-sheet`, `character-sheet-container`, `characterSheet`, `character-sheet-modal`.
- Pause menu container: `pause-menu`, `pauseMenu`, `PauseMenuRoot`, `pause-modal`.

## Portraits folder layout
```
Assets/Resources/Portraits/
  EarthPony/Female/portrait.png
  EarthPony/Male/portrait.png
  Unicorn/Female/portrait.png
  Unicorn/Male/portrait.png
  Pegasus/Female/portrait.png
  Pegasus/Male/portrait.png
  Griffon/Female/portrait.png
  Griffon/Male/portrait.png
  Dragon/Female/portrait.png
  Dragon/Male/portrait.png
  Human/Female/portrait.png
  Human/Male/portrait.png
  BatPony/Female/portrait.png
  BatPony/Male/portrait.png
  Silhouettes/[optional race silhouettes].png
  Default_Silhouette.png
```

> If a portrait is missing, a race silhouette (or `Default_Silhouette`) is used.

## Notes
- Unity 6.2 APIs: uses `FindFirstObjectByType`/`FindAnyObjectByType` and UI Toolkit events.
- If your IDs differ, either rename in UXML or add your ids to the arrays in the scripts.
- This patch **removes any reference to "Changeling"** from the portrait loading logic.
