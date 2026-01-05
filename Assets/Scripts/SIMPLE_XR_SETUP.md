# Simple XR Setup - Use Built-in Prefabs

Forget the auto-setup script! Unity provides pre-made XR Origin prefabs with everything configured.

## Step 1: Import Starter Assets (CRITICAL!)

1. **Window → Package Manager**
2. Top-left dropdown: **"Packages: Unity Registry"**
3. Find **"XR Interaction Toolkit"** in the list
4. Select it, then on the right side find **"Samples"**
5. Find **"Starter Assets"** and click **"Import"**

This imports pre-configured XR Origin prefabs!

## Step 2: Find the Complete XR Origin Prefab

After importing Starter Assets, search in Project window:

1. Click in **Project** window
2. Search for: **"Complete XR Origin Set Up"**
3. You should find a prefab (blue cube icon)
4. Location: `Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/Prefabs/`

## Step 3: Use the Pre-Made XR Origin

### Option A: Replace Your Current XR Origin

1. **Delete** your current "XR Origin (VR)" from Hierarchy
2. **Drag** the "Complete XR Origin Set Up" prefab into Hierarchy
3. **Rename** it to "XR Origin" if you want
4. Done! It has everything pre-configured:
   - ✅ Locomotion system
   - ✅ Continuous movement (left thumbstick)
   - ✅ Snap turn (right thumbstick)
   - ✅ Character Controller
   - ✅ Input actions

### Option B: If You Can't Find the Prefab

Just use the XR Device-based Rig:

1. Delete current XR Origin
2. **GameObject → XR → Device-based → XR Origin (XR Rig)**
3. This creates a simpler rig

Then manually add components to XR Origin:
- **Character Controller** (for collision)
- **Locomotion Mediator** (or whatever the new system is called)
- **Continuous Move Provider**
- **Snap Turn Provider**

But the prefab approach (Option A) is WAY easier!

## Step 4: Position for Home Area

Select XR Origin, set Transform:
- **Position**: (0, 0, -2)
- **Rotation**: (0, 0, 0)

## Step 5: Test

1. Click **Play**
2. If you have XR Device Simulator in scene, use:
   - **Right Mouse + Drag**: Look around
   - **Shift + WASD**: Simulate left thumbstick movement
   - **Ctrl + Q/E**: Simulate right thumbstick turn

## Step 6: Build to Quest 3

Once it works in editor:
1. **File → Build Settings**
2. **Platform**: Android
3. Connect Quest 3
4. **Build And Run**

The controls should work immediately:
- **Left thumbstick**: Move
- **Right thumbstick**: Snap turn

---

## If Still Having Issues

The XR Interaction Toolkit version (3.3.1) has changed APIs multiple times. The pre-made prefabs from Starter Assets are the most reliable way to get started.

If you can't find the prefabs after importing Starter Assets, let me know and we'll try a manual setup approach or downgrade to an older, more stable XRI version.
