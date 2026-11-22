using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using Fusion.VR.StarterKit;

namespace Fusion.VR.StarterKit.Editor
{
    /// <summary>Builds the jungle station prefabs from procedural meshes and textures.</summary>
    public static class StationBuilders
    {
        public const string RoomComputerName = "FusionVR_RoomComputer";
        public const string NameStationName = "FusionVR_NameStation";
        public const string PublicRoomBoardName = "FusionVR_PublicRoomBoard";
        public const string ColourStationName = "FusionVR_ColourStation";
        public const string LeaderboardName = "FusionVR_Leaderboard";
        public const string WardrobeName = "FusionVR_Wardrobe";
        public const string ControlPanelName = "FusionVR_ControlPanel";

        public static readonly string[] AllNames =
        {
            RoomComputerName, PublicRoomBoardName, NameStationName, ColourStationName,
            LeaderboardName, WardrobeName, ControlPanelName
        };

        private static readonly Color Green = new Color(0.6f, 1f, 0.5f);
        private static readonly Color Amber = new Color(1f, 0.85f, 0.4f);

        public static string PrefabPath(string name) => JungleMaterials.PrefabDir + "/" + name + ".prefab";

        /// <summary>Builds all station prefabs (only missing ones unless <paramref name="force"/>).</summary>
        public static void BuildAll(bool force)
        {
            try
            {
                JungleMaterials.EnsureDir(JungleMaterials.PrefabDir);
                JungleMaterials.EnsureDir(JungleMaterials.MeshDir);

                for (int i = 0; i < AllNames.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("FusionVR Starter Kit", "Generating " + AllNames[i], i / (float)AllNames.Length);
                    BuildOne(AllNames[i], force);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[FusionVR] Starter Kit prefabs are in " + JungleMaterials.PrefabDir);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void BuildOne(string name, bool force)
        {
            string path = PrefabPath(name);
            if (!force && AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var bank = new MeshBank(JungleMaterials.MeshDir + "/" + name + "_Meshes.asset");
            var p = new PropBuilder(bank);

            GameObject root;
            switch (name)
            {
                case RoomComputerName: root = RoomComputer(p); break;
                case NameStationName: root = NameStation(p); break;
                case PublicRoomBoardName: root = PublicRoomBoard(p); break;
                case ColourStationName: root = ColourStation(p); break;
                case LeaderboardName: root = Leaderboard(p); break;
                case WardrobeName: root = Wardrobe(p); break;
                default: root = ControlPanel(p); break;
            }

            try { PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            bank.Save();
        }

        private static Vector2 V(float x, float y) => new Vector2(x, y);

        // ------------------------------------------------------------------ Shared keyboard

        private static Dictionary<string, FusionVRInteractableButton> Keyboard(
            PropBuilder p, Transform board, (string name, string label, float x, float w, JungleMat mat)[] bottom)
        {
            var result = new Dictionary<string, FusionVRInteractableButton>();
            string[] rows = { "1234567890", "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };
            float[] z = { -0.15f, -0.075f, 0f, 0.075f };

            for (int r = 0; r < rows.Length; r++)
            {
                string row = rows[r];
                for (int i = 0; i < row.Length; i++)
                {
                    float x = (i - (row.Length - 1) * 0.5f) * 0.075f;
                    string ch = row[i].ToString();
                    p.Key(board, ch, ch, new Vector3(x, 0f, z[r]), new Vector2(0.058f, 0.058f));
                }
            }

            foreach (var b in bottom)
            {
                result[b.name] = p.Key(board, b.name, b.label, new Vector3(b.x, 0f, 0.165f), new Vector2(b.w, 0.058f), b.mat);
            }
            return result;
        }

        // ------------------------------------------------------------------ Room computer

        private static GameObject RoomComputer(PropBuilder p)
        {
            var root = new GameObject(RoomComputerName);
            Transform t = root.transform;

            p.Box(t, "DeskTop", new Vector3(1.2f, 0.07f, 0.62f), 0.025f, new Vector3(0f, 0.92f, 0f), Vector3.zero, JungleMat.Wood);
            p.Box(t, "Apron", new Vector3(1.08f, 0.09f, 0.5f), 0.02f, new Vector3(0f, 0.835f, 0f), Vector3.zero, JungleMat.DarkWood);

            int leg = 1;
            foreach (float x in new[] { -0.52f, 0.52f })
            {
                foreach (float z in new[] { -0.22f, 0.22f })
                {
                    p.Trunk(t, "Leg" + leg, new Vector3(x, 0f, z), 0.9f, 0.045f, 0.02f, leg);
                    leg++;
                }
            }
            p.Moss(t, new Vector3(-0.5f, 0.955f, -0.22f), 0.1f, 1);
            p.Moss(t, new Vector3(0.52f, 0.955f, 0.2f), 0.08f, 2);

            Transform board = p.Empty(t, "KeyboardBoard", new Vector3(0f, 0.96f, 0.13f), new Vector3(10f, 0f, 0f));
            p.Box(board, "Plate", new Vector3(0.88f, 0.03f, 0.42f), 0.012f, new Vector3(0f, -0.015f, 0.01f), Vector3.zero, JungleMat.DarkWood);

            var keys = Keyboard(p, board, new[]
            {
                ("Clear", "CLR", -0.33f, 0.10f, JungleMat.Wood),
                ("Backspace", "DEL", -0.21f, 0.10f, JungleMat.Wood),
                ("Public", "PUBLIC", 0.00f, 0.20f, JungleMat.Moss),
                ("Enter", "GO", 0.30f, 0.14f, JungleMat.Leaf)
            });
            FusionVRInteractableButton pub = keys["Public"];
            pub.Action = FusionVRInteractableButton.QuickAction.JoinRandomRoom;
            pub.Queue = "Default";
            pub.MaxPlayers = 10;

            Transform screen = p.Empty(t, "Screen", new Vector3(0f, 1.5f, -0.24f), new Vector3(-8f, 0f, 0f));
            p.Box(screen, "Frame", new Vector3(1f, 0.62f, 0.07f), 0.03f, Vector3.zero, Vector3.zero, JungleMat.Wood);
            p.Box(screen, "Glass", new Vector3(0.86f, 0.48f, 0.03f), 0.01f, new Vector3(0f, 0f, 0.03f), Vector3.zero, JungleMat.Screen);
            p.Text(screen, "Title", "ROOM COMPUTER", 0.035f, PropBuilder.Cream, new Vector3(0f, 0.18f, 0.047f), new Vector2(0.8f, 0.08f));
            TextMeshPro code = p.Text(screen, "RoomCode", "ENTER CODE", 0.09f, Green, new Vector3(0f, 0.03f, 0.047f), new Vector2(0.8f, 0.15f));
            TextMeshPro status = p.Text(screen, "Status", "OFFLINE", 0.035f, Amber, new Vector3(0f, -0.17f, 0.047f), new Vector2(0.8f, 0.08f));

            p.Bamboo(t, "PostL", new Vector3(-0.38f, 0.955f, -0.24f), 0.28f, 0.03f);
            p.Bamboo(t, "PostR", new Vector3(0.38f, 0.955f, -0.24f), 0.28f, 0.03f);
            p.Leaves(screen, "LeavesL", new Vector3(-0.46f, 0.3f, 0.02f), 7, 0.3f, 5);
            p.Leaves(screen, "LeavesR", new Vector3(0.46f, 0.3f, 0.02f), 7, 0.3f, 6);
            p.Vine(screen, new Vector3(-0.5f, -0.3f, 0.04f), new Vector3(-0.42f, 0.3f, 0.05f), 0.012f, 0.03f, 3);
            p.Vine(screen, new Vector3(0.5f, -0.3f, 0.04f), new Vector3(0.43f, 0.3f, 0.05f), 0.012f, 0.03f, 4);

            FusionVRRoomKeypad keypad = root.AddComponent<FusionVRRoomKeypad>();
            keypad.RoomCodeText = code;
            keypad.StatusText = status;
            keypad.MaxPlayers = 10;
            return root;
        }

        // ------------------------------------------------------------------ Name station (lectern)

        private static GameObject NameStation(PropBuilder p)
        {
            var root = new GameObject(NameStationName);
            Transform t = root.transform;

            p.Trunk(t, "Post", Vector3.zero, 0.9f, 0.13f, 0.04f, 4);
            Transform board = p.Empty(t, "Board", new Vector3(0f, 0.98f, 0f), new Vector3(35f, 0f, 0f));
            p.Box(board, "Plate", new Vector3(0.98f, 0.05f, 0.7f), 0.02f, Vector3.zero, Vector3.zero, JungleMat.Wood);
            Transform top = p.Empty(board, "Surface", new Vector3(0f, 0.025f, 0f), Vector3.zero);

            p.Box(top, "Glass", new Vector3(0.8f, 0.012f, 0.14f), 0.005f, new Vector3(0f, 0.006f, -0.26f), Vector3.zero, JungleMat.Screen);
            Vector3 flat = new Vector3(90f, 180f, 0f);
            TextMeshPro nameText = p.Text(top, "Name", "", 0.06f, Green, new Vector3(0f, 0.014f, -0.26f), new Vector2(0.78f, 0.12f), TextAlignmentOptions.Center, flat);
            TextMeshPro status = p.Text(top, "Status", "TYPE YOUR NAME", 0.025f, Amber, new Vector3(0f, 0.014f, -0.175f), new Vector2(0.78f, 0.05f), TextAlignmentOptions.Center, flat);

            Transform keys = p.Empty(top, "Keys", new Vector3(0f, 0f, 0.1f), Vector3.zero);
            Keyboard(p, keys, new[]
            {
                ("Clear", "CLR", -0.34f, 0.10f, JungleMat.Wood),
                ("Backspace", "DEL", -0.22f, 0.10f, JungleMat.Wood),
                ("Shift", "SHIFT", -0.07f, 0.12f, JungleMat.Moss),
                ("Space", "SPACE", 0.11f, 0.22f, JungleMat.Wood),
                ("Enter", "SAVE", 0.30f, 0.14f, JungleMat.Leaf)
            });

            p.Leaves(t, "Crown", new Vector3(0f, 0.95f, -0.05f), 8, 0.35f, 6);
            p.Vine(t, new Vector3(0.1f, 0.05f, 0.05f), new Vector3(0.05f, 0.85f, 0.1f), 0.012f, 0.04f, 7);
            p.Moss(t, new Vector3(0.28f, 0.02f, 0.1f), 0.12f, 3);

            FusionVRNameStation station = root.AddComponent<FusionVRNameStation>();
            station.NameText = nameText;
            station.StatusText = status;
            return root;
        }

        // ------------------------------------------------------------------ Public room totem

        private static List<Vector2> Drum(float r, float h)
        {
            return new List<Vector2> { V(0f, 0f), V(r * 0.92f, 0f), V(r, 0.02f), V(r * 1.08f, h * 0.5f), V(r, h - 0.02f), V(r * 0.92f, h), V(0f, h) };
        }

        private static GameObject PublicRoomBoard(PropBuilder p)
        {
            var root = new GameObject(PublicRoomBoardName);
            Transform t = root.transform;

            Mesh baseMesh = p.M("totem_base", () => ProcMesh.Lathe("", new List<Vector2> { V(0f, 0f), V(0.42f, 0f), V(0.42f, 0.06f), V(0.34f, 0.12f), V(0.3f, 0.2f), V(0f, 0.2f) }, 24, 0.6f));
            p.Part(t, "Base", baseMesh, JungleMat.Stone, Vector3.zero, Vector3.zero);

            Mesh drum = p.M("totem_drum", () => ProcMesh.Lathe("", Drum(0.22f, 0.45f), 20, 0.9f));
            Mesh ring = p.M("totem_ring", () => ProcMesh.Lathe("", new List<Vector2> { V(0f, 0f), V(0.26f, 0f), V(0.26f, 0.04f), V(0.2f, 0.05f), V(0f, 0.05f) }, 20, 0.6f));
            for (int k = 0; k < 3; k++)
            {
                float y = 0.2f + k * 0.45f;
                p.Part(t, "Drum" + k, drum, JungleMat.Bark, new Vector3(0f, y, 0f), new Vector3(0f, k * 40f, 0f));
                p.Part(t, "Ring" + k, ring, JungleMat.Stone, new Vector3(0f, y - 0.01f, 0f), Vector3.zero);
            }

            // Carved eyes on the top drum
            Mesh eye = p.M("totem_eye", () => ProcMesh.Lathe("", PropBuilder.DomeProfile(0.035f, 0.025f, 6), 12, 0.2f));
            Material eyeMat = p.Glow("Amber", new Color(1f, 0.7f, 0.15f));
            p.Part(t, "EyeL", eye, eyeMat, new Vector3(-0.09f, 1.45f, 0.235f), new Vector3(90f, 0f, 0f));
            p.Part(t, "EyeR", eye, eyeMat, new Vector3(0.09f, 1.45f, 0.235f), new Vector3(90f, 0f, 0f));

            // Screen on the middle drum
            p.Box(t, "ScreenFrame", new Vector3(0.5f, 0.36f, 0.04f), 0.015f, new Vector3(0f, 0.92f, 0.245f), Vector3.zero, JungleMat.Wood);
            p.Box(t, "Glass", new Vector3(0.44f, 0.3f, 0.03f), 0.008f, new Vector3(0f, 0.92f, 0.255f), Vector3.zero, JungleMat.Screen);
            p.Text(t, "Heading", "PUBLIC ROOMS", 0.03f, PropBuilder.Cream, new Vector3(0f, 1.04f, 0.272f), new Vector2(0.4f, 0.06f));
            TextMeshPro queueText = p.Text(t, "Queue", "DEFAULT", 0.06f, Green, new Vector3(0f, 0.94f, 0.272f), new Vector2(0.42f, 0.1f));
            TextMeshPro status = p.Text(t, "Status", "OFFLINE", 0.027f, Amber, new Vector3(0f, 0.81f, 0.272f), new Vector2(0.42f, 0.06f));

            // Ledge with buttons
            p.Box(t, "Ledge", new Vector3(0.78f, 0.06f, 0.26f), 0.02f, new Vector3(0f, 0.62f, 0.32f), Vector3.zero, JungleMat.Wood);
            Material amber = p.Glow("Amber", new Color(1f, 0.7f, 0.15f));
            Material green = p.Glow("Green", new Color(0.2f, 0.9f, 0.3f));
            Vector3 flat = new Vector3(90f, 180f, 0f);
            Vector3 labelOffset = new Vector3(0f, 0.012f, 0.1f);

            FusionVRInteractableButton prev = p.DomeButton(t, "PrevButton", "PREV", new Vector3(-0.26f, 0.65f, 0.33f), 0.045f, amber, labelOffset, flat);
            FusionVRInteractableButton join = p.DomeButton(t, "JoinButton", "JOIN", new Vector3(0f, 0.65f, 0.33f), 0.065f, green, labelOffset, flat, 0.035f);
            FusionVRInteractableButton next = p.DomeButton(t, "NextButton", "NEXT", new Vector3(0.26f, 0.65f, 0.33f), 0.045f, amber, labelOffset, flat);

            p.Leaves(t, "Crown", new Vector3(0f, 1.62f, 0f), 12, 0.45f, 8);
            p.Vine(t, new Vector3(-0.2f, 0.25f, 0.1f), new Vector3(-0.15f, 1.5f, 0.12f), 0.014f, 0.05f, 9);
            p.Vine(t, new Vector3(0.2f, 0.3f, 0.08f), new Vector3(0.18f, 1.45f, 0.1f), 0.014f, 0.05f, 10);
            p.Moss(t, new Vector3(0.3f, 0.2f, 0.2f), 0.14f, 4);

            FusionVRPublicRoomBoard board = root.AddComponent<FusionVRPublicRoomBoard>();
            board.QueueText = queueText;
            board.StatusText = status;
            board.PrevButton = prev;
            board.NextButton = next;
            board.JoinButton = join;
            return root;
        }

        // ------------------------------------------------------------------ Colour station (paint pots)

        private static GameObject ColourStation(PropBuilder p)
        {
            var root = new GameObject(ColourStationName);
            Transform t = root.transform;

            p.Box(t, "ShelfLow", new Vector3(1.7f, 0.07f, 0.55f), 0.02f, new Vector3(0f, 0.55f, 0.12f), Vector3.zero, JungleMat.Wood);
            p.Box(t, "ShelfHigh", new Vector3(1.7f, 0.07f, 0.4f), 0.02f, new Vector3(0f, 0.9f, -0.15f), Vector3.zero, JungleMat.Wood);
            p.Box(t, "Panel", new Vector3(1.8f, 0.8f, 0.06f), 0.02f, new Vector3(0f, 1.45f, -0.32f), Vector3.zero, JungleMat.DarkWood);

            int post = 1;
            foreach (float x in new[] { -0.78f, 0.78f })
            {
                p.Trunk(t, "PostFront" + post, new Vector3(x, 0f, 0.3f), 0.55f, 0.05f, 0.02f, 20 + post);
                p.Trunk(t, "PostBack" + post, new Vector3(x, 0f, -0.32f), 1.9f, 0.06f, 0.03f, 30 + post);
                post++;
            }

            p.Text(t, "Sign", "PICK YOUR COLOUR", 0.08f, PropBuilder.Cream, new Vector3(0f, 1.56f, -0.285f), new Vector2(1.6f, 0.14f));
            TextMeshPro hex = p.Text(t, "Hex", "#FFFFFF", 0.05f, Green, new Vector3(0f, 1.34f, -0.285f), new Vector2(1.0f, 0.09f));

            // Pots
            Mesh potMesh = p.M("pot", () => ProcMesh.Lathe("", new List<Vector2>
            {
                V(0f, 0f), V(0.05f, 0f), V(0.058f, 0.008f), V(0.07f, 0.05f), V(0.08f, 0.1f), V(0.072f, 0.128f),
                V(0.082f, 0.14f), V(0.082f, 0.152f), V(0.07f, 0.152f), V(0.07f, 0.14f), V(0f, 0.14f)
            }, 18, 0.4f));
            Mesh paintMesh = p.M("pot_paint", () => ProcMesh.Lathe("", new List<Vector2> { V(0f, 0f), V(0.068f, 0f), V(0.068f, 0.004f), V(0f, 0.004f) }, 18, 0.2f));
            Material clay = JungleMaterials.Solid("Clay", new Color(0.55f, 0.3f, 0.18f), 0.3f, Color.black);

            (string name, Color c)[] colours =
            {
                ("Red", new Color(0.9f, 0.12f, 0.1f)), ("Orange", new Color(1f, 0.5f, 0.05f)), ("Yellow", new Color(1f, 0.85f, 0.1f)),
                ("Lime", new Color(0.55f, 0.85f, 0.15f)), ("Green", new Color(0.1f, 0.6f, 0.2f)), ("Teal", new Color(0.05f, 0.6f, 0.55f)),
                ("Cyan", new Color(0.1f, 0.8f, 0.95f)), ("Blue", new Color(0.15f, 0.3f, 0.9f)), ("Purple", new Color(0.5f, 0.2f, 0.8f)),
                ("Pink", new Color(1f, 0.4f, 0.7f)), ("White", new Color(0.95f, 0.95f, 0.95f)), ("Black", new Color(0.08f, 0.08f, 0.1f))
            };

            for (int i = 0; i < colours.Length; i++)
            {
                bool lower = i < 7;
                int idx = lower ? i : i - 7;
                float x = lower ? -0.66f + idx * 0.22f : -0.55f + idx * 0.22f;
                Vector3 pos = lower ? new Vector3(x, 0.585f, 0.12f) : new Vector3(x, 0.935f, -0.15f);
                FusionVRInteractableButton pot = Pot(p, t, "Pot_" + colours[i].name, pos, potMesh, paintMesh, clay, p.Paint(colours[i].name, colours[i].c));
                pot.Action = FusionVRInteractableButton.QuickAction.SetColour;
                pot.ColourValue = colours[i].c;
            }

            // Random pot (rainbow-ish glow)
            FusionVRInteractableButton randomPot = Pot(p, t, "Pot_Random", new Vector3(0.55f, 0.935f, -0.15f), potMesh, paintMesh, clay, p.Glow("Random", new Color(0.9f, 0.3f, 0.9f)));
            p.Text(t, "RandomLabel", "RANDOM", 0.025f, PropBuilder.Cream, new Vector3(0.55f, 1.12f, -0.15f), new Vector2(0.2f, 0.05f));

            // Preview pod
            Mesh pod = p.M("pod", () =>
            {
                var prof = new List<Vector2>();
                ProcMesh.Arc(prof, 0f, 0.15f, 0.13f, 0.15f, -90f, 90f, 14);
                return ProcMesh.Lathe("", prof, 20, 0.5f);
            });
            GameObject podObj = p.Part(t, "Preview", pod, p.Paint("PreviewWhite", Color.white), new Vector3(0f, 1.85f, -0.3f), Vector3.zero);

            p.Leaves(t, "LeavesL", new Vector3(-0.85f, 1.88f, -0.3f), 8, 0.4f, 11);
            p.Leaves(t, "LeavesR", new Vector3(0.85f, 1.88f, -0.3f), 8, 0.4f, 12);
            p.Vine(t, new Vector3(-0.8f, 0.3f, -0.3f), new Vector3(-0.75f, 1.8f, -0.3f), 0.014f, 0.06f, 13);
            p.Moss(t, new Vector3(0.7f, 0.59f, 0.3f), 0.1f, 5);

            FusionVRColourStation station = root.AddComponent<FusionVRColourStation>();
            station.PreviewRenderers = new[] { podObj.GetComponent<Renderer>() };
            station.LabelText = hex;
            station.RandomButton = randomPot;
            return root;
        }

        private static FusionVRInteractableButton Pot(PropBuilder p, Transform parent, string name, Vector3 pos, Mesh potMesh, Mesh paintMesh, Material clay, Material paint)
        {
            Transform root = p.Empty(parent, name, pos, Vector3.zero);
            GameObject vis = p.Part(root, "Visual", potMesh, clay, Vector3.zero, Vector3.zero);
            p.Part(vis.transform, "Paint", paintMesh, paint, new Vector3(0f, 0.139f, 0f), Vector3.zero);

            BoxCollider col = root.gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0.1f, 0f);
            col.size = new Vector3(0.17f, 0.2f, 0.17f);

            FusionVRInteractableButton btn = root.gameObject.AddComponent<FusionVRInteractableButton>();
            btn.HandTag = string.Empty;
            btn.VisualTarget = vis.transform;
            btn.PressOffset = new Vector3(0f, -0.015f, 0f);
            return btn;
        }

        // ------------------------------------------------------------------ Leaderboard

        private static GameObject Leaderboard(PropBuilder p)
        {
            var root = new GameObject(LeaderboardName);
            Transform t = root.transform;

            p.Bamboo(t, "PostL", new Vector3(-0.85f, 0f, -0.04f), 2.6f, 0.045f);
            p.Bamboo(t, "PostR", new Vector3(0.85f, 0f, -0.04f), 2.6f, 0.045f);
            p.Box(t, "Board", new Vector3(1.8f, 1.3f, 0.08f), 0.03f, new Vector3(0f, 1.7f, 0f), Vector3.zero, JungleMat.Wood);
            p.Box(t, "Glass", new Vector3(1.56f, 1.08f, 0.02f), 0.008f, new Vector3(0f, 1.7f, 0.05f), Vector3.zero, JungleMat.Screen);

            float z = 0.062f;
            TextMeshPro title = p.Text(t, "Title", "TOP PLAYERS", 0.09f, PropBuilder.Cream, new Vector3(0f, 2.15f, z), new Vector2(1.4f, 0.12f));
            TextMeshPro ranks = p.Text(t, "Ranks", "", 0.07f, Amber, new Vector3(-0.62f, 1.62f, z), new Vector2(0.2f, 0.85f), TextAlignmentOptions.TopLeft);
            TextMeshPro names = p.Text(t, "Names", "", 0.07f, Color.white, new Vector3(-0.1f, 1.62f, z), new Vector2(0.9f, 0.85f), TextAlignmentOptions.TopLeft);
            TextMeshPro scores = p.Text(t, "Scores", "", 0.07f, Green, new Vector3(0.6f, 1.62f, z), new Vector2(0.34f, 0.85f), TextAlignmentOptions.TopRight);
            TextMeshPro footer = p.Text(t, "Footer", "", 0.04f, Amber, new Vector3(0f, 1.22f, z), new Vector2(1.4f, 0.08f));

            p.Leaves(t, "LeavesL", new Vector3(-0.85f, 2.35f, 0.04f), 9, 0.4f, 14);
            p.Leaves(t, "LeavesR", new Vector3(0.85f, 2.35f, 0.04f), 9, 0.4f, 15);
            p.Moss(t, new Vector3(0.3f, 2.35f, 0f), 0.16f, 6);
            p.Moss(t, new Vector3(-0.4f, 2.35f, 0f), 0.12f, 7);
            p.Vine(t, new Vector3(-0.92f, 0.3f, 0.02f), new Vector3(-0.9f, 2.3f, 0.04f), 0.014f, 0.06f, 16);
            p.Vine(t, new Vector3(0.92f, 0.4f, 0.02f), new Vector3(0.9f, 2.3f, 0.04f), 0.014f, 0.06f, 17);

            FusionVRLeaderboard board = root.AddComponent<FusionVRLeaderboard>();
            board.TitleText = title;
            board.RanksText = ranks;
            board.NamesText = names;
            board.ScoresText = scores;
            board.FooterText = footer;
            return root;
        }

        // ------------------------------------------------------------------ Wardrobe

        private static GameObject Wardrobe(PropBuilder p)
        {
            var root = new GameObject(WardrobeName);
            Transform t = root.transform;

            p.Box(t, "Back", new Vector3(1.4f, 2.1f, 0.06f), 0.015f, new Vector3(0f, 1.08f, -0.33f), Vector3.zero, JungleMat.DarkWood);
            p.Box(t, "SideL", new Vector3(0.06f, 2.1f, 0.55f), 0.015f, new Vector3(-0.67f, 1.08f, -0.07f), Vector3.zero, JungleMat.Wood);
            p.Box(t, "SideR", new Vector3(0.06f, 2.1f, 0.55f), 0.015f, new Vector3(0.67f, 1.08f, -0.07f), Vector3.zero, JungleMat.Wood);
            p.Box(t, "Top", new Vector3(1.5f, 0.08f, 0.66f), 0.025f, new Vector3(0f, 2.16f, -0.05f), Vector3.zero, JungleMat.Wood);
            p.Box(t, "Base", new Vector3(1.4f, 0.14f, 0.56f), 0.02f, new Vector3(0f, 0.07f, -0.07f), Vector3.zero, JungleMat.DarkWood);
            p.Box(t, "Shelf", new Vector3(1.28f, 0.04f, 0.5f), 0.012f, new Vector3(0f, 1.45f, -0.07f), Vector3.zero, JungleMat.Wood);

            // Hanging rail with leaf "garments"
            GameObject rail = p.Bamboo(t, "Rail", new Vector3(0.6f, 1.85f, -0.1f), 1.2f, 0.018f);
            rail.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            for (int i = 0; i < 5; i++)
            {
                p.Part(t, "Garment" + i, p.LeafMesh(i), JungleMat.Leaf, new Vector3(-0.45f + i * 0.22f, 1.83f, -0.1f), new Vector3(90f, 0f, 0f), Vector3.one * 3f);
            }

            // Open doors
            Transform doorL = p.Empty(t, "DoorL", new Vector3(-0.66f, 1.08f, 0.2f), new Vector3(0f, -100f, 0f));
            p.Box(doorL, "Panel", new Vector3(0.62f, 2.0f, 0.04f), 0.015f, new Vector3(0.31f, 0f, 0f), Vector3.zero, JungleMat.Wood);
            p.Vine(doorL, new Vector3(0.1f, -0.8f, 0.03f), new Vector3(0.2f, 0.8f, 0.03f), 0.012f, 0.04f, 18);
            Transform doorR = p.Empty(t, "DoorR", new Vector3(0.66f, 1.08f, 0.2f), new Vector3(0f, 100f, 0f));
            p.Box(doorR, "Panel", new Vector3(0.62f, 2.0f, 0.04f), 0.015f, new Vector3(-0.31f, 0f, 0f), Vector3.zero, JungleMat.Wood);
            p.Vine(doorR, new Vector3(-0.1f, -0.8f, 0.03f), new Vector3(-0.2f, 0.8f, 0.03f), 0.012f, 0.04f, 19);

            // Lamp and crown
            Mesh lamp = p.M("wardrobe_lamp", () => ProcMesh.Lathe("", PropBuilder.DomeProfile(0.05f, 0.04f, 6), 12, 0.2f));
            p.Part(t, "Lamp", lamp, p.Glow("Amber", new Color(1f, 0.7f, 0.15f)), new Vector3(0f, 2.12f, -0.1f), new Vector3(180f, 0f, 0f));
            p.Leaves(t, "Crown", new Vector3(0f, 2.2f, -0.05f), 12, 0.5f, 21);
            p.Moss(t, new Vector3(0.4f, 2.2f, -0.1f), 0.14f, 8);

            // Control plinth
            p.Trunk(t, "Plinth", new Vector3(0f, 0f, 0.95f), 0.8f, 0.1f, 0.03f, 9);
            Transform board = p.Empty(t, "Board", new Vector3(0f, 0.9f, 0.95f), new Vector3(30f, 0f, 0f));
            p.Box(board, "Plate", new Vector3(1.0f, 0.05f, 0.42f), 0.02f, Vector3.zero, Vector3.zero, JungleMat.Wood);
            Transform top = p.Empty(board, "Surface", new Vector3(0f, 0.025f, 0f), Vector3.zero);
            Vector3 flat = new Vector3(90f, 180f, 0f);

            p.Box(top, "Glass", new Vector3(0.8f, 0.012f, 0.17f), 0.005f, new Vector3(0f, 0.006f, -0.1f), Vector3.zero, JungleMat.Screen);
            TextMeshPro partText = p.Text(top, "Part", "HEAD", 0.03f, Amber, new Vector3(0f, 0.014f, -0.15f), new Vector2(0.7f, 0.05f), TextAlignmentOptions.Center, flat);
            TextMeshPro cosmeticText = p.Text(top, "Cosmetic", "(NONE)", 0.045f, Green, new Vector3(0f, 0.014f, -0.105f), new Vector2(0.76f, 0.07f), TextAlignmentOptions.Center, flat);
            TextMeshPro statusText = p.Text(top, "Status", "", 0.025f, Amber, new Vector3(0f, 0.014f, -0.055f), new Vector2(0.7f, 0.04f), TextAlignmentOptions.Center, flat);

            Material amber = p.Glow("Amber", new Color(1f, 0.7f, 0.15f));
            Material green = p.Glow("Green", new Color(0.2f, 0.9f, 0.3f));
            Material red = p.Glow("Red", new Color(0.95f, 0.2f, 0.15f));
            Vector3 lo = new Vector3(0f, 0.012f, 0.07f);

            FusionVRInteractableButton prevPart = p.DomeButton(top, "PrevPart", "PART -", new Vector3(-0.36f, 0f, 0.07f), 0.03f, amber, lo, flat, 0.018f);
            FusionVRInteractableButton nextPart = p.DomeButton(top, "NextPart", "PART +", new Vector3(-0.24f, 0f, 0.07f), 0.03f, amber, lo, flat, 0.018f);
            FusionVRInteractableButton prevItem = p.DomeButton(top, "PrevItem", "ITEM -", new Vector3(-0.08f, 0f, 0.07f), 0.03f, amber, lo, flat, 0.018f);
            FusionVRInteractableButton nextItem = p.DomeButton(top, "NextItem", "ITEM +", new Vector3(0.04f, 0f, 0.07f), 0.03f, amber, lo, flat, 0.018f);
            FusionVRInteractableButton equip = p.DomeButton(top, "Equip", "EQUIP", new Vector3(0.2f, 0f, 0.07f), 0.03f, green, lo, flat, 0.018f);
            FusionVRInteractableButton remove = p.DomeButton(top, "Remove", "REMOVE", new Vector3(0.34f, 0f, 0.07f), 0.03f, red, lo, flat, 0.018f);

            FusionVRWardrobe wardrobe = root.AddComponent<FusionVRWardrobe>();
            wardrobe.PartText = partText;
            wardrobe.CosmeticText = cosmeticText;
            wardrobe.StatusText = statusText;
            wardrobe.PrevPartButton = prevPart;
            wardrobe.NextPartButton = nextPart;
            wardrobe.PrevItemButton = prevItem;
            wardrobe.NextItemButton = nextItem;
            wardrobe.EquipButton = equip;
            wardrobe.RemoveButton = remove;
            wardrobe.PlayerPrefabTemplate = global::Fusion.VR.Editor.FusionVRPrefabBuilder.LoadPlayerPrefab();
            return root;
        }

        // ------------------------------------------------------------------ Control panel

        private static GameObject ControlPanel(PropBuilder p)
        {
            var root = new GameObject(ControlPanelName);
            Transform t = root.transform;

            p.Bamboo(t, "PostL", new Vector3(-0.42f, 0f, 0f), 0.95f, 0.04f);
            p.Bamboo(t, "PostR", new Vector3(0.42f, 0f, 0f), 0.95f, 0.04f);

            Transform slab = p.Empty(t, "Slab", new Vector3(0f, 1.0f, 0f), new Vector3(20f, 0f, 0f));
            p.Box(slab, "Stone", new Vector3(1.1f, 0.1f, 0.5f), 0.03f, Vector3.zero, Vector3.zero, JungleMat.Stone);
            Transform top = p.Empty(slab, "Surface", new Vector3(0f, 0.05f, 0f), Vector3.zero);
            Vector3 flat = new Vector3(90f, 180f, 0f);
            Vector3 labelOffset = new Vector3(0f, 0.012f, 0.15f);

            Material green = p.Glow("Green", new Color(0.2f, 0.9f, 0.3f));
            Material amber = p.Glow("Amber", new Color(1f, 0.7f, 0.15f));
            Material red = p.Glow("Red", new Color(0.95f, 0.2f, 0.15f));

            FusionVRInteractableButton mute = p.DomeButton(top, "MuteButton", "MUTE", new Vector3(-0.32f, 0f, 0.08f), 0.07f, green, labelOffset, flat, 0.035f);
            FusionVRInteractableButton deafen = p.DomeButton(top, "DeafenButton", "DEAFEN", new Vector3(0f, 0f, 0.08f), 0.07f, amber, labelOffset, flat, 0.035f);
            FusionVRInteractableButton leave = p.DomeButton(top, "LeaveButton", "LEAVE", new Vector3(0.32f, 0f, 0.08f), 0.07f, red, labelOffset, flat, 0.035f);

            Mesh lampMesh = p.M("panel_lamp", () => ProcMesh.Lathe("", PropBuilder.DomeProfile(0.03f, 0.03f, 6), 12, 0.2f));
            Material lampMat = p.Glow("Lamp", new Color(0.3f, 0.9f, 0.3f));
            GameObject muteLamp = p.Part(top, "MuteLamp", lampMesh, lampMat, new Vector3(-0.32f, 0f, -0.13f), Vector3.zero);
            GameObject deafenLamp = p.Part(top, "DeafenLamp", lampMesh, lampMat, new Vector3(0f, 0f, -0.13f), Vector3.zero);

            p.Box(top, "Glass", new Vector3(0.28f, 0.012f, 0.1f), 0.005f, new Vector3(0.32f, 0.006f, -0.13f), Vector3.zero, JungleMat.Screen);
            TextMeshPro status = p.Text(top, "Status", "VOICE", 0.035f, Green, new Vector3(0.32f, 0.014f, -0.13f), new Vector2(0.26f, 0.08f), TextAlignmentOptions.Center, flat);

            p.Leaves(t, "Leaves", new Vector3(-0.5f, 1.05f, -0.1f), 7, 0.3f, 22);
            p.Moss(t, new Vector3(0.45f, 1.08f, -0.05f), 0.1f, 9);
            p.Vine(t, new Vector3(0.45f, 0.1f, 0.02f), new Vector3(0.42f, 0.95f, 0.02f), 0.012f, 0.04f, 23);

            FusionVRControlPanel panel = root.AddComponent<FusionVRControlPanel>();
            panel.MuteButton = mute;
            panel.DeafenButton = deafen;
            panel.LeaveButton = leave;
            panel.MuteLamp = muteLamp.GetComponent<Renderer>();
            panel.DeafenLamp = deafenLamp.GetComponent<Renderer>();
            panel.StatusText = status;
            return root;
        }
    }
}
