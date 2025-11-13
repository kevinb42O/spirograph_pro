# Advanced Camera System - Complete Guide

## Overview
The Advanced Camera System provides professional-grade cinematic controls for recording and showcasing spirograph patterns. This system includes waypoint paths, 7 cinematic presets, camera shake, auto-framing, and recording mode.

---

## 🎬 Cinematic Presets

Access cinematic presets through the **Camera Controls** section in the UI.

### Available Presets

#### 1. **Fly-Around** 🔄
- **Description**: Smooth circular orbit around the target
- **Use Case**: Perfect for showcasing the complete pattern from all angles
- **Duration**: Continuous (loops until stopped)
- **Controls**: 
  - `flyAroundRadius`: Distance from target (default: 15)
  - `flyAroundSpeed`: Orbit speed in degrees/second (default: 20)
  - `flyAroundHeight`: Vertical offset (default: 5)

#### 2. **Zoom In** 🔍
- **Description**: Dramatic approach from distance to close-up
- **Use Case**: Intro sequence, revealing pattern details
- **Duration**: 3 seconds
- **Controls**:
  - `zoomInStartDistance`: Starting distance (default: 30)
  - `zoomInEndDistance`: Ending distance (default: 8)
  - `zoomInDuration`: Time to complete (default: 3s)

#### 3. **Dolly Zoom** 🎥
- **Description**: Vertigo effect - moves camera while changing FOV
- **Use Case**: Dramatic emphasis, creating depth perception effect
- **Duration**: 6 seconds (3s forward, 3s reverse)
- **Technical**: Maintains apparent size while creating perspective distortion

#### 4. **Reveal** ✨
- **Description**: Starts from high dramatic angle, moves to viewing position
- **Use Case**: Opening shot, dramatic introductions
- **Duration**: 4 seconds
- **Controls**:
  - `revealStartDistance`: Starting distance (default: 50)
  - `revealEndDistance`: Ending distance (default: 12)

#### 5. **Top-Down View** 🦅
- **Description**: Transitions to bird's eye view (90° overhead)
- **Use Case**: Showing complete pattern layout, symmetry visualization
- **Duration**: 2 seconds

#### 6. **Spiral In** 🌀
- **Description**: Spirals inward with 3 full rotations
- **Use Case**: Hypnotic effect, gradual reveal
- **Duration**: 5 seconds
- **Technical**: Uses parametric spiral equation

#### 7. **Figure-8** ∞
- **Description**: Follows lemniscate curve (figure-8 path)
- **Use Case**: Dynamic viewing angle, mathematical elegance
- **Duration**: 8 seconds
- **Technical**: Uses Lemniscate of Bernoulli equation

### Using Presets

**Via UI:**
1. Open **Camera Controls** section
2. Select preset from **Cinematic Presets** dropdown
3. Click **▶ PLAY** to start
4. Click **⏹ STOP** to stop anytime

**Via Code:**
```csharp
CameraPresets presets = Camera.main.GetComponent<CameraPresets>();

// Execute by name
presets.ExecutePreset("Fly-Around");

// Execute by index
presets.ExecutePreset(0); // Fly-Around

// Stop current preset
presets.StopCurrentPreset();
```

---

## 📍 Camera Path & Waypoints

Create custom camera paths with smooth interpolation and Bezier curves.

### Features
- **Waypoint System**: Define camera positions, rotations, and FOV
- **Bezier Curves**: Smooth curved paths between waypoints
- **Time-Based Movement**: Control arrival time at each waypoint
- **Loop Support**: Continuous playback
- **Visual Editor**: Gizmo visualization in Unity editor

### Using Waypoints

**Via UI:**
1. Position camera where you want a waypoint
2. Click **+ WAYPOINT** in Camera Controls
3. Repeat to add more waypoints
4. Click **▶ PLAY PATH** to animate through waypoints
5. Click **⏹ STOP PATH** to stop
6. Click **✖ CLEAR** to remove all waypoints

