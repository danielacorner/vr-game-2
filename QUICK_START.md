# Quick Start - Immediate Actions

## 🔴 TOP PRIORITY: Debug Spell System

### The Problem:
- Trigger button doesn't cast spells
- Radial menu doesn't appear on thumbstick hold
- Built to Quest 3, nothing happens

### What to Investigate with Unity MCP:
```
1. Select "LeftHand Controller" in Hierarchy
   - Check SpellCaster component:
     - Is controllerTransform assigned?
     - What is triggerAction value? (null?)
     - Is castPoint assigned?

2. Select "RightHand Controller"
   - Same checks for SpellCaster
   - Check SpellRadialMenuUI component:
     - Is controllerTransform assigned?
     - What is thumbstickAction value? (null?)
     - Is menuCanvas assigned?

3. Select "SpellManager"
   - Check availableSpells list (should have 4 items)
   - Check currentSpell (should be Fireball)

4. Play Mode - Check Console for:
   - "[SpellCaster] Found trigger action for Left/Right hand"
   - "[RadialMenuUI] Found thumbstick action"
   - If these DON'T appear, auto-detection failed!
```

### Expected Action Names:
- Trigger: `"XRI Left/Activate"` or `"XRI Right/Activate"`
- Thumbstick: `"XRI Right/Thumbstick"`

### If Auto-Detection Failed:
InputActions are being searched for in Resources with:
```csharp
Resources.FindObjectsOfTypeAll<InputActionAsset>()
// Looking for assets with name containing "XRI" or "Input"
```

## 🎯 After Spells Work:

### 1. Add Hands:
Unity Editor → **VR Dungeon → Add Low-Poly Hands to Controllers**

### 2. Build & Run:
File → Build Settings → Build And Run

### 3. Test Everything:
- ✅ Locomotion (left stick move, right stick turn)
- ✅ Hands visible
- ✅ Trigger casts spell (colored projectile)
- ✅ Right thumbstick hold → radial menu
- ✅ Move thumbstick → select spell → release

## 📁 Key Files (if MCP needs them):

```
Scene: Assets/Scenes/HomeArea.unity

Components to inspect:
- XR Origin/Camera Offset/LeftHand Controller
  → SpellCaster component
- XR Origin/Camera Offset/RightHand Controller
  → SpellCaster component
  → SpellRadialMenuUI component
- SpellManager (root level GameObject)
  → SpellManager component

Scripts to read if debugging:
- Assets/Scripts/Player/SpellCaster.cs (line 57-80: TryFindTriggerAction)
- Assets/Scripts/Player/SpellRadialMenuUI.cs (line 72-92: TryFindThumbstickAction)
```

## 🔧 Emergency Fixes:

### If Auto-Detection Completely Fails:
Create manual InputAction assignment script:
```csharp
// Get reference to XRI Default Input Actions asset
// Manually assign to SpellCaster.triggerAction
// Manually assign to SpellRadialMenuUI.thumbstickAction
```

## Next: Port BSP Dungeon
See SESSION_SUMMARY.md for full details.
