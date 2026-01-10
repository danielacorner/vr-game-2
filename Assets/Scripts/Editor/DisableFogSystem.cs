using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VRDungeonCrawler.Environment;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Disables fog systems in the scene
    /// Run from menu: Tools/VR Dungeon Crawler/Disable Fog System
    /// </summary>
    public class DisableFogSystem : UnityEditor.Editor
    {
        [MenuItem("Tools/VR Dungeon Crawler/Disable Fog System")]
        public static void DisableFog()
        {
            Debug.Log("========================================");
            Debug.Log("Disabling Fog Systems");
            Debug.Log("========================================");

            int disabledCount = 0;

            // Disable GroundFogEffect components
            GroundFogEffect[] groundFogs = FindObjectsOfType<GroundFogEffect>();
            foreach (GroundFogEffect fog in groundFogs)
            {
                fog.enabled = false;
                EditorUtility.SetDirty(fog);
                Debug.Log($"✓ Disabled GroundFogEffect on {fog.gameObject.name}");
                disabledCount++;
            }

            // Disable fog in AtmosphericParticles
            AtmosphericParticles[] atmosphericParticles = FindObjectsOfType<AtmosphericParticles>();
            foreach (AtmosphericParticles atm in atmosphericParticles)
            {
                atm.enableFog = false;

                // Stop fog particle system if it's running
                if (atm.fogSystem != null)
                {
                    atm.fogSystem.Stop();
                    atm.fogSystem.gameObject.SetActive(false);
                    Debug.Log($"✓ Stopped fog system in {atm.gameObject.name}");
                }

                EditorUtility.SetDirty(atm);
                disabledCount++;
            }

            // Find and disable any GameObject named "GroundFog" or "FogSystem"
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Fog") && obj.name != "GroundFogEffect")
                {
                    obj.SetActive(false);
                    Debug.Log($"✓ Disabled GameObject: {obj.name}");
                    disabledCount++;
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log($"✓✓✓ Fog System Disabled! ({disabledCount} items affected)");
            Debug.Log("========================================");
        }

        [MenuItem("Tools/VR Dungeon Crawler/Enable Fog System")]
        public static void EnableFog()
        {
            Debug.Log("========================================");
            Debug.Log("Enabling Fog Systems");
            Debug.Log("========================================");

            int enabledCount = 0;

            // Enable GroundFogEffect components
            GroundFogEffect[] groundFogs = FindObjectsOfType<GroundFogEffect>(true);
            foreach (GroundFogEffect fog in groundFogs)
            {
                fog.enabled = true;
                EditorUtility.SetDirty(fog);
                Debug.Log($"✓ Enabled GroundFogEffect on {fog.gameObject.name}");
                enabledCount++;
            }

            // Enable fog in AtmosphericParticles
            AtmosphericParticles[] atmosphericParticles = FindObjectsOfType<AtmosphericParticles>(true);
            foreach (AtmosphericParticles atm in atmosphericParticles)
            {
                atm.enableFog = true;

                // Start fog particle system
                if (atm.fogSystem != null)
                {
                    atm.fogSystem.gameObject.SetActive(true);
                    atm.fogSystem.Play();
                    Debug.Log($"✓ Started fog system in {atm.gameObject.name}");
                }

                EditorUtility.SetDirty(atm);
                enabledCount++;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("========================================");
            Debug.Log($"✓✓✓ Fog System Enabled! ({enabledCount} items affected)");
            Debug.Log("========================================");
        }
    }
}
