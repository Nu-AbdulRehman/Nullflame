using UnityEngine;

namespace Nullflame
{
    public enum OpeningKind { Open, OpenFrame, Door, ContainmentDoor, ShowOnlyDoor, GateSlot }
    public enum PropKind { Crate, Cart, Cabinet, Bunk, Locker, LanternRack, LabBench }

    [System.Serializable] public struct AreaSpec { public string id; public Rect xz; public bool playerAccess; }
    [System.Serializable] public struct WallSpec { public Vector2 a, b; }
    [System.Serializable] public struct OpeningSpec { public string id; public Vector2 center; public float width; public OpeningKind kind; }
    [System.Serializable] public struct PropSpec { public string id; public PropKind kind; public Vector3 center; public Vector3 size; public float yaw; }
    [System.Serializable] public struct BlockerSpec { public string id; public Vector2 center; public Vector2 size; }
    [System.Serializable] public struct CrackPointSpec { public string id; public float zMin, zMax, stripX, dropX, wallFaceX; }

    // Facility geometry in metres, X east, Z north (design doc §4, IMPL §6.1, Appendix A.1-A.3).
    // Defaults are the approved Appendix A values (AD-003); edit them in the asset, then rebuild.
    [CreateAssetMenu(menuName = "Nullflame/Facility Layout", fileName = "FacilityLayout")]
    public sealed class FacilityLayout : ScriptableObject
    {
        public float wallThickness = 0.2f;
        public float ceilingLightRange = 8f;      // look only; gameplay never reads lights
        public float ceilingLightIntensity = 1.5f;

        public AreaSpec[] areas =
        {
            Area("TimeRoom", 0f, 7.5f, 0f, 6f, true),
            Area("ScpRoomA", 7.5f, 15f, 0f, 6f, false),
            Area("ScpRoomB", 15f, 22.5f, 0f, 6f, false),
            Area("ScpRoomC", 22.5f, 30f, 0f, 6f, false),
            Area("ScpRoomD", 30f, 37.5f, 0f, 6f, false),
            Area("AccessCorridor", 0f, 37.5f, 6f, 12f, true),
            Area("MainCorridor", 15.75f, 21.75f, 12f, 40f, true),
            Area("PlayerRoom", 8.25f, 15.75f, 22.25f, 29.75f, true),
            Area("Lab", 21.75f, 32f, 19.5f, 29.75f, true),
            Area("Barracks", 21.75f, 32f, 29.75f, 40f, true),
            Area("SleepingAlcove", 32f, 33.75f, 34.85f, 40f, false),
            Area("SemiFinalGate", 15.75f, 21.75f, 40f, 41.5f, true),
            Area("FinalCorridor", 15.75f, 21.75f, 41.5f, 56f, true),
            Area("WestSideCorridor", 9.75f, 15.75f, 53f, 56f, false),
            Area("EastSideCorridor", 21.75f, 27.75f, 53f, 56f, false),
            Area("FinalGate", 15.75f, 21.75f, 56f, 57.5f, true),
        };

