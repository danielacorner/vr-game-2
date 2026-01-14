# Unity Crash Prevention Guide

## Confirmed Crash Causes

1. **Unity AI Toolkit** - Makes repeated failing API calls on startup
   - Causes 100% CPU usage for extended periods
   - Each failed call retries multiple times
   - Status: ApiNoLongerSupported errors

2. **MCP Connection Issues** - Session ID problems causing hangs

3. **Auto-Restore Scripts** - Now FIXED (disabled)

## Immediate Fix

### Remove Unity AI Toolkit Package

```bash
# Option 1: Via Unity Package Manager (Recommended)
# 1. Open Unity (if it starts)
# 2. Window > Package Manager
# 3. Find "AI Toolkit"
# 4. Click "Remove"

# Option 2: Manual removal (if Unity won't start)
# Edit manifest.json to remove the package
```

### Manual Package Removal

1. Close Unity completely
2. Edit this file:
   `/Users/danielcorner/vr-game-2/Packages/manifest.json`
3. Remove this line:
   `"com.unity.ai.toolkit": "...version..."`
4. Save and restart Unity

## Prevention Checklist

- [ ] Remove Unity AI Toolkit package
- [ ] Auto-restore scripts disabled (✓ Done)
- [ ] MCP connection stable
- [ ] Library folder clean (if needed)

## If Unity Still Hangs

1. **Wait 5 minutes** - Asset imports can take time
2. **Check CPU** - If at 100% for >10 mins, force quit
3. **Check log** - `tail -f ~/Library/Logs/Unity/Editor.log`
4. **Nuclear option** - Delete Library folder and reimport (10-15 mins)

## Clean Restart Steps

1. Kill all Unity processes
2. Delete Library folder (optional but recommended)
3. Remove AI Toolkit from manifest.json
4. Start Unity
5. Wait for full asset import
6. Open scene
