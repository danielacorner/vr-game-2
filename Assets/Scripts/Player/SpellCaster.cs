using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Casts spells from VR controller
    /// Attached to each controller (left and right hands)
    /// Trigger button fires the currently selected spell
    /// Compatible with XRI 3.0+
    /// </summary>
    public class SpellCaster : MonoBehaviour
    {
        [Header("References")]
        public Transform controllerTransform; // Controller transform for position/rotation
        public Transform castPoint; // Where projectiles spawn

        [Header("Settings")]
        public bool canCast = true;
        public float cooldownRemaining = 0f;

        [Header("Visual Feedback")]
        public ParticleSystem chargingEffect;
        public Light castLight;

        [Header("Input")]
        [Tooltip("Optionally assign the XRI Activate action. If null, will auto-detect from ControllerInputActionManager.")]
        public InputActionReference triggerActionRef;

        private SpellData currentSpell;
        private bool triggerPressed = false;
        private InputAction triggerAction;
        private object inputReader; // Store the inputReader itself

        private void Start()
        {
            Debug.Log($"!!!!! SPELLCASTER START() CALLED ON {gameObject.name} !!!!!");

            // VISUAL INDICATOR: Make cast light bright to show script is running
            if (castLight != null)
            {
                castLight.enabled = true;
                castLight.color = Color.green; // Green = script is running!
                castLight.intensity = 5f;
                Debug.Log($"Set castLight to GREEN on {gameObject.name}");
            }
            else
            {
                Debug.LogError($"castLight is NULL on {gameObject.name}!");
            }

            // Try to get the InputAction from the reference first
            if (triggerActionRef != null)
            {
                triggerAction = triggerActionRef.action;
                if (triggerAction != null)
                {
                    triggerAction.Enable();
                    Debug.Log($"[SpellCaster] Using assigned trigger action on {gameObject.name}");
                    if (castLight != null) castLight.color = Color.cyan; // Cyan = action assigned!
                    return;
                }
            }

            // Auto-detect from NearFarInteractor (XRI 3.0+) - Use PUBLIC PROPERTY instead of private field
            var nearFarInteractor = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>();
            Debug.Log($"[SpellCaster] NearFarInteractor found: {nearFarInteractor != null} on {gameObject.name}");

            if (nearFarInteractor != null)
            {
                // Access the activateInput PUBLIC PROPERTY
                var activateInputProp = nearFarInteractor.GetType().GetProperty("activateInput", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                Debug.Log($"[SpellCaster] activateInput property found: {activateInputProp != null}");

                if (activateInputProp != null)
                {
                    this.inputReader = activateInputProp.GetValue(nearFarInteractor);
                    Debug.Log($"[SpellCaster] inputReader from property: {this.inputReader != null}, type: {this.inputReader?.GetType().FullName}");

                    if (this.inputReader != null)
                    {
                        // List all properties to find the right one
                        var props = inputReader.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        Debug.Log($"[SpellCaster] InputReader has {props.Length} properties:");
                        foreach (var p in props)
                        {
                            Debug.Log($"[SpellCaster]   - {p.Name} ({p.PropertyType.Name})");
                        }

                        // Try common property names for getting the InputAction
                        // NOTE: Use inputActionValue for continuous reading, not inputActionPerformed (which is event-based)
                        string[] possibleProps = { "inputActionValue", "inputActionPerformed", "action", "inputAction", "inputActionReference", "reference" };
                        foreach (var propName in possibleProps)
                        {
                            var prop = inputReader.GetType().GetProperty(propName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (prop != null)
                            {
                                Debug.Log($"[SpellCaster] Found property '{propName}'! Trying to get value...");
                                var value = prop.GetValue(inputReader);

                                // Try casting to InputAction
                                triggerAction = value as InputAction;
                                if (triggerAction != null)
                                {
                                    triggerAction.Enable();
                                    Debug.Log($"[SpellCaster] ✅ SUCCESS via '{propName}'! Got InputAction on {gameObject.name}");
                                    if (castLight != null) castLight.color = Color.yellow;
                                    return;
                                }

                                // Try getting action from InputActionReference
                                var actionRef = value as InputActionReference;
                                if (actionRef != null && actionRef.action != null)
                                {
                                    triggerAction = actionRef.action;
                                    triggerAction.Enable();
                                    Debug.Log($"[SpellCaster] ✅ SUCCESS via '{propName}' -> action! Got InputAction on {gameObject.name}");
                                    if (castLight != null) castLight.color = Color.yellow;
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            Debug.LogWarning($"[SpellCaster] Could not find trigger action on {gameObject.name}!");
            if (castLight != null) castLight.color = Color.red; // Red = FAILED to find action!
        }

        private void OnEnable()
        {
            if (triggerAction != null)
                triggerAction.Enable();
        }

        private void OnDisable()
        {
            if (triggerAction != null)
                triggerAction.Disable();
        }

        private float debugTimer = 0f;

        private void Update()
        {
            // Debug: Log every 2 seconds to confirm Update is running
            debugTimer += Time.deltaTime;
            if (debugTimer > 2f)
            {
                Debug.Log($"[SpellCaster] Update running on {gameObject.name}, triggerAction: {triggerAction != null}, enabled: {triggerAction?.enabled}");
                debugTimer = 0f;
            }

            // Update cooldown
            if (cooldownRemaining > 0)
            {
                cooldownRemaining -= Time.deltaTime;
            }

            // Get current spell
            if (SpellManager.Instance != null)
            {
                currentSpell = SpellManager.Instance.GetCurrentSpell();
            }

            // Check trigger button
            bool triggerCurrentlyPressed = IsTriggerPressed();

            if (triggerCurrentlyPressed && !triggerPressed && cooldownRemaining <= 0)
            {
                // Trigger just pressed - cast spell
                Debug.Log($"[SpellCaster] Trigger pressed on {gameObject.name}! Attempting to cast spell...");
                CastSpell();
            }

            triggerPressed = triggerCurrentlyPressed;

            // Update cast light color based on current spell
            if (castLight != null && currentSpell != null)
            {
                castLight.color = currentSpell.spellColor;
                castLight.intensity = cooldownRemaining > 0 ? 0.5f : 1.5f;
            }
        }

        private bool IsTriggerPressed()
        {
            // Try reading from inputReader directly using reflection
            if (inputReader != null)
            {
                try
                {
                    // Try to call ReadValue() method on the inputReader
                    var readValueMethod = inputReader.GetType().GetMethod("ReadValue", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (readValueMethod != null)
                    {
                        var value = readValueMethod.Invoke(inputReader, null);
                        float triggerValue = System.Convert.ToSingle(value);

                        if (triggerValue > 0.01f)
                        {
                            Debug.Log($"[SpellCaster] 🎮 Trigger value FROM READER: {triggerValue} on {gameObject.name}");
                        }
                        return triggerValue > 0.5f;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SpellCaster] Error reading from inputReader: {e.Message}");
                }
            }

            // Fallback: Use the auto-detected trigger action
            if (triggerAction != null && triggerAction.enabled)
            {
                float triggerValue = triggerAction.ReadValue<float>();
                if (triggerValue > 0.01f) // Debug: log ANY trigger input (even tiny amounts)
                {
                    Debug.Log($"[SpellCaster] 🎮 Trigger value FROM ACTION: {triggerValue} on {gameObject.name}");
                }
                return triggerValue > 0.5f; // Trigger is pressed
            }
            else if (triggerAction == null && inputReader == null)
            {
                if (Time.frameCount % 120 == 0) // Log every 2 seconds
                {
                    Debug.LogWarning($"[SpellCaster] NO INPUT SOURCE on {gameObject.name}");
                }
            }

            return false;
        }

        private void CastSpell()
        {
            if (!canCast || currentSpell == null) return;

            Debug.Log($"[SpellCaster] Casting {currentSpell.spellName}");

            // Spawn projectile
            if (currentSpell.projectilePrefab != null && castPoint != null)
            {
                GameObject projectile = Instantiate(
                    currentSpell.projectilePrefab,
                    castPoint.position,
                    castPoint.rotation
                );

                // Set projectile velocity
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = castPoint.forward * currentSpell.projectileSpeed;
                }

                // Set projectile damage
                SpellProjectile projScript = projectile.GetComponent<SpellProjectile>();
                if (projScript != null)
                {
                    projScript.damage = currentSpell.damage;
                    projScript.spellColor = currentSpell.spellColor;
                    projScript.hitEffect = currentSpell.hitEffect;
                }

                // Destroy projectile after lifetime
                Destroy(projectile, currentSpell.projectileLifetime);
            }

            // Spawn cast effect
            if (currentSpell.castEffect != null)
            {
                GameObject effect = Instantiate(
                    currentSpell.castEffect,
                    castPoint.position,
                    castPoint.rotation
                );
                Destroy(effect, 2f);
            }

            // Play cast sound (TODO: Add AudioSource)

            // Start cooldown
            cooldownRemaining = currentSpell.castCooldown;

            // Haptic feedback (use HapticImpulsePlayer for XRI 3.0+)
            var hapticPlayer = controllerTransform?.GetComponent<UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticImpulsePlayer>();
            if (hapticPlayer != null)
            {
                hapticPlayer.SendHapticImpulse(0.5f, 0.1f);
                Debug.Log($"[SpellCaster] Sent haptic feedback on {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"[SpellCaster] No HapticImpulsePlayer found on {controllerTransform?.name}");
            }
        }

        /// <summary>
        /// Manually trigger a cast (for testing or alternate input methods)
        /// </summary>
        public void ManualCast()
        {
            CastSpell();
        }
    }
}
