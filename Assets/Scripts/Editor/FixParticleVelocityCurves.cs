using UnityEngine;
using UnityEditor;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Fixes "Particle Velocity curves must all be in the same mode" error
    /// Ensures all particle systems have velocity curves in the same mode
    /// </summary>
    public class FixParticleVelocityCurves : UnityEditor.Editor
    {
        [MenuItem("Tools/VR Dungeon Crawler/Fix Particle Velocity Curves")]
        public static void FixAllParticleSystems()
        {
            Debug.Log("========================================");
            Debug.Log("Fixing Particle Velocity Curves");
            Debug.Log("========================================");

            // Find all particle systems in the scene
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();
            int fixedCount = 0;

            foreach (ParticleSystem ps in particleSystems)
            {
                if (FixParticleSystem(ps))
                {
                    fixedCount++;
                    EditorUtility.SetDirty(ps);
                }
            }

            Debug.Log($"✓ Fixed {fixedCount} particle systems");
            Debug.Log("========================================");
        }

        static bool FixParticleSystem(ParticleSystem ps)
        {
            bool wasFixed = false;

            // Fix Velocity Over Lifetime module
            var velocityModule = ps.velocityOverLifetime;
            if (velocityModule.enabled)
            {
                // Set all axes to use Constant mode (simplest fix)
                velocityModule.x = new ParticleSystem.MinMaxCurve(0f);
                velocityModule.y = new ParticleSystem.MinMaxCurve(0f);
                velocityModule.z = new ParticleSystem.MinMaxCurve(0f);
                velocityModule.space = ParticleSystemSimulationSpace.Local;

                Debug.Log($"✓ Fixed velocity curves for: {ps.gameObject.name}");
                wasFixed = true;
            }

            // Also check Force Over Lifetime
            var forceModule = ps.forceOverLifetime;
            if (forceModule.enabled)
            {
                forceModule.x = new ParticleSystem.MinMaxCurve(0f);
                forceModule.y = new ParticleSystem.MinMaxCurve(0f);
                forceModule.z = new ParticleSystem.MinMaxCurve(0f);
            }

            // Check Limit Velocity Over Lifetime
            var limitVelocity = ps.limitVelocityOverLifetime;
            if (limitVelocity.enabled && limitVelocity.separateAxes)
            {
                limitVelocity.limitX = new ParticleSystem.MinMaxCurve(1f);
                limitVelocity.limitY = new ParticleSystem.MinMaxCurve(1f);
                limitVelocity.limitZ = new ParticleSystem.MinMaxCurve(1f);
            }

            return wasFixed;
        }
    }
}
