# Easy XR Setup (Automated)

Much simpler approach using automated scripts!

## Step 1: Check if XR Origin Already Exists

Looking at your screenshot, you already have **XR Origin** in your scene! Perfect.

If not, create it:
- Right-click in Hierarchy → **XR → XR Origin (VR)**

## Step 2: Add Auto-Setup Script to XR Origin

1. **Select "XR Origin"** in the Hierarchy
2. In **Inspector**, click **"Add Component"**
3. Search for **"XRRigAutoSetup"**
4. Click to add it

**That's it!** The script will automatically add all the locomotion components when you press Play.

## Step 3: Add Character Controller Manually (Important!)

The XRRigAutoSetup script requires a Character Controller but can't add it automatically.

1. Select **XR Origin** in Hierarchy
2. In Inspector, click **"Add Component"**
3. Search for **"Character Controller"**
4. Add it

The auto-setup script will configure it automatically!

## Step 4: Import XR Interaction Toolkit Samples (For Input Actions)

This is critical for controller input to work:

1. **Window → Package Manager**
2. In top-left dropdown, select **"Packages: Unity Registry"**
3. Find **"XR Interaction Toolkit"** in the list
4. Click on it
5. Look for **"Samples"** section on the right
6. Find **"Starter Assets"** and click **"Import"**

This imports the input action presets for VR controllers.

## Step 5: Test in Editor

1. Click **Play** button
2. Check Console - you should see:
   ```
   [XRRigAutoSetup] Setting up XR Rig components...
   [XRRigAutoSetup] Character Controller configured
   [XRRigAutoSetup] Added LocomotionSystem
   [XRRigAutoSetup] Added Continuous Move Provider
   [XRRigAutoSetup] Added Snap Turn Provider
   [XRRigAutoSetup] XR Rig setup complete!
   ```

## Step 6: Add XR Device Simulator (For Desktop Testing)

1. In **Project** window, search for **"XR Device Simulator"**
2. It should be under: `Packages/XR Interaction Toolkit/Runtime/XR Device Simulator/`
3. Drag the **XR Device Simulator** prefab into your scene

Now you can test VR controls on desktop:
- **WASD**: Move view
- **Right Mouse + Drag**: Look around
- **Shift + WASD**: Simulate left thumbstick movement
- **Q/E**: Simulate snap turning

## Step 7: Build to Quest 3

1. **File → Build Settings**
2. **Add Open Scenes**
3. **Platform**: Android (switch if needed)
4. Connect Quest 3
5. **Build And Run**

## Troubleshooting

### "Script not found"
- Unity needs to compile. Wait for spinning icon bottom-right to finish

### "Controllers not responding in VR"
- Make sure you imported **Starter Assets** from XR Interaction Toolkit package (Step 4)
- The input actions must be configured

### "Can't add XRRigAutoSetup"
- Make sure the script compiled successfully
- Check Console for errors

## What the Auto-Setup Script Does

When you press Play, it automatically:
1. ✅ Configures Character Controller (height, radius, collision)
2. ✅ Adds Locomotion System component
3. ✅ Adds Continuous Move Provider (left thumbstick movement at 2.5 m/s)
4. ✅ Adds Snap Turn Provider (right thumbstick turns at 45°)
5. ✅ Connects all components together

No manual menu navigation needed!
