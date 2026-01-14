# Claude AI Assistant Guidelines for vr-game-2 Project

## Critical Preferences

### Unity Model Context Protocol (MCP) Usage
**ALWAYS use Unity MCP for Unity Editor operations.** This is a strict requirement.

- ✅ **DO:** Use Unity MCP tools (`mcp__UnityMCP__*`) for all Unity operations
  - Reading/modifying GameObjects and components
  - Scene manipulation
  - Asset management
  - Editor state queries
  - Menu item execution

- ❌ **DO NOT:** Write C# editor scripts as a fallback solution
  - Scripts should only be written if explicitly requested for runtime functionality
  - Never write scripts to work around MCP limitations

### When MCP is Unavailable or Broken
**Debug the MCP connection instead of falling back to scripts.**

If MCP tools return errors:
1. Check if Unity is running and MCP is enabled
2. Ask user to restart Unity and reconnect MCP
3. Use `ListMcpResourcesTool` and `ReadMcpResourceTool` to verify connectivity
4. Check `unity://editor/state` resource to confirm connection
5. Report specific MCP errors to user for investigation
6. **NEVER** silently fall back to writing editor scripts

### Exception
The only time to write scripts is when:
- User explicitly requests runtime game functionality
- Creating gameplay features, components, or systems
- Adding tools that will be used repeatedly in the project

## Project Structure

### Key Directories
- `/Assets/Scripts/` - Runtime game scripts
- `/Assets/Scripts/Editor/` - Unity Editor scripts (avoid creating new ones)
- `/Assets/Scripts/Player/` - Player-related scripts
- `/Assets/Scripts/Spells/` - Spell system (charge-and-release mechanics)
- `/Assets/Scripts/Environment/` - Environment/world scripts
- `/Assets/Scenes/HomeArea.unity` - Main starting scene

### Important Systems
- **Spell System:** Uses `VRDungeonCrawler.Spells.SpellCaster` (charge-and-release)
  - NOT `VRDungeonCrawler.Player.SpellCaster` (old instant-cast version)
- **XR Setup:** XRI 3.0+ with XR Origin structure
- **Controllers:** Left for movement, Right for teleportation and spells
- **Spell Menu:** HalfLifeAlyxSpellMenu with fixed rotation (fire spells pointing up)

## Unity Editor Quirks

### Auto-Restore Scripts
The project has auto-restore scripts that run on Unity startup:
- `RestoreCompleteSpellSystem.cs` - Restores spell system components
- `AutoRestoreHands.cs` - Restores hand models
- These have EditorPrefs flags to prevent repeated runs
- Use menu items under "Tools/VR Dungeon Crawler/Reset [Feature] Flag" to re-run

### Recent Issues
- Unity has been crashing frequently - investigate console errors and logs
- Spawn position and teleportation have been problematic areas
- MCP's `set_component_property` has bugs with certain property types

## Communication Style
- Be concise and technical
- Use MCP tools directly without explaining why you're not using scripts
- If MCP fails, report the error and ask user to fix MCP connection
- Don't apologize for MCP limitations - just state what needs manual fixing

## VR Dungeon Crawler Specifics
- Target platform: Meta Quest 3
- URP (Universal Render Pipeline)
- XR Interaction Toolkit 3.0+
- Spell system uses charge-and-release mechanics (hold trigger to charge, release to fire)
- Teleportation should only work in designated areas
- Snap-turn on right joystick (45° increments)
