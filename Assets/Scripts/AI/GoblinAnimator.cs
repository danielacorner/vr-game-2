using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Procedural animation for Goblin monsters
    /// Handles walking, idle, and attack animations
    /// </summary>
    public class GoblinAnimator : MonoBehaviour
    {
        [Header("Animation Speeds")]
        [Tooltip("Speed of walk cycle animation")]
        public float walkCycleSpeed = 6f;

        [Tooltip("Speed of idle sway/twitch")]
        public float idleTwitchSpeed = 2f;

        [Tooltip("Speed of attack animation")]
        public float attackSpeed = 3f;

        [Header("Walk Animation")]
        [Tooltip("How much legs swing during walk")]
        public float legSwingAngle = 30f;

        [Tooltip("How much arms swing during walk")]
        public float armSwingAngle = 20f;

        [Tooltip("How much body bobs up/down")]
        public float bodyBobAmount = 0.08f;

        [Header("Idle Animation")]
        [Tooltip("How much head twitches during idle")]
        public float headTwitchAmount = 8f;

        [Tooltip("How much ears wiggle")]
        public float earWiggleAmount = 15f;

        [Tooltip("How much body sways")]
        public float idleSwayAmount = 3f;

        [Header("Attack Animation")]
        [Tooltip("How far arms swing during attack")]
        public float attackSwingAngle = 60f;

        [Tooltip("How much body lunges forward")]
        public float attackLungeAmount = 0.3f;

        [Header("Detection")]
        [Tooltip("Speed threshold to detect walking")]
        public float walkSpeedThreshold = 0.1f;

        [Tooltip("Enable debug logging")]
        public bool showDebug = false;

        // Bone references
        private Transform body;
        private Transform head;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftHand;
        private Transform rightHand;
        private Transform leftLeg;
        private Transform rightLeg;
        private Transform leftFoot;
        private Transform rightFoot;
        private Transform leftEar;
        private Transform rightEar;

        // Original rotations
        private Quaternion bodyOriginal;
        private Quaternion headOriginal;
        private Quaternion leftArmOriginal;
        private Quaternion rightArmOriginal;
        private Quaternion leftHandOriginal;
        private Quaternion rightHandOriginal;
        private Quaternion leftLegOriginal;
        private Quaternion rightLegOriginal;
        private Quaternion leftFootOriginal;
        private Quaternion rightFootOriginal;
        private Quaternion leftEarOriginal;
        private Quaternion rightEarOriginal;

        // Original positions
        private Vector3 bodyOriginalPos;

        // Animation state
        private float walkCycle = 0f;
        private float idleCycle = 0f;
        private float attackCycle = 0f;
        private bool isAttacking = false;
        private float attackCooldown = 0f;

        // Movement tracking
        private Vector3 lastPosition;
        private float currentSpeed;
        private Rigidbody rb;

        // AI reference
        private MonsterAI monsterAI;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            monsterAI = GetComponent<MonsterAI>();
            lastPosition = transform.position;

            FindBones();
            StoreOriginalTransforms();

            if (showDebug)
                Debug.Log($"[GoblinAnimator] Initialized for {gameObject.name}");
        }

        void FindBones()
        {
            // Find bones by name (created by MonsterBuilder)
            // All goblin bones are direct children of the goblin transform
            body = transform.Find("Body");
            head = transform.Find("Head");
            leftArm = transform.Find("LeftArm");
            rightArm = transform.Find("RightArm");
            leftLeg = transform.Find("LeftLeg");
            rightLeg = transform.Find("RightLeg");

            // Ears are children of head
            if (head != null)
            {
                leftEar = head.Find("LeftEar");
                rightEar = head.Find("RightEar");
            }

            // Find child bones (hands and feet)
            if (leftArm != null) leftHand = leftArm.Find("Hand");
            if (rightArm != null) rightHand = rightArm.Find("Hand");
            if (leftLeg != null) leftFoot = leftLeg.Find("Foot");
            if (rightLeg != null) rightFoot = rightLeg.Find("Foot");

            if (showDebug)
            {
                Debug.Log($"[GoblinAnimator] Found bones: body={body != null}, head={head != null}, " +
                    $"leftArm={leftArm != null}, rightArm={rightArm != null}, " +
                    $"leftLeg={leftLeg != null}, rightLeg={rightLeg != null}, " +
                    $"leftEar={leftEar != null}, rightEar={rightEar != null}");
            }
        }

        void StoreOriginalTransforms()
        {
            if (body != null)
            {
                bodyOriginal = body.localRotation;
                bodyOriginalPos = body.localPosition;
            }
            if (head != null) headOriginal = head.localRotation;
            if (leftArm != null) leftArmOriginal = leftArm.localRotation;
            if (rightArm != null) rightArmOriginal = rightArm.localRotation;
            if (leftHand != null) leftHandOriginal = leftHand.localRotation;
            if (rightHand != null) rightHandOriginal = rightHand.localRotation;
            if (leftLeg != null) leftLegOriginal = leftLeg.localRotation;
            if (rightLeg != null) rightLegOriginal = rightLeg.localRotation;
            if (leftFoot != null) leftFootOriginal = leftFoot.localRotation;
            if (rightFoot != null) rightFootOriginal = rightFoot.localRotation;
            if (leftEar != null) leftEarOriginal = leftEar.localRotation;
            if (rightEar != null) rightEarOriginal = rightEar.localRotation;
        }

        void Update()
        {
            // Calculate current speed
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            currentSpeed = horizontalVelocity.magnitude;

            // Update attack cooldown
            if (attackCooldown > 0f)
                attackCooldown -= Time.deltaTime;

            // Check if we should trigger attack animation
            bool isAggro = monsterAI != null && monsterAI.IsAggro;
            if (isAggro && !isAttacking && attackCooldown <= 0f)
            {
                // Attack more frequently when aggro (10% chance per frame = attack every ~0.5 seconds)
                if (Random.value < 0.1f)
                {
                    StartAttack();
                }
            }

            // Animate based on current state
            if (isAttacking)
            {
                AnimateAttack();
            }
            else if (currentSpeed > walkSpeedThreshold)
            {
                AnimateWalk();
            }
            else
            {
                AnimateIdle();
            }
        }

        void AnimateWalk()
        {
            if (body == null) return;

            // Advance walk cycle
            walkCycle += Time.deltaTime * walkCycleSpeed;

            // Leg swing (opposing legs)
            if (leftLeg != null)
            {
                float leftLegAngle = Mathf.Sin(walkCycle) * legSwingAngle;
                leftLeg.localRotation = leftLegOriginal * Quaternion.Euler(leftLegAngle, 0, 0);
            }

            if (rightLeg != null)
            {
                float rightLegAngle = Mathf.Sin(walkCycle + Mathf.PI) * legSwingAngle;
                rightLeg.localRotation = rightLegOriginal * Quaternion.Euler(rightLegAngle, 0, 0);
            }

            // Arm swing (opposite to legs - goblin style)
            if (leftArm != null)
            {
                float leftArmAngle = Mathf.Sin(walkCycle + Mathf.PI) * armSwingAngle;
                leftArm.localRotation = leftArmOriginal * Quaternion.Euler(leftArmAngle, 0, 0);
            }

            if (rightArm != null)
            {
                float rightArmAngle = Mathf.Sin(walkCycle) * armSwingAngle;
                rightArm.localRotation = rightArmOriginal * Quaternion.Euler(rightArmAngle, 0, 0);
            }

            // Body bob
            float bodyBob = Mathf.Abs(Mathf.Sin(walkCycle * 2f)) * bodyBobAmount;
            body.localPosition = bodyOriginalPos + Vector3.up * bodyBob;

            // Head twitches while walking
            if (head != null)
            {
                float headTwitch = Mathf.Sin(walkCycle * 1.5f) * (headTwitchAmount * 0.5f);
                head.localRotation = headOriginal * Quaternion.Euler(0, headTwitch, 0);
            }

            // Ear wiggle while walking
            if (leftEar != null)
            {
                float leftEarWiggle = Mathf.Sin(walkCycle * 3f) * earWiggleAmount;
                leftEar.localRotation = leftEarOriginal * Quaternion.Euler(0, 0, leftEarWiggle);
            }

            if (rightEar != null)
            {
                float rightEarWiggle = Mathf.Sin(walkCycle * 3f + Mathf.PI) * earWiggleAmount;
                rightEar.localRotation = rightEarOriginal * Quaternion.Euler(0, 0, rightEarWiggle);
            }
        }

        void AnimateIdle()
        {
            if (body == null) return;

            // Advance idle cycle
            idleCycle += Time.deltaTime * idleTwitchSpeed;

            // Reset limbs to original positions (smoothly)
            if (leftLeg != null)
                leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation, leftLegOriginal, Time.deltaTime * 5f);
            if (rightLeg != null)
                rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation, rightLegOriginal, Time.deltaTime * 5f);
            if (leftArm != null)
                leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, leftArmOriginal, Time.deltaTime * 5f);
            if (rightArm != null)
                rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, rightArmOriginal, Time.deltaTime * 5f);

            // Body sway
            float sway = Mathf.Sin(idleCycle) * idleSwayAmount;
            body.localRotation = bodyOriginal * Quaternion.Euler(0, 0, sway);
            body.localPosition = bodyOriginalPos;

            // Head twitches
            if (head != null)
            {
                float headTwitch = Mathf.Sin(idleCycle * 2f) * headTwitchAmount;
                head.localRotation = headOriginal * Quaternion.Euler(0, headTwitch, 0);
            }

            // Ear wiggle (alternating)
            if (leftEar != null)
            {
                float leftEarWiggle = Mathf.Sin(idleCycle * 3f) * earWiggleAmount;
                leftEar.localRotation = leftEarOriginal * Quaternion.Euler(0, 0, leftEarWiggle);
            }

            if (rightEar != null)
            {
                float rightEarWiggle = Mathf.Sin(idleCycle * 3f + Mathf.PI) * earWiggleAmount;
                rightEar.localRotation = rightEarOriginal * Quaternion.Euler(0, 0, rightEarWiggle);
            }
        }

        void StartAttack()
        {
            isAttacking = true;
            attackCycle = 0f;
            attackCooldown = 1.5f; // 1.5 second cooldown between attacks

            if (showDebug)
                Debug.Log($"[GoblinAnimator] {gameObject.name} starting attack!");
        }

        void AnimateAttack()
        {
            if (body == null) return;

            // Advance attack cycle
            attackCycle += Time.deltaTime * attackSpeed;

            // Attack animation lasts ~1 second
            float attackProgress = attackCycle / 1f;

            if (attackProgress >= 1f)
            {
                // Attack finished
                isAttacking = false;
                return;
            }

            // Lunge forward (first half of animation)
            if (attackProgress < 0.5f)
            {
                float lungeAmount = Mathf.Sin(attackProgress * Mathf.PI * 2f) * attackLungeAmount;
                body.localPosition = bodyOriginalPos + Vector3.forward * lungeAmount;

                // Arms swing down
                if (leftArm != null)
                {
                    float armAngle = Mathf.Sin(attackProgress * Mathf.PI * 2f) * attackSwingAngle;
                    leftArm.localRotation = leftArmOriginal * Quaternion.Euler(-armAngle, 0, -20f);
                }

                if (rightArm != null)
                {
                    float armAngle = Mathf.Sin(attackProgress * Mathf.PI * 2f) * attackSwingAngle;
                    rightArm.localRotation = rightArmOriginal * Quaternion.Euler(-armAngle, 0, 20f);
                }
            }
            else
            {
                // Return to normal (second half)
                float returnProgress = (attackProgress - 0.5f) * 2f; // 0-1 for second half
                body.localPosition = Vector3.Lerp(body.localPosition, bodyOriginalPos, returnProgress);

                if (leftArm != null)
                    leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, leftArmOriginal, returnProgress);
                if (rightArm != null)
                    rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, rightArmOriginal, returnProgress);
            }
        }
    }
}