        public WallSpec[] walls =
        {
            Wall(0f, 0f, 37.5f, 0f), Wall(0f, 0f, 0f, 12f), Wall(37.5f, 0f, 37.5f, 12f),
            Wall(7.5f, 0f, 7.5f, 6f), Wall(15f, 0f, 15f, 6f), Wall(22.5f, 0f, 22.5f, 6f), Wall(30f, 0f, 30f, 6f),
            Wall(0f, 6f, 37.5f, 6f),
            Wall(0f, 12f, 15.75f, 12f), Wall(21.75f, 12f, 37.5f, 12f),
            Wall(15.75f, 12f, 15.75f, 40f),
            Wall(8.25f, 22.25f, 15.75f, 22.25f), Wall(8.25f, 29.75f, 15.75f, 29.75f), Wall(8.25f, 22.25f, 8.25f, 29.75f),
            Wall(21.75f, 12f, 21.75f, 40f),
            Wall(21.75f, 19.5f, 32f, 19.5f), Wall(32f, 19.5f, 32f, 29.75f), Wall(21.75f, 29.75f, 32f, 29.75f),
            Wall(32f, 29.75f, 32f, 40f),
            Wall(32f, 34.85f, 33.75f, 34.85f), Wall(33.75f, 34.85f, 33.75f, 40f), Wall(21.75f, 40f, 33.75f, 40f),
            Wall(15.75f, 40f, 15.75f, 41.5f), Wall(21.75f, 40f, 21.75f, 41.5f),
            Wall(15.75f, 41.5f, 15.75f, 53f), Wall(21.75f, 41.5f, 21.75f, 53f),
            Wall(9.75f, 53f, 15.75f, 53f), Wall(9.75f, 56f, 15.75f, 56f), Wall(9.75f, 53f, 9.75f, 56f),
            Wall(21.75f, 53f, 27.75f, 53f), Wall(21.75f, 56f, 27.75f, 56f), Wall(27.75f, 53f, 27.75f, 56f),
            Wall(15.75f, 56f, 15.75f, 57.5f), Wall(21.75f, 56f, 21.75f, 57.5f), Wall(15.75f, 57.5f, 21.75f, 57.5f),
        };

        public OpeningSpec[] openings =
        {
            Opening("D-SCP-T", 3.75f, 6f, 2f, OpeningKind.ContainmentDoor),
            Opening("D-SCP-A", 11.25f, 6f, 2f, OpeningKind.ContainmentDoor),
            Opening("D-SCP-B", 18.75f, 6f, 2f, OpeningKind.ContainmentDoor),
            Opening("D-SCP-C", 26.25f, 6f, 2f, OpeningKind.ContainmentDoor),
            Opening("D-SCP-D", 33.75f, 6f, 2f, OpeningKind.ContainmentDoor),
            Opening("D-PLAYER", 15.75f, 26f, 2f, OpeningKind.OpenFrame),
            Opening("D-BARRACKS", 21.75f, 35f, 2.5f, OpeningKind.Door),
            Opening("D-LAB", 27f, 29.75f, 2f, OpeningKind.Door),
            Opening("D-SLEEP", 32f, 37.4f, 2f, OpeningKind.Open),
            Opening("D-SLEEP-SHOW", 33.75f, 37.4f, 2f, OpeningKind.ShowOnlyDoor),
            Opening("D-SIDE-W", 9.75f, 54.5f, 2f, OpeningKind.ShowOnlyDoor),
            Opening("D-SIDE-E", 27.75f, 54.5f, 2f, OpeningKind.ShowOnlyDoor),
        };

        public PropSpec[] props =
        {
            Prop("Crate1", PropKind.Crate, 16.45f, 16f, 1.2f, 1.2f, 1.2f),
            Prop("Crate2", PropKind.Crate, 16.45f, 33f, 1.2f, 1.2f, 1.2f),
            Prop("Cart", PropKind.Cart, 6f, 8f, 1.4f, 0.8f, 1f),
            Prop("Cabinet", PropKind.Cabinet, 1f, 5.6f, 1.2f, 0.6f, 2f),
            Prop("LanternRack", PropKind.LanternRack, 29f, 39.6f, 2f, 0.5f, 1.8f),
            Prop("Bunk1", PropKind.Bunk, 23.6f, 31.5f, 1f, 2f, 1.2f),
            Prop("Bunk2", PropKind.Bunk, 30.2f, 31.5f, 1f, 2f, 1.2f),
            Prop("Locker1", PropKind.Locker, 25.2f, 30.2f, 0.6f, 0.5f, 2f),
            Prop("Locker2", PropKind.Locker, 28.9f, 30.2f, 0.6f, 0.5f, 2f),
            Prop("LabBench1", PropKind.LabBench, 25f, 24f, 2f, 0.8f, 0.9f),
            Prop("LabBench2", PropKind.LabBench, 29f, 24f, 2f, 0.8f, 0.9f),
        };

