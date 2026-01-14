# Unity Crash Investigation Report
Date: 2026-01-13

## Findings

### Primary Cause: Unity MCP `set_component_property` Bug
**Location:** `ManageGameObject.cs` lines 2009, 2099, 2348

**Symptoms:**
- Unity stalls/freezes when MCP tries to set certain component properties
- JSON parsing errors: `Unexpected error converting token to System.Boolean`
- Error message: `"Property 'enabled' not found. Did you mean: enabled?"` (circular error)

**Affected Operations:**
- Setting boolean properties like `enabled`
- Setting Transform references via properties
- Setting enum values like `handedness`

**Workaround:**
- Avoid `mcp__UnityMCP__manage_gameobject` action `set_component_property` for problematic properties
- Use GameObject `modify` action with `set_active` instead of component `enabled` property
- Manually edit properties in Unity Inspector when MCP fails

### Secondary Issues

#### 1. TransformHandle Serialization Errors (12 instances)
```
[GameObjectSerializer] Unexpected error serializing value of type UnityEngine.TransformHandle:
System.NullReferenceException: The TransformHandle object is null.
```
**Impact:** Low - serialization warnings but not causing crashes
**Recommendation:** Monitor but not urgent to fix

#### 2. Android Logcat Connection Failures
```
[Logcat] Failed to get process id for com.UnityTechnologies.com.unity.template.urpblank:
Exception has been thrown by the target of an invocation.
```
**Impact:** Low - only affects Android Logcat window when Quest 3 is disconnected
**Recommendation:** Close Android Logcat window when not deploying to device

#### 3. Large Log Files (17MB per session)
**Cause:** Verbose logging from MCP, Android tools, and editor operations
**Impact:** Medium - can slow down Unity over time
**Recommendation:** Clear logs periodically via Unity menu

## Immediate Actions Taken

### ✅ Fixed: Auto-Restore Popup Loop
- **Issue:** `RestoreCompleteSpellSystem.cs` and `AutoRestoreHands.cs` were re-verifying components on every startup
- **Root Cause:** Scripts checked EditorPrefs flag, then re-verified components existed, then cleared flag if missing
- **Fix:** Removed re-verification logic - now trusts EditorPrefs flag only
- **Result:** Dialogs will only appear once per flag setting

### ✅ Created: CLAUDE.md Guidelines
- Documented strict preference for Unity MCP usage
- Instructions to debug MCP instead of falling back to scripts
- Project-specific knowledge and preferences

## Recommendations

### Short-term
1. **When Unity stalls:** Force-quit immediately (⌘+Option+Esc)
2. **Before using MCP:** Verify Unity MCP is connected via `unity://editor/state`
3. **After stalls:** Check Console for MCP errors before retrying
4. **For component changes:** Use Unity Inspector for properties MCP can't set

### Long-term
1. **Report MCP bugs:** Submit issues to Unity MCP GitHub with specific failing operations
2. **Monitor MCP updates:** Check for package updates that fix `set_component_property`
3. **Cleanup scripts:** Remove unnecessary editor scripts from `/Assets/Scripts/Editor/`
4. **Log rotation:** Clear Unity logs weekly to prevent bloat

## MCP Known Limitations (as of Jan 2026)

### Cannot Set These Properties:
- `enabled` (boolean) - Use `set_active` on GameObject instead
- `handedness` (enum) - Manual edit required
- Transform references (requires specific format)

### Workarounds:
- **GameObject active state:** Use `modify` action with `set_active` parameter
- **Component references:** Use instanceID format: `{"instanceID": 12345}`
- **Enums:** Manual edit in Inspector or create dedicated menu item script

## Testing Checklist

After fixing spawn/teleportation issues:
- [ ] Test spawn position (should be 2.5m behind campfire)
- [ ] Test teleportation on right controller only
- [ ] Test snap-turn on right joystick (45° increments)
- [ ] Verify no auto-restore dialogs on Unity restart
- [ ] Monitor Unity stability during 10-min editing session

## Log Locations
- **Current session:** `~/Library/Logs/Unity/Editor.log`
- **Previous session:** `~/Library/Logs/Unity/Editor-prev.log`
- **Crash dumps:** `~/Library/Logs/Unity/` (look for `.dmp` files)
