using UnityEngine;

namespace VRDungeonCrawler.AI
{
    /// <summary>
    /// Procedurally builds Polytopia-style animals with higher polygon counts
    /// Creates rabbits, squirrels, birds, deer, and foxes using low-poly but refined meshes
    /// Animals use icospheres and faceted primitives for a sophisticated low-poly look
    /// Each animal has distinct colors for body, face, ears, nose, etc.
    /// </summary>
    public static class AnimalBuilder
    {
        /// <summary>
        /// Creates a rabbit with Polytopia-style angular blocky shapes (brown/gray fur, pink nose, white tail)
        /// Uses cubes and boxes like true Polytopia style
        /// </summary>
        public static GameObject CreateRabbit(Transform parent = null)
        {
            GameObject rabbit = new GameObject("Rabbit");
            if (parent != null)
                rabbit.transform.SetParent(parent);

            // === BODY (realistic brown/tan fur) - Angular cube ===
            GameObject body = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            body.name = "Body";
            body.transform.SetParent(rabbit.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.3f, 0.25f, 0.35f);
            ApplyMaterial(body, new Color(0.7f, 0.55f, 0.4f)); // Warm tan-brown fur

            // === UNDERBELLY (lighter cream color) ===
            GameObject belly = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            belly.name = "Belly";
            belly.transform.SetParent(body.transform);
            belly.transform.localPosition = new Vector3(0f, -0.7f, 0.3f);
            belly.transform.localScale = new Vector3(0.85f, 0.6f, 0.7f);
            ApplyMaterial(belly, new Color(0.95f, 0.9f, 0.85f)); // Cream belly

            // === HEAD ===
            GameObject head = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            head.name = "Head";
            head.transform.SetParent(rabbit.transform);
            head.transform.localPosition = new Vector3(0f, 0.1f, 0.25f);
            head.transform.localScale = new Vector3(0.22f, 0.2f, 0.22f);
            ApplyMaterial(head, new Color(0.72f, 0.57f, 0.42f)); // Slightly lighter brown

            // === FACE PATCH (lighter around face) ===
            GameObject facePatch = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            facePatch.name = "FacePatch";
            facePatch.transform.SetParent(head.transform);
            facePatch.transform.localPosition = new Vector3(0f, -0.2f, 0.5f);
            facePatch.transform.localScale = new Vector3(0.8f, 0.6f, 0.7f);
            ApplyMaterial(facePatch, new Color(0.9f, 0.85f, 0.75f)); // Light tan face

            // === NOSE (pink) - positioned in front of face
            GameObject nose = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            nose.name = "Nose";
            nose.transform.SetParent(head.transform);
            nose.transform.localPosition = new Vector3(0f, -0.3f, 1.0f); // Moved forward from 0.7f to 1.0f
            nose.transform.localScale = new Vector3(0.2f, 0.15f, 0.2f);
            ApplyMaterial(nose, new Color(1f, 0.75f, 0.8f)); // Pink nose

            // === LEFT EAR (long rabbit ears) ===
            GameObject leftEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.4f, 0.8f, 0f);
            leftEar.transform.localRotation = Quaternion.Euler(-10f, 0f, -15f);
            leftEar.transform.localScale = new Vector3(0.15f, 0.5f, 0.1f);
            ApplyMaterial(leftEar, new Color(0.6f, 0.5f, 0.4f)); // Same as body

            // === RIGHT EAR ===
            GameObject rightEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.4f, 0.8f, 0f);
            rightEar.transform.localRotation = Quaternion.Euler(-10f, 0f, 15f);
            rightEar.transform.localScale = new Vector3(0.15f, 0.5f, 0.1f);
            ApplyMaterial(rightEar, new Color(0.6f, 0.5f, 0.4f));

