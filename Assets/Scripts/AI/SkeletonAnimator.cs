using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Procedural animation for skeleton monsters
    /// Animates bones (arms, legs, ribs) based on movement state
    /// </summary>
    public class SkeletonAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Walk cycle speed (higher = faster animation)")]
        public float walkCycleSpeed = 4f;

        [Tooltip("Leg swing angle when walking")]
        public float legSwingAngle = 30f;

        [Tooltip("Arm swing angle when walking")]
        public float armSwingAngle = 20f;

        [Tooltip("Idle sway amount")]
        public float idleSwayAmount = 3f;

        [Tooltip("Idle sway speed")]
        public float idleSwaySpeed = 1.5f;

        [Header("Debug")]
        public bool showDebug = false;

        // Bone references (found by name in MonsterBuilder skeleton structure)
        private Transform leftUpperLeg;
        private Transform leftLowerLeg;
        private Transform rightUpperLeg;
        private Transform rightLowerLeg;
        private Transform leftUpperArm;
        private Transform leftLowerArm;
        private Transform rightUpperArm;
        private Transform rightLowerArm;
        private Transform body;
        private Transform head;

        // Animation state
        private float walkCycle = 0f;
        private float idleCycle = 0f;
        private bool isWalking = false;
        private Vector3 lastPosition;
        private MonsterAI monsterAI;

        // Store original local rotations
        private Quaternion leftUpperLegOriginal;
        private Quaternion leftLowerLegOriginal;
        private Quaternion rightUpperLegOriginal;
        private Quaternion rightLowerLegOriginal;
        private Quaternion leftUpperArmOriginal;
        private Quaternion leftLowerArmOriginal;
        private Quaternion rightUpperArmOriginal;
        private Quaternion rightLowerArmOriginal;
        private Quaternion bodyOriginal;
        private Quaternion headOriginal;

        void Start()
        {
            // Find bone transforms by name (MonsterBuilder naming convention)
            FindBones();

            // Store original rotations
            StoreOriginalRotations();

            lastPosition = transform.position;
            monsterAI = GetComponent<MonsterAI>();

            if (showDebug)
                Debug.Log($"[SkeletonAnimator] Initialized on {gameObject.name}");
        }

        void FindBones()
        {
            // MonsterBuilder creates bones with specific names
            body = transform.Find("Body");
            head = transform.Find("Head");

            // Arms (MonsterBuilder names: "LeftArmUpper" -> "LeftArmLower" -> "Hand")
            leftUpperArm = transform.Find("LeftArmUpper");
            if (leftUpperArm != null)
            {
                leftLowerArm = leftUpperArm.Find("LeftArmLower");
            }

            rightUpperArm = transform.Find("RightArmUpper");
            if (rightUpperArm != null)
            {
                rightLowerArm = rightUpperArm.Find("RightArmLower");
            }

            // Legs (MonsterBuilder names: "LeftLegUpper" -> "LeftLegLower" -> "Foot")
            leftUpperLeg = transform.Find("LeftLegUpper");
            if (leftUpperLeg != null)
            {
                leftLowerLeg = leftUpperLeg.Find("LeftLegLower");
            }

            rightUpperLeg = transform.Find("RightLegUpper");
            if (rightUpperLeg != null)
            {
                rightLowerLeg = rightUpperLeg.Find("RightLegLower");
            }

            if (showDebug)
            {
                Debug.Log($"[SkeletonAnimator] Found bones - Body: {body != null}, Head: {head != null}, " +
                          $"LeftArmUpper: {leftUpperArm != null}, LeftArmLower: {leftLowerArm != null}, " +
                          $"RightArmUpper: {rightUpperArm != null}, RightArmLower: {rightLowerArm != null}, " +
                          $"LeftLegUpper: {leftUpperLeg != null}, LeftLegLower: {leftLowerLeg != null}, " +
                          $"RightLegUpper: {rightUpperLeg != null}, RightLegLower: {rightLowerLeg != null}");
            }
        }

        void StoreOriginalRotations()
        {
            if (leftUpperLeg != null) leftUpperLegOriginal = leftUpperLeg.localRotation;
            if (leftLowerLeg != null) leftLowerLegOriginal = leftLowerLeg.localRotation;
            if (rightUpperLeg != null) rightUpperLegOriginal = rightUpperLeg.localRotation;
            if (rightLowerLeg != null) rightLowerLegOriginal = rightLowerLeg.localRotation;
            if (leftUpperArm != null) leftUpperArmOriginal = leftUpperArm.localRotation;
            if (leftLowerArm != null) leftLowerArmOriginal = leftLowerArm.localRotation;
            if (rightUpperArm != null) rightUpperArmOriginal = rightUpperArm.localRotation;
            if (rightLowerArm != null) rightLowerArmOriginal = rightLowerArm.localRotation;
            if (body != null) bodyOriginal = body.localRotation;
            if (head != null) headOriginal = head.localRotation;
        }

        void Update()
        {
            // Detect if skeleton is moving
            Vector3 currentPosition = transform.position;
            float movementSpeed = (currentPosition - lastPosition).magnitude / Time.deltaTime;
            lastPosition = currentPosition;

            // Consider walking if moving faster than 0.1 units/sec
            isWalking = movementSpeed > 0.1f;

            if (isWalking)
            {
                AnimateWalking();
            }
            else
            {
                AnimateIdle();
            }
        }

        void AnimateWalking()
        {
            // Advance walk cycle
            walkCycle += Time.deltaTime * walkCycleSpeed;

            // Reset idle cycle
            idleCycle = 0f;

            // Leg animation (opposing legs)
            float leftLegAngle = Mathf.Sin(walkCycle) * legSwingAngle;
            float rightLegAngle = Mathf.Sin(walkCycle + Mathf.PI) * legSwingAngle;

            if (leftUpperLeg != null)
            {
                leftUpperLeg.localRotation = leftUpperLegOriginal * Quaternion.Euler(leftLegAngle, 0, 0);
            }
            if (rightUpperLeg != null)
            {
                rightUpperLeg.localRotation = rightUpperLegOriginal * Quaternion.Euler(rightLegAngle, 0, 0);
            }

            // Lower leg bends more when upper leg swings back
            if (leftLowerLeg != null)
            {
                float leftLowerBend = Mathf.Max(0, -leftLegAngle * 0.5f);
                leftLowerLeg.localRotation = leftLowerLegOriginal * Quaternion.Euler(leftLowerBend, 0, 0);
            }
            if (rightLowerLeg != null)
            {
                float rightLowerBend = Mathf.Max(0, -rightLegAngle * 0.5f);
                rightLowerLeg.localRotation = rightLowerLegOriginal * Quaternion.Euler(rightLowerBend, 0, 0);
            }

            // Arm animation (opposing arms, opposite to legs)
            float leftArmAngle = Mathf.Sin(walkCycle + Mathf.PI) * armSwingAngle;
            float rightArmAngle = Mathf.Sin(walkCycle) * armSwingAngle;

            if (leftUpperArm != null)
            {
                leftUpperArm.localRotation = leftUpperArmOriginal * Quaternion.Euler(leftArmAngle, 0, 0);
            }
            if (rightUpperArm != null)
            {
                rightUpperArm.localRotation = rightUpperArmOriginal * Quaternion.Euler(rightArmAngle, 0, 0);
            }

            // Lower arms bend slightly
            if (leftLowerArm != null)
            {
                leftLowerArm.localRotation = leftLowerArmOriginal * Quaternion.Euler(-10, 0, 0);
            }
            if (rightLowerArm != null)
            {
                rightLowerArm.localRotation = rightLowerArmOriginal * Quaternion.Euler(-10, 0, 0);
            }

            // Body bob (subtle up/down)
            if (body != null)
            {
                float bodyBob = Mathf.Sin(walkCycle * 2f) * 2f; // Small angle
                body.localRotation = bodyOriginal * Quaternion.Euler(0, 0, bodyBob);
            }
        }

        void AnimateIdle()
        {
            // Advance idle cycle
            idleCycle += Time.deltaTime * idleSwaySpeed;

            // Reset walk cycle
            walkCycle = 0f;

            // Subtle swaying motion
            float sway = Mathf.Sin(idleCycle) * idleSwayAmount;

            // Body sway
            if (body != null)
            {
                body.localRotation = bodyOriginal * Quaternion.Euler(0, 0, sway);
            }

            // Head slight tilt
            if (head != null)
            {
                float headTilt = Mathf.Sin(idleCycle * 0.7f) * (idleSwayAmount * 0.5f);
                head.localRotation = headOriginal * Quaternion.Euler(0, headTilt, 0);
            }

            // Arms hang naturally
            if (leftUpperArm != null)
            {
                float armSway = Mathf.Sin(idleCycle * 0.5f) * (idleSwayAmount * 0.3f);
                leftUpperArm.localRotation = leftUpperArmOriginal * Quaternion.Euler(armSway, 0, 0);
            }
            if (rightUpperArm != null)
            {
                float armSway = Mathf.Sin(idleCycle * 0.5f + Mathf.PI) * (idleSwayAmount * 0.3f);
                rightUpperArm.localRotation = rightUpperArmOriginal * Quaternion.Euler(armSway, 0, 0);
            }

            // Legs return to neutral
            if (leftUpperLeg != null)
                leftUpperLeg.localRotation = leftUpperLegOriginal;
            if (leftLowerLeg != null)
                leftLowerLeg.localRotation = leftLowerLegOriginal;
            if (rightUpperLeg != null)
                rightUpperLeg.localRotation = rightUpperLegOriginal;
            if (rightLowerLeg != null)
                rightLowerLeg.localRotation = rightLowerLegOriginal;
            if (leftLowerArm != null)
                leftLowerArm.localRotation = leftLowerArmOriginal;
            if (rightLowerArm != null)
                rightLowerArm.localRotation = rightLowerArmOriginal;
        }
    }
}
