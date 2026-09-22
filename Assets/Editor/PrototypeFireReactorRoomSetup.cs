#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElementalAnomaly.Editor
{
    [InitializeOnLoad]
    public static class PrototypeFireReactorRoomSetup
    {
        private const string RoomRootName = "Fire Reactor Room";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string MaterialFolder = "Assets/Materials";

        private readonly struct PropPlacement
        {
            public readonly string FileName;
            public readonly Vector3 Position;
            public readonly Vector3 Scale;
            public readonly float Yaw;

            public PropPlacement(string fileName, Vector3 position, Vector3 scale, float yaw = 0f)
            {
                FileName = fileName;
                Position = position;
                Scale = scale;
                Yaw = yaw;
            }
        }

        static PrototypeFireReactorRoomSetup()
        {
            EditorApplication.delayCall += BuildRoomIfMissing;
        }

        [MenuItem("Elemental Anomaly/Build Fire Reactor Room")]
        public static void BuildRoomIfMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                return;
            }

            if (GameObject.Find(RoomRootName) != null)
            {
                return;
            }

            GameObject roomRoot = new GameObject(RoomRootName);
            Undo.RegisterCreatedObjectUndo(roomRoot, "Build Fire Reactor Room");

            BuildRoomShell(roomRoot.transform);
            BuildFireReactor(roomRoot.transform);
            PlaceSciFiProps(roomRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void BuildRoomShell(Transform parent)
        {
            GameObject floor = GameObject.Find("Prototype Test Floor");
            if (floor != null)
            {
                floor.transform.position = new Vector3(0f, -0.1f, 0f);
                floor.transform.localScale = new Vector3(12f, 0.2f, 8f);
                floor.transform.SetParent(parent, true);
                floor.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial(
                    "PrototypeFloor.mat", new Color(0.16f, 0.18f, 0.22f));
            }

            Material wallMaterial = GetOrCreateMaterial("PrototypeWall.mat", new Color(0.08f, 0.1f, 0.14f));
            Material trimMaterial = GetOrCreateMaterial("PrototypeTrim.mat", new Color(0.24f, 0.28f, 0.34f));

            CreateBlock("Wall Left", parent, new Vector3(-6f, 2f, 0f), new Vector3(0.2f, 4f, 8f), wallMaterial);
            CreateBlock("Wall Right", parent, new Vector3(6f, 2f, 0f), new Vector3(0.2f, 4f, 8f), wallMaterial);
            CreateBlock("Wall Back", parent, new Vector3(0f, 2f, 4f), new Vector3(12f, 4f, 0.2f), wallMaterial);
            CreateBlock("Wall Front Left", parent, new Vector3(-4.3f, 2f, -4f), new Vector3(3.4f, 4f, 0.2f), wallMaterial);
            CreateBlock("Wall Front Right", parent, new Vector3(4.3f, 2f, -4f), new Vector3(3.4f, 4f, 0.2f), wallMaterial);
            CreateBlock("Door Lintel", parent, new Vector3(0f, 3.4f, -4f), new Vector3(3.2f, 1.2f, 0.2f), trimMaterial);
            CreateBlock("Ceiling", parent, new Vector3(0f, 4f, 0f), new Vector3(12f, 0.2f, 8f), wallMaterial);

            CreateBlock("Exit Door", parent, new Vector3(0f, 1.7f, -3.9f), new Vector3(2.7f, 3.3f, 0.15f), trimMaterial);
        }

        private static void BuildFireReactor(Transform parent)
        {
            var reactor = new GameObject("Fire Reactor");
            reactor.transform.SetParent(parent, false);
            reactor.transform.position = new Vector3(0f, 0f, 2.4f);

            Material darkMetal = GetOrCreateMaterial("PrototypeReactorMetal.mat", new Color(0.07f, 0.08f, 0.1f));
            Material fireGlow = GetOrCreateMaterial("PrototypeFireGlow.mat", new Color(1f, 0.08f, 0.01f), true);

            CreatePrimitive("Reactor Base", PrimitiveType.Cylinder, reactor.transform,
                new Vector3(0f, 0.45f, 0f), new Vector3(2.3f, 0.45f, 2.3f), darkMetal);
            CreatePrimitive("Reactor Core", PrimitiveType.Sphere, reactor.transform,
                new Vector3(0f, 1.75f, 0f), new Vector3(1.05f, 1.05f, 1.05f), fireGlow);
            CreatePrimitive("Reactor Ring", PrimitiveType.Cylinder, reactor.transform,
                new Vector3(0f, 1.75f, 0f), new Vector3(1.55f, 0.08f, 1.55f), fireGlow);
            CreatePrimitive("Reactor Column", PrimitiveType.Cylinder, reactor.transform,
                new Vector3(0f, 2.4f, 0f), new Vector3(0.35f, 0.7f, 0.35f), darkMetal);

            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.25f;
                CreatePrimitive("Reactor Support", PrimitiveType.Cube, reactor.transform,
                    new Vector3(offset.x, 0.9f, offset.z), new Vector3(0.25f, 0.9f, 0.25f), darkMetal);
            }

            var lightObject = new GameObject("Fire Reactor Light");
            lightObject.transform.SetParent(reactor.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.12f, 0.02f);
            light.intensity = 4f;
            light.range = 7f;
        }

        private static void PlaceSciFiProps(Transform parent)
        {
            var placements = new List<PropPlacement>
            {
                new("Prop_Crate.fbx", new Vector3(-4.6f, 0.45f, 2.8f), Vector3.one * 0.9f, 15f),
                new("Prop_Crate_Large.fbx", new Vector3(-3.5f, 0.7f, 2.9f), Vector3.one * 0.8f, -8f),
                new("Prop_Barrel1.fbx", new Vector3(-4.9f, 0.65f, 1.3f), Vector3.one * 0.8f, 20f),
                new("Prop_Barrel2_Closed.fbx", new Vector3(-4.9f, 0.65f, 0.2f), Vector3.one * 0.8f, -15f),
                new("Prop_Desk_Medium.fbx", new Vector3(3.9f, 0f, 2.8f), Vector3.one * 0.75f, 180f),
                new("Prop_Chair.fbx", new Vector3(3.9f, 0f, 1.8f), Vector3.one * 0.75f),
                new("Prop_Locker.fbx", new Vector3(5.1f, 1f, 1.9f), Vector3.one * 0.9f, 180f),
                new("Prop_Shelves_WideTall.fbx", new Vector3(4.9f, 0f, -0.2f), Vector3.one * 0.65f, 180f),
                new("Prop_Shelves_ThinTall.fbx", new Vector3(-5.1f, 0f, -1.8f), Vector3.one * 0.65f),
                new("Prop_Desk_Small.fbx", new Vector3(3.9f, 0f, -1.8f), Vector3.one * 0.75f, 180f),
                new("Prop_Chest.fbx", new Vector3(2.6f, 0.45f, -2.4f), Vector3.one * 0.75f, 20f),
                new("Prop_Ammo.fbx", new Vector3(3.3f, 0.25f, 2.4f), Vector3.one * 0.5f),
                new("Prop_Ammo_Small.fbx", new Vector3(4.2f, 1.05f, 2.4f), Vector3.one * 0.5f),
                new("Prop_HealthPack.fbx", new Vector3(-3.2f, 0.25f, -2.7f), Vector3.one * 0.55f),
                new("Prop_KeyCard.fbx", new Vector3(3.5f, 1.05f, 2.4f), Vector3.one * 0.35f),
                new("Prop_Grenade.fbx", new Vector3(4.6f, 0.95f, 2.4f), Vector3.one * 0.4f),
                new("Prop_Mine.fbx", new Vector3(-2.2f, 0.2f, 2.7f), Vector3.one * 0.45f),
                new("Prop_Syringe.fbx", new Vector3(2.9f, 1.05f, 2.4f), Vector3.one * 0.4f),
                new("Enemy_EyeDrone.fbx", new Vector3(-2.8f, 2.8f, 3.5f), Vector3.one * 0.7f),
                new("Enemy_QuadShell.fbx", new Vector3(2.4f, 0.55f, 3.2f), Vector3.one * 0.7f, 180f)
            };

            foreach (PropPlacement placement in placements)
            {
                string path = $"{PrefabFolder}/{placement.FileName}";
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                {
                    Debug.LogWarning($"Could not place missing asset: {path}");
                    continue;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
                instance.name = placement.FileName.Replace(".fbx", string.Empty);
                instance.transform.SetParent(parent, true);
                instance.transform.position = placement.Position;
                instance.transform.rotation = Quaternion.Euler(0f, placement.Yaw, 0f);
                instance.transform.localScale = placement.Scale;
                Undo.RegisterCreatedObjectUndo(instance, "Place Fire Reactor Room prop");
            }
        }

        private static GameObject CreateBlock(
            string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            return CreatePrimitive(name, PrimitiveType.Cube, parent, position, scale, material);
        }

        private static GameObject CreatePrimitive(
            string name, PrimitiveType type, Transform parent, Vector3 localPosition,
            Vector3 localScale, Material material)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            Undo.RegisterCreatedObjectUndo(primitive, "Create Fire Reactor Room object");
            return primitive;
        }

        private static Material GetOrCreateMaterial(string fileName, Color color, bool emission = false)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            string path = $"{MaterialFolder}/{fileName}";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 4f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
#endif
