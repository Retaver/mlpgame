# MLP Game HUD System

This HUD system is inspired by TiTS (The Interactive Text-based Simulator) but adapted for a family-friendly MLP (My Little Pony) themed Unity game.

## Features

### 🎨 **HUD Layout**
- **Left Panel**: Character portrait, location info, minimap, system buttons
- **Center Panel**: Main game display area
- **Right Panel**: Character stats, status effects, quick actions

### 👤 **Character Portrait**
- Displays the current character's portrait
- Placeholder system ready for integration with portrait assets

### 🗺️ **Minimap**
- Shows current location and nearby points of interest
- Color-coded location types (shops, farms, libraries, etc.)
- Player position indicator
- Location discovery system

### 💾 **Save/Load System**
- Quick save functionality
- Load game functionality
- JSON-based save files
- Automatic notifications for save/load status

### 🎛️ **System Buttons**
- Save Game (💾)
- Load Game (📁)
- Main Menu (☰)
- Inventory (🎒)
- Character Sheet (👤)
- World Map (🗺️)

## Setup Instructions

### 1. Add HUD to Scene
1. Open your Game scene
2. Create a new GameObject called "MLPGameHUD"
3. Add the `MLPGameHUDBootstrap` component to it
4. Configure the HUD settings as needed

### 2. UI Assets Setup
1. Copy `MLPGameHUD.uxml` to `Assets/Resources/MLPGameHUD.uxml`
2. Copy `MLPGameHUD.uss` to `Assets/Resources/MLPGameHUD.uss`
3. Ensure you have a PanelSettings asset for UI Toolkit

### 3. Integration with Existing Systems
The HUD automatically integrates with:
- `CharacterSystem` - for character data
- `InventorySystem` - for inventory management
- `MapSystem` - for location and minimap data
- `GameEventSystem` - for event notifications

## Technical Details

### Architecture
- **MLPGameHUD.cs**: Main HUD controller
- **SimpleMinimap.cs**: Minimap component
- **SaveLoadSystem.cs**: Save/load functionality
- **MLPGameHUDBootstrap.cs**: Scene setup helper

### UI Toolkit Integration
- Uses Unity UI Toolkit (UIElements)
- Responsive design with USS styling
- Event-driven updates
- Notification system for user feedback

### Save System
- JSON serialization
- Stores character data, inventory, and map progress
- Located in `Application.persistentDataPath/Saves/`
- Quick save and manual save options

## Customization

### Styling
Edit `MLPGameHUD.uss` to customize:
- Colors and themes
- Button styles
- Layout spacing
- Responsive breakpoints

### Layout
Modify `MLPGameHUD.uxml` to:
- Add new UI elements
- Rearrange panels
- Change element hierarchy

### Functionality
Extend `MLPGameHUD.cs` to:
- Add new system buttons
- Integrate additional game systems
- Customize notification messages
- Add new HUD panels

## TiTS Inspiration

This HUD is inspired by TiTS's layout but adapted for family-friendly content:
- **TiTS Left Sidebar** → MLP Left Panel (portrait, minimap, buttons)
- **TiTS Right Sidebar** → MLP Right Panel (stats, effects, actions)
- **TiTS Save System** → MLP SaveLoadSystem (JSON-based persistence)
- **TiTS Minimap** → MLP SimpleMinimap (location-based navigation)

## Future Enhancements

- [ ] Portrait asset integration
- [ ] Full map system integration
- [ ] Status effect icons
- [ ] Inventory quick-access
- [ ] Settings panel
- [ ] Achievement notifications

## Troubleshooting

### HUD Not Appearing
1. Check that UIDocument component is attached
2. Verify UXML and USS files are in Resources folder
3. Check console for initialization errors

### Save/Load Not Working
1. Check file permissions in persistent data path
2. Verify CharacterSystem and InventorySystem are present
3. Check console for serialization errors

### Minimap Not Updating
1. Ensure MapSystem is implemented
2. Check for location data updates
3. Verify minimap refresh calls

---

*This HUD system maintains the technical excellence of TiTS while ensuring all content is appropriate for a family-friendly MLP game.*