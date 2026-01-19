# UI Hand Interaction Setup Guide

This guide explains how to set up finger-pointing hand poses and ray visualization for UI interaction.

## What's Been Implemented

1. **New Hand Pose**: Added `Pointing` pose to `HandPoseState` enum
   - Index finger fully extended
   - Other fingers curled down
   - Thumb curled against palm

2. **UIHandPoseManager**: Automatically switches to pointing pose when hovering UI
   - Detects UI layer (layer 5)
   - Saves and restores previous pose
   - Controls line visual visibility

3. **UIRayVisualConfig**: Configures the XR ray visual appearance
   - Gold line color (matches menu theme)
   - Green color when hovering buttons
   - Sphere reticle at intersection point
   - Smooth color transitions

## Setup Instructions

### 1. Find Your XR Controllers in the Scene

In your HomeArea scene, locate:
- `XR Origin (XR Rig)/Camera Offset/RightHand Controller`
- `XR Origin (XR Rig)/Camera Offset/LeftHand Controller`

### 2. Add Components to Right Hand Controller

The right hand should already have:
- `XRRayInteractor` or `NearFarInteractor` component
- `XRInteractorLineVisual` component
- `HandPoseController` component (in a child GameObject with the hand model)

**Add these new components:**

1. **Add UIHandPoseManager**:
   - Select the Right Hand Controller GameObject
   - Add Component → Scripts → VRDungeonCrawler.Player → UIHandPoseManager
   - In the Inspector:
     - **Hand Pose Controller**: Drag the HandPoseController component (from the hand model child)
     - **UI Layer**: Set to 5 (should be default)
     - **Show Debug**: Enable for testing, disable later

2. **Add UIRayVisualConfig**:
   - Select the Right Hand Controller GameObject
   - Add Component → Scripts → VRDungeonCrawler.Player → UIRayVisualConfig
   - In the Inspector:
     - **Line Width**: 0.005
     - **Line Color**: Gold (R:1, G:0.9, B:0.4, A:0.8)
     - **Hover Color**: Green (R:0.3, G:1, B:0.3, A:0.9)
     - **Show Reticle**: Enabled
     - **Reticle Size**: 0.02

### 3. Optional: Add to Left Hand (if needed)

If you want the left hand to also point at UI:
- Repeat step 2 for the Left Hand Controller
- Make sure the HandPoseController reference points to the left hand's HandPoseController

### 4. Verify XRInteractorLineVisual Setup

Select the Right Hand Controller and check the `XRInteractorLineVisual` component:
- **Line Width**: 0.005
- **Valid Color Gradient**: Set to gold/yellow colors
- **Invalid Color Gradient**: Set to red colors
- **Smooth Movement**: Enabled
- **Follow Transform**: Should reference the controller transform

### 5. Test in Play Mode

1. Enter Play mode in Unity Editor
2. Point your controller at the portal menu
3. You should see:
   - ✅ Hand switches to pointing pose (index finger extended)
   - ✅ Gold ray appears from finger to menu
   - ✅ Reticle sphere appears at intersection point
   - ✅ Ray turns green when hovering over buttons
   - ✅ Buttons highlight slightly when ray is over them
4. Move away from menu:
   - ✅ Ray disappears
   - ✅ Hand returns to previous pose

### 6. Troubleshooting

**Hand doesn't point:**
- Check that HandPoseController reference is set in UIHandPoseManager
- Verify the hand model has finger bone transforms assigned in HandPoseController
- Check console for "[UIHandPoseManager]" logs (enable showDebug)

**Ray doesn't appear:**
- Verify XRInteractorLineVisual is enabled on the controller
- Check that UILayer is set to 5 in UIHandPoseManager
- Ensure the portal menu canvas is on layer 5 (UI layer)

**Ray doesn't turn green on buttons:**
- Verify buttons have the `Button` component
- Check that TrackedDeviceGraphicRaycaster is on the Canvas
- Ensure the button's Image component has "Raycast Target" enabled

**Reticle doesn't appear:**
- Check that "Show Reticle" is enabled in UIRayVisualConfig
- Verify the ray is actually hitting the UI (check with showDebug in UIHandPoseManager)

## How It Works

### Interaction Flow

1. **Detection**: UIHandPoseManager checks every frame if the XRRayInteractor is hovering over an object on layer 5 (UI)

2. **Pose Switch**: When UI is detected:
   - Saves the current hand pose
   - Switches to `HandPoseState.Pointing`
   - Enables the XRInteractorLineVisual

3. **Visual Feedback**:
   - UIRayVisualConfig updates the line color (gold → green when hovering buttons)
   - Reticle appears at the intersection point
   - Buttons use their ColorBlock.highlightedColor when hovered

4. **Restoration**: When no longer hovering UI:
   - Restores the previous hand pose
   - Disables the line visual
   - Hides the reticle

### Button Highlighting

Buttons automatically highlight when hovered because:
- They use Unity's `Button` component with `ColorBlock`
- The `TrackedDeviceGraphicRaycaster` on the Canvas enables VR controller interaction
- The `highlightedColor` in ColorBlock is already configured in PortalMenuSetup.cs:
  ```csharp
  colors.highlightedColor = new Color(0.3f, 0.5f, 0.3f, 0.95f); // Greenish highlight
  ```

## Configuration Options

### UIHandPoseManager

- **UI Layer**: Change if your UI uses a different layer
- **Show Debug**: Enable to see console logs about pose switching

### UIRayVisualConfig

- **Line Width**: Thickness of the ray (0.005 = 5mm)
- **Line Color**: Default ray color when pointing at UI
- **Hover Color**: Color when hovering over interactable buttons
- **Show Reticle**: Toggle the sphere indicator at ray end
- **Reticle Size**: Size of the sphere (0.02 = 2cm diameter)

## Known Limitations

- Currently only supports XRRayInteractor (not other interactor types)
- Reticle is a simple sphere (could be upgraded to a custom model/sprite)
- Line visual requires the XRInteractorLineVisual component
- Hand pose switching happens instantly (could add interpolation for smoother transitions)

## Future Enhancements

- Add haptic feedback when hovering buttons
- Add sound effects on hover/click
- Custom reticle sprite that rotates to face camera
- Smooth pose interpolation
- Support for other interactor types
- Configurable pose priorities (e.g., spell casting overrides pointing)
