# Orbit Presets Guide

## Overview
New orbit preset buttons have been added to provide quick camera angles for viewing the spirograph structure from different perspectives.

## Performance Improvements

### Cached Structure Center
The structure center calculation is now **cached and updated periodically** instead of being calculated every frame:

```csharp
// Updates every 0.5 seconds by default (configurable)
if (Time.time - lastStructureCenterUpdate > structureCenterUpdateInterval)
{
    cachedStructureCenter = CalculateStructureCenter();
    lastStructureCenterUpdate = Time.time;
}
```

**Performance Benefits:**
- Reduces expensive `FindObjectsOfType<TrailRenderer>()` calls from 60+ per second to 2 per second
- Eliminates repeated array allocations for trail positions
- Smoother frame rate during orbit mode
- Configurable update interval via inspector: `structureCenterUpdateInterval` (default: 0.5s)

## Orbit Presets

### Available Presets

#### 🔹 Top View (⬇)
- **Elevation:** 89° (looking straight down)
- **Distance:** 25 units
- **Orbit Speed:** 5°/sec
- **Use Case:** Perfect for seeing the complete pattern layout from above

#### 🔹 Side View (↔)
- **Elevation:** 0° (level with structure)
- **Distance:** 20 units
- **Orbit Speed:** 8°/sec
- **Use Case:** Great for viewing the structure's profile and depth

#### 🔹 Front View (→)
- **Elevation:** 10° (slight upward angle)
- **Distance:** 15 units
- **Orbit Speed:** 0°/sec (static)
- **Use Case:** Static front-facing view, ideal for presentations

#### 🔹 Isometric View (◇)
- **Elevation:** 35.264° (classic isometric angle)
- **Distance:** 30 units
- **Orbit Speed:** 3°/sec
- **Use Case:** Technical/architectural view showing depth and dimension

### How to Use

1. **Enable Orbit Mode** - Click "🎬 AUTO ORBIT" button (or preset buttons will auto-enable it)
2. **Select a Preset** - Click any of the four preset buttons
3. **Camera smoothly transitions** to the preset view angle
4. **Adjust as needed** - Scroll to zoom, or let it auto-orbit

### UI Integration

The preset buttons are automatically created by `SpirographUIManager` in the Camera Controls section:

```
📷 CAMERA CONTROLS
  ✈ Free Fly    ◎ Follow
  🎬 AUTO ORBIT
  
  Orbit Presets:
  [⬇ Top] [↔ Side] [→ Front] [◇ Iso]
```

### Technical Details

#### Preset Implementation
```csharp
public enum OrbitPreset { Free, TopView, SideView, FrontView, IsometricView }

void SetOrbitPreset(OrbitPreset preset)
{
    if (!isOrbiting)
        ToggleAutoOrbit();  // Enable orbit mode
    
    currentOrbitPreset = preset;
    
    // Set elevation, distance, and speed based on preset
    // Smooth transition via orbitTransitionProgress
}
```

#### Button Connection
Buttons are automatically connected in `ConnectToUI()`:
```csharp
topViewButton = GameObject.Find("TopViewButton")?.GetComponent<Button>();
// ... find other buttons
if (topViewButton != null)
    topViewButton.onClick.AddListener(() => SetOrbitPreset(OrbitPreset.TopView));
```

## Customization

### Adjusting Preset Values
Edit the `SetOrbitPreset()` method in `CameraController.cs`:

```csharp
case OrbitPreset.TopView:
    orbitElevation = 89f;    // Angle
    orbitDistance = 25f;     // Distance
    orbitSpeed = 5f;         // Rotation speed
    break;
```

### Adjusting Cache Update Rate
In Unity Inspector, adjust `Structure Center Update Interval`:
- **Lower values** (0.1-0.2s) = More responsive to changes, slightly higher CPU
- **Higher values** (1.0-2.0s) = Better performance, less responsive

### Adding New Presets
1. Add to enum: `OrbitPreset.MyCustomView`
2. Create button in `SpirographUIManager.cs`
3. Add case in `SetOrbitPreset()` switch statement
4. Connect button in `ConnectToUI()`

## Compatibility

✅ **Works with UI Auto-Generation** - All buttons are created automatically
✅ **Backward Compatible** - Existing projects work without changes
✅ **No Breaking Changes** - All original functionality preserved

## Debug Output

Enable debug logging to see preset activation:
```
Auto Orbit: ENABLED - Orbiting complete structure center
Orbit Preset: TOP VIEW
```

---

**Tip:** Combine presets with the scroll wheel to zoom in/out while maintaining the preset angle!
