using UnityEngine;

namespace VRDungeonCrawler.Environment
{
    /// <summary>
    /// One-click setup for all atmospheric elements in the home area
    /// Automatically creates skybox, campfire, fog, particles, and portal with proper positioning
    /// </summary>
    public class HomeAreaAtmosphereSetup : MonoBehaviour
    {
        [Header("Auto-Setup")]
        [Tooltip("Click to automatically create all atmospheric elements")]
        public bool autoSetup = true;

        [Header("Element Toggles")]
        public bool createSkybox = true;
        public bool createCampfire = true;
        public bool createGroundFog = true;
        public bool createAmbientParticles = true;
        public bool createPortal = true;

        [Header("Positions")]
        [Tooltip("Campfire position (center of home area)")]
        public Vector3 campfirePosition = new Vector3(-8f, 0f, -8f);

        [Tooltip("Portal position (edge of home area)")]
        public Vector3 portalPosition = new Vector3(20f, 0f, 20f);

        [Header("Debug")]
        public bool showDebug = true;

        void Start()
        {
            if (autoSetup)
            {
                SetupAllElements();
            }
        }

        [ContextMenu("Setup All Atmospheric Elements")]
        public void SetupAllElements()
        {
            if (showDebug)
                Debug.Log("[HomeAreaAtmosphereSetup] Setting up atmospheric elements...");

            if (createSkybox)
                SetupSkybox();

            if (createCampfire)
                SetupCampfire();

            if (createGroundFog)
                SetupGroundFog();

            if (createAmbientParticles)
                SetupAmbientParticles();

            if (createPortal)
                SetupPortal();

            if (showDebug)
                Debug.Log("[HomeAreaAtmosphereSetup] ✓ All atmospheric elements created!");
        }

        void SetupSkybox()
        {
            GameObject skyboxObj = new GameObject("ProceduralSkybox");
            skyboxObj.transform.SetParent(transform);
            skyboxObj.transform.position = Vector3.zero;

            ProceduralSkybox skybox = skyboxObj.AddComponent<ProceduralSkybox>();
            skybox.starCount = 800;
            skybox.showMilkyWay = true;
            skybox.showDebug = showDebug;

            if (showDebug)
                Debug.Log("[HomeAreaAtmosphereSetup] ✓ Skybox created");
        }

        void SetupCampfire()
        {
            GameObject campfireObj = new GameObject("EnhancedCampfire");
            campfireObj.transform.SetParent(transform);
            campfireObj.transform.position = campfirePosition;

            EnhancedCampfire campfire = campfireObj.AddComponent<EnhancedCampfire>();
            campfire.fireIntensity = 1.2f;
            campfire.showEmbers = true;
            campfire.showDebug = showDebug;

            if (showDebug)
                Debug.Log($"[HomeAreaAtmosphereSetup] ✓ Campfire created at {campfirePosition}");
        }

        void SetupGroundFog()
        {
            GameObject fogObj = new GameObject("GroundFog");
            fogObj.transform.SetParent(transform);
            fogObj.transform.position = Vector3.zero;

            GroundFogEffect fog = fogObj.AddComponent<GroundFogEffect>();
            fog.fogRadius = 30f;
            fog.fogDensity = 40;
            fog.enableDrift = true;
            fog.showDebug = showDebug;

            if (showDebug)
                Debug.Log("[HomeAreaAtmosphereSetup] ✓ Ground fog created");
        }

        void SetupAmbientParticles()
        {
            GameObject particlesObj = new GameObject("AmbientParticles");
            particlesObj.transform.SetParent(transform);
            particlesObj.transform.position = Vector3.zero;

            AmbientParticles particles = particlesObj.AddComponent<AmbientParticles>();
            particles.enableFireflies = true;
            particles.fireflyCount = 20;
            particles.enableDustMotes = true;
            particles.enableMagicalSparkles = true;
            particles.sparkleCenter = portalPosition;
            particles.sparkleRadius = 8f;
            particles.showDebug = showDebug;

            if (showDebug)
                Debug.Log("[HomeAreaAtmosphereSetup] ✓ Ambient particles created");
        }

        void SetupPortal()
        {
            GameObject portalObj = new GameObject("EnhancedPortal");
            portalObj.transform.SetParent(transform);
            portalObj.transform.position = portalPosition;
            portalObj.transform.rotation = Quaternion.Euler(0, 45, 0);

            // Add trigger collider for portal
            SphereCollider trigger = portalObj.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 2f;

            EnhancedPortal portal = portalObj.AddComponent<EnhancedPortal>();
            portal.portalSize = 2.5f;
            portal.targetSceneName = "DungeonEntrance";
            portal.requireButtonPress = false; // Auto-teleport
            portal.showDebug = showDebug;

            if (showDebug)
                Debug.Log($"[HomeAreaAtmosphereSetup] ✓ Portal created at {portalPosition}");
        }

        void OnDrawGizmosSelected()
        {
            // Draw campfire position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(campfirePosition, 1f);
            Gizmos.DrawLine(campfirePosition, campfirePosition + Vector3.up * 2f);

            // Draw portal position
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(portalPosition, 2f);
            Gizmos.DrawLine(portalPosition, portalPosition + Vector3.up * 3f);
        }
    }
}
