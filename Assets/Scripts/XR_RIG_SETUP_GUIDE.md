# XR Rig Setup Guide

Follow these steps in Unity Editor to set up the VR system.

## Step 1: Create XR Origin (Action-based)

1. In Unity Editor, open the **HomeArea** scene (or SampleScene for now)
2. Right-click in Hierarchy → **XR → XR Origin (Action-based)**
3. This creates:
   ```
   XR Origin (Action-based)
   ├── Camera Offset
   │   └── Main Camera
   ├── Left Controller
   │   ├── XR Controller (Action-based)
   │   └── XR Ray Interactor
   └── Right Controller
       ├── XR Controller (Action-based)
       └── XR Ray Interactor
   ```

## Step 2: Add Locomotion System

1. Right-click on **XR Origin** → **XR → Locomotion System (Action-based)**
2. This adds the locomotion manager

## Step 3: Add Continuous Movement

1. Right-click on **XR Origin** → **XR → Continuous Move Provider (Action-based)**
2. In Inspector, configure:
   - **System**: Drag the Locomotion System component
   - **Move Speed**: 2.5
   - **Enable Strafe**: ✓ (checked)
   - **Use Gravity**: ✓ (checked)
   - **Gravity Application Mode**: Attempting Move
   - **Left Hand Move Action**: XRI LeftHand/Move
   - **Right Hand Move Action**: XRI RightHand/Move

## Step 4: Add Snap Turn

1. Right-click on **XR Origin** → **XR → Snap Turn Provider (Action-based)**
2. In Inspector, configure:
   - **System**: Drag the Locomotion System component
   - **Turn Amount**: 45 (degrees)
   - **Left Hand Turn Action**: XRI LeftHand/Turn
   - **Right Hand Turn Action**: XRI RightHand/Turn

## Step 5: Configure Character Controller

1. Select **XR Origin** in Hierarchy
2. In Inspector, add **Character Controller** component
3. Configure:
   - **Center**: Y = 1
   - **Radius**: 0.3
   - **Height**: 1.8
   - **Skin Width**: 0.08

## Step 6: Tag the Camera

1. Select **Main Camera** (under XR Origin → Camera Offset)
2. In Inspector, set **Tag** to "MainCamera"
3. OR: Create a new tag "Player" and apply it to XR Origin

## Step 7: Test VR (Optional - Desktop Simulator)

1. In Project window, find **XR Device Simulator** prefab
   - Location: Packages → XR Interaction Toolkit → Runtime → XR Device Simulator
2. Drag it into your scene
3. This lets you test VR with keyboard/mouse:
   - **WASD**: Move camera
   - **Hold Right Mouse**: Look around
   - **Q/E**: Rotate
   - **Space/C**: Up/down

## Step 8: Configure Input Actions

The XR Interaction Toolkit uses the Input System. The actions should be pre-configured, but verify:

1. **Window → Asset Management → Package Manager**
2. Find **XR Interaction Toolkit**
3. In the package details, look for **Samples**
4. Import **"Starter Assets"** if not already imported
5. This includes pre-configured input actions

## Current Scene Hierarchy Should Look Like:

```
HomeArea (Scene)
├── Directional Light
├── GameManager (with GameManager.cs script)
├── XR Origin (Action-based)
│   ├── Character Controller
│   ├── Locomotion System
│   ├── Continuous Move Provider
│   ├── Snap Turn Provider
│   ├── Camera Offset
│   │   └── Main Camera (Tag: MainCamera or Player)
│   ├── Left Controller
│   └── Right Controller
└── [Your game objects will go here]
```

## Build Settings for Quest 3

1. **File → Build Settings**
2. **Platform**: Android
3. Click **Switch Platform** (if not already Android)
4. **Texture Compression**: ASTC
5. **Run Device**: Your Quest 3 (when connected)

## Project Settings for Quest 3

1. **Edit → Project Settings**
2. **XR Plug-in Management**:
   - Switch to **Android** tab (icon at top)
   - ✓ Check **Oculus**
3. **Player** (Android tab):
   - **Company Name**: Your name
   - **Product Name**: VR Dungeon Crawler
   - **Minimum API Level**: Android 10.0 (API level 29)
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARM64 ✓

## Testing

Once set up:
1. **Play in Editor**: Should work with XR Device Simulator
2. **Build to Quest 3**: File → Build Settings → Build And Run

The locomotion should work immediately - no custom code needed!