            // === EYES (black) ===
            GameObject leftEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.35f, 0.15f, 0.6f);
            leftEye.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(leftEye, Color.black);

            GameObject rightEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.35f, 0.15f, 0.6f);
            rightEye.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(rightEye, Color.black);

            // === TAIL (white fluffy) ===
            GameObject tail = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            tail.name = "Tail";
            tail.transform.SetParent(rabbit.transform);
            tail.transform.localPosition = new Vector3(0f, 0.05f, -0.3f);
            tail.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(tail, new Color(0.95f, 0.95f, 0.95f)); // White tail

            // === LEGS ===
            CreateRabbitLeg(rabbit.transform, "FrontLeftLeg", new Vector3(-0.1f, -0.15f, 0.12f));
            CreateRabbitLeg(rabbit.transform, "FrontRightLeg", new Vector3(0.1f, -0.15f, 0.12f));
            CreateRabbitLeg(rabbit.transform, "BackLeftLeg", new Vector3(-0.1f, -0.15f, -0.08f), true);
            CreateRabbitLeg(rabbit.transform, "BackRightLeg", new Vector3(0.1f, -0.15f, -0.08f), true);

            return rabbit;
        }

        static void CreateRabbitLeg(Transform parent, string name, Vector3 position, bool isBackLeg = false)
        {
            GameObject leg = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            leg.name = name;
            leg.transform.SetParent(parent);
            leg.transform.localPosition = position;
            leg.transform.localScale = isBackLeg ? new Vector3(0.08f, 0.15f, 0.08f) : new Vector3(0.06f, 0.1f, 0.06f);
            ApplyMaterial(leg, new Color(0.58f, 0.48f, 0.38f)); // Darker brown for legs

            // Foot (wider at bottom for back legs)
            if (isBackLeg)
            {
                GameObject foot = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
                foot.name = "Foot";
                foot.transform.SetParent(leg.transform);
                foot.transform.localPosition = new Vector3(0f, -0.7f, 0.3f);
                foot.transform.localScale = new Vector3(1.2f, 0.5f, 1.5f);
                ApplyMaterial(foot, new Color(0.58f, 0.48f, 0.38f));
            }
        }

        /// <summary>
        /// Creates a squirrel with realistic colors (reddish-brown fur, bushy tail)
        /// </summary>
        public static GameObject CreateSquirrel(Transform parent = null)
        {
            GameObject squirrel = new GameObject("Squirrel");
            if (parent != null)
                squirrel.transform.SetParent(parent);

            // === BODY (realistic reddish-brown fur) ===
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(squirrel.transform);
            body.transform.localPosition = new Vector3(0f, 0f, 0f);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            body.transform.localScale = new Vector3(0.15f, 0.2f, 0.15f);
            ApplyMaterial(body, new Color(0.8f, 0.45f, 0.25f)); // Rich reddish-brown

            // === UNDERBELLY (white/cream) ===
            GameObject belly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            belly.name = "Belly";
            belly.transform.SetParent(body.transform);
            belly.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            belly.transform.localScale = new Vector3(0.8f, 0.5f, 0.9f);
            ApplyMaterial(belly, new Color(0.98f, 0.95f, 0.9f)); // Off-white belly

            // === HEAD ===
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(squirrel.transform);
            head.transform.localPosition = new Vector3(0f, 0.05f, 0.22f);
            head.transform.localScale = new Vector3(0.18f, 0.16f, 0.18f);
            ApplyMaterial(head, new Color(0.82f, 0.47f, 0.27f)); // Slightly lighter reddish-brown

            // === CHEEK PATCHES (lighter) ===
            GameObject leftCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftCheek.name = "LeftCheek";
            leftCheek.transform.SetParent(head.transform);
            leftCheek.transform.localPosition = new Vector3(-0.6f, -0.1f, 0.4f);
            leftCheek.transform.localScale = new Vector3(0.4f, 0.35f, 0.4f);
            ApplyMaterial(leftCheek, new Color(0.95f, 0.9f, 0.85f));

            GameObject rightCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightCheek.name = "RightCheek";
            rightCheek.transform.SetParent(head.transform);
            rightCheek.transform.localPosition = new Vector3(0.6f, -0.1f, 0.4f);
            rightCheek.transform.localScale = new Vector3(0.4f, 0.35f, 0.4f);
            ApplyMaterial(rightCheek, new Color(0.95f, 0.9f, 0.85f));

            // === NOSE (pink/brown) ===
            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nose.name = "Nose";
            nose.transform.SetParent(head.transform);
            nose.transform.localPosition = new Vector3(0f, -0.2f, 0.75f);
            nose.transform.localScale = Vector3.one * 0.18f;
            ApplyMaterial(nose, new Color(0.5f, 0.3f, 0.2f)); // Dark brown nose

            // === EARS (rounded) ===
            GameObject leftEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.45f, 0.6f, 0.1f);
            leftEar.transform.localScale = new Vector3(0.3f, 0.35f, 0.2f);
            ApplyMaterial(leftEar, new Color(0.7f, 0.4f, 0.2f));

            GameObject rightEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.45f, 0.6f, 0.1f);
            rightEar.transform.localScale = new Vector3(0.3f, 0.35f, 0.2f);
            ApplyMaterial(rightEar, new Color(0.7f, 0.4f, 0.2f));

            // === EYES ===
            GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.3f, 0.1f, 0.65f);
            leftEye.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(leftEye, Color.black);

            GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.3f, 0.1f, 0.65f);
            rightEye.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(rightEye, Color.black);

            // === BUSHY TAIL (reddish-brown, large) ===
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tail.name = "Tail";
            tail.transform.SetParent(squirrel.transform);
            tail.transform.localPosition = new Vector3(0f, 0.15f, -0.25f);
            tail.transform.localScale = new Vector3(0.2f, 0.35f, 0.2f);
            ApplyMaterial(tail, new Color(0.75f, 0.45f, 0.25f)); // Lighter reddish-brown

            // === LEGS (small, agile) ===
            CreateSquirrelLeg(squirrel.transform, "FrontLeftLeg", new Vector3(-0.08f, -0.12f, 0.08f));
            CreateSquirrelLeg(squirrel.transform, "FrontRightLeg", new Vector3(0.08f, -0.12f, 0.08f));
            CreateSquirrelLeg(squirrel.transform, "BackLeftLeg", new Vector3(-0.08f, -0.12f, -0.06f));
            CreateSquirrelLeg(squirrel.transform, "BackRightLeg", new Vector3(0.08f, -0.12f, -0.06f));

            return squirrel;
        }

        static void CreateSquirrelLeg(Transform parent, string name, Vector3 position)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = name;
            leg.transform.SetParent(parent);
            leg.transform.localPosition = position;
            leg.transform.localScale = new Vector3(0.05f, 0.08f, 0.05f);
            ApplyMaterial(leg, new Color(0.68f, 0.38f, 0.18f));
        }

        /// <summary>
        /// Creates a bird with realistic colors (blue/brown body, yellow beak)
        /// </summary>
        public static GameObject CreateBird(Transform parent = null)
        {
            GameObject bird = new GameObject("Bird");
            if (parent != null)
                bird.transform.SetParent(parent);

            // === BODY (blue/gray) ===
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(bird.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.2f, 0.18f, 0.25f);
            ApplyMaterial(body, new Color(0.4f, 0.5f, 0.7f)); // Blue-gray

            // === CHEST (lighter color) ===
            GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            chest.name = "Chest";
            chest.transform.SetParent(body.transform);
            chest.transform.localPosition = new Vector3(0f, -0.3f, 0.6f);
            chest.transform.localScale = new Vector3(0.9f, 0.7f, 0.6f);
            ApplyMaterial(chest, new Color(0.9f, 0.85f, 0.7f)); // Cream chest

            // === HEAD ===
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(bird.transform);
            head.transform.localPosition = new Vector3(0f, 0.08f, 0.18f);
            head.transform.localScale = new Vector3(0.16f, 0.15f, 0.16f);
            ApplyMaterial(head, new Color(0.42f, 0.52f, 0.72f));

            // === CROWN/FOREHEAD (slightly darker blue-gray) ===
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Crown";
            crown.transform.SetParent(head.transform);
            crown.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            crown.transform.localScale = new Vector3(0.9f, 0.5f, 0.8f);
            ApplyMaterial(crown, new Color(0.38f, 0.48f, 0.68f)); // Slightly darker blue

            // === BEAK (yellow/orange) ===
            GameObject beak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beak.name = "Beak";
            beak.transform.SetParent(head.transform);
            beak.transform.localPosition = new Vector3(0f, -0.15f, 0.9f);
            beak.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            beak.transform.localScale = new Vector3(0.12f, 0.15f, 0.2f);
            ApplyMaterial(beak, new Color(1f, 0.8f, 0.2f)); // Orange-yellow beak

            // === EYES with white rings (common in birds) ===
            // Left eye ring (white/cream)
            GameObject leftEyeRing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEyeRing.name = "LeftEyeRing";
            leftEyeRing.transform.SetParent(head.transform);
            leftEyeRing.transform.localPosition = new Vector3(-0.4f, 0.1f, 0.5f);
            leftEyeRing.transform.localScale = Vector3.one * 0.2f;
            ApplyMaterial(leftEyeRing, new Color(0.95f, 0.93f, 0.88f)); // Off-white ring

            GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(leftEyeRing.transform);
            leftEye.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            leftEye.transform.localScale = Vector3.one * 0.6f;
            ApplyMaterial(leftEye, Color.black);

            // Right eye ring (white/cream)
            GameObject rightEyeRing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEyeRing.name = "RightEyeRing";
            rightEyeRing.transform.SetParent(head.transform);
            rightEyeRing.transform.localPosition = new Vector3(0.4f, 0.1f, 0.5f);
            rightEyeRing.transform.localScale = Vector3.one * 0.2f;
            ApplyMaterial(rightEyeRing, new Color(0.95f, 0.93f, 0.88f)); // Off-white ring

            GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(rightEyeRing.transform);
            rightEye.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            rightEye.transform.localScale = Vector3.one * 0.6f;
            ApplyMaterial(rightEye, Color.black);

            // === WINGS (blue with darker tips) ===
            GameObject leftWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWing.name = "LeftWing";
            leftWing.transform.SetParent(bird.transform);
            leftWing.transform.localPosition = new Vector3(-0.15f, 0f, 0f);
            leftWing.transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
            leftWing.transform.localScale = new Vector3(0.3f, 0.05f, 0.2f);
            ApplyMaterial(leftWing, new Color(0.35f, 0.45f, 0.65f)); // Darker blue

            // Left wing tip (even darker, like primary feathers)
            GameObject leftWingTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWingTip.name = "LeftWingTip";
            leftWingTip.transform.SetParent(leftWing.transform);
            leftWingTip.transform.localPosition = new Vector3(-0.65f, 0f, 0f);
            leftWingTip.transform.localScale = new Vector3(0.4f, 1.05f, 0.9f);
            ApplyMaterial(leftWingTip, new Color(0.25f, 0.32f, 0.48f)); // Very dark blue-gray

            GameObject rightWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWing.name = "RightWing";
            rightWing.transform.SetParent(bird.transform);
            rightWing.transform.localPosition = new Vector3(0.15f, 0f, 0f);
            rightWing.transform.localRotation = Quaternion.Euler(0f, 0f, 20f);
            rightWing.transform.localScale = new Vector3(0.3f, 0.05f, 0.2f);
            ApplyMaterial(rightWing, new Color(0.35f, 0.45f, 0.65f));

            // Right wing tip (even darker, like primary feathers)
            GameObject rightWingTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWingTip.name = "RightWingTip";
            rightWingTip.transform.SetParent(rightWing.transform);
            rightWingTip.transform.localPosition = new Vector3(0.65f, 0f, 0f);
            rightWingTip.transform.localScale = new Vector3(0.4f, 1.05f, 0.9f);
            ApplyMaterial(rightWingTip, new Color(0.25f, 0.32f, 0.48f)); // Very dark blue-gray

            // === TAIL FEATHERS (with darker tip) ===
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tail.name = "Tail";
            tail.transform.SetParent(bird.transform);
            tail.transform.localPosition = new Vector3(0f, 0.02f, -0.2f);
            tail.transform.localRotation = Quaternion.Euler(-30f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.15f, 0.05f, 0.2f);
            ApplyMaterial(tail, new Color(0.3f, 0.4f, 0.6f));

            // Tail tip (darker feathers)
            GameObject tailTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tailTip.name = "TailTip";
            tailTip.transform.SetParent(tail.transform);
            tailTip.transform.localPosition = new Vector3(0f, 0f, -0.6f);
            tailTip.transform.localScale = new Vector3(0.95f, 1.05f, 0.5f);
            ApplyMaterial(tailTip, new Color(0.22f, 0.3f, 0.45f)); // Dark blue-gray tip

            // === LEGS (thin) ===
            CreateBirdLeg(bird.transform, "LeftLeg", new Vector3(-0.06f, -0.15f, 0f));
            CreateBirdLeg(bird.transform, "RightLeg", new Vector3(0.06f, -0.15f, 0f));

            return bird;
        }

        static void CreateBirdLeg(Transform parent, string name, Vector3 position)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = name;
            leg.transform.SetParent(parent);
            leg.transform.localPosition = position;
            leg.transform.localScale = new Vector3(0.02f, 0.08f, 0.02f);
            ApplyMaterial(leg, new Color(0.8f, 0.6f, 0.3f)); // Orange-brown legs
        }

        /// <summary>
        /// Creates a deer with Polytopia-style angular blocky shapes (larger, majestic animal)
        /// Uses cubes and boxes for true Polytopia geometric style
        /// </summary>
        public static GameObject CreateDeer(Transform parent = null)
        {
            GameObject deer = new GameObject("Deer");
            if (parent != null)
                deer.transform.SetParent(parent);

            // === BODY (horizontal, elongated, deer-like proportions) ===
            GameObject body = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            body.name = "Body";
            body.transform.SetParent(deer.transform);
            body.transform.localPosition = new Vector3(0f, 0.15f, 0f); // Raised higher
            body.transform.localRotation = Quaternion.identity; // Horizontal, not rotated
            body.transform.localScale = new Vector3(0.25f, 0.25f, 0.55f); // Long body front-to-back
            ApplyMaterial(body, new Color(0.6f, 0.45f, 0.3f)); // Rich brown

            // === CHEST (lighter, under neck area) ===
            GameObject chest = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            chest.name = "Chest";
            chest.transform.SetParent(body.transform);
            chest.transform.localPosition = new Vector3(0f, -0.6f, 0.6f); // Front of body
            chest.transform.localScale = new Vector3(0.8f, 0.7f, 0.6f);
            ApplyMaterial(chest, new Color(0.9f, 0.85f, 0.75f)); // Cream chest

            // === NECK (longer, more elegant) ===
            GameObject neck = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            neck.name = "Neck";
            neck.transform.SetParent(deer.transform);
            neck.transform.localPosition = new Vector3(0f, 0.35f, 0.35f); // Higher and forward
            neck.transform.localRotation = Quaternion.Euler(75f, 0f, 0f); // More upright
            neck.transform.localScale = new Vector3(0.14f, 0.35f, 0.14f); // Longer, thinner
            ApplyMaterial(neck, new Color(0.62f, 0.47f, 0.32f));

            // === HEAD (smaller, more refined) ===
            GameObject head = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            head.name = "Head";
            head.transform.SetParent(deer.transform);
            head.transform.localPosition = new Vector3(0f, 0.62f, 0.48f); // Higher up
            head.transform.localScale = new Vector3(0.18f, 0.16f, 0.22f); // Smaller, narrower
            ApplyMaterial(head, new Color(0.58f, 0.43f, 0.28f));

            // === SNOUT (narrower, more refined) ===
            GameObject snout = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            snout.name = "Snout";
            snout.transform.SetParent(head.transform);
            snout.transform.localPosition = new Vector3(0f, -0.2f, 0.85f);
            snout.transform.localRotation = Quaternion.Euler(85f, 0f, 0f);
            snout.transform.localScale = new Vector3(0.5f, 0.55f, 0.5f); // Narrower
            ApplyMaterial(snout, new Color(0.65f, 0.5f, 0.35f));

            // === NOSE (smaller, more delicate) ===
            GameObject nose = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            nose.name = "Nose";
            nose.transform.SetParent(head.transform);
            nose.transform.localPosition = new Vector3(0f, -0.35f, 1.15f);
            nose.transform.localScale = new Vector3(0.2f, 0.16f, 0.2f); // Smaller
            ApplyMaterial(nose, new Color(0.2f, 0.15f, 0.12f)); // Dark nose

            // === EARS (long, pointed) ===
            GameObject leftEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.5f, 0.7f, 0.2f);
            leftEar.transform.localRotation = Quaternion.Euler(-20f, -30f, -25f);
            leftEar.transform.localScale = new Vector3(0.15f, 0.4f, 0.12f);
            ApplyMaterial(leftEar, new Color(0.55f, 0.4f, 0.25f));

            GameObject rightEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.5f, 0.7f, 0.2f);
            rightEar.transform.localRotation = Quaternion.Euler(-20f, 30f, 25f);
            rightEar.transform.localScale = new Vector3(0.15f, 0.4f, 0.12f);
            ApplyMaterial(rightEar, new Color(0.55f, 0.4f, 0.25f));

            // === EYES ===
            GameObject leftEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.35f, 0.2f, 0.75f);
            leftEye.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(leftEye, new Color(0.1f, 0.08f, 0.05f)); // Dark brown eyes

            GameObject rightEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.35f, 0.2f, 0.75f);
            rightEye.transform.localScale = Vector3.one * 0.15f;
            ApplyMaterial(rightEye, new Color(0.1f, 0.08f, 0.05f));

            // === ANTLERS (majestic) ===
            CreateDeerAntler(head.transform, "LeftAntler", new Vector3(-0.3f, 0.8f, 0.1f), -1);
            CreateDeerAntler(head.transform, "RightAntler", new Vector3(0.3f, 0.8f, 0.1f), 1);

            // === TAIL (short) ===
            GameObject tail = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            tail.name = "Tail";
            tail.transform.SetParent(deer.transform);
            tail.transform.localPosition = new Vector3(0f, 0.08f, -0.45f);
            tail.transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
            ApplyMaterial(tail, new Color(0.58f, 0.43f, 0.28f));

            // === LEGS (long, elegant) ===
            CreateDeerLeg(deer.transform, "FrontLeftLeg", new Vector3(-0.15f, -0.25f, 0.2f));
            CreateDeerLeg(deer.transform, "FrontRightLeg", new Vector3(0.15f, -0.25f, 0.2f));
            CreateDeerLeg(deer.transform, "BackLeftLeg", new Vector3(-0.15f, -0.25f, -0.15f));
            CreateDeerLeg(deer.transform, "BackRightLeg", new Vector3(0.15f, -0.25f, -0.15f));

            return deer;
        }

        static void CreateDeerAntler(Transform parent, string name, Vector3 position, int side)
        {
            GameObject antlerBase = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            antlerBase.name = name;
            antlerBase.transform.SetParent(parent);
            antlerBase.transform.localPosition = position;
            antlerBase.transform.localRotation = Quaternion.Euler(-20f, side * 15f, side * 20f);
            antlerBase.transform.localScale = new Vector3(0.06f, 0.25f, 0.06f);
            ApplyMaterial(antlerBase, new Color(0.85f, 0.8f, 0.7f)); // Bone white

            // Antler branches
            GameObject branch1 = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            branch1.name = "Branch1";
            branch1.transform.SetParent(antlerBase.transform);
            branch1.transform.localPosition = new Vector3(side * 0.5f, 0.5f, 0.2f);
            branch1.transform.localRotation = Quaternion.Euler(0f, side * 30f, side * 40f);
            branch1.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            ApplyMaterial(branch1, new Color(0.85f, 0.8f, 0.7f));

            GameObject branch2 = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            branch2.name = "Branch2";
            branch2.transform.SetParent(antlerBase.transform);
            branch2.transform.localPosition = new Vector3(side * -0.3f, 0.7f, -0.1f);
            branch2.transform.localRotation = Quaternion.Euler(0f, side * -20f, side * -30f);
            branch2.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            ApplyMaterial(branch2, new Color(0.85f, 0.8f, 0.7f));
        }

        static void CreateDeerLeg(Transform parent, string name, Vector3 position)
        {
            GameObject leg = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            leg.name = name;
            leg.transform.SetParent(parent);
            leg.transform.localPosition = position;
            leg.transform.localScale = new Vector3(0.08f, 0.3f, 0.08f);
            ApplyMaterial(leg, new Color(0.55f, 0.42f, 0.28f));

            // Hoof
            GameObject hoof = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            hoof.name = "Hoof";
            hoof.transform.SetParent(leg.transform);
            hoof.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            hoof.transform.localScale = new Vector3(0.9f, 0.3f, 0.9f);
            ApplyMaterial(hoof, new Color(0.15f, 0.12f, 0.08f)); // Dark hooves
        }

        /// <summary>
        /// Creates a fox with Polytopia-style angular blocky shapes (medium-sized, sleek predator)
        /// Uses cubes and boxes for true Polytopia geometric style
        /// </summary>
        public static GameObject CreateFox(Transform parent = null)
        {
            GameObject fox = new GameObject("Fox");
            if (parent != null)
                fox.transform.SetParent(parent);

            // === BODY (sleek, elongated) ===
            GameObject body = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            body.name = "Body";
            body.transform.SetParent(fox.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            body.transform.localScale = new Vector3(0.25f, 0.35f, 0.25f);
            ApplyMaterial(body, new Color(0.85f, 0.45f, 0.15f)); // Orange-red fox fur

            // === CHEST (white/cream) ===
            GameObject chest = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            chest.name = "Chest";
            chest.transform.SetParent(body.transform);
            chest.transform.localPosition = new Vector3(0f, -0.4f, 0.5f);
            chest.transform.localScale = new Vector3(0.75f, 0.5f, 0.6f);
            ApplyMaterial(chest, new Color(0.98f, 0.95f, 0.9f)); // White chest

            // === HEAD (angular, pointed) ===
            GameObject head = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            head.name = "Head";
            head.transform.SetParent(fox.transform);
            head.transform.localPosition = new Vector3(0f, 0.08f, 0.32f);
            head.transform.localScale = new Vector3(0.22f, 0.18f, 0.24f);
            ApplyMaterial(head, new Color(0.83f, 0.43f, 0.13f));

            // === SNOUT (long, pointed) ===
            GameObject snout = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            snout.name = "Snout";
            snout.transform.SetParent(head.transform);
            snout.transform.localPosition = new Vector3(0f, -0.2f, 0.9f);
            snout.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            snout.transform.localScale = new Vector3(0.55f, 0.6f, 0.55f);
            ApplyMaterial(snout, new Color(0.95f, 0.9f, 0.85f)); // Light snout

            // === NOSE (black) ===
            GameObject nose = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            nose.name = "Nose";
            nose.transform.SetParent(head.transform);
            nose.transform.localPosition = new Vector3(0f, -0.3f, 1.3f);
            nose.transform.localScale = new Vector3(0.22f, 0.18f, 0.22f);
            ApplyMaterial(nose, new Color(0.1f, 0.08f, 0.08f)); // Black nose

            // === EARS (triangular, pointed) ===
            GameObject leftEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox()); // Triangle
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.45f, 0.7f, 0.2f);
            leftEar.transform.localRotation = Quaternion.Euler(-10f, -20f, -20f);
            leftEar.transform.localScale = new Vector3(0.3f, 0.35f, 0.25f);
            ApplyMaterial(leftEar, new Color(0.8f, 0.4f, 0.1f));

            // Ear tip (black)
            GameObject leftEarTip = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEarTip.name = "LeftEarTip";
            leftEarTip.transform.SetParent(leftEar.transform);
            leftEarTip.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            leftEarTip.transform.localScale = Vector3.one * 0.4f;
            ApplyMaterial(leftEarTip, new Color(0.15f, 0.1f, 0.08f));

            GameObject rightEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.45f, 0.7f, 0.2f);
            rightEar.transform.localRotation = Quaternion.Euler(-10f, 20f, 20f);
            rightEar.transform.localScale = new Vector3(0.3f, 0.35f, 0.25f);
            ApplyMaterial(rightEar, new Color(0.8f, 0.4f, 0.1f));

            GameObject rightEarTip = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEarTip.name = "RightEarTip";
            rightEarTip.transform.SetParent(rightEar.transform);
            rightEarTip.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            rightEarTip.transform.localScale = Vector3.one * 0.4f;
            ApplyMaterial(rightEarTip, new Color(0.15f, 0.1f, 0.08f));

            // === EYES (amber/yellow) ===
            GameObject leftEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.32f, 0.15f, 0.7f);
            leftEye.transform.localScale = Vector3.one * 0.16f;
            ApplyMaterial(leftEye, new Color(0.9f, 0.75f, 0.2f)); // Amber eyes

            GameObject rightEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.32f, 0.15f, 0.7f);
            rightEye.transform.localScale = Vector3.one * 0.16f;
            ApplyMaterial(rightEye, new Color(0.9f, 0.75f, 0.2f));

            // === BUSHY TAIL (large, orange with white tip) ===
            GameObject tail = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            tail.name = "Tail";
            tail.transform.SetParent(fox.transform);
            tail.transform.localPosition = new Vector3(0f, 0.12f, -0.35f);
            tail.transform.localRotation = Quaternion.Euler(-40f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.18f, 0.35f, 0.18f);
            ApplyMaterial(tail, new Color(0.82f, 0.42f, 0.12f));

            // Tail tip (white)
            GameObject tailTip = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            tailTip.name = "TailTip";
            tailTip.transform.SetParent(tail.transform);
            tailTip.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            tailTip.transform.localScale = new Vector3(1.1f, 0.6f, 1.1f);
            ApplyMaterial(tailTip, new Color(0.98f, 0.95f, 0.9f)); // White tip

            // === LEGS (slender, agile) ===
            CreateFoxLeg(fox.transform, "FrontLeftLeg", new Vector3(-0.12f, -0.18f, 0.12f));
            CreateFoxLeg(fox.transform, "FrontRightLeg", new Vector3(0.12f, -0.18f, 0.12f));
            CreateFoxLeg(fox.transform, "BackLeftLeg", new Vector3(-0.12f, -0.18f, -0.1f));
            CreateFoxLeg(fox.transform, "BackRightLeg", new Vector3(0.12f, -0.18f, -0.1f));

            return fox;
        }

        static void CreateFoxLeg(Transform parent, string name, Vector3 position)
        {
            GameObject leg = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            leg.name = name;
            leg.transform.SetParent(parent);
            leg.transform.localPosition = position;
            leg.transform.localScale = new Vector3(0.06f, 0.15f, 0.06f);
            ApplyMaterial(leg, new Color(0.75f, 0.38f, 0.1f));

            // Paw
            GameObject paw = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            paw.name = "Paw";
            paw.transform.SetParent(leg.transform);
            paw.transform.localPosition = new Vector3(0f, -0.8f, 0f);
            paw.transform.localScale = Vector3.one * 1.3f;
            ApplyMaterial(paw, new Color(0.2f, 0.15f, 0.12f)); // Dark paws
        }

        /// <summary>
        /// Helper method to create a GameObject with a custom mesh
        /// </summary>
        static GameObject CreateWithMesh(Mesh mesh)
        {
            GameObject obj = new GameObject();
            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            obj.AddComponent<MeshRenderer>();
            return obj;
        }

        static void ApplyMaterial(GameObject obj, Color color)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = color;
                mat.SetFloat("_Smoothness", 0.3f); // Matte finish for fur/feathers
                renderer.material = mat;
            }

            // Remove collider (will be added at root)
            Collider col = obj.GetComponent<Collider>();
            if (col != null)
            {
                Object.Destroy(col);
            }
        }
    }
}
