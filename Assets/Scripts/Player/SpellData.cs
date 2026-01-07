using UnityEngine;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Data for a spell - used in the radial menu and casting
    /// Scriptable Object for easy configuration in editor
    /// </summary>
    [CreateAssetMenu(fileName = "New Spell", menuName = "VR Dungeon/Spell Data")]
    public class SpellData : ScriptableObject
    {
        [Header("Spell Info")]
        public string spellName = "Fireball";
        public Sprite icon; // Icon shown in radial menu
        public Color spellColor = Color.red;

        [Tooltip("Spell tier: 1 = basic (inner ring), 2 = advanced (outer ring)")]
        [Range(1, 2)]
        public int tier = 1;

        [Header("Casting")]
        public GameObject projectilePrefab;
        public float castCooldown = 0.5f;
        public float projectileSpeed = 20f;
        public float projectileLifetime = 5f;

        [Header("Damage")]
        public float damage = 25f;
        public float splashRadius = 0f; // 0 = no splash damage

        [Header("Visual Effects")]
        public GameObject castEffect; // Particle effect when casting
        public GameObject hitEffect; // Particle effect on hit
        public AudioClip castSound;
        public AudioClip hitSound;

        [Header("Description")]
        [TextArea(3, 5)]
        public string description = "A basic spell";
    }
}