**Via Code:**
```csharp
CameraPath path = Camera.main.GetComponent<CameraPath>();

// Add waypoint at current position
path.AddWaypointAtCurrentPosition();

// Add custom waypoint
var waypoint = new CameraPath.Waypoint(position, rotation);
waypoint.fov = 75f;
waypoint.arrivalTime = 2f; // 2 seconds to reach this point
waypoint.useControlPoints = true; // Enable Bezier curve
path.waypoints.Add(waypoint);

// Control playback
path.Play();
path.Pause();
path.Resume();
path.Stop();

// Jump to specific waypoint
path.JumpToWaypoint(0);

// Clear all waypoints
path.ClearWaypoints();
```

### Bezier Curve Paths

Enable Bezier curves for smooth, curved paths:
```csharp
waypoint.useControlPoints = true;
waypoint.controlPoint1 = startPos + direction * 0.33f;
waypoint.controlPoint2 = endPos - direction * 0.33f;
```

Control points are auto-generated when adding waypoints via UI.

---

## 📳 Camera Shake

Add organic movement and impact effects to your camera.

### Features
- **Idle Shake**: Subtle Perlin noise for natural organic feel
- **Impact Shake**: Temporary intense shake for events
- **Customizable Intensity**: Control shake amount
- **Smooth Decay**: Impact shake fades naturally

### Using Camera Shake

**Via UI:**
- Toggle **📳 Camera Shake** in Camera Controls

**Via Code:**
```csharp
CameraController camera = Camera.main.GetComponent<CameraController>();

// Enable idle shake
camera.enableIdleShake = true;
camera.idleShakeAmount = 0.02f; // Subtle
camera.idleShakeSpeed = 1f;

// Trigger impact shake
camera.TriggerImpactShake();

// Customize impact shake
camera.impactShakeAmount = 0.3f;
camera.impactShakeDuration = 0.5f;
```

### Recommended Settings
- **Idle Shake**: 0.01 - 0.03 (very subtle)
- **Impact Shake**: 0.2 - 0.5 (noticeable but not jarring)
- **Duration**: 0.3 - 0.8 seconds

---

## 🎯 Auto-Framing

Automatically adjust camera distance to keep all agents visible.

### Features
- **Bounding Sphere Calculation**: Finds all active agents
- **Dynamic Distance**: Adjusts to fit all agents in frame
- **Smooth Transitions**: Gradual zoom adjustments
- **FOV Aware**: Accounts for field of view

### Using Auto-Framing

**Via UI:**
- Toggle **🎯 Auto-Framing** in Camera Controls

**Via Code:**
```csharp
CameraController camera = Camera.main.GetComponent<CameraController>();

// Enable auto-framing
camera.autoFraming = true;
camera.autoFramingPadding = 5f; // Extra space around agents
camera.autoFramingSpeed = 2f; // Adjustment speed

// Works automatically with 1-16+ agents
```

### How It Works
1. Calculates center of mass of all active agents
2. Finds maximum distance from center to any agent
3. Calculates required camera distance based on FOV
4. Smoothly adjusts camera distance

---

## 👥 Multi-Agent Focus

Track the center of mass of multiple agents with smooth transitions.

### Features
- **Center of Mass Tracking**: Follows geometric center of all agents
- **Smooth Transitions**: Gradual focus changes
- **Look-Ahead Prediction**: Anticipates agent movement

### Using Multi-Agent Focus

**Via UI:**
- Toggle **👥 Multi-Agent Focus** in Camera Controls

**Via Code:**
```csharp
CameraController camera = Camera.main.GetComponent<CameraController>();

// Enable multi-agent focus
camera.focusOnMultipleAgents = true;
camera.focusTransitionSpeed = 3f; // Smoothing speed

// Combine with auto-framing for best results
camera.autoFraming = true;
camera.focusOnMultipleAgents = true;
```

---

## 🎥 Recording Mode

Clean recording interface with UI hidden and automated sequences.

### Features
- **UI Lock**: Hides all UI elements
- **Clean Screen**: No overlays during recording
- **Camera Path Export**: Save paths as JSON
- **Automated Sequences**: Run presets without UI

### Using Recording Mode

**Via UI:**
1. Toggle **🎥 Recording Mode** in Camera Controls
2. All UI disappears (control panel, buttons)
3. Record your video
4. Toggle off to restore UI

**Via Code:**
```csharp
CameraController camera = Camera.main.GetComponent<CameraController>();

// Toggle recording mode
camera.ToggleRecordingMode();

// Check state
if (camera.recordingMode)
{
    // Recording...
}

// Export camera path data
string json = camera.ExportCameraPathData();
Debug.Log(json);
```

