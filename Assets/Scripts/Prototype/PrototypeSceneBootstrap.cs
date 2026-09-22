using UnityEngine;

namespace ElementalAnomaly.Prototype
{
    public static class PrototypeSceneBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateMissingPrototypeObjects()
        {
            PrototypePlayerController player = Object.FindFirstObjectByType<PrototypePlayerController>();
            bool sceneHasCollider = Object.FindFirstObjectByType<Collider>() != null;

            if (!sceneHasCollider)
            {
                CreateTestFloor();
            }

            if (player == null)
            {
                player = CreatePlayer();
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = CreateCamera();
            }

            PrototypeThirdPersonCamera cameraController =
                mainCamera.GetComponent<PrototypeThirdPersonCamera>();
            if (cameraController == null)
            {
                cameraController = mainCamera.gameObject.AddComponent<PrototypeThirdPersonCamera>();
            }

            cameraController.Target = player.CameraTarget;
        }

        private static PrototypePlayerController CreatePlayer()
        {
            var playerObject = new GameObject("Prototype Player");
            playerObject.transform.position = Vector3.zero;

            CharacterController characterController = playerObject.AddComponent<CharacterController>();
            characterController.center = new Vector3(0f, 1f, 0f);
            characterController.height = 2f;
            characterController.radius = 0.45f;
            characterController.stepOffset = 0.3f;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Player Visual";
            visual.transform.SetParent(playerObject.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);

            Collider visualCollider = visual.GetComponent<Collider>();
            visualCollider.enabled = false;
            Object.Destroy(visualCollider);

            Renderer renderer = visual.GetComponent<Renderer>();
            renderer.material.color = new Color(0.15f, 0.55f, 1f);

            return playerObject.AddComponent<PrototypePlayerController>();
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 3f, -5f);
            cameraObject.AddComponent<AudioListener>();
            return cameraObject.AddComponent<Camera>();
        }

        private static void CreateTestFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Prototype Test Floor";
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(20f, 0.2f, 20f);

            Renderer renderer = floor.GetComponent<Renderer>();
            renderer.material.color = new Color(0.16f, 0.18f, 0.22f);
        }
    }
}
