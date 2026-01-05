using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Half-Life: Alyx style radial menu for spell selection
    /// Hold thumbstick button + move thumbstick to select spell
    /// Release to confirm selection
    /// Compatible with XRI 3.0+
    /// </summary>
    public class SpellRadialMenuUI : MonoBehaviour
    {
        [Header("References")]
        public Transform controllerTransform; // Just the transform, not the controller component
        public Transform menuCanvas;
        public RectTransform menuContainer;

        [Header("UI Elements")]
        public Image[] spellIcons = new Image[4];
        public Image[] highlightRings = new Image[4];

        [Header("Settings")]
        public float selectionDeadzone = 0.3f; // Thumbstick must move this much
        public Color normalColor = new Color(1f, 1f, 1f, 0.6f);
        public Color highlightColor = new Color(1f, 1f, 0f, 1f);
        public float menuDistance = 0.3f; // Distance from controller

        [Header("Input (Auto-detected)")]
        public InputAction thumbstickAction; // Auto-find in Start

        private bool menuActive = false;
        private int hoveredIndex = -1;
        private Vector2 thumbstickInput;
        private bool thumbstickPressed = false;
        private float thumbstickPressStartTime = 0f;

        private void Start()
        {
            // Hide menu initially
            if (menuCanvas != null)
                menuCanvas.gameObject.SetActive(false);

            // Try to auto-find thumbstick input action
            TryFindThumbstickAction();

            // Enable input action
            if (thumbstickAction != null)
            {
                thumbstickAction.Enable();
            }

            // Update spell icons from SpellManager
            UpdateSpellIcons();
        }

        private void OnEnable()
        {
            if (thumbstickAction != null)
                thumbstickAction.Enable();
            UpdateSpellIcons();
        }

        private void OnDisable()
        {
            if (thumbstickAction != null)
                thumbstickAction.Disable();
        }

        private void TryFindThumbstickAction()
        {
            // Try to find the thumbstick action from XRI Default Input Actions
            var inputActionAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            foreach (var asset in inputActionAssets)
            {
                if (asset.name.Contains("XRI") || asset.name.Contains("Input"))
                {
                    // Look for right thumbstick action
                    var action = asset.FindAction("XRI Right/Thumbstick");
                    if (action != null)
                    {
                        thumbstickAction = action;
                        Debug.Log("[RadialMenuUI] Found thumbstick action");
                        return;
                    }
                }
            }

            Debug.LogWarning("[RadialMenuUI] Could not auto-find thumbstick input action");
        }

        private void Update()
        {
            if (controllerTransform == null) return;

            // Read thumbstick button state (press/click)
            bool currentlyPressed = IsThumbstickButtonPressed();

            // Read thumbstick movement
            thumbstickInput = GetThumbstickInput();

            // Menu state machine
            if (currentlyPressed && !thumbstickPressed)
            {
                // Button just pressed - show menu
                ShowMenu();
            }
            else if (!currentlyPressed && thumbstickPressed)
            {
                // Button just released - select and hide menu
                SelectHoveredSpell();
                HideMenu();
            }

            thumbstickPressed = currentlyPressed;

            // Update menu if active
            if (menuActive)
            {
                UpdateMenuPosition();
                UpdateSelection();
                UpdateVisuals();
            }
        }

        private bool IsThumbstickButtonPressed()
        {
            // Read thumbstick input first
            Vector2 input = GetThumbstickInput();

            // Consider it "pressed" if thumbstick has significant input and held for a moment
            bool hasSignificantInput = input.magnitude > selectionDeadzone;

            if (hasSignificantInput && thumbstickPressStartTime == 0f)
            {
                thumbstickPressStartTime = Time.time;
            }
            else if (!hasSignificantInput)
            {
                thumbstickPressStartTime = 0f;
            }

            // Menu activates after holding thumbstick for 0.1 seconds
            return hasSignificantInput && (Time.time - thumbstickPressStartTime) > 0.1f;
        }

        private Vector2 GetThumbstickInput()
        {
            // Use the auto-detected thumbstick action
            if (thumbstickAction != null && thumbstickAction.enabled)
            {
                return thumbstickAction.ReadValue<Vector2>();
            }

            return Vector2.zero;
        }

        private void ShowMenu()
        {
            menuActive = true;
            if (menuCanvas != null)
                menuCanvas.gameObject.SetActive(true);

            Debug.Log("[RadialMenuUI] Menu opened");

            // Haptic feedback (find controller component if needed)
            var controllerComponent = controllerTransform?.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
            if (controllerComponent != null)
            {
                controllerComponent.SendHapticImpulse(0.3f, 0.1f);
            }
        }

        private void HideMenu()
        {
            menuActive = false;
            if (menuCanvas != null)
                menuCanvas.gameObject.SetActive(false);

            hoveredIndex = -1;
            Debug.Log("[RadialMenuUI] Menu closed");
        }

        private void UpdateMenuPosition()
        {
            // Position menu in front of controller
            if (menuCanvas != null && controllerTransform != null)
            {
                Vector3 forward = controllerTransform.forward;
                Vector3 menuPosition = controllerTransform.position + forward * menuDistance;

                menuCanvas.position = menuPosition;
                menuCanvas.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
        }

        private void UpdateSelection()
        {
            // No selection if thumbstick not moved enough
            if (thumbstickInput.magnitude < selectionDeadzone)
            {
                hoveredIndex = -1;
                return;
            }

            // Calculate angle from thumbstick input
            // Angle is in degrees, 0° = right, 90° = up
            float angle = Mathf.Atan2(thumbstickInput.y, thumbstickInput.x) * Mathf.Rad2Deg;

            // Normalize to 0-360
            if (angle < 0) angle += 360f;

            // Map to 4 quadrants (each 90°)
            // Top = 45° to 135°, Right = 315° to 45°, Bottom = 135° to 225°, Left = 225° to 315°
            if (angle >= 45f && angle < 135f)
                hoveredIndex = 0; // Top
            else if (angle >= 315f || angle < 45f)
                hoveredIndex = 1; // Right
            else if (angle >= 135f && angle < 225f)
                hoveredIndex = 2; // Bottom
            else
                hoveredIndex = 3; // Left
        }

        private void UpdateVisuals()
        {
            // Update highlight rings
            for (int i = 0; i < highlightRings.Length; i++)
            {
                if (highlightRings[i] != null)
                {
                    if (i == hoveredIndex)
                    {
                        highlightRings[i].color = highlightColor;
                        highlightRings[i].transform.localScale = Vector3.one * 1.2f;
                    }
                    else
                    {
                        highlightRings[i].color = normalColor;
                        highlightRings[i].transform.localScale = Vector3.one;
                    }
                }
            }
        }

        private void SelectHoveredSpell()
        {
            if (hoveredIndex >= 0 && SpellManager.Instance != null)
            {
                SpellManager.Instance.SelectSpell(hoveredIndex);
                Debug.Log($"[RadialMenuUI] Selected spell at index {hoveredIndex}");

                // Haptic feedback (find controller component if needed)
                var controllerComponent = controllerTransform?.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
                if (controllerComponent != null)
                {
                    controllerComponent.SendHapticImpulse(0.5f, 0.2f);
                }
            }
        }

        private void UpdateSpellIcons()
        {
            if (SpellManager.Instance == null) return;

            for (int i = 0; i < spellIcons.Length && i < SpellManager.Instance.availableSpells.Count; i++)
            {
                SpellData spell = SpellManager.Instance.availableSpells[i];
                if (spellIcons[i] != null)
                {
                    spellIcons[i].color = spell.spellColor;
                    if (spell.icon != null)
                    {
                        spellIcons[i].sprite = spell.icon;
                    }
                }
            }
        }
    }
}
