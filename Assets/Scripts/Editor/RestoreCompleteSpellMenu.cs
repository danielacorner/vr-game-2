using UnityEngine;
using UnityEditor;
using VRDungeonCrawler.Player;
using Unity.XR.CoreUtils;
using UnityEditor.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Restores the complete HalfLifeAlyxSpellMenu system with all dependencies
    /// </summary>
    [InitializeOnLoad]
    public static class RestoreCompleteSpellMenu
    {
        private const string PREF_KEY = "VRD_SpellMenuRestored_v1";

        static RestoreCompleteSpellMenu()
        {
            // Only run once
            if (EditorPrefs.GetBool(PREF_KEY, false))
            {
                Debug.Log("[RestoreSpellMenu] Spell menu already restored");
                return;
            }

            // Delay execution
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;

                Debug.Log("========================================");
                Debug.Log("[RestoreSpellMenu] RESTORING COMPLETE SPELL MENU!");
                Debug.Log("========================================");

                DoRestore();

                EditorPrefs.SetBool(PREF_KEY, true);
            };
        }

        private static void DoRestore()
        {
            // 1. Find or create SpellManager
            SpellManager spellManager = Object.FindFirstObjectByType<SpellManager>();
            if (spellManager == null)
            {
                GameObject managerObj = new GameObject("SpellManager");
                spellManager = managerObj.AddComponent<SpellManager>();

                // Create example spells
                CreateExampleSpells(spellManager);

                Debug.Log("[RestoreSpellMenu] ✓ Created SpellManager");
            }
            else
            {
                Debug.Log("[RestoreSpellMenu] ✓ Found existing SpellManager");
            }

            // 2. Find XR Origin and controllers
            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[RestoreSpellMenu] No XR Origin found!");
                return;
            }

            Transform rightController = xrOrigin.transform.Find("Camera Offset/Right Controller");
            if (rightController == null)
            {
                Debug.LogError("[RestoreSpellMenu] Right Controller not found!");
                return;
            }

            // 3. Find the hand model (PolytopiaHand_R)
            Transform handModel = rightController.Find("PolytopiaHand_R");
            if (handModel == null)
            {
                Debug.LogError("[RestoreSpellMenu] Hand model not found! Run 'Restore Hands' first!");
                return;
            }

            // 4. Add or find HandPoseController on hand model
            HandPoseController handPose = handModel.GetComponent<HandPoseController>();
            if (handPose == null)
            {
                handPose = handModel.gameObject.AddComponent<HandPoseController>();
                handPose.isLeftHand = false;

                // Wire up finger bones
                Transform palm = handModel.Find("Palm");
                if (palm != null)
                {
                    handPose.thumbRoot = palm.Find("Thumb_Segment0");
                    handPose.indexRoot = palm.Find("Index_Segment0");
                    handPose.middleRoot = palm.Find("Middle_Segment0");
                    handPose.ringRoot = palm.Find("Ring_Segment0");
                    handPose.pinkyRoot = palm.Find("Pinky_Segment0");
                }

                Debug.Log("[RestoreSpellMenu] ✓ Added HandPoseController");
            }

            // 5. Remove old spell menu components
            HalfLifeAlyxSpellMenu oldMenu = rightController.GetComponentInChildren<HalfLifeAlyxSpellMenu>();
            if (oldMenu != null)
            {
                Object.DestroyImmediate(oldMenu);
                Debug.Log("[RestoreSpellMenu] Removed old spell menu");
            }

            // 6. Add HalfLifeAlyxSpellMenu to the RIGHT CONTROLLER (not hand model)
            HalfLifeAlyxSpellMenu spellMenu = rightController.gameObject.AddComponent<HalfLifeAlyxSpellMenu>();
            spellMenu.isLeftHand = false;
            spellMenu.handTransform = rightController; // Use controller transform
            spellMenu.handPoseController = handPose;

            // Configure menu settings
            spellMenu.menuDistance = 0.15f;
            spellMenu.tier1Radius = 0.12f;
            spellMenu.tier2Radius = 0.24f;
            spellMenu.iconSize = 0.1f;
            spellMenu.hoverDetectionRadius = 0.08f;
            spellMenu.selectionDeadzone = 0.3f;
            spellMenu.menuTiltAngle = -40f;
            spellMenu.menuZRotation = 30f;

            Debug.Log("[RestoreSpellMenu] ✓ Added HalfLifeAlyxSpellMenu to Right Controller");

            EditorUtility.SetDirty(rightController.gameObject);
            EditorUtility.SetDirty(handModel.gameObject);
            EditorUtility.SetDirty(spellManager.gameObject);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log("[RestoreSpellMenu] ✓✓✓ COMPLETE!");
            Debug.Log("[RestoreSpellMenu] Spell menu will open on right joystick click");
            Debug.Log("========================================");

            EditorUtility.DisplayDialog(
                "Spell Menu Restored!",
                "✓ SpellManager created with example spells\n" +
                "✓ HandPoseController added to hand\n" +
                "✓ HalfLifeAlyxSpellMenu on right controller\n" +
                "✓ All references wired up\n\n" +
                "Click right joystick in VR to test!",
                "OK"
            );
        }

        private static void CreateExampleSpells(SpellManager manager)
        {
            // Create example spell data in memory
            // Note: These won't persist unless saved as assets

            SpellData fireball = ScriptableObject.CreateInstance<SpellData>();
            fireball.name = "Fireball";
            fireball.spellName = "Fireball";
            fireball.tier = 1;
            fireball.spellColor = new Color(1f, 0.3f, 0f); // Orange
            fireball.castCooldown = 1f;
            fireball.damage = 25f;

            SpellData ice = ScriptableObject.CreateInstance<SpellData>();
            ice.name = "Ice Shard";
            ice.spellName = "Ice Shard";
            ice.tier = 1;
            ice.spellColor = new Color(0.3f, 0.7f, 1f); // Light blue
            ice.castCooldown = 0.8f;
            ice.damage = 20f;

            SpellData lightning = ScriptableObject.CreateInstance<SpellData>();
            lightning.name = "Lightning";
            lightning.spellName = "Lightning";
            lightning.tier = 1;
            lightning.spellColor = new Color(0.7f, 0.3f, 1f); // Purple
            lightning.castCooldown = 1.5f;
            lightning.damage = 30f;

            SpellData heal = ScriptableObject.CreateInstance<SpellData>();
            heal.name = "Heal";
            heal.spellName = "Heal";
            heal.tier = 2;
            heal.spellColor = new Color(0.3f, 1f, 0.3f); // Green
            heal.castCooldown = 3f;
            heal.damage = -20f; // Negative = healing

            manager.availableSpells.Clear();
            manager.availableSpells.Add(fireball);
            manager.availableSpells.Add(ice);
            manager.availableSpells.Add(lightning);
            manager.availableSpells.Add(heal);

            Debug.Log("[RestoreSpellMenu] ✓ Created 4 example spells (3 tier-1, 1 tier-2)");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Reset Spell Menu Flag", priority = 41)]
        public static void ResetFlag()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            Debug.Log("[RestoreSpellMenu] Flag reset");
        }
    }
}
