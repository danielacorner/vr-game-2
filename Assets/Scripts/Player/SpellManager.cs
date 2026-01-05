using UnityEngine;
using System.Collections.Generic;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Manages all available spells and current active spell
    /// Singleton for easy access from radial menu and casters
    /// </summary>
    public class SpellManager : MonoBehaviour
    {
        public static SpellManager Instance { get; private set; }

        [Header("Available Spells")]
        public List<SpellData> availableSpells = new List<SpellData>();

        [Header("Current Selection")]
        public SpellData currentSpell;

        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Set Fire spell as default (find by name, fallback to first spell)
            if (availableSpells.Count > 0)
            {
                SpellData fireSpell = availableSpells.Find(s =>
                    s.spellName.ToLower().Contains("fire") || s.spellName.ToLower().Contains("flame"));

                currentSpell = fireSpell != null ? fireSpell : availableSpells[0];
                Debug.Log($"[SpellManager] Default spell: {currentSpell.spellName}");
            }
        }

        /// <summary>
        /// Select a spell by index
        /// </summary>
        public void SelectSpell(int index)
        {
            if (index >= 0 && index < availableSpells.Count)
            {
                currentSpell = availableSpells[index];
                Debug.Log($"[SpellManager] Selected: {currentSpell.spellName}");

                // Notify listeners (optional - for UI updates)
                OnSpellChanged?.Invoke(currentSpell);
            }
        }

        /// <summary>
        /// Select a spell by reference
        /// </summary>
        public void SelectSpell(SpellData spell)
        {
            if (availableSpells.Contains(spell))
            {
                currentSpell = spell;
                Debug.Log($"[SpellManager] Selected: {currentSpell.spellName}");
                OnSpellChanged?.Invoke(currentSpell);
            }
        }

        /// <summary>
        /// Get current spell
        /// </summary>
        public SpellData GetCurrentSpell()
        {
            return currentSpell;
        }

        /// <summary>
        /// Event fired when spell changes (for UI updates)
        /// </summary>
        public System.Action<SpellData> OnSpellChanged;
    }
}
