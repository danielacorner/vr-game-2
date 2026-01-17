using UnityEngine;
using UnityEditor;

public class StartPortalParticles
{
    [MenuItem("Tools/VR Dungeon Crawler/Start Portal Particles")]
    static void StartParticles()
    {
        GameObject portal = GameObject.Find("Portal");
        if (portal == null)
        {
            Debug.LogError("[StartPortalParticles] Portal GameObject not found!");
            return;
        }

        // Find the Particles child
        Transform particlesTransform = portal.transform.Find("Particles");
        if (particlesTransform == null)
        {
            Debug.LogError("[StartPortalParticles] Particles child not found!");
            return;
        }

        ParticleSystem ps = particlesTransform.GetComponent<ParticleSystem>();
        if (ps == null)
        {
            Debug.LogError("[StartPortalParticles] ParticleSystem component not found!");
            return;
        }

        // Start the particle system
        ps.Play();
        Debug.Log("[StartPortalParticles] Portal particles started!");
    }
}
