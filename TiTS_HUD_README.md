# TiTS-Inspired HUD System for Unity 6.2

A comprehensive Heads-Up Display system inspired by TiTS (Trials in Tainted Space) with MLP-themed Twilight styling, optimized for Unity 6.2 and UI Toolkit.

## Features

### 🎯 Core Components

- **TiTS Side Bars**: Animated stat bars positioned on screen edges (Health, Energy, Magic, Friendship, Discord)
- **Enhanced Minimap**: Zoomable minimap with location markers, fog of war, and fast travel
- **Notification System**: Slide-in notifications with auto-dismiss and multiple types
- **Quick Actions Panel**: Hotkey-enabled quick access to common game functions
- **Twilight Theme**: Complete color scheme matching your main menu aesthetic

### 🎨 Visual Features

- Smooth animations with easing curves
- Responsive design for different screen sizes
- Twilight color palette (dark purples, magenta accents)
- Backdrop blur effects and shadows
- Status-based color coding (normal/low/critical)
- Pulse animations for critical states

## Installation

1. **Import Components**: Add all the C# scripts and USS files to your Unity project
2. **Create HUD System**: Attach `TiTSHUDSystem.cs` to a GameObject with a UIDocument
3. **Configure Styling**: Ensure USS files are referenced in your UI Document
4. **Initialize**: The system will auto-initialize on Awake()

## Usage

### Basic Setup

```csharp
using MyGameNamespace.UI;

public class GameController : MonoBehaviour
{
    private TiTSHUDSystem hudSystem;

    void Start()
    {
        // Get reference to HUD system
        hudSystem = FindObjectOfType<TiTSHUDSystem>();

        // Update player stats
        hudSystem.UpdateStat("health", 85f, 100f);
        hudSystem.UpdateStat("magic", 60f, 100f);

        // Update player position
        hudSystem.UpdatePlayerPosition(new Vector2(10f, 15f), 45f);

        // Show notifications
        hudSystem.ShowNotification("Welcome!", "Your adventure begins!");
    }
}
```

### Component Access

```csharp
// Access individual components
var sideBars = hudSystem.SideBars;
var minimap = hudSystem.Minimap;
var notifications = hudSystem.Notifications;
var quickActions = hudSystem.QuickActions;

// Update side bars
sideBars.UpdateStat("health", 75f, 100f);
sideBars.SetAllStats(80f, 100f, 60f, 100f, 40f, 100f, 90f, 100f, 20f, 100f);

// Control minimap
minimap.DiscoverLocation("Sweet Apple Acres");
minimap.HighlightLocation("Ponyville Town Square", true);
minimap.CenterOnPlayer();

// Show different notification types
notifications.ShowSuccess("Quest Complete!", "You found the hidden treasure!");
notifications.ShowAchievement("Explorer", "Discovered 10 new locations");
notifications.ShowError("Danger!", "Watch out for that manticore!");
```

### Hotkeys

- **I**: Open Inventory
- **C**: Open Character Sheet
- **M**: Open/Center Map
- **Q**: Open Quest Log
- **S**: Open Spell Book
- **Tab**: Quick Item Select
- **T**: Open Social Panel
- **Esc**: Open Settings

## Component Details

### TiTS Side Bars

Animated stat bars that appear on the left and right sides of the screen.

**Features:**
- Smooth value transitions with easing
- Status-based coloring (normal/low/critical)
- Pulse animation for critical values
- Configurable thresholds and colors

**Usage:**
```csharp
// Update individual stat
sideBars.UpdateStat("health", currentHealth, maxHealth);

// Update all stats at once
sideBars.SetAllStats(health, maxHealth, energy, maxEnergy, magic, maxMagic,
                    friendship, maxFriendship, discord, maxDiscord);
```

### Enhanced Minimap

Advanced minimap with TiTS-style features.

**Features:**
- Zoom in/out with mouse wheel
- Pan by dragging
- Location markers with different types
- Fog of war (undiscovered areas)
- Fast travel by clicking locations
- Compass with player rotation

**Usage:**
```csharp
// Update player position
minimap.UpdatePlayerPosition(new Vector2(x, y), rotation);

// Discover and visit locations
minimap.DiscoverLocation("Carousel Boutique");
minimap.VisitLocation("Golden Oak Library");

// Highlight locations
minimap.HighlightLocation("Sweet Apple Acres", true);

// Center on player
minimap.CenterOnPlayer();
```

### Notification System

Slide-in notification system with multiple types.

**Types:**
- `Info`: General information
- `Success`: Positive feedback
- `Warning`: Caution messages
- `Error`: Error messages
- `Achievement`: Achievement unlocks
- `Quest`: Quest updates
- `LevelUp`: Level advancement
- `Item`: Item received

