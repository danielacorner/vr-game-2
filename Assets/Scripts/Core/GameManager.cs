using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRDungeonCrawler.Core
{
    /// <summary>
    /// Main game manager - handles game state and scene transitions
    /// Singleton pattern for easy access
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        public GameState currentState = GameState.Home;

        [Header("Scene Names")]
        public string homeSceneName = "HomeArea";
        public string dungeonSceneName = "Dungeon";

        [Header("Game Mode")]
        public string currentGameMode = "Standard";

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Debug.Log("[GameManager] Starting game in Home state");
        }

        public void EnterHomeArea()
        {
            Debug.Log("[GameManager] Entering home area");
            currentState = GameState.Home;
            SceneManager.LoadScene(homeSceneName);
        }

        public void EnterDungeon()
        {
            Debug.Log("[GameManager] Entering dungeon");
            currentState = GameState.Playing;
            SceneManager.LoadScene(dungeonSceneName);
        }

        public void ReturnToHome()
        {
            Debug.Log("[GameManager] Returning to home");
            EnterHomeArea();
        }

        public void SetGameMode(string mode)
        {
            currentGameMode = mode;
            Debug.Log($"[GameManager] Game mode set to: {mode}");
        }
    }

    public enum GameState
    {
        Home,
        Playing,
        Paused,
        GameOver
    }
}
