using UnityEngine;
using UnityEditor;
using VRDungeonCrawler.Player;
using Unity.XR.CoreUtils;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Diagnoses and fixes spell system issues
    /// </summary>
    public static class DiagnoseSpellSystem
    {
        [MenuItem("Tools/VR Dungeon Crawler/Diagnose Spell System", priority = 60)]
        public static void Diagnose()
        {
            Debug.Log("========================================");
            Debug.Log("[Diagnose] CHECKING SPELL SYSTEM...");
            Debug.Log("========================================");

            // 1. Check SpellManager
            SpellManager spellManager = Object.FindFirstObjectByType<SpellManager>();
            if (spellManager == null)
            {
                Debug.LogError("[Diagnose] ❌ NO SPELLMANAGER FOUND!");
                Debug.Log("[Diagnose] Creating SpellManager with example spells...");

                GameObject managerObj = new GameObject("SpellManager");
                spellManager = managerObj.AddComponent<SpellManager>();
                CreateExampleSpells(spellManager);

                Debug.Log("[Diagnose] ✓ Created SpellManager");
            }
            else
            {
                Debug.Log($"[Diagnose] ✓ SpellManager exists: {spellManager.gameObject.name}");
            }

            // 2. Check if SpellManager has spells
            if (spellManager.availableSpells == null || spellManager.availableSpells.Count == 0)
            {
                Debug.LogError("[Diagnose] ❌ SpellManager has NO SPELLS!");
                Debug.Log("[Diagnose] Creating example spells...");
                CreateExampleSpells(spellManager);
            }
            else
            {
                Debug.Log($"[Diagnose] ✓ SpellManager has {spellManager.availableSpells.Count} spells");
                foreach (var spell in spellManager.availableSpells)
                {
                    Debug.Log($"[Diagnose]   - {spell.spellName} (tier {spell.tier})");
                }
            }

            // 3. Check if a default spell is set
            if (spellManager.currentSpell == null)
            {
                Debug.LogWarning("[Diagnose] ⚠️ NO DEFAULT SPELL SET!");

                // Set first spell as default
                if (spellManager.availableSpells.Count > 0)
                {
                    SpellData fireball = spellManager.availableSpells.Find(s => s.spellName.ToLower().Contains("fire"));
                    if (fireball != null)
                    {
                        spellManager.currentSpell = fireball;
                        Debug.Log($"[Diagnose] ✓ Set default spell: {fireball.spellName}");

                        // Trigger the OnSpellChanged event manually
                        spellManager.OnSpellChanged?.Invoke(fireball);
                        Debug.Log("[Diagnose] ✓ Triggered OnSpellChanged event");
                    }
                    else
                    {
                        spellManager.currentSpell = spellManager.availableSpells[0];
                        Debug.Log($"[Diagnose] ✓ Set default spell: {spellManager.availableSpells[0].spellName}");

                        // Trigger the OnSpellChanged event manually
                        spellManager.OnSpellChanged?.Invoke(spellManager.availableSpells[0]);
                        Debug.Log("[Diagnose] ✓ Triggered OnSpellChanged event");
                    }

                    EditorUtility.SetDirty(spellManager);
                }
            }
            else
            {
                Debug.Log($"[Diagnose] ✓ Default spell: {spellManager.currentSpell.spellName}");

                // Check if spell has projectile prefab
                if (spellManager.currentSpell.projectilePrefab == null)
                {
                    Debug.LogWarning("[Diagnose] ⚠️ Current spell has NO PROJECTILE PREFAB!");
                    Debug.LogWarning("[Diagnose] Spell can be equipped but won't fire projectiles!");
                }
                else
                {
                    Debug.Log($"[Diagnose] ✓ Projectile prefab: {spellManager.currentSpell.projectilePrefab.name}");
                }
            }

            // 4. Check XR Origin and controllers
            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[Diagnose] ❌ NO XR ORIGIN!");
                return;
            }

            Transform leftController = xrOrigin.transform.Find("Camera Offset/Left Controller");
            Transform rightController = xrOrigin.transform.Find("Camera Offset/Right Controller");

            Debug.Log("[Diagnose] Checking LEFT controller...");
            CheckController(leftController, "LEFT");

            Debug.Log("[Diagnose] Checking RIGHT controller...");
            CheckController(rightController, "RIGHT");

            Debug.Log("========================================");
            Debug.Log("[Diagnose] DIAGNOSIS COMPLETE!");
            Debug.Log("========================================");

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }

        private static void CheckController(Transform controller, string name)
        {
            if (controller == null)
            {
                Debug.LogError($"[Diagnose] ❌ {name} controller not found!");
                return;
            }

            // Check SpellCaster
            VRDungeonCrawler.Player.SpellCaster caster = controller.GetComponent<VRDungeonCrawler.Player.SpellCaster>();
            if (caster == null)
            {
                Debug.LogError($"[Diagnose] ❌ {name}: NO SpellCaster component!");
            }
            else
            {
                Debug.Log($"[Diagnose] ✓ {name}: SpellCaster exists");
                Debug.Log($"[Diagnose]   - castPoint: {(caster.castPoint != null ? "SET" : "❌ NULL")}");
                Debug.Log($"[Diagnose]   - castLight: {(caster.castLight != null ? "SET" : "❌ NULL")}");
                Debug.Log($"[Diagnose]   - controllerTransform: {(caster.controllerTransform != null ? "SET" : "❌ NULL")}");
            }

            // Check HalfLifeAlyxSpellMenu
            HalfLifeAlyxSpellMenu menu = controller.GetComponent<HalfLifeAlyxSpellMenu>();
            if (menu == null)
            {
                Debug.LogError($"[Diagnose] ❌ {name}: NO HalfLifeAlyxSpellMenu component!");
            }
            else
            {
                Debug.Log($"[Diagnose] ✓ {name}: HalfLifeAlyxSpellMenu exists");
                Debug.Log($"[Diagnose]   - handTransform: {(menu.handTransform != null ? "SET" : "❌ NULL")}");
                Debug.Log($"[Diagnose]   - handPoseController: {(menu.handPoseController != null ? "SET" : "❌ NULL")}");
            }

            // Check hand model
            Transform handModel = controller.Find(name == "LEFT" ? "PolytopiaHand_L" : "PolytopiaHand_R");
            if (handModel == null)
            {
                Debug.LogError($"[Diagnose] ❌ {name}: NO hand model!");
            }
            else
            {
                Debug.Log($"[Diagnose] ✓ {name}: Hand model exists");

                // Check SpellHandVisualEffect
                VRDungeonCrawler.Spells.SpellHandVisualEffect vfx = handModel.GetComponent<VRDungeonCrawler.Spells.SpellHandVisualEffect>();
                if (vfx == null)
                {
                    Debug.LogError($"[Diagnose] ❌ {name}: NO SpellHandVisualEffect on hand model!");
                }
                else
                {
                    Debug.Log($"[Diagnose] ✓ {name}: SpellHandVisualEffect exists");
                }

                // Check CastPoint
                Transform castPoint = handModel.Find("CastPoint");
                if (castPoint == null)
                {
                    Debug.LogError($"[Diagnose] ❌ {name}: NO CastPoint in hand model!");
                }
                else
                {
                    Debug.Log($"[Diagnose] ✓ {name}: CastPoint exists at {castPoint.localPosition}");
                }
            }
        }

        private static void CreateExampleSpells(SpellManager manager)
        {
            // Create example spell data in memory
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

            manager.availableSpells.Clear();
            manager.availableSpells.Add(fireball);
            manager.availableSpells.Add(ice);
            manager.availableSpells.Add(lightning);

            // Set fireball as default
            manager.currentSpell = fireball;

            Debug.Log("[Diagnose] ✓ Created 3 example spells (Fireball, Ice Shard, Lightning)");

            EditorUtility.SetDirty(manager);
        }
    }
}
