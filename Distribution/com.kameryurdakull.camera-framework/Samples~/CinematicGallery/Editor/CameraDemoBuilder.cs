using CameraFramework.Demo;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.ProBuilder;
using System.IO;

namespace CameraFramework.Editor
{
    public static class CameraDemoBuilder
    {
        private static string Root
        {
            get
            {
                var assets = AssetDatabase.FindAssets("CameraDemo t:VisualTreeAsset", new[] { "Assets/Samples" });
                if (assets.Length == 0) throw new FileNotFoundException("Import the Cinematic Gallery sample first.");
                var path = AssetDatabase.GUIDToAssetPath(assets[0]);
                return Path.GetDirectoryName(path).Replace('\\', '/');
            }
        }

        private static string ScenePath => Root + "/CameraFrameworkDemo.unity";
        private static string ProfilePath => Root + "/CameraDemoProfile.asset";
        private static string PanelPath => Root + "/CameraDemoPanel.asset";

        [MenuItem("Tools/Camera Framework/Create Demo Scene")]
        public static void CreateScene()
        {
            var scenePath = AssetDatabase.GenerateUniqueAssetPath(ScenePath);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var profile = LoadOrCreate<CameraEffectProfile>(ProfilePath);
            var panelSettings = LoadOrCreate<PanelSettings>(PanelPath);
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1920, 1080);

