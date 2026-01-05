# Next Steps - Unity VR Dungeon Crawler Setup

## ✅ What's Been Done

1. ✅ Unity project created with URP
2. ✅ XR packages installed (XR Interaction Toolkit, Oculus XR Plugin)
3. ✅ Quest 3 target enabled in Oculus settings
4. ✅ Folder structure created (Scripts/Core, Scripts/Entities, etc.)
5. ✅ Core scripts created:
   - `GameManager.cs` - Game state and scene management
   - `Portal.cs` - Portal entity with trigger detection
   - `HomeAreaBuilder.cs` - Procedural room builder
6. ✅ Setup guide created: `Assets/Scripts/XR_RIG_SETUP_GUIDE.md`

## 🎯 What You Need to Do in Unity Editor

### Step 1: Use the Easy Automated Setup

**NEW: Much easier approach!**

Open `Assets/Scripts/EASY_SETUP.md` and follow the simple steps:
1. Make sure XR Origin exists (you already have it!)
2. Add **Character Controller** component to XR Origin
3. Add **XRRigAutoSetup** script to XR Origin
4. Import **Starter Assets** from XR Interaction Toolkit package
5. Press Play - locomotion auto-configures!

The script does all the complex setup automatically.

**Alternatively:** If you prefer manual setup, follow `Assets/Scripts/XR_RIG_SETUP_GUIDE.md`

### Step 2: Create HomeArea Scene

1. **File → New Scene**
2. **File → Save As** → Name it `HomeArea`
3. Save location: `Assets/Scenes/HomeArea.unity`

### Step 3: Add Core Objects to HomeArea Scene

In the HomeArea scene, create this hierarchy:

```
HomeArea (Scene)
├── Directional Light (should exist by default)
├── GameManager (Empty GameObject)
│   └── Add Component: GameManager script
├── HomeAreaBuilder (Empty GameObject)
│   └── Add Component: HomeAreaBuilder script
└── XR Origin (Action-based) [Follow XR_RIG_SETUP_GUIDE.md]
    └── [All locomotion components as per guide]
```

**To create GameManager:**
1. Right-click in Hierarchy → Create Empty
2. Name it "GameManager"
3. In Inspector → Add Component → Search "GameManager" → Add it
4. Configure:
   - Home Scene Name: "HomeArea"
   - Dungeon Scene Name: "Dungeon"

**To create HomeAreaBuilder:**
1. Right-click in Hierarchy → Create Empty
2. Name it "HomeAreaBuilder"
3. In Inspector → Add Component → Search "HomeAreaBuilder" → Add it
4. Right-click on the component → **"Build Home Area Room"**
   - This will procedurally create the 10x10 room!

### Step 4: Position Player Spawn

1. In Hierarchy, select **XR Origin**
2. In Inspector, set Transform Position to: `(0, 0, -2)`
3. Set Rotation to: `(0, 0, 0)`

This places the player in the center-front of the room, facing the portal.

### Step 5: Test in Editor (Desktop Simulator)

1. Make sure you followed the XR Device Simulator setup in the guide
2. Click **Play** button
3. Controls (if simulator is working):
   - **WASD**: Move camera view
   - **Right Mouse + Drag**: Look around
   - **Q/E**: Simulate snap turning
   - **Shift + WASD**: Simulate thumbstick movement

You should see the home area room and be able to move around!

### Step 6: Build to Quest 3 (First Test)

1. **File → Build Settings**
2. Click **Add Open Scenes** (adds HomeArea)
3. **Platform**: Android (switch if needed)
4. Connect Quest 3 via USB-C
5. Enable Developer Mode on Quest 3
6. Click **Build And Run**

The VR locomotion should work immediately with left thumbstick movement and right thumbstick snap turning!

## 🔧 Troubleshooting

### "XR Origin not found" or VR not working
- Make sure you followed XR_RIG_SETUP_GUIDE.md completely
- Verify Locomotion System component is on XR Origin
- Verify both Move and Turn providers are configured

### "Scripts are missing"
- Unity might need to recompile. Wait for it to finish.
- Check Console for errors

### "Can't move in VR"
- Verify Character Controller is on XR Origin
- Verify Continuous Move Provider has correct action: "XRI LeftHand/Move"
- Check that Locomotion System is assigned to the providers

### "No room visible"
- Select HomeAreaBuilder in Hierarchy
- Right-click on component → "Build Home Area Room"

## 📋 Next Development Steps (After Basic VR Works)

Once you confirm VR locomotion works:

1. **Create Portal Prefab**
   - Create 3D model with rotating rings
   - Add particle system
   - Add Portal.cs script
   - Assign to HomeAreaBuilder's portalPrefab field

2. **Create Materials**
   - Low-poly flat-shaded materials for walls
   - Emissive materials for portal
   - Floor/ceiling materials

3. **Port BSP Dungeon Generator**
   - Convert BSPDungeonGenerator.js → C#
   - Create Dungeon scene
   - Generate rooms procedurally

4. **Enemy AI with NavMesh**
   - Create low-poly enemy models
   - Add NavMeshAgent component
   - Bake NavMesh on dungeon

5. **Spell System**
   - Gesture detection or button-based casting
   - Projectile system with pooling
   - Particle effects

## 🎮 Current Priority: GET VR MOVEMENT WORKING

Focus on Steps 1-6 above. Once you can move around in VR on Quest 3, we'll tackle the rest!

Let me know when you've completed the XR Rig setup and tested the build on Quest 3.
