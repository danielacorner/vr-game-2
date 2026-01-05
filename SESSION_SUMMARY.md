# VR Dungeon Crawler - Session Summary
*Migrating from Three.js WebXR to Unity for Quest 3*

## 🎯 Project Goal
Build a VR dungeon crawler for Quest 3 with:
- VR locomotion (smooth movement + snap turn)
- Home area (10x10m room) with portal to dungeon
- Spell casting system with Half-Life: Alyx style radial menu
- Low-poly art style
- BSP dungeon generation (TODO: port from Three.js)

## ✅ Completed
### 1. Unity Project Setup
- **Location**: `/Users/danielcorner/vr-game-2`
- **Unity Version**: 6000.3.2f1 (Unity 6)
- **Render Pipeline**: URP (Universal Render Pipeline)
- **XR Packages**:
  - XR Interaction Toolkit 3.3.1 (XRI 3.0+)
  - Oculus XR Plugin 4.5.2
  - XR Plugin Management
- **Target**: Quest 3 (Android, IL2CPP, ARM64)
- **Build Method**: USB-C Link → Build And Run

### 2. VR Locomotion ✅ WORKING
- XR Origin (XR Rig) with Character Controller
- Continuous move (left thumbstick)
- Snap turn (right thumbstick)
- **Status**: Tested and confirmed working on Quest 3

### 3. Home Area Scene ✅ CREATED
**File**: `Assets/Scenes/HomeArea.unity`

**Structure**:
- 10x10m room centered at origin (walls at ±5m)
- Floor at y=0, ceiling at y=3m
- Player spawn: (0, 1.6, -2) facing portal
- Furniture: table, chest, glowing orb with light

**Scripts**:
- `HomeAreaBuilder.cs` - Procedurally builds room
- `GameManager.cs` - Manages scene transitions
- `SetupHomeAreaMenu.cs` - Editor automation: "VR Dungeon → Setup Home Area Scene"

