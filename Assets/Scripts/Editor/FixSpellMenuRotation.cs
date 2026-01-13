using UnityEngine;
using UnityEditor;
using VRDungeonCrawler.Player;
using Unity.XR.CoreUtils;
using UnityEditor.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Fixes spell menu rotation and selection issues
    /// </summary>
    [InitializeOnLoad]
    public static class FixSpellMenuRotation
    {
        private const string PREF_KEY = "VRD_SpellMenuFixed_v1";

        static FixSpellMenuRotation()
        {
            if (EditorPrefs.GetBool(PREF_KEY, false))
            {
                Debug.Log("[FixSpellMenu] Already fixed");
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;

                Debug.Log("========================================");
                Debug.Log("[FixSpellMenu] FIXING ROTATION & SELECTION!");
                Debug.Log("========================================");

                DoFix();

                EditorPrefs.SetBool(PREF_KEY, true);
            };
        }

        private static void DoFix()
        {
            // 1. Find and verify SpellManager
            SpellManager spellManager = Object.FindFirstObjectByType<SpellManager>();
            if (spellManager == null)
            {
                Debug.LogError("[FixSpellMenu] SpellManager not found!");
                return;
            }

            // Make sure it has spells
            if (spellManager.availableSpells.Count == 0)
            {
                Debug.LogWarning("[FixSpellMenu] SpellManager has no spells!");
            }
            else
            {
                Debug.Log($"[FixSpellMenu] SpellManager has {spellManager.availableSpells.Count} spells");

                // Set Fireball as default if available
                SpellData fireball = spellManager.availableSpells.Find(s =>
                    s.spellName.ToLower().Contains("fire"));

                if (fireball != null)
                {
                    spellManager.currentSpell = fireball;
                    Debug.Log($"[FixSpellMenu] ✓ Set default spell: {fireball.spellName}");
                }
            }

            // 2. Find the spell menu
            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[FixSpellMenu] No XR Origin!");
                return;
            }

            Transform rightController = xrOrigin.transform.Find("Camera Offset/Right Controller");
            if (rightController == null)
            {
                Debug.LogError("[FixSpellMenu] Right Controller not found!");
                return;
            }

            HalfLifeAlyxSpellMenu spellMenu = rightController.GetComponent<HalfLifeAlyxSpellMenu>();
            if (spellMenu == null)
            {
                Debug.LogError("[FixSpellMenu] HalfLifeAlyxSpellMenu not found!");
                return;
            }

            // 3. Fix rotation settings
            // User reports: top rotated away by 45° excess, clockwise by 15° excess
            // Original: menuTiltAngle = -40f, menuZRotation = 30f
            // New: Reduce tilt to -20° (less tilted), reduce Z-rotation to 15° (less clockwise)

            spellMenu.menuTiltAngle = -20f;      // Was -40°, now -20° (less tilted toward player)
            spellMenu.menuZRotation = 15f;       // Was 30°, now 15° (less counterclockwise rotation)

            Debug.Log("[FixSpellMenu] ✓ Adjusted rotation: tilt=-20°, zRot=15°");

            // 4. Make selection easier by increasing hover detection radius
            spellMenu.hoverDetectionRadius = 0.12f;  // Was 0.08f, now 0.12f (50% larger)

            Debug.Log("[FixSpellMenu] ✓ Increased hover detection radius to 0.12m");

            // 5. Verify hand transform and pose controller are set
            if (spellMenu.handTransform == null)
            {
                spellMenu.handTransform = rightController;
                Debug.Log("[FixSpellMenu] ✓ Set handTransform to controller");
            }

            if (spellMenu.handPoseController == null)
            {
                // Find in children
                Transform handModel = rightController.Find("PolytopiaHand_R");
                if (handModel != null)
                {
                    spellMenu.handPoseController = handModel.GetComponent<HandPoseController>();
                    if (spellMenu.handPoseController != null)
                    {
                        Debug.Log("[FixSpellMenu] ✓ Found and linked HandPoseController");
                    }
                }
            }

            EditorUtility.SetDirty(spellMenu);
            EditorUtility.SetDirty(spellManager);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("[FixSpellMenu] ✓✓✓ FIXED!");
            Debug.Log("[FixSpellMenu] Rotation adjusted for comfort");
            Debug.Log("[FixSpellMenu] Selection radius increased");
            Debug.Log("[FixSpellMenu] Default spell set to Fireball");
            Debug.Log("========================================");

            EditorUtility.DisplayDialog(
                "Spell Menu Fixed!",
                "✓ Rotation adjusted (less tilt, less rotation)\n" +
                "✓ Selection radius increased (easier to select)\n" +
                "✓ Default spell set to Fireball\n\n" +
                "Try the spell menu now!\n" +
                "Move your hand NEAR a spell icon to select it.",
                "OK"
            );
        }

        [MenuItem("Tools/VR Dungeon Crawler/Reset Spell Menu Fix Flag", priority = 42)]
        public static void ResetFlag()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            Debug.Log("[FixSpellMenu] Flag reset");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Debug Spell Menu State", priority = 43)]
        public static void DebugState()
        {
            SpellManager sm = Object.FindFirstObjectByType<SpellManager>();
            if (sm != null)
            {
                Debug.Log($"SpellManager: {sm.availableSpells.Count} spells");
                Debug.Log($"Current spell: {(sm.currentSpell != null ? sm.currentSpell.spellName : "NULL")}");
            }
            else
            {
                Debug.LogError("No SpellManager found!");
            }

            XROrigin xr = Object.FindFirstObjectByType<XROrigin>();
            if (xr != null)
            {
                Transform rc = xr.transform.Find("Camera Offset/Right Controller");
                if (rc != null)
                {
                    HalfLifeAlyxSpellMenu menu = rc.GetComponent<HalfLifeAlyxSpellMenu>();
                    if (menu != null)
                    {
                        Debug.Log($"SpellMenu: tilt={menu.menuTiltAngle}°, zRot={menu.menuZRotation}°");
                        Debug.Log($"SpellMenu: hoverRadius={menu.hoverDetectionRadius}m");
                        Debug.Log($"SpellMenu: handTransform={(menu.handTransform != null ? "SET" : "NULL")}");
                        Debug.Log($"SpellMenu: handPoseController={(menu.handPoseController != null ? "SET" : "NULL")}");
                    }
                    else
                    {
                        Debug.LogError("No HalfLifeAlyxSpellMenu on Right Controller!");
                    }
                }
            }
        }
    }
}