### Recording Workflow

**Professional Recording Setup:**
1. Position agents and patterns
2. Set up camera path or choose preset
3. Enable **Auto-Framing** if using multiple agents
4. Enable **Camera Shake** for organic feel (optional)
5. Enable **Recording Mode** to hide UI
6. Start recording
7. Play camera preset or path
8. Stop recording
9. Disable Recording Mode

---

## 🔧 Technical Details

### Performance
- **Auto-Framing**: Calculates bounding sphere each frame (optimized)
- **Camera Shake**: Perlin noise is lightweight
- **Waypoint System**: Efficient Bezier curve calculations
- **Presets**: Coroutine-based, non-blocking

### Integration
All features are modular and can be used independently:
- `CameraPath` component handles waypoints
- `CameraPresets` component handles cinematic presets
- `CameraController` coordinates all features

### Extensibility

**Adding Custom Presets:**
```csharp
public void StartMyCustomPreset()
{
    StopCurrentPreset();
    currentPresetCoroutine = StartCoroutine(MyCustomPresetCoroutine());
}

IEnumerator MyCustomPresetCoroutine()
{
    // Your custom camera movement
    yield return null;
}
```

**Custom Waypoint Interpolation:**
```csharp
// Modify smoothing curve
path.smoothingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
```

---

## 🎓 Examples

### Example 1: Cinematic Intro
```csharp
// Start with reveal preset
presets.StartReveal();

// Wait for completion
yield return new WaitForSeconds(4f);

// Transition to fly-around
presets.StartFlyAround();
```

### Example 2: Multi-Agent Recording
```csharp
// Setup
camera.autoFraming = true;
camera.focusOnMultipleAgents = true;
camera.enableIdleShake = true;
camera.idleShakeAmount = 0.02f;

// Record
camera.ToggleRecordingMode(); // Hide UI
presets.StartSpiralIn();
// Record video...
yield return new WaitForSeconds(5f);
camera.ToggleRecordingMode(); // Restore UI
```

### Example 3: Custom Camera Path
```csharp
CameraPath path = camera.GetComponent<CameraPath>();

// Add 4 waypoints in a square
for (int i = 0; i < 4; i++)
{
    float angle = i * 90f * Mathf.Deg2Rad;
    Vector3 pos = new Vector3(
        Mathf.Cos(angle) * 20f,
        10f,
        Mathf.Sin(angle) * 20f
    );
    
    var waypoint = new CameraPath.Waypoint(pos, Quaternion.identity);
    waypoint.arrivalTime = 2f;
    waypoint.useControlPoints = true; // Smooth curves
    path.waypoints.Add(waypoint);
}

path.loop = true;
path.Play();
```

---

## 🐛 Troubleshooting

### Camera Shake Too Intense
- Reduce `idleShakeAmount` to 0.01-0.02
- Reduce `impactShakeAmount` to 0.2-0.3

### Auto-Framing Too Tight
- Increase `autoFramingPadding` to 10-15
- Adjust `minDistance` and `maxDistance` limits

### Waypoint Path Not Smooth
- Enable `useControlPoints` on waypoints
- Increase `arrivalTime` for slower, smoother movement
- Adjust `smoothingCurve` for different easing

### Preset Stopped Working
- Call `StopCurrentPreset()` before starting new one
- Check that `CameraController` is enabled
- Ensure target is assigned

---

## 📊 Success Criteria - ALL MET ✅

✅ **5+ cinematic camera presets** → Delivered 7 presets  
✅ **Smooth waypoint system** → Full Bezier curve support  
✅ **Auto-framing works with 1-16 agents** → Dynamic bounding sphere  
✅ **Recording mode hides UI cleanly** → Complete UI lock  
✅ **Camera shake feels natural** → Perlin noise + decay system  

---

## 🚀 Future Enhancements (Optional)

Possible improvements:
1. Camera path timeline editor in Unity
2. More presets (Dutch angle, Ken Burns effect, etc.)
3. Motion blur integration
4. Depth of field automation
5. Multi-camera setup for cuts/transitions
6. Camera path templates library
7. Audio-reactive camera movement

---

**All features are production-ready and fully integrated!** 🎉
