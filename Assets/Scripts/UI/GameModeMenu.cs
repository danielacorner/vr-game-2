using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRDungeonCrawler.Core;

namespace VRDungeonCrawler.UI
{
    /// <summary>
    /// Game mode selection menu that appears when entering portal
    /// Allows player to choose game mode before starting dungeon
    /// </summary>
    public class GameModeMenu : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Parent panel containing all UI elements")]
        public GameObject menuPanel;

        [Tooltip("Game mode buttons")]
        public Button standardModeButton;
        public Button challengeModeButton;
        public Button endlessModeButton;

        [Tooltip("Play button to confirm selection")]
        public Button playButton;

        [Header("Mode Indicators")]
        [Tooltip("Text showing currently selected mode")]
        public TextMeshProUGUI selectedModeText;

        [Tooltip("Description text for selected mode")]
        public TextMeshProUGUI modeDescriptionText;

        [Header("Settings")]
        [Tooltip("Distance from player to show menu")]
        public float menuDistance = 2f;

        [Tooltip("Height offset from player eye level")]
        public float menuHeightOffset = -0.3f;

        private string selectedMode = "Standard";
        private Transform playerCamera;

        private void Start()
        {
            // Find player camera
            playerCamera = Camera.main.transform;

            // Setup button listeners
            if (standardModeButton != null)
                standardModeButton.onClick.AddListener(() => SelectMode("Standard"));

            if (challengeModeButton != null)
                challengeModeButton.onClick.AddListener(() => SelectMode("Challenge"));

            if (endlessModeButton != null)
                endlessModeButton.onClick.AddListener(() => SelectMode("Endless"));

            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            // Select standard mode by default
            SelectMode("Standard");

            // Hide menu initially
            if (menuPanel != null)
                menuPanel.SetActive(false);
        }

        /// <summary>
        /// Shows the menu in front of the player
        /// </summary>
        public void ShowMenu()
        {
            if (menuPanel != null)
                menuPanel.SetActive(true);

            // Position menu in front of player
            PositionMenuInFrontOfPlayer();
        }

        /// <summary>
        /// Hides the menu
        /// </summary>
        public void HideMenu()
        {
            if (menuPanel != null)
                menuPanel.SetActive(false);
        }

        /// <summary>
        /// Positions the menu in front of the player's view
        /// </summary>
        private void PositionMenuInFrontOfPlayer()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main.transform;
                if (playerCamera == null)
                {
                    Debug.LogError("[GameModeMenu] Cannot find player camera!");
                    return;
                }
            }

            // Position menu in front of player
            Vector3 forwardDirection = playerCamera.forward;
            forwardDirection.y = 0; // Keep menu horizontal
            forwardDirection.Normalize();

            Vector3 menuPosition = playerCamera.position + forwardDirection * menuDistance;
            menuPosition.y = playerCamera.position.y + menuHeightOffset;

            transform.position = menuPosition;

            // Face the player
            Vector3 lookDirection = playerCamera.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        /// <summary>
        /// Selects a game mode
        /// </summary>
        private void SelectMode(string mode)
        {
            selectedMode = mode;

            // Update UI
            UpdateModeDisplay();

            // Update button visuals (highlight selected)
            UpdateButtonStates();

            Debug.Log($"[GameModeMenu] Selected mode: {mode}");
        }

        /// <summary>
        /// Updates the mode display text
        /// </summary>
        private void UpdateModeDisplay()
        {
            if (selectedModeText != null)
            {
                selectedModeText.text = $"Mode: {selectedMode}";
            }

            if (modeDescriptionText != null)
            {
                string description = selectedMode switch
                {
                    "Standard" => "Classic dungeon crawl with balanced difficulty. Complete rooms to progress.",
                    "Challenge" => "Increased enemy count and damage. For experienced adventurers.",
                    "Endless" => "Survive as long as you can. Difficulty increases with each room.",
                    _ => ""
                };
                modeDescriptionText.text = description;
            }
        }

        /// <summary>
        /// Updates button visual states to show selection
        /// </summary>
        private void UpdateButtonStates()
        {
            // Highlight selected button
            SetButtonHighlight(standardModeButton, selectedMode == "Standard");
            SetButtonHighlight(challengeModeButton, selectedMode == "Challenge");
            SetButtonHighlight(endlessModeButton, selectedMode == "Endless");
        }

        /// <summary>
        /// Sets button highlight state
        /// </summary>
        private void SetButtonHighlight(Button button, bool highlighted)
        {
            if (button == null) return;

            ColorBlock colors = button.colors;
            if (highlighted)
            {
                colors.normalColor = new Color(0.2f, 0.8f, 0.2f); // Green highlight
            }
            else
            {
                colors.normalColor = Color.white;
            }
            button.colors = colors;
        }

        /// <summary>
        /// Called when Play button is clicked
        /// </summary>
        private void OnPlayClicked()
        {
            Debug.Log($"[GameModeMenu] Starting dungeon with mode: {selectedMode}");

            // Store selected mode in GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(selectedMode);
                GameManager.Instance.EnterDungeon();
            }
            else
            {
                Debug.LogError("[GameModeMenu] GameManager not found!");
            }

            // Hide menu
            HideMenu();
        }

        private void Update()
        {
            // Keep menu facing player
            if (menuPanel != null && menuPanel.activeSelf)
            {
                PositionMenuInFrontOfPlayer();
            }
        }
    }
}
