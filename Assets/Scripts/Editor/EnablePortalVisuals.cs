using UnityEngine;
using UnityEditor;

public class EnablePortalVisuals
{
    [MenuItem("Tools/VR Dungeon Crawler/Enable Portal Visuals")]
    static void EnablePortal()
    {
        GameObject portal = GameObject.Find("Portal");
        if (portal == null)
        {
            Debug.LogError("Portal GameObject not found!");
            return;
        }

        // Enable all portal children
        Transform outerRing = portal.transform.Find("OuterRing");
        Transform middleRing = portal.transform.Find("MiddleRing");
        Transform innerRing = portal.transform.Find("InnerRing");
        Transform particles = portal.transform.Find("Particles");

        if (outerRing != null) outerRing.gameObject.SetActive(true);
        if (middleRing != null) middleRing.gameObject.SetActive(true);
        if (innerRing != null) innerRing.gameObject.SetActive(true);
        if (particles != null) particles.gameObject.SetActive(true);

        Debug.Log("[EnablePortalVisuals] Enabled all portal visual elements!");
    }
}
