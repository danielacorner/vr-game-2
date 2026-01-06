using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Simple spell switcher for testing
    /// Press thumbstick left/right to cycle through spells
    /// Later can be replaced with radial menu
    /// </summary>
    public class SimpleSpellSwitcher : MonoBehaviour
    {
        [Header("References")]
#pragma warning disable CS0618 // ActionBasedController deprecated in XRI 3.0
        public ActionBasedController controller;
#pragma warning restore CS0618

        [Header("Settings")]
        public float inputCooldown = 0.3f; // Prevent rapid switching

        private float cooldownTimer = 0f;
        private Vector2 lastThumbstick;

        private void Update()
        {
            if (SpellManager.Instance == null || controller == null) return;

            // Update cooldown
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
                return;
            }

            // Read thumbstick (using move action for left stick)
            Vector2 thumbstick = Vector2.zero;
            if (controller.translateAnchorAction != null && controller.translateAnchorAction.action != null)
            {
                thumbstick = controller.translateAnchorAction.action.ReadValue<Vector2>();
            }

            // Check for left/right input
            float threshold = 0.7f;

            if (thumbstick.x > threshold && lastThumbstick.x <= threshold)
            {
                // Thumbstick moved right - next spell
                NextSpell();
                cooldownTimer = inputCooldown;
            }
            else if (thumbstick.x < -threshold && lastThumbstick.x >= -threshold)
            {
                // Thumbstick moved left - previous spell
                PreviousSpell();
                cooldownTimer = inputCooldown;
            }

            lastThumbstick = thumbstick;
        }

        private void NextSpell()
        {
            if (SpellManager.Instance.availableSpells.Count == 0) return;

            int currentIndex = SpellManager.Instance.availableSpells.IndexOf(SpellManager.Instance.currentSpell);
            int nextIndex = (currentIndex + 1) % SpellManager.Instance.availableSpells.Count;

            SpellManager.Instance.SelectSpell(nextIndex);

            Debug.Log($"[SpellSwitcher] → {SpellManager.Instance.currentSpell.spellName}");

            // Haptic feedback
            if (controller != null)
            {
                controller.SendHapticImpulse(0.3f, 0.1f);
            }
        }

        private void PreviousSpell()
        {
            if (SpellManager.Instance.availableSpells.Count == 0) return;

            int currentIndex = SpellManager.Instance.availableSpells.IndexOf(SpellManager.Instance.currentSpell);
            int prevIndex = currentIndex - 1;
            if (prevIndex < 0) prevIndex = SpellManager.Instance.availableSpells.Count - 1;

            SpellManager.Instance.SelectSpell(prevIndex);

            Debug.Log($"[SpellSwitcher] ← {SpellManager.Instance.currentSpell.spellName}");

            // Haptic feedback
            if (controller != null)
            {
                controller.SendHapticImpulse(0.3f, 0.1f);
            }
        }
    }
}