            var focus = new GameObject("Sculpture Focus").transform;
            focus.position = new Vector3(0f, 3.2f, 0f);
            var focusTarget = new GameObject("Demo Focus Target").transform;
            focusTarget.position = new Vector3(12f, 6f, 0f);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CinemachineBrain));
            cameraObject.tag = "MainCamera";
            var brain = cameraObject.GetComponent<CinemachineBrain>();
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1.15f);
            var slots = new[]
            {
                MakeCamera(CameraView.Overview, new Vector3(0f, 11f, -25f), focus, 52f),
                MakeCamera(CameraView.Showcase, new Vector3(18f, 7f, -13f), focus, 45f),
                MakeCamera(CameraView.Detail, new Vector3(-9f, 5.5f, -8f), focus, 36f)
            };
            cameraObject.transform.SetPositionAndRotation(slots[0].Camera.transform.position,
                slots[0].Camera.transform.rotation);

            var frameworkObject = new GameObject("Camera Framework");
            var controller = frameworkObject.AddComponent<CameraFrameworkController>();
            controller.Configure(brain, profile, slots, CameraView.Overview);

            var uiObject = new GameObject("Demo Interface", typeof(UIDocument), typeof(CameraDemoPanel));
            var ui = uiObject.GetComponent<UIDocument>();
            ui.panelSettings = panelSettings;
            ui.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Root + "/CameraDemo.uxml");
            var demoPanel = uiObject.GetComponent<CameraDemoPanel>();
            demoPanel.Configure(focusTarget);

            var scopeObject = new GameObject("Demo Dependencies");
            scopeObject.AddComponent<CameraDemoScope>().Configure(controller, demoPanel);

            var lightObject = new GameObject("Sun", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
            var sun = lightObject.GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.25f;
            sun.color = new Color(.8f, .92f, 1f);
            RenderSettings.ambientLight = new Color(.3f, .38f, .48f);
            CreateGallery();

            EditorSceneManager.SaveScene(scene, scenePath);
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!buildScenes.Exists(item => item.path == scenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Camera Framework demo created: {scenePath}");
        }

        private static CameraSlot MakeCamera(CameraView view, Vector3 position, Transform focus, float fov)
        {
            var go = new GameObject($"{view} Camera", typeof(CinemachineCamera),
                typeof(CinemachineRotationComposer), typeof(CameraEffectsExtension));
            go.transform.position = position;
            go.transform.LookAt(focus);
            var camera = go.GetComponent<CinemachineCamera>();
            camera.LookAt = focus;
            var lens = camera.Lens;
            lens.FieldOfView = fov;
            camera.Lens = lens;
            return new CameraSlot { View = view, Camera = camera };
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void CreateGallery()
        {
            Cube("Gallery Floor", new Vector3(90f, 1f, 90f), new Vector3(0f, -.6f, 0f), "Obsidian");
            Cube("Platform / lower", new Vector3(18f, .7f, 18f), new Vector3(0f, .1f, 0f), "Stone");
            Cube("Platform / upper", new Vector3(13f, .6f, 13f), new Vector3(0f, .8f, 0f), "Ivory");
            Cylinder("Sculpture / plinth", 3.2f, 1.2f, new Vector3(0f, 1.6f, 0f), "Ivory");
            Cylinder("Sculpture / core", 1.25f, 5.5f, new Vector3(0f, 4.5f, 0f), "DeepBlue");
            Sphere("Sculpture / crown", 1.7f, new Vector3(0f, 8f, 0f), "Teal");
            Torus("Sculpture / ring A", 4.2f, .18f, new Vector3(0f, 5f, 0f),
                new Vector3(25f, 0f, 20f), "Copper");
            Torus("Sculpture / ring B", 3.5f, .13f, new Vector3(0f, 5f, 0f),
                new Vector3(70f, 32f, 0f), "Copper");
            Torus("Sculpture / halo", 2.7f, .13f, new Vector3(0f, 8f, 0f),
                new Vector3(90f, 0f, 0f), "Copper");

            foreach (var direction in new[] { -1f, 1f })
            {
                Cube($"Path / vertical {direction}", new Vector3(5f, .16f, 25f),
                    new Vector3(0f, -.03f, direction * 22f), "Ivory");
                Cube($"Path / horizontal {direction}", new Vector3(25f, .16f, 5f),
                    new Vector3(direction * 22f, -.03f, 0f), "Ivory");
                Cylinder($"Beacon / {direction}", .9f, 6f,
                    new Vector3(direction * 31f, 2.6f, 0f), "DeepBlue");
                Sphere($"Beacon light / {direction}", 1.15f,
                    new Vector3(direction * 31f, 6.2f, 0f), "Teal");
            }

            foreach (var x in new[] { -16f, 16f })
            foreach (var z in new[] { -15f, 15f })
                Cube($"Monolith / {x} / {z}", new Vector3(2.1f, 11f, 2.1f),
                    new Vector3(x, 5f, z), "Stone");
            Cube("Lintel / north", new Vector3(34f, 1.4f, 1.8f), new Vector3(0f, 11f, 15f), "Stone");
            Cube("Lintel / south", new Vector3(34f, 1.4f, 1.8f), new Vector3(0f, 11f, -15f), "Stone");
            Cylinder("Focus beacon / base", 1.3f, 4f, new Vector3(12f, 1.5f, 0f), "DeepBlue");
            Sphere("Focus beacon / prism", 1.1f, new Vector3(12f, 4.4f, 0f), "Teal");

            PointLight("Key / cyan", new Vector3(7f, 11f, -6f), new Color(.13f, .9f, .84f), 85f, 28f);
            PointLight("Rim / amber", new Vector3(-7f, 10f, 6f), new Color(1f, .46f, .2f), 75f, 26f);
            PointLight("Sculpture / fill", new Vector3(0f, 8f, 0f), new Color(.58f, .88f, 1f), 35f, 15f);
        }

        private static void Cube(string name, Vector3 size, Vector3 position, string material) =>
            Finish(ShapeGenerator.GenerateCube(PivotLocation.Center, size), name, position, Vector3.zero, material);

        private static void Cylinder(string name, float radius, float height, Vector3 position, string material) =>
            Finish(ShapeGenerator.GenerateCylinder(PivotLocation.Center, 24, radius, height, 0),
                name, position, Vector3.zero, material);

        private static void Sphere(string name, float radius, Vector3 position, string material) =>
            Finish(ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, radius, 2),
                name, position, Vector3.zero, material);

        private static void Torus(string name, float radius, float tube, Vector3 position,
            Vector3 rotation, string material) =>
            Finish(ShapeGenerator.GenerateTorus(PivotLocation.Center, 10, 48, radius, tube, true, 360f, 360f),
                name, position, rotation, material);

        private static void Finish(ProBuilderMesh mesh, string name, Vector3 position,
            Vector3 rotation, string material)
        {
            mesh.name = name;
            mesh.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
            var asset = AssetDatabase.LoadAssetAtPath<Material>($"{Root}/Materials/{material}.mat");
            if (asset != null) mesh.GetComponent<MeshRenderer>().sharedMaterial = asset;
        }

        private static void PointLight(string name, Vector3 position, Color color, float intensity, float range)
        {
            var go = new GameObject(name, typeof(Light));
            go.transform.position = position;
            var light = go.GetComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
        }
    }
}