### 4. Portal ✅ CREATED (but position was wrong, then fixed)
**Position**: (0, 1.2, 3.5) - centered on back wall
- 3 rotating rings (cylinders, not torus - Unity doesn't have PrimitiveType.Torus)
- Particle effects
- Glowing material
- Trigger collider for scene transition

**Scripts**:
- `Portal.cs` - Handles collision and scene loading
- `CreatePortalMenu.cs` - Editor automation

**Note**: Portal was initially at (0, 1.2, 4) which was outside the room. Fixed to 3.5.

### 5. Spell System 📦 CREATED BUT NOT WORKING YET

#### Architecture:
**Data Layer**:
- `SpellData.cs` - ScriptableObject for spell configuration
- 4 spell assets created in `Assets/ScriptableObjects/Spells/`:
  - Fireball (orange, 20 m/s, 25 dmg)
  - IceShard (light blue, 15 m/s, 20 dmg)
  - Lightning (yellow, 30 m/s, 35 dmg)
  - WindBlast (light green, 25 m/s, 15 dmg)

**Management**:
- `SpellManager.cs` - Singleton managing available spells and current selection
- Located in scene, has 4 spells assigned

**Casting**:
- `SpellCaster.cs` - Attached to each controller (left/right)
  - **FIXED FOR XRI 3.0**: Uses `Transform controllerTransform` + auto-detected `InputAction`
  - Reads trigger button
  - Spawns projectiles from cast point
  - Provides haptic feedback

**Projectiles**:
- `SpellProjectile.cs` - Handles collision, damage, lifetime
- 4 prefabs in `Assets/Prefabs/Spells/`:
  - Sphere mesh with Rigidbody
  - Trail renderer
  - Point light matching spell color
  - Emissive URP Lit material

**UI - Radial Menu (Half-Life: Alyx style)**:
- `SpellRadialMenuUI.cs` - Hold thumbstick → menu appears
  - **FIXED FOR XRI 3.0**: Auto-detects thumbstick InputAction
  - World-space canvas 30cm in front of right controller
  - 4 slots: top, right, bottom, left
  - Move thumbstick to select, release to confirm
  - Designed for muscle-memory selection

**Editor Automation**:
- `SetupSpellSystemMenu.cs` - "VR Dungeon → Setup Spell System"
- `CreateSpellDataMenu.cs` - "VR Dungeon → Create Spell Data Assets"
- `AssignSpellsToManagerMenu.cs` - "VR Dungeon → Assign Spells to Manager"
- `SetupRadialMenuUI.cs` - "VR Dungeon → Setup Radial Spell Menu UI"
- `FixSpellCastersMenu.cs` - "VR Dungeon → Fix Spell Casters (XRI 3.0+)"

### 6. Low-Poly Hands ✅ CREATED
**Scripts**:
- `LowPolyHandGenerator.cs` - Generates blocky hand geometry
  - Palm: 0.09m × 0.11m × 0.025m (thick and meaty)
  - 5 fingers: 3 segments each, tapered 8% per segment
  - Thumb: 2 segments at 50° angle, 16mm thick
  - Flat shading (duplicated vertices per face)
  - ~150 triangles per hand
- `AddLowPolyHandsMenu.cs` - "VR Dungeon → Add Low-Poly Hands to Controllers"
  - Creates left/right hand meshes
  - Hides default Quest controller models
  - Peachy skin color: RGB(1, 0.85, 0.7)

**Status**: Script created, ready to run menu command

## ❌ Known Issues

### CRITICAL: Spell System Not Working in VR
**Symptom**: Trigger button doesn't cast spells, thumbstick radial menu doesn't appear

**Investigated**:
- SpellCaster components exist on both controllers
- `controllerTransform` references are assigned (verified with grep on scene file)
- SpellManager has 4 spells assigned
- Spell data assets exist

**Root Cause (Suspected)**:
- Input action auto-detection might be failing at runtime
- Or action names might not match ("XRI Left/Activate", "XRI Right/Activate")
- Need Unity MCP to inspect actual runtime state

**Files to Check**:
- Scene: `Assets/Scenes/HomeArea.unity`
- SpellCaster components on: "XR Origin/Camera Offset/LeftHand Controller" and "RightHand Controller"

### XRI 3.0 API Changes
Unity 6 with XRI 3.3.1 uses new API that's incompatible with XRI 2.x:
- ❌ `ActionBasedController` is obsolete
- ❌ `LocomotionSystem` → `LocomotionMediator`
- ❌ `controller.activateAction.action` no longer exists
- ✅ **FIXED**: Updated SpellCaster and SpellRadialMenuUI to auto-detect InputActions

### Minor Issues:
- PrimitiveType.Torus doesn't exist (used Cylinder for portal rings)
- Portal positioning (fixed)

## 📁 Important Files

### Core Scripts:
```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs          # Scene transitions
│   └── HomeAreaBuilder.cs      # Builds 10x10m room
├── Entities/
│   └── Portal.cs               # Portal trigger
├── Player/
│   ├── SpellData.cs           # ScriptableObject
│   ├── SpellManager.cs        # Singleton
│   ├── SpellCaster.cs         # Trigger → cast spell ⚠️ NOT WORKING
│   ├── SpellProjectile.cs     # Projectile behavior
│   ├── SimpleSpellSwitcher.cs # Fallback: cycle with thumbstick
│   └── SpellRadialMenuUI.cs   # Radial menu ⚠️ NOT WORKING
├── Utils/
│   └── LowPolyHandGenerator.cs # Low-poly hand mesh
└── Editor/
    ├── SetupHomeAreaMenu.cs
    ├── CreatePortalMenu.cs
    ├── SetupSpellSystemMenu.cs
    ├── CreateSpellDataMenu.cs
    ├── AssignSpellsToManagerMenu.cs
    ├── SetupRadialMenuUI.cs
    ├── FixSpellCastersMenu.cs
    └── AddLowPolyHandsMenu.cs
```

### Assets:
```
Assets/
├── Scenes/
│   ├── HomeArea.unity         # Main scene
│   └── SampleScene.unity      # Empty default scene
├── ScriptableObjects/
│   └── Spells/
│       ├── Fireball.asset
│       ├── IceShard.asset
│       ├── Lightning.asset
│       └── WindBlast.asset
└── Prefabs/
    └── Spells/
        ├── FireballProjectile.prefab
        ├── IceShardProjectile.prefab
        ├── LightningProjectile.prefab
        └── WindBlastProjectile.prefab
```

## 🔧 Unity MCP Setup

### Server Configuration:
**MCP Server Running**: Unity Editor plugin at `http://localhost:8080/mcp`

**Claude Code Config**: `~/.claude.json`
```json
{
  "projects": {
    "/Users/danielcorner/vr-game-2": {
      "mcpServers": {
        "unityMCP": {
          "url": "http://localhost:8080/mcp"
        }
      }
    }
  }
}
```

**Important**: Only works when Claude Code is started FROM `/Users/danielcorner/vr-game-2` directory!

### Repository:
Unity MCP cloned at: `~/unity-mcp/`

## 🎯 Next Steps (PRIORITY ORDER)

### 1. Debug Spell System with Unity MCP 🔴 CRITICAL
**Use Unity MCP to inspect**:
- Are SpellCaster components properly attached?
- Are `controllerTransform` references actually assigned at runtime?
- What InputActions are available?
- What are the actual action names? ("XRI Left/Activate" vs something else?)
- Is `triggerAction` null at runtime?
- Check SpellManager.currentSpell value
- Check if cast points exist

**If auto-detection fails**:
- Manually assign InputAction references in Inspector
- Or create alternative input reading method

### 2. Test Low-Poly Hands
Run: **VR Dungeon → Add Low-Poly Hands to Controllers**
Then Build & Run to see hands instead of controllers

### 3. Verify Spell Casting Works
Once debugged with MCP:
- Trigger button should cast spells
- Right thumbstick hold should show radial menu
- Spell selection should work
- Projectiles should spawn and fly

### 4. Port BSP Dungeon Generator from Three.js
**Source**: Previous Three.js project had `BSPDungeonGenerator.js`
**Target**: Create `BSPDungeonGenerator.cs` in Unity
**Features needed**:
- Recursive binary space partitioning
- Room and corridor generation
- NavMesh baking for enemy pathfinding
- Procedural geometry (walls, floors, doors)

### 5. Enemy AI
- Create enemy prefabs
- NavMeshAgent pathfinding
- Health system
- Combat

### 6. Advanced Spell Effects
- Particle systems for cast effects
- Hit effects with pooling
- Sound effects
- Spell upgrades/skill tree

## 💡 Technical Notes

### Unity 6 + XRI 3.0 Compatibility
- XRI 3.0 is a major breaking change from 2.x
- Can't use `ActionBasedController` reference fields directly
- Must auto-detect `InputAction` from `InputActionAsset`
- Or use manual assignment in Inspector

### Quest 3 Build Process
1. Unity: File → Build Settings → Android
2. Switch platform if needed (takes time)
3. Player Settings:
   - IL2CPP scripting backend
   - ARM64 architecture
   - Oculus → Quest 3 enabled
4. Connect Quest 3 via USB-C
5. Enable Developer Mode on headset
6. Build And Run (automatically installs and launches)

### Low-Poly Art Style
- Flat shading (duplicate vertices per face, not shared)
- Blocky geometry (boxes, simple shapes)
- URP Lit materials with low smoothness
- Emissive colors for glowing effects

## 📝 Commands to Remember

### Unity Editor Menu Commands:
```
VR Dungeon → Setup Home Area Scene
VR Dungeon → Create Portal in Scene
VR Dungeon → Setup Spell System
VR Dungeon → Create Spell Data Assets
VR Dungeon → Assign Spells to Manager
VR Dungeon → Setup Radial Spell Menu UI
VR Dungeon → Fix Spell Casters (XRI 3.0+)
VR Dungeon → Add Low-Poly Hands to Controllers
```

### Git Workflow:
User hasn't committed yet. Should commit once spell system is working:
```bash
git add .
git commit -m "Add VR locomotion, home area, portal, and spell system

- Unity 6 with XRI 3.0 for Quest 3
- Home area: 10x10m room with portal
- Spell system: 4 elemental spells with radial menu
- Low-poly hand models
- Editor automation scripts

🤖 Generated with Claude Code
Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

## 🐛 Debugging Tips

### Check Scene Hierarchy:
- XR Origin
  - Camera Offset
    - Main Camera
    - LeftHand Controller (has SpellCaster)
    - RightHand Controller (has SpellCaster, SpellRadialMenu)
- SpellManager (root GameObject)
- HomeAreaBuilder (or home area geometry)
- Portal

### Console Log Filters:
- `[SpellCaster]` - Spell casting events
- `[SpellManager]` - Spell selection
- `[RadialMenuUI]` - Radial menu events
- `[Hands]` - Hand model creation

### Common Fixes:
- Null references → Check Inspector assignments
- Input not working → Check InputAction is enabled
- Wrong spell firing → Check SpellManager.currentSpell
- Radial menu not showing → Check thumbstick action detection

## 🔗 References

### Documentation Read:
- Unity XR Interaction Toolkit 3.0 docs (XRI migration guide)
- Quest 3 development setup
- URP shader system

### Key Learnings:
- Quest 3 requires Oculus XR Plugin + XRI
- XRI 3.0 breaks backward compatibility with 2.x
- VR best practices: 10x10m play area, snap turn for comfort
- Input System package is separate from XRI

---

## Quick Start for Next Session

1. **Start Claude Code from vr-game-2**:
   ```bash
   cd /Users/danielcorner/vr-game-2
   claude
   ```

2. **Unity MCP should connect automatically**

3. **First Priority**: Debug spell system
   - Use MCP to inspect SpellCaster components
   - Check InputAction references
   - Verify trigger and thumbstick input at runtime

4. **Quick wins**:
   - Add hands: "VR Dungeon → Add Low-Poly Hands"
   - Build & Run to test

5. **After spell system works**:
   - Port BSP dungeon generator
   - Add enemy AI
   - Iterate on gameplay

---

*Session ended: Moving to vr-game-2 directory for Unity MCP access*
*Continue with: `cd vr-game-2 && claude`*
