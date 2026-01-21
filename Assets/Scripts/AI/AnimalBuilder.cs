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
        /// Creates a rabbit with realistic proportions and coloring
        /// Features: long ears, fluffy white tail, cottontail appearance
        /// </summary>
        public static GameObject CreateRabbit(Transform parent = null)
        {
            GameObject rabbit = new GameObject("Rabbit");
            if (parent != null)
                rabbit.transform.SetParent(parent);

            // === BODY (compact, rounded) ===
            GameObject body = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            body.name = "Body";
            body.transform.SetParent(rabbit.transform);
            body.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            body.transform.localScale = new Vector3(0.24f, 0.22f, 0.32f); // Compact body
            ApplyMaterial(body, new Color(0.65f, 0.52f, 0.38f)); // Natural brown

            // === UNDERBELLY (white/cream extends to chest) ===
            GameObject belly = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            belly.name = "Belly";
            belly.transform.SetParent(body.transform);
            belly.transform.localPosition = new Vector3(0f, -0.75f, 0.1f);
            belly.transform.localScale = new Vector3(0.88f, 0.55f, 0.85f);
            ApplyMaterial(belly, new Color(0.97f, 0.95f, 0.92f)); // Pure white belly

            // === HEAD (rounder, more realistic) ===
            GameObject head = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            head.name = "Head";
            head.transform.SetParent(rabbit.transform);
            head.transform.localPosition = new Vector3(0f, 0.15f, 0.22f);
            head.transform.localScale = new Vector3(0.2f, 0.19f, 0.2f);
            ApplyMaterial(head, new Color(0.67f, 0.54f, 0.4f));

            // === FACE PATCH (lighter around muzzle and forehead) ===
            GameObject facePatch = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            facePatch.name = "FacePatch";
            facePatch.transform.SetParent(head.transform);
            facePatch.transform.localPosition = new Vector3(0f, -0.15f, 0.55f);
            facePatch.transform.localScale = new Vector3(0.85f, 0.65f, 0.75f);
            ApplyMaterial(facePatch, new Color(0.92f, 0.88f, 0.8f)); // Light cream face

            // === NOSE (pink, small) ===
            GameObject nose = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            nose.name = "Nose";
            nose.transform.SetParent(head.transform);
            nose.transform.localPosition = new Vector3(0f, -0.28f, 1.05f);
            nose.transform.localScale = new Vector3(0.18f, 0.13f, 0.15f);
            ApplyMaterial(nose, new Color(0.95f, 0.7f, 0.75f)); // Pink nose

            // === WHISKER SPOTS (dark patches where whiskers would be) ===
            GameObject leftWhiskerSpot = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftWhiskerSpot.name = "LeftWhiskerSpot";
            leftWhiskerSpot.transform.SetParent(head.transform);
            leftWhiskerSpot.transform.localPosition = new Vector3(-0.25f, -0.22f, 0.85f);
            leftWhiskerSpot.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);
            ApplyMaterial(leftWhiskerSpot, new Color(0.45f, 0.35f, 0.25f));

            GameObject rightWhiskerSpot = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightWhiskerSpot.name = "RightWhiskerSpot";
            rightWhiskerSpot.transform.SetParent(head.transform);
            rightWhiskerSpot.transform.localPosition = new Vector3(0.25f, -0.22f, 0.85f);
            rightWhiskerSpot.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);
            ApplyMaterial(rightWhiskerSpot, new Color(0.45f, 0.35f, 0.25f));

            // === LEFT EAR (iconic long rabbit ears) ===
            GameObject leftEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.38f, 0.85f, -0.05f);
            leftEar.transform.localRotation = Quaternion.Euler(-8f, -5f, -18f);
            leftEar.transform.localScale = new Vector3(0.14f, 0.6f, 0.09f); // Very long ears
            ApplyMaterial(leftEar, new Color(0.62f, 0.49f, 0.36f));

            // Inner ear (pink/light)
            GameObject leftEarInner = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEarInner.name = "LeftEarInner";
            leftEarInner.transform.SetParent(leftEar.transform);
            leftEarInner.transform.localPosition = new Vector3(0f, 0.05f, -0.25f);
            leftEarInner.transform.localScale = new Vector3(0.45f, 0.85f, 0.5f);
            ApplyMaterial(leftEarInner, new Color(0.98f, 0.88f, 0.85f)); // Light pink inner ear

            // === RIGHT EAR ===
            GameObject rightEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.38f, 0.85f, -0.05f);
            rightEar.transform.localRotation = Quaternion.Euler(-8f, 5f, 18f);
            rightEar.transform.localScale = new Vector3(0.14f, 0.6f, 0.09f);
            ApplyMaterial(rightEar, new Color(0.62f, 0.49f, 0.36f));

            GameObject rightEarInner = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEarInner.name = "RightEarInner";
            rightEarInner.transform.SetParent(rightEar.transform);
            rightEarInner.transform.localPosition = new Vector3(0f, 0.05f, -0.25f);
            rightEarInner.transform.localScale = new Vector3(0.45f, 0.85f, 0.5f);
            ApplyMaterial(rightEarInner, new Color(0.98f, 0.88f, 0.85f));

            // === EYES (large, dark) ===
            GameObject leftEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.42f, 0.2f, 0.55f);
            leftEye.transform.localScale = Vector3.one * 0.16f; // Larger eyes
            ApplyMaterial(leftEye, new Color(0.05f, 0.04f, 0.03f)); // Very dark brown

            GameObject rightEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.42f, 0.2f, 0.55f);
            rightEye.transform.localScale = Vector3.one * 0.16f;
            ApplyMaterial(rightEye, new Color(0.05f, 0.04f, 0.03f));

            // === TAIL (iconic fluffy white cottontail) ===
            GameObject tail = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            tail.name = "Tail";
            tail.transform.SetParent(rabbit.transform);
            tail.transform.localPosition = new Vector3(0f, 0.08f, -0.28f);
            tail.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f); // Rounder, fluffier
            ApplyMaterial(tail, new Color(0.99f, 0.98f, 0.97f)); // Pure white cottontail

            // === LEGS (back legs larger for hopping) ===
            CreateRabbitLeg(rabbit.transform, "FrontLeftLeg", new Vector3(-0.09f, -0.12f, 0.1f), false);
            CreateRabbitLeg(rabbit.transform, "FrontRightLeg", new Vector3(0.09f, -0.12f, 0.1f), false);
            CreateRabbitLeg(rabbit.transform, "BackLeftLeg", new Vector3(-0.09f, -0.1f, -0.08f), true);
            CreateRabbitLeg(rabbit.transform, "BackRightLeg", new Vector3(0.09f, -0.1f, -0.08f), true);

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
        /// Creates a squirrel with realistic proportions and iconic bushy tail
        /// Features: reddish-brown fur, white belly, puffy cheeks, huge tail
        /// </summary>
        public static GameObject CreateSquirrel(Transform parent = null)
        {
            GameObject squirrel = new GameObject("Squirrel");
            if (parent != null)
                squirrel.transform.SetParent(parent);

            // === BODY (compact, realistic reddish-brown) ===
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(squirrel.transform);
            body.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            body.transform.localScale = new Vector3(0.16f, 0.22f, 0.16f);
            ApplyMaterial(body, new Color(0.72f, 0.42f, 0.22f)); // Natural reddish-brown

            // === UNDERBELLY (cream/white) ===
            GameObject belly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            belly.name = "Belly";
            belly.transform.SetParent(body.transform);
            belly.transform.localPosition = new Vector3(0f, -0.55f, 0.05f);
            belly.transform.localScale = new Vector3(0.82f, 0.52f, 0.88f);
            ApplyMaterial(belly, new Color(0.96f, 0.93f, 0.88f)); // Cream belly

            // === HEAD (rounded, alert expression) ===
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(squirrel.transform);
            head.transform.localPosition = new Vector3(0f, 0.08f, 0.2f);
            head.transform.localScale = new Vector3(0.19f, 0.17f, 0.19f);
            ApplyMaterial(head, new Color(0.74f, 0.44f, 0.24f));

            // === CHEEK POUCHES (characteristic squirrel feature) ===
            GameObject leftCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftCheek.name = "LeftCheek";
            leftCheek.transform.SetParent(head.transform);
            leftCheek.transform.localPosition = new Vector3(-0.58f, -0.08f, 0.42f);
            leftCheek.transform.localScale = new Vector3(0.42f, 0.38f, 0.45f);
            ApplyMaterial(leftCheek, new Color(0.93f, 0.88f, 0.82f)); // Light cheek fur

            GameObject rightCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightCheek.name = "RightCheek";
            rightCheek.transform.SetParent(head.transform);
            rightCheek.transform.localPosition = new Vector3(0.58f, -0.08f, 0.42f);
            rightCheek.transform.localScale = new Vector3(0.42f, 0.38f, 0.45f);
            ApplyMaterial(rightCheek, new Color(0.93f, 0.88f, 0.82f));

            // === NOSE (small, dark) ===
            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nose.name = "Nose";
            nose.transform.SetParent(head.transform);
            nose.transform.localPosition = new Vector3(0f, -0.18f, 0.78f);
            nose.transform.localScale = Vector3.one * 0.16f;
            ApplyMaterial(nose, new Color(0.2f, 0.15f, 0.12f)); // Dark brown nose

            // === EARS (rounded, fuzzy) ===
            GameObject leftEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.48f, 0.65f, 0.08f);
            leftEar.transform.localScale = new Vector3(0.32f, 0.38f, 0.22f);
            ApplyMaterial(leftEar, new Color(0.68f, 0.38f, 0.18f));

            // Ear tuft (fuzzy tips)
            GameObject leftEarTuft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEarTuft.name = "LeftEarTuft";
            leftEarTuft.transform.SetParent(leftEar.transform);
            leftEarTuft.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            leftEarTuft.transform.localScale = Vector3.one * 0.4f;
            ApplyMaterial(leftEarTuft, new Color(0.64f, 0.36f, 0.16f)); // Slightly darker tuft

            GameObject rightEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.48f, 0.65f, 0.08f);
            rightEar.transform.localScale = new Vector3(0.32f, 0.38f, 0.22f);
            ApplyMaterial(rightEar, new Color(0.68f, 0.38f, 0.18f));

            GameObject rightEarTuft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEarTuft.name = "RightEarTuft";
            rightEarTuft.transform.SetParent(rightEar.transform);
            rightEarTuft.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            rightEarTuft.transform.localScale = Vector3.one * 0.4f;
            ApplyMaterial(rightEarTuft, new Color(0.64f, 0.36f, 0.16f));

            // === EYES (large, alert) ===
            GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.35f, 0.12f, 0.68f);
            leftEye.transform.localScale = Vector3.one * 0.18f; // Larger squirrel eyes
            ApplyMaterial(leftEye, new Color(0.05f, 0.04f, 0.03f)); // Very dark

            GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.35f, 0.12f, 0.68f);
            rightEye.transform.localScale = Vector3.one * 0.18f;
            ApplyMaterial(rightEye, new Color(0.05f, 0.04f, 0.03f));

            // === BUSHY TAIL (iconic huge squirrel tail) ===
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tail.name = "Tail";
            tail.transform.SetParent(squirrel.transform);
            tail.transform.localPosition = new Vector3(0f, 0.18f, -0.22f);
            tail.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.22f, 0.42f, 0.22f); // Much larger, bushier
            ApplyMaterial(tail, new Color(0.7f, 0.4f, 0.2f));

            // Tail fluff (makes it even bushier)
            GameObject tailFluff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tailFluff.name = "TailFluff";
            tailFluff.transform.SetParent(tail.transform);
            tailFluff.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            tailFluff.transform.localScale = new Vector3(1.05f, 0.85f, 1.05f);
            ApplyMaterial(tailFluff, new Color(0.68f, 0.38f, 0.18f)); // Slightly darker fluff

            // === LEGS (nimble, climbing legs) ===
            CreateSquirrelLeg(squirrel.transform, "FrontLeftLeg", new Vector3(-0.08f, -0.1f, 0.08f));
            CreateSquirrelLeg(squirrel.transform, "FrontRightLeg", new Vector3(0.08f, -0.1f, 0.08f));
            CreateSquirrelLeg(squirrel.transform, "BackLeftLeg", new Vector3(-0.08f, -0.1f, -0.06f));
            CreateSquirrelLeg(squirrel.transform, "BackRightLeg", new Vector3(0.08f, -0.1f, -0.06f));

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
        /// Creates a bird with realistic songbird proportions and coloring
        /// Features: blue jay inspired colors, detailed wing/tail feathers
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
        /// Creates a deer with realistic proportions (larger, majestic animal)
        /// More elongated body, taller legs, graceful neck, and realistic coloring
        /// </summary>
        public static GameObject CreateDeer(Transform parent = null)
        {
            GameObject deer = new GameObject("Deer");
            if (parent != null)
                deer.transform.SetParent(parent);

            // === BODY (horizontal, elongated, realistic deer proportions) ===
            GameObject body = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            body.name = "Body";
            body.transform.SetParent(deer.transform);
            body.transform.localPosition = new Vector3(0f, 0.3f, 0f); // Raised higher off ground
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = new Vector3(0.2f, 0.22f, 0.7f); // Longer, thinner body
            ApplyMaterial(body, new Color(0.52f, 0.38f, 0.25f)); // Realistic brown

            // === UNDERBELLY (white/cream stripe along bottom) ===
            GameObject belly = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            belly.name = "Belly";
            belly.transform.SetParent(body.transform);
            belly.transform.localPosition = new Vector3(0f, -0.85f, -0.05f); // Slightly back-centered
            belly.transform.localScale = new Vector3(0.75f, 0.35f, 0.85f); // Slightly shorter to not extend past body
            ApplyMaterial(belly, new Color(0.95f, 0.92f, 0.88f)); // Off-white belly

            // === CHEST (lighter patch at front) ===
            GameObject chest = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            chest.name = "Chest";
            chest.transform.SetParent(body.transform);
            chest.transform.localPosition = new Vector3(0f, -0.5f, 0.65f); // Front of body
            chest.transform.localScale = new Vector3(0.85f, 0.6f, 0.5f);
            ApplyMaterial(chest, new Color(0.88f, 0.82f, 0.7f)); // Light tan chest

            // === NECK (longer, graceful, thinner) ===
            GameObject neck = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            neck.name = "Neck";
            neck.transform.SetParent(deer.transform);
            neck.transform.localPosition = new Vector3(0f, 0.52f, 0.42f); // Higher and forward
            neck.transform.localRotation = Quaternion.Euler(70f, 0f, 0f); // Upright angle
            neck.transform.localScale = new Vector3(0.11f, 0.42f, 0.11f); // Much longer and thinner
            ApplyMaterial(neck, new Color(0.54f, 0.4f, 0.27f));

            // === HEAD (smaller, more elongated) ===
            GameObject head = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            head.name = "Head";
            head.transform.SetParent(deer.transform);
            head.transform.localPosition = new Vector3(0f, 0.82f, 0.52f); // Higher up for taller deer
            head.transform.localScale = new Vector3(0.15f, 0.14f, 0.26f); // More elongated forward
            ApplyMaterial(head, new Color(0.5f, 0.37f, 0.24f));

            // === FOREHEAD PATCH (lighter) ===
            GameObject foreheadPatch = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            foreheadPatch.name = "ForeheadPatch";
            foreheadPatch.transform.SetParent(head.transform);
            foreheadPatch.transform.localPosition = new Vector3(0f, 0.5f, 0.3f);
            foreheadPatch.transform.localScale = new Vector3(0.7f, 0.5f, 0.6f);
            ApplyMaterial(foreheadPatch, new Color(0.7f, 0.58f, 0.42f)); // Lighter forehead

            // === SNOUT (longer, more refined) ===
            GameObject snout = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            snout.name = "Snout";
            snout.transform.SetParent(head.transform);
            snout.transform.localPosition = new Vector3(0f, -0.25f, 0.95f);
            snout.transform.localRotation = Quaternion.Euler(80f, 0f, 0f);
            snout.transform.localScale = new Vector3(0.45f, 0.65f, 0.45f); // Longer snout
            ApplyMaterial(snout, new Color(0.68f, 0.55f, 0.4f)); // Lighter snout

            // === NOSE (black, realistic size) ===
            GameObject nose = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            nose.name = "Nose";
            nose.transform.SetParent(head.transform);
            nose.transform.localPosition = new Vector3(0f, -0.4f, 1.35f);
            nose.transform.localScale = new Vector3(0.18f, 0.14f, 0.18f);
            ApplyMaterial(nose, new Color(0.12f, 0.1f, 0.08f)); // Black nose

            // === EARS (larger, more natural) ===
            GameObject leftEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.45f, 0.75f, 0.15f);
            leftEar.transform.localRotation = Quaternion.Euler(-15f, -25f, -30f);
            leftEar.transform.localScale = new Vector3(0.16f, 0.5f, 0.1f); // Larger ears
            ApplyMaterial(leftEar, new Color(0.48f, 0.35f, 0.22f));

            // Inner ear (lighter)
            GameObject leftEarInner = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEarInner.name = "LeftEarInner";
            leftEarInner.transform.SetParent(leftEar.transform);
            leftEarInner.transform.localPosition = new Vector3(0f, 0f, -0.3f);
            leftEarInner.transform.localScale = new Vector3(0.5f, 0.7f, 0.6f);
            ApplyMaterial(leftEarInner, new Color(0.85f, 0.78f, 0.68f)); // Light inner ear

            GameObject rightEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.45f, 0.75f, 0.15f);
            rightEar.transform.localRotation = Quaternion.Euler(-15f, 25f, 30f);
            rightEar.transform.localScale = new Vector3(0.16f, 0.5f, 0.1f);
            ApplyMaterial(rightEar, new Color(0.48f, 0.35f, 0.22f));

            // Inner ear (lighter)
            GameObject rightEarInner = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEarInner.name = "RightEarInner";
            rightEarInner.transform.SetParent(rightEar.transform);
            rightEarInner.transform.localPosition = new Vector3(0f, 0f, -0.3f);
            rightEarInner.transform.localScale = new Vector3(0.5f, 0.7f, 0.6f);
            ApplyMaterial(rightEarInner, new Color(0.85f, 0.78f, 0.68f));

            // === EYES (dark, realistic) ===
            GameObject leftEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.38f, 0.15f, 0.8f);
            leftEye.transform.localScale = Vector3.one * 0.13f;
            ApplyMaterial(leftEye, new Color(0.08f, 0.06f, 0.04f)); // Very dark brown

            GameObject rightEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.38f, 0.15f, 0.8f);
            rightEye.transform.localScale = Vector3.one * 0.13f;
            ApplyMaterial(rightEye, new Color(0.08f, 0.06f, 0.04f));

            // === ANTLERS (more prominent and realistic) ===
            CreateDeerAntler(head.transform, "LeftAntler", new Vector3(-0.28f, 0.85f, 0.05f), -1);
            CreateDeerAntler(head.transform, "RightAntler", new Vector3(0.28f, 0.85f, 0.05f), 1);

            // === TAIL (short, realistic with white underside) ===
            GameObject tail = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            tail.name = "Tail";
            tail.transform.SetParent(deer.transform);
            tail.transform.localPosition = new Vector3(0f, 0.22f, -0.6f);
            tail.transform.localRotation = Quaternion.Euler(-35f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.07f, 0.14f, 0.07f);
            ApplyMaterial(tail, new Color(0.5f, 0.36f, 0.23f));

            // White tail underside
            GameObject tailWhite = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            tailWhite.name = "TailWhite";
            tailWhite.transform.SetParent(tail.transform);
            tailWhite.transform.localPosition = new Vector3(0f, -0.3f, -0.5f);
            tailWhite.transform.localScale = new Vector3(1.2f, 0.7f, 0.8f);
            ApplyMaterial(tailWhite, new Color(0.98f, 0.96f, 0.92f)); // Bright white

            // === LEGS (much taller and thinner for realistic proportions) ===
            CreateDeerLeg(deer.transform, "FrontLeftLeg", new Vector3(-0.12f, -0.1f, 0.25f));
            CreateDeerLeg(deer.transform, "FrontRightLeg", new Vector3(0.12f, -0.1f, 0.25f));
            CreateDeerLeg(deer.transform, "BackLeftLeg", new Vector3(-0.12f, -0.1f, -0.2f));
            CreateDeerLeg(deer.transform, "BackRightLeg", new Vector3(0.12f, -0.1f, -0.2f));

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
            // Upper leg (thigh)
            GameObject upperLeg = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            upperLeg.name = name;
            upperLeg.transform.SetParent(parent);
            upperLeg.transform.localPosition = position;
            upperLeg.transform.localScale = new Vector3(0.065f, 0.24f, 0.065f); // Thicker upper leg
            ApplyMaterial(upperLeg, new Color(0.5f, 0.37f, 0.24f));

            // Lower leg (shin - thinner and longer)
            GameObject lowerLeg = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            lowerLeg.name = "LowerLeg";
            lowerLeg.transform.SetParent(upperLeg.transform);
            lowerLeg.transform.localPosition = new Vector3(0f, -0.95f, 0f);
            lowerLeg.transform.localScale = new Vector3(0.8f, 0.7f, 0.8f); // Thinner lower leg
            ApplyMaterial(lowerLeg, new Color(0.48f, 0.35f, 0.22f)); // Slightly darker

            // Hoof (small and dark)
            GameObject hoof = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            hoof.name = "Hoof";
            hoof.transform.SetParent(lowerLeg.transform);
            hoof.transform.localPosition = new Vector3(0f, -0.9f, 0f);
            hoof.transform.localScale = new Vector3(1.1f, 0.35f, 1.1f); // Slightly wider hoof
            ApplyMaterial(hoof, new Color(0.1f, 0.08f, 0.06f)); // Very dark hooves
        }

        /// <summary>
        /// Creates a fox with realistic proportions (sleek predator)
        /// Features: orange-red fur, white chest/snout, bushy tail with white tip
        /// </summary>
        public static GameObject CreateFox(Transform parent = null)
        {
            GameObject fox = new GameObject("Fox");
            if (parent != null)
                fox.transform.SetParent(parent);

            // === BODY (sleek, agile predator body) ===
            GameObject body = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            body.name = "Body";
            body.transform.SetParent(fox.transform);
            body.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = new Vector3(0.2f, 0.18f, 0.42f); // More realistic proportions
            ApplyMaterial(body, new Color(0.82f, 0.42f, 0.12f)); // Natural fox orange

            // === UNDERBELLY (white stripe) ===
            GameObject belly = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            belly.name = "Belly";
            belly.transform.SetParent(body.transform);
            belly.transform.localPosition = new Vector3(0f, -0.78f, 0.05f);
            belly.transform.localScale = new Vector3(0.7f, 0.4f, 0.92f);
            ApplyMaterial(belly, new Color(0.96f, 0.93f, 0.88f)); // Cream belly

            // === CHEST (white bib extending forward) ===
            GameObject chest = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            chest.name = "Chest";
            chest.transform.SetParent(body.transform);
            chest.transform.localPosition = new Vector3(0f, -0.55f, 0.7f);
            chest.transform.localScale = new Vector3(0.8f, 0.55f, 0.5f);
            ApplyMaterial(chest, new Color(0.98f, 0.96f, 0.92f)); // Pure white chest

            // === NECK (connects body to head) ===
            GameObject neck = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            neck.name = "Neck";
            neck.transform.SetParent(fox.transform);
            neck.transform.localPosition = new Vector3(0f, 0.18f, 0.28f);
            neck.transform.localScale = new Vector3(0.14f, 0.13f, 0.15f);
            ApplyMaterial(neck, new Color(0.84f, 0.44f, 0.14f));

            // === HEAD (wedge-shaped, cunning fox head) ===
            GameObject head = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            head.name = "Head";
            head.transform.SetParent(fox.transform);
            head.transform.localPosition = new Vector3(0f, 0.22f, 0.38f);
            head.transform.localScale = new Vector3(0.18f, 0.16f, 0.22f);
            ApplyMaterial(head, new Color(0.8f, 0.4f, 0.1f));

            // === FOREHEAD MARKINGS (darker patch) ===
            GameObject foreheadMark = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            foreheadMark.name = "ForeheadMark";
            foreheadMark.transform.SetParent(head.transform);
            foreheadMark.transform.localPosition = new Vector3(0f, 0.45f, 0.1f);
            foreheadMark.transform.localScale = new Vector3(0.65f, 0.4f, 0.7f);
            ApplyMaterial(foreheadMark, new Color(0.65f, 0.32f, 0.08f)); // Darker reddish

            // === SNOUT (long, narrow, pointed - signature fox feature) ===
            GameObject snout = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            snout.name = "Snout";
            snout.transform.SetParent(head.transform);
            snout.transform.localPosition = new Vector3(0f, -0.18f, 0.95f);
            snout.transform.localRotation = Quaternion.Euler(85f, 0f, 0f);
            snout.transform.localScale = new Vector3(0.48f, 0.68f, 0.48f); // Long, narrow
            ApplyMaterial(snout, new Color(0.97f, 0.94f, 0.9f)); // White/cream snout

            // === NOSE (small, black) ===
            GameObject nose = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            nose.name = "Nose";
            nose.transform.SetParent(head.transform);
            nose.transform.localPosition = new Vector3(0f, -0.32f, 1.38f);
            nose.transform.localScale = new Vector3(0.2f, 0.16f, 0.18f);
            ApplyMaterial(nose, new Color(0.08f, 0.06f, 0.05f)); // Black nose

            // === EARS (large, triangular, alert - iconic fox ears) ===
            GameObject leftEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            leftEar.name = "LeftEar";
            leftEar.transform.SetParent(head.transform);
            leftEar.transform.localPosition = new Vector3(-0.42f, 0.75f, 0.12f);
            leftEar.transform.localRotation = Quaternion.Euler(-8f, -18f, -22f);
            leftEar.transform.localScale = new Vector3(0.28f, 0.42f, 0.22f); // Large triangular ears
            ApplyMaterial(leftEar, new Color(0.78f, 0.38f, 0.08f));

            // Inner ear (white)
            GameObject leftEarInner = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEarInner.name = "LeftEarInner";
            leftEarInner.transform.SetParent(leftEar.transform);
            leftEarInner.transform.localPosition = new Vector3(0f, 0.1f, -0.28f);
            leftEarInner.transform.localScale = new Vector3(0.48f, 0.75f, 0.45f);
            ApplyMaterial(leftEarInner, new Color(0.95f, 0.92f, 0.88f)); // Light inner ear

            // Ear tip (black characteristic marking)
            GameObject leftEarTip = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEarTip.name = "LeftEarTip";
            leftEarTip.transform.SetParent(leftEar.transform);
            leftEarTip.transform.localPosition = new Vector3(0f, 0.82f, 0f);
            leftEarTip.transform.localScale = Vector3.one * 0.38f;
            ApplyMaterial(leftEarTip, new Color(0.12f, 0.08f, 0.06f)); // Black ear tip

            GameObject rightEar = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            rightEar.name = "RightEar";
            rightEar.transform.SetParent(head.transform);
            rightEar.transform.localPosition = new Vector3(0.42f, 0.75f, 0.12f);
            rightEar.transform.localRotation = Quaternion.Euler(-8f, 18f, 22f);
            rightEar.transform.localScale = new Vector3(0.28f, 0.42f, 0.22f);
            ApplyMaterial(rightEar, new Color(0.78f, 0.38f, 0.08f));

            GameObject rightEarInner = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEarInner.name = "RightEarInner";
            rightEarInner.transform.SetParent(rightEar.transform);
            rightEarInner.transform.localPosition = new Vector3(0f, 0.1f, -0.28f);
            rightEarInner.transform.localScale = new Vector3(0.48f, 0.75f, 0.45f);
            ApplyMaterial(rightEarInner, new Color(0.95f, 0.92f, 0.88f));

            GameObject rightEarTip = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEarTip.name = "RightEarTip";
            rightEarTip.transform.SetParent(rightEar.transform);
            rightEarTip.transform.localPosition = new Vector3(0f, 0.82f, 0f);
            rightEarTip.transform.localScale = Vector3.one * 0.38f;
            ApplyMaterial(rightEarTip, new Color(0.12f, 0.08f, 0.06f));

            // === EYES (amber/golden - sharp fox eyes) ===
            GameObject leftEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.38f, 0.18f, 0.72f);
            leftEye.transform.localScale = Vector3.one * 0.14f;
            ApplyMaterial(leftEye, new Color(0.92f, 0.78f, 0.22f)); // Golden amber

            GameObject rightEye = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.38f, 0.18f, 0.72f);
            rightEye.transform.localScale = Vector3.one * 0.14f;
            ApplyMaterial(rightEye, new Color(0.92f, 0.78f, 0.22f));

            // === BUSHY TAIL (iconic huge fox tail with white tip) ===
            GameObject tail = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            tail.name = "Tail";
            tail.transform.SetParent(fox.transform);
            tail.transform.localPosition = new Vector3(0f, 0.15f, -0.38f);
            tail.transform.localRotation = Quaternion.Euler(-32f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.16f, 0.42f, 0.16f); // Long bushy tail
            ApplyMaterial(tail, new Color(0.8f, 0.4f, 0.1f));

            // Tail fluff (makes it bushier)
            GameObject tailFluff = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateBox());
            tailFluff.name = "TailFluff";
            tailFluff.transform.SetParent(tail.transform);
            tailFluff.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            tailFluff.transform.localScale = new Vector3(1.15f, 0.6f, 1.15f);
            ApplyMaterial(tailFluff, new Color(0.76f, 0.36f, 0.08f)); // Slightly darker

            // Tail tip (signature white tip)
            GameObject tailTip = CreateWithMesh(PolytopiaStyleMeshGenerator.CreateCube());
            tailTip.name = "TailTip";
            tailTip.transform.SetParent(tail.transform);
            tailTip.transform.localPosition = new Vector3(0f, -0.88f, 0f);
            tailTip.transform.localScale = new Vector3(1.12f, 0.65f, 1.12f);
            ApplyMaterial(tailTip, new Color(0.99f, 0.97f, 0.94f)); // Bright white tip

            // === LEGS (slender, agile predator legs) ===
            CreateFoxLeg(fox.transform, "FrontLeftLeg", new Vector3(-0.11f, -0.08f, 0.15f));
            CreateFoxLeg(fox.transform, "FrontRightLeg", new Vector3(0.11f, -0.08f, 0.15f));
            CreateFoxLeg(fox.transform, "BackLeftLeg", new Vector3(-0.11f, -0.08f, -0.12f));
            CreateFoxLeg(fox.transform, "BackRightLeg", new Vector3(0.11f, -0.08f, -0.12f));

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
