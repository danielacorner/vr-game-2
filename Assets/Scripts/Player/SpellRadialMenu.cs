using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Radial spell selection menu (Half-Life: Alyx style)
    /// Appears when thumbstick button is held
    /// Selection based on controller direction
    /// </summary>
    public class SpellRadialMenu : MonoBehaviour
    {
        [Header("References")]
        public ActionBasedController controller; // XR Controller
        public Transform menuCanvas; // World space canvas
        public RectTransform menuContainer; // Container for spell icons

        [Header("Menu Settings")]
        public float menuRadius = 0.3f; // Distance from controller
        public float iconRadius = 0.15f; // Radius of icon circle
        public float selectionDeadzone = 0.3f; // Thumbstick must move this much

        [Header("Visual Feedback")]
        public Image[] spellIcons = new Image[4]; // 4 spell slots
        public Image[] highlightRings = new Image[4]; // Highlight for hovered spell
        public Color normalColor = new Color(1f, 1f, 1f, 0.6f);
        public Color highlightColor = new Color(1f, 1f, 0f, 1f);

        private bool menuActive = false;
        private int hoveredIndex = -1;
        private Vector2 thumbstickInput;

        private void Start()
        {
            // Hide menu initially
            if (menuCanvas != null)
                menuCanvas.gameObject.SetActive(false);

            // Setup spell icons
            UpdateSpellIcons();
        }

        private void Update()
        {
            if (controller == null) return;

            // Get thumbstick button state (typically the thumbstick click)
            // We'll use the thumbstick position as input
            bool thumbstickPressed = IsThumbstickPressed();

            if (thumbstickPressed && !menuActive)
            {
                // Show menu
                ShowMenu();
            }
            else if (!thumbstickPressed && menuActive)
            {
                // Hide menu and select hovered spell
                SelectHoveredSpell();
                HideMenu();
            }

            // Update menu if active
            if (menuActive)
            {
                UpdateMenuPosition();
                UpdateSelection();
            }
        }

        private bool IsThumbstickPressed()
        {
            // Check if thumbstick is pressed/moved significantly
            // We'll use the magnitude of thumbstick input as "pressed"
            if (controller.selectAction != null && controller.selectAction.action != null)
            {
                thumbstickInput = controller.selectAction.action.ReadValue<Vector2>();
                return thumbstickInput.magnitude > selectionDeadzone;
            }
            return false;
        }

        private void ShowMenu()
        {
            menuActive = true;
            if (menuCanvas != null)
                menuCanvas.gameObject.SetActive(true);

            Debug.Log("[RadialMenu] Menu opened");
        }

        private void HideMenu()
        {
            menuActive = false;
            if (menuCanvas != null)
                menuCanvas.gameObject.SetActive(false);

            hoveredIndex = -1;
            Debug.Log("[RadialMenu] Menu closed");
        }

        private void UpdateMenuPosition()
        {
            // Position menu in front of controller
            if (menuCanvas != null && controller != null)
            {
                Vector3 forward = controller.transform.forward;
                Vector3 menuPosition = controller.transform.position + forward * menuRadius;

                menuCanvas.position = menuPosition;
                menuCanvas.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
        }

        private void UpdateSelection()
        {
            // Get thumbstick input (already read in IsThumbstickPressed)
            if (thumbstickInput.magnitude < selectionDeadzone)
            {
                hoveredIndex = -1;
                UpdateVisuals();
                return;
            }

            // Calculate angle from thumbstick input
            float angle = Mathf.Atan2(thumbstickInput.y, thumbstickInput.x) * Mathf.Rad2Deg;

            // Convert to 0-360 range
            if (angle < 0) angle += 360f;

            // Determine which quadrant (4 spells = 4 quadrants)
            int spellCount = SpellManager.Instance?.availableSpells.Count ?? 4;
            float segmentAngle = 360f / spellCount;

            // Adjust angle so 0° is at top
            angle = (angle + 90f) % 360f;

            hoveredIndex = Mathf.FloorToInt(angle / segmentAngle);

            UpdateVisuals();
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
                Debug.Log($"[RadialMenu] Selected spell at index {hoveredIndex}");
            }
        }

        private void UpdateSpellIcons()
        {
            if (SpellManager.Instance == null) return;

            for (int i = 0; i < spellIcons.Length; i++)
            {
                if (i < SpellManager.Instance.availableSpells.Count)
                {
                    SpellData spell = SpellManager.Instance.availableSpells[i];
                    if (spellIcons[i] != null)
                    {
                        spellIcons[i].sprite = spell.icon;
                        spellIcons[i].color = spell.spellColor;
                    }
                }
            }
        }

        private void OnEnable()
        {
            // Refresh icons when enabled
            UpdateSpellIcons();
        }
    }
}