        public BlockerSpec[] blockers =
        {
            Blocker("Block-SCP-A", 11.25f, 6f), Blocker("Block-SCP-B", 18.75f, 6f),
            Blocker("Block-SCP-C", 26.25f, 6f), Blocker("Block-SCP-D", 33.75f, 6f),
        };

        public Vector2[] ceilingLights =
        {
            new Vector2(4f, 9f), new Vector2(12f, 9f), new Vector2(20f, 9f), new Vector2(28f, 9f), new Vector2(34f, 9f),
            new Vector2(18.75f, 15f), new Vector2(18.75f, 21f), new Vector2(18.75f, 27f), new Vector2(18.75f, 33f), new Vector2(18.75f, 39f),
            new Vector2(18.75f, 44f), new Vector2(18.75f, 49f), new Vector2(18.75f, 54f),
            new Vector2(3.75f, 3f), new Vector2(26.875f, 34.875f), new Vector2(26.875f, 24.625f),
        };

        [Header("Meltdown flame lights (Appendix A.3)")]
        public Vector2[] alarmLights =
        {
            new Vector2(8f, 8f), new Vector2(30f, 11f), new Vector2(20.5f, 14f), new Vector2(20.5f, 30f),
        };
        public Vector3[] wallFireLanterns =
        {
            new Vector3(15.95f, 2f, 44f), new Vector3(15.95f, 2f, 48.5f), new Vector3(15.95f, 2f, 53f),
            new Vector3(21.55f, 2f, 44f), new Vector3(21.55f, 2f, 48.5f), new Vector3(21.55f, 2f, 53f),
        };
        [Header("Final Corridor pipes (Appendix A.6, AD-012)")]
        public float pipeHeight = 2.5f;
        public float pipeStripWidth = 1f;
        public CrackPointSpec[] crackPoints =
        {
            Crack("W1", 42f, true), Crack("W2", 45f, true), Crack("W3", 48f, true), Crack("W4", 51f, true),
            Crack("E1", 42f, false), Crack("E2", 45f, false), Crack("E3", 48f, false), Crack("E4", 51f, false),
        };

        public Color alarmColor = new Color(1f, 0.1f, 0.05f);
        public Color fireColor = new Color(1f, 0.5f, 0.15f);

        public bool TryGetArea(string id, out Rect xz)
        {
            for (int i = 0; i < areas.Length; i++)
            {
                if (areas[i].id == id) { xz = areas[i].xz; return true; }
            }
            xz = default;
            return false;
        }

        static AreaSpec Area(string id, float x0, float x1, float z0, float z1, bool access) =>
            new AreaSpec { id = id, xz = Rect.MinMaxRect(x0, z0, x1, z1), playerAccess = access };
        // 3 m segments; west strips X 15.85-16.85 with drops at X 17.45, east strips X 20.65-21.65 with drops at X 20.05.
        static CrackPointSpec Crack(string id, float z0, bool west) => west
            ? new CrackPointSpec { id = id, zMin = z0, zMax = z0 + 3f, stripX = 16.35f, dropX = 17.45f, wallFaceX = 15.85f }
            : new CrackPointSpec { id = id, zMin = z0, zMax = z0 + 3f, stripX = 21.15f, dropX = 20.05f, wallFaceX = 21.65f };
        static WallSpec Wall(float ax, float az, float bx, float bz) =>
            new WallSpec { a = new Vector2(ax, az), b = new Vector2(bx, bz) };
        static OpeningSpec Opening(string id, float x, float z, float width, OpeningKind kind) =>
            new OpeningSpec { id = id, center = new Vector2(x, z), width = width, kind = kind };
        static PropSpec Prop(string id, PropKind kind, float x, float z, float w, float d, float h) =>
            new PropSpec { id = id, kind = kind, center = new Vector3(x, h * 0.5f, z), size = new Vector3(w, h, d) };
        static BlockerSpec Blocker(string id, float x, float z) =>
            new BlockerSpec { id = id, center = new Vector2(x, z), size = new Vector2(2f, 0.4f) };
    }
}
