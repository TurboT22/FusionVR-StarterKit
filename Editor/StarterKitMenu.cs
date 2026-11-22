using UnityEditor;
using UnityEngine;

namespace Fusion.VR.StarterKit.Editor
{
    /// <summary>Menu items for generating and placing Starter Kit stations.</summary>
    public static class StarterKitMenu
    {
        private const string Root = "Tools/FusionVR/Starter Kit/";

        [MenuItem(Root + "Generate All Prefabs")]
        private static void GenerateAll() => StationBuilders.BuildAll(false);

        [MenuItem(Root + "Rebuild All Prefabs (regenerate)")]
        private static void RebuildAll() => StationBuilders.BuildAll(true);

        [MenuItem(Root + "Regenerate Textures and Materials")]
        private static void RegenerateMaterials() => JungleMaterials.RegenerateAll();

        [MenuItem(Root + "Add Room Computer to Scene")]
        private static void AddComputer() => AddToScene(StationBuilders.RoomComputerName);

        [MenuItem(Root + "Add Public Room Totem to Scene")]
        private static void AddTotem() => AddToScene(StationBuilders.PublicRoomBoardName);

        [MenuItem(Root + "Add Name Station to Scene")]
        private static void AddName() => AddToScene(StationBuilders.NameStationName);

        [MenuItem(Root + "Add Colour Station to Scene")]
        private static void AddColour() => AddToScene(StationBuilders.ColourStationName);

        [MenuItem(Root + "Add Leaderboard to Scene")]
        private static void AddLeaderboard() => AddToScene(StationBuilders.LeaderboardName);

        [MenuItem(Root + "Add Wardrobe to Scene")]
        private static void AddWardrobe() => AddToScene(StationBuilders.WardrobeName);

        [MenuItem(Root + "Add Control Panel to Scene")]
        private static void AddControlPanel() => AddToScene(StationBuilders.ControlPanelName);

        [MenuItem(Root + "Add All Stations to Scene")]
        private static void AddAll()
        {
            float x = 0f;
            foreach (string name in StationBuilders.AllNames)
            {
                GameObject go = AddToScene(name);
                if (go != null) go.transform.position = new Vector3(x, 0f, 0f);
                x += 3f;
            }
        }

        [MenuItem(Root + "Setup Hand Triggers")]
        private static void SetupHands()
        {
            FusionVRManager manager = Object.FindFirstObjectByType<FusionVRManager>();
            if (manager == null)
            {
                EditorUtility.DisplayDialog("FusionVR", "No FusionVRManager in the scene. Run Tools > FusionVR > Setup Wizard first.", "OK");
                return;
            }

            int added = 0;
            foreach (Transform hand in new[] { manager.LeftHand, manager.RightHand })
            {
                if (hand == null || hand.GetComponent<FusionVRHandTrigger>() != null) continue;
                Undo.AddComponent<FusionVRHandTrigger>(hand.gameObject);
                added++;
            }
            EditorUtility.DisplayDialog("FusionVR",
                added > 0 ? "Added hand triggers to " + added + " hand(s)." : "Hands already set up (or not assigned on the manager).", "OK");
        }

        private static GameObject AddToScene(string name)
        {
            string path = StationBuilders.PrefabPath(name);
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) StationBuilders.BuildAll(false);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError("[FusionVR] Could not load " + path);
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Add " + name);

            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                Vector3 pivot = view.pivot;
                instance.transform.position = new Vector3(pivot.x, 0f, pivot.z);
            }
            Selection.activeGameObject = instance;
            return instance;
        }
    }
}
