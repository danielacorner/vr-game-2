using UnityEngine;

namespace VRDungeonCrawler.Dungeon
{
    /// <summary>
    /// Boss door that opens when all enemies are defeated
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BossDoor : MonoBehaviour
    {
        [Header("Door Settings")]
        [Tooltip("Is door currently locked")]
        public bool isLocked = true;
        
        [Tooltip("Material for locked door")]
        public Material lockedMaterial;
        
        [Tooltip("Material for unlocked door")]
        public Material unlockedMaterial;
        
        private Renderer doorRenderer;
        private Collider doorCollider;
        
        private void Awake()
        {
            doorRenderer = GetComponent<Renderer>();
            doorCollider = GetComponent<Collider>();
            
            UpdateDoorState();
        }
        
        private void Start()
        {
            // Check for enemies in the scene
            CheckEnemies();
        }
        
        private void Update()
        {
            if (isLocked)
            {
                CheckEnemies();
            }
        }
        
        private void CheckEnemies()
        {
            // Check if Enemy tag exists to avoid exceptions
            try
            {
                // Find all enemy objects
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

                if (enemies.Length == 0)
                {
                    UnlockDoor();
                }
            }
            catch (UnityException)
            {
                // Enemy tag doesn't exist - unlock door by default
                UnlockDoor();
            }
        }
        
        public void UnlockDoor()
        {
            if (!isLocked) return;
            
            isLocked = false;
            Debug.Log("[BossDoor] All enemies defeated! Door unlocked!");
            
            UpdateDoorState();
        }
        
        private void UpdateDoorState()
        {
            if (isLocked)
            {
                if (lockedMaterial != null && doorRenderer != null)
                    doorRenderer.material = lockedMaterial;
                    
                if (doorCollider != null)
                    doorCollider.enabled = true;
            }
            else
            {
                if (unlockedMaterial != null && doorRenderer != null)
                    doorRenderer.material = unlockedMaterial;
                    
                if (doorCollider != null)
                    doorCollider.enabled = false;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isLocked)
            {
                Debug.Log("[BossDoor] Door is locked! Defeat all enemies first.");
            }
        }
    }
}
