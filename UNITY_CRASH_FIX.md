# Unity Crash/Hang Fix Guide

## Current Issue
Unity is stuck in infinite loop at 100% CPU during loading.

## Likely Causes
1. **Auto-restore scripts** running on every startup (even though we fixed them)
2. **MCP connection issues** causing Unity to hang
3. **Scene corruption** in HomeArea.unity
4. **Compilation deadlock** from script errors

## Safe Restart Steps

### Option 1: Disable Auto-Restore Scripts (Recommended First Try)
1. Before opening Unity, temporarily rename the restore scripts:
```bash
cd /Users/danielcorner/vr-game-2/Assets/Scripts/Editor
mv RestoreCompleteSpellSystem.cs RestoreCompleteSpellSystem.cs.disabled
mv RestoreCompleteSpellMenu.cs RestoreCompleteSpellMenu.cs.disabled
mv AutoRestoreHands.cs AutoRestoreHands.cs.disabled
```

2. Open Unity - should load faster without these
3. If it works, re-enable them one at a time to find the culprit

### Option 2: Load a Blank Scene
1. Before opening Unity, edit the EditorBuildSettings to load a different scene
2. Or create a new minimal scene and set it as the startup scene

### Option 3: Clear Unity Cache (Nuclear Option)
**Warning: This will force Unity to reimport everything (takes 5-10 minutes)**
```bash
cd /Users/danielcorner/vr-game-2
rm -rf Library/
```

Then restart Unity - it will rebuild the Library folder.

## After Restart
1. Check console for errors immediately
2. Look for "InitializeOnLoad" messages that might indicate which script is running
3. Consider temporarily disabling MCP if issues persist

## Prevention
- Don't use `[InitializeOnLoad]` scripts unless absolutely necessary
- Always add early-exit checks in static constructors
- Test scripts in a blank scene first before adding to main scene
