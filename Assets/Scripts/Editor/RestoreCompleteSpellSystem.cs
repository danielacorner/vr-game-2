using UnityEngine;
using UnityEditor;
using VRDungeonCrawler.Player;
using VRDungeonCrawler.Spells;
using Unity.XR.CoreUtils;
using UnityEditor.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Restores the COMPLETE spell system to BOTH hands:
    /// - SpellCaster (casting spells with trigger)
    /// - SpellHandVisualEffect (Bioshock-style hand effects)
    /// - HalfLifeAlyxSpellMenu (radial menu on joystick click)
    /// </summary>
    [InitializeOnLoad]
    public static class RestoreCompleteSpellSystem
    {
        private const string PREF_KEY = "VRD_CompleteSpellSystem_v1";

        static RestoreCompleteSpellSystem()
        {
            // Check both EditorPrefs flag AND if components actually exist
            if (EditorPrefs.GetBool(PREF_KEY, false))
            {
                // Verify components actually exist before skipping
                XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
                if (xrOrigin != null)
                {
                    Transform rightController = xrOrigin.transform.Find("Camera Offset/Right Controller");
                    if (rightController != null)
                    {
                        var caster = rightController.GetComponent<VRDungeonCrawler.Spells.SpellCaster>();
                        if (caster != null && caster.spawnPoint != null)
                        {
                            Debug.Log("[CompleteSpellSystem] Already restored and verified");
                            return;
                        }
                    }
                }

                // Components missing despite flag - clear flag and continue
                Debug.LogWarning("[CompleteSpellSystem] Flag set but components missing - will restore");
                EditorPrefs.DeleteKey(PREF_KEY);
            }

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;

                Debug.Log("========================================");
                Debug.Log("[CompleteSpellSystem] RESTORING COMPLETE SPELL SYSTEM!");
                Debug.Log("========================================");

                DoRestore();

                EditorPrefs.SetBool(PREF_KEY, true);
            };
        }

        private static void DoRestore()
        {
            // Find XR Origin
            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[CompleteSpellSystem] No XR Origin!");
                return;
            }

            Transform leftController = xrOrigin.transform.Find("Camera Offset/Left Controller");
            Transform rightController = xrOrigin.transform.Find("Camera Offset/Right Controller");

            if (leftController == null || rightController == null)
            {
                Debug.LogError("[CompleteSpellSystem] Controllers not found!");
                return;
            }

            // Setup BOTH hands
            SetupSpellCasting(leftController, true);
            SetupSpellCasting(rightController, false);

            // Setup spell menu on BOTH controllers
            SetupSpellMenu(leftController, true);
            SetupSpellMenu(rightController, false);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("[CompleteSpellSystem] ✓✓✓ COMPLETE!");
            Debug.Log("[CompleteSpellSystem] Both hands: SpellCaster + Visual Effects");
            Debug.Log("[CompleteSpellSystem] Both hands: Spell Menu");
            Debug.Log("========================================");

            EditorUtility.DisplayDialog(
                "Complete Spell System Restored!",
                "✓ SpellCaster on both hands (CHARGE-AND-RELEASE)\n" +
                "✓ SpellHandVisualEffect on both hands (Bioshock style)\n" +
                "✓ Spell Menu on both hands (joystick click)\n" +
                "✓ Default spell: Fireball\n\n" +
                "Controls:\n" +
                "- Joystick click: Open spell menu\n" +
                "- Move hand near spell icon to select\n" +
                "- HOLD trigger: Charge spell (watch bubble grow)\n" +
                "- RELEASE trigger: Fire charged spell",
                "OK"
            );
        }

        private static void SetupSpellCasting(Transform controller, bool isLeft)
        {
            string handName = isLeft ? "LEFT" : "RIGHT";
            Debug.Log($"[CompleteSpellSystem] Setting up spell casting for {handName} hand...");

            // Find hand model
            Transform handModel = controller.Find(isLeft ? "PolytopiaHand_L" : "PolytopiaHand_R");
            if (handModel == null)
            {
                Debug.LogWarning($"[CompleteSpellSystem] {handName} hand model not found!");
                return;
            }

            // 1. Create spawn point (where projectiles spawn) - in hand model, not controller
            Transform spawnPoint = handModel.Find("SpawnPoint");
            if (spawnPoint == null)
            {
                GameObject spawnPointObj = new GameObject("SpawnPoint");
                spawnPointObj.transform.SetParent(handModel);
                spawnPointObj.transform.localPosition = new Vector3(0f, 0f, 0.15f); // 15cm forward from hand
                spawnPointObj.transform.localRotation = Quaternion.identity;
                spawnPoint = spawnPointObj.transform;
                Debug.Log($"[CompleteSpellSystem] ✓ Created SpawnPoint for {handName}");
            }

            // 2. Remove old Player.SpellCaster if it exists
            VRDungeonCrawler.Player.SpellCaster oldPlayerCaster = controller.GetComponent<VRDungeonCrawler.Player.SpellCaster>();
            if (oldPlayerCaster != null)
            {
                Object.DestroyImmediate(oldPlayerCaster);
                Debug.Log($"[CompleteSpellSystem] Removed old Player.SpellCaster from {handName}");
            }

            // 3. Add Spells.SpellCaster to controller (charge-and-release version)
            VRDungeonCrawler.Spells.SpellCaster caster = controller.GetComponent<VRDungeonCrawler.Spells.SpellCaster>();
            if (caster == null)
            {
                caster = controller.gameObject.AddComponent<VRDungeonCrawler.Spells.SpellCaster>();
                Debug.Log($"[CompleteSpellSystem] ✓ Added Spells.SpellCaster (charge-and-release) to {handName}");
            }

            // Wire up references for charge-and-release SpellCaster
            caster.isLeftHand = isLeft;
            caster.spawnPoint = spawnPoint;
            caster.spawnDistance = 0.3f;

            Debug.Log($"[CompleteSpellSystem] ✓ Configured SpellCaster for {handName} (charge-and-release enabled)");

            // 4. Add SpellHandVisualEffect to hand model
            SpellHandVisualEffect visualEffect = handModel.GetComponent<SpellHandVisualEffect>();
            if (visualEffect == null)
            {
                visualEffect = handModel.gameObject.AddComponent<SpellHandVisualEffect>();
                Debug.Log($"[CompleteSpellSystem] ✓ Added SpellHandVisualEffect to {handName}");
            }

            EditorUtility.SetDirty(controller.gameObject);
            EditorUtility.SetDirty(handModel.gameObject);
        }

        private static void SetupSpellMenu(Transform controller, bool isLeft)
        {
            string handName = isLeft ? "LEFT" : "RIGHT";
            Debug.Log($"[CompleteSpellSystem] Setting up spell menu for {handName} hand...");

            // Remove old spell menu if exists
            HalfLifeAlyxSpellMenu oldMenu = controller.GetComponent<HalfLifeAlyxSpellMenu>();
            if (oldMenu != null)
            {
                Object.DestroyImmediate(oldMenu);
                Debug.Log($"[CompleteSpellSystem] Removed old spell menu from {handName}");
            }

            // Add HalfLifeAlyxSpellMenu
            HalfLifeAlyxSpellMenu menu = controller.gameObject.AddComponent<HalfLifeAlyxSpellMenu>();
            menu.isLeftHand = isLeft;
            menu.handTransform = controller;

            // Find hand pose controller
            Transform handModel = controller.Find(isLeft ? "PolytopiaHand_L" : "PolytopiaHand_R");
            if (handModel != null)
            {
                menu.handPoseController = handModel.GetComponent<HandPoseController>();
            }

            // Configure menu settings with FIXED rotation
            menu.menuDistance = 0.15f;
            menu.tier1Radius = 0.12f;
            menu.tier2Radius = 0.24f;
            menu.iconSize = 0.1f;
            menu.hoverDetectionRadius = 0.12f; // Increased for easier selection
            menu.selectionDeadzone = 0.3f;

            // FIXED ROTATION VALUES
            menu.menuTiltAngle = -20f;  // Less tilt (was -40°)
            menu.menuZRotation = 15f;   // Less rotation (was 30°)

            Debug.Log($"[CompleteSpellSystem] ✓ Added spell menu to {handName} (tilt=-20°, zRot=15°)");

            EditorUtility.SetDirty(controller.gameObject);
        }

        [MenuItem("Tools/VR Dungeon Crawler/Force Restore Complete Spell System", priority = 49)]
        public static void ForceRestore()
        {
            Debug.Log("========================================");
            Debug.Log("[CompleteSpellSystem] FORCE RESTORING SPELL SYSTEM!");
            Debug.Log("========================================");

            DoRestore();

            Debug.Log("[CompleteSpellSystem] Force restore complete!");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Reset Complete Spell System Flag", priority = 50)]
        public static void ResetFlag()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            Debug.Log("[CompleteSpellSystem] Flag reset");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Debug Complete Spell System", priority = 51)]
        public static void DebugSystem()
        {
            XROrigin xr = Object.FindFirstObjectByType<XROrigin>();
            if (xr != null)
            {
                Transform lc = xr.transform.Find("Camera Offset/Left Controller");
                Transform rc = xr.transform.Find("Camera Offset/Right Controller");

                DebugController(lc, "LEFT");
                DebugController(rc, "RIGHT");
            }
        }

        private static void DebugController(Transform controller, string name)
        {
            if (controller == null) return;

            Debug.Log($"=== {name} CONTROLLER ===");

            // Check for Spells.SpellCaster (charge-and-release)
            VRDungeonCrawler.Spells.SpellCaster spellsCaster = controller.GetComponent<VRDungeonCrawler.Spells.SpellCaster>();
            Debug.Log($"Spells.SpellCaster (charge-and-release): {(spellsCaster != null ? "YES ✓" : "NO")}");
            if (spellsCaster != null)
            {
                Debug.Log($"  - spawnPoint: {(spellsCaster.spawnPoint != null ? "SET" : "NULL")}");
                Debug.Log($"  - isLeftHand: {spellsCaster.isLeftHand}");
            }

            // Check for old Player.SpellCaster (instant cast - should be removed)
            VRDungeonCrawler.Player.SpellCaster playerCaster = controller.GetComponent<VRDungeonCrawler.Player.SpellCaster>();
            if (playerCaster != null)
            {
                Debug.LogWarning($"Player.SpellCaster (old instant-cast): YES - SHOULD BE REMOVED!");
            }

            HalfLifeAlyxSpellMenu menu = controller.GetComponent<HalfLifeAlyxSpellMenu>();
            Debug.Log($"SpellMenu: {(menu != null ? "YES" : "NO")}");
            if (menu != null)
            {
                Debug.Log($"  - tilt: {menu.menuTiltAngle}°, zRot: {menu.menuZRotation}°");
            }

            Transform handModel = controller.Find(name == "LEFT" ? "PolytopiaHand_L" : "PolytopiaHand_R");
            if (handModel != null)
            {
                SpellHandVisualEffect vfx = handModel.GetComponent<SpellHandVisualEffect>();
                Debug.Log($"SpellHandVisualEffect: {(vfx != null ? "YES" : "NO")}");

                Transform spawnPoint = handModel.Find("SpawnPoint");
                Debug.Log($"SpawnPoint: {(spawnPoint != null ? "YES" : "NO")}");
            }
        }
    }
}