**Usage:**
```csharp
// Basic notification
notifications.ShowNotification("Title", "Message", NotificationType.Info, 3.0f);

// Convenience methods
notifications.ShowSuccess("Quest Complete!", "You saved Ponyville!");
notifications.ShowAchievement("Friendship Master", "Made 10 new friends");
notifications.ShowLevelUp(5, "+5 Health, +3 Magic");
notifications.ShowItemReceived("Magic Wand", 1);
```

### Quick Actions Panel

Hotkey-enabled quick access panel.

**Features:**
- Grid layout with icons and labels
- Hotkey indicators
- Highlight animation on activation
- Customizable actions and callbacks

**Usage:**
```csharp
// Set custom callback
quickActions.SetActionCallback(QuickActionType.Inventory, () => OpenInventory());

// Enable/disable actions
quickActions.SetActionEnabled(QuickActionType.Spells, hasSpells);

// Add custom action
quickActions.AddCustomAction(QuickActionType.Map, "World Map", "🌍",
                           KeyCode.W, () => OpenWorldMap());
```

## Styling

### Color Palette

The system uses a Twilight theme with these colors:
- **Background**: `rgb(10, 8, 12)` - Deep purple-black
- **Panels**: `rgba(22, 18, 24, 0.92)` - Semi-transparent dark purple
- **Borders**: `rgb(108, 92, 110)` - Purple-gray
- **Accent**: `rgb(244, 162, 232)` - Bright magenta-pink
- **Text**: `rgb(180, 170, 190)` - Light purple-gray

### USS Files

- `TiTSSideBars.uss`: Side bar styling
- `TiTSMinimap.uss`: Minimap styling
- `TiTSNotifications.uss`: Notification styling
- `TiTSQuickActions.uss`: Quick actions styling

### Responsive Design

All components include responsive design for different screen sizes:
- Desktop: Full feature set
- Tablet: Adjusted layouts
- Mobile: Compact layouts with single-column designs

## Integration

### With Existing UI

The system is designed to work alongside your existing UI:

```csharp
// In your existing HUD controller
public class MLPGameHUD : MonoBehaviour
{
    private TiTSHUDSystem titsHUD;

    void Start()
    {
        titsHUD = FindObjectOfType<TiTSHUDSystem>();

        // Sync with existing character system
        UpdateCharacterStats();
    }

    void UpdateCharacterStats()
    {
        if (titsHUD != null && character != null)
        {
            titsHUD.UpdateStat("health", character.Health, character.MaxHealth);
            titsHUD.UpdateStat("magic", character.Magic, character.MaxMagic);
            titsHUD.UpdatePlayerPosition(character.Position, character.Rotation);
        }
    }
}
```

### Event System Integration

```csharp
// Subscribe to game events
public class GameEventHandler : MonoBehaviour
{
    private TiTSHUDSystem hud;

    void Start()
    {
        hud = FindObjectOfType<TiTSHUDSystem>();

        // Subscribe to events
        GameEventSystem.OnLocationDiscovered += OnLocationDiscovered;
        GameEventSystem.OnQuestCompleted += OnQuestCompleted;
        GameEventSystem.OnItemReceived += OnItemReceived;
    }

    void OnLocationDiscovered(string locationName)
    {
        hud.DiscoverLocation(locationName);
        hud.ShowNotification("New Location!", $"Discovered {locationName}",
                           TiTSNotifications.NotificationType.Info);
    }

    void OnQuestCompleted(string questName)
    {
        hud.ShowNotification("Quest Complete!", questName,
                           TiTSNotifications.NotificationType.Success);
    }

    void OnItemReceived(string itemName, int quantity)
    {
        hud.ShowItemReceived(itemName, quantity);
    }
}
```

## Performance

### Optimization Features

- Object pooling for notifications
- Efficient animation curves
- Minimal draw calls with UI Toolkit
- Backdrop blur effects (Unity 6.2 feature)
- Pointer events management to avoid input blocking

### Memory Management

- Automatic cleanup of completed animations
- Notification queue with size limits
- Component pooling for frequently created elements

## Compatibility

- **Unity Version**: 6.2+
- **UI System**: UI Toolkit (UIElements)
- **Rendering**: Compatible with URP and Built-in RP
- **Platform**: Windows, macOS, Linux, WebGL, Mobile

## Troubleshooting

### Common Issues

1. **Components not appearing**: Ensure USS files are properly referenced in UIDocument
2. **Colors not matching**: Check that USS variables are correctly defined
3. **Animations not working**: Verify Unity version supports required features
4. **Input not responding**: Check that pointer events are properly configured

### Debug Mode

Enable verbose logging in components for debugging:

```csharp
// In TiTSHUDSystem inspector
verboseLogging = true;
```

## Future Enhancements

- **Radial Menu**: Alternative to grid quick actions
- **Status Effects Panel**: Visual status effect display
- **Mini Quest Tracker**: Compact quest progress display
- **Inventory Quick Access**: Drag-and-drop from side bars
- **Customizable Layout**: User-configurable HUD positions

## License

This HUD system is provided as-is for use in MLP-themed Unity projects. Feel free to modify and extend as needed for your game development.