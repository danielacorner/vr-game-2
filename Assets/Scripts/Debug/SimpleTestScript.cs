using UnityEngine;
using VRDungeonCrawler.Player;

namespace VRDungeonCrawler.Debugging
{
    /// <summary>
    /// Super simple test script to verify scripts are running
    /// </summary>
    public class SimpleTestScript : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("========================================");
            Debug.Log("SIMPLE TEST SCRIPT AWAKE CALLED!!!");
            Debug.Log("GameObject: " + gameObject.name);
            Debug.Log("========================================");
        }

        void Start()
        {
            Debug.Log("========================================");
            Debug.Log("SIMPLE TEST SCRIPT START CALLED!!!");
            Debug.Log("========================================");

            // Check for PlayerMovementController
            var pmc = GetComponent<PlayerMovementController>();
            if (pmc != null)
            {
                Debug.Log("✓ PlayerMovementController found on this GameObject");
                Debug.Log("  - enableDualJoystickMovement: " + pmc.enableDualJoystickMovement);
                Debug.Log("  - showDebug: " + pmc.showDebug);
            }
            else
            {
                Debug.LogError("✗ PlayerMovementController NOT FOUND!");
            }

            // Check for CharacterController
            var cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                Debug.Log("✓ CharacterController found");
            }
            else
            {
                Debug.LogError("✗ CharacterController NOT FOUND!");
            }
        }
    }
}
