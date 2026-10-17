using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nullflame.EditorTools
{
    // Builds the facility greybox from FacilityLayout (IMPL §6.2). Idempotent: it clears and rebuilds
    // only the roots it owns (Level_Generated, State_Meltdown/Generated_Meltdown, State_Normal/Generated_Normal).
    public static class FacilityBuilder
    {
        public const string LayoutPath = "Assets/_Project/Settings/FacilityLayout.asset";
        public const string TuningPath = "Assets/_Project/Settings/GameTuning.asset";
        public const string RegistryPath = "Assets/_Project/Art/AssetRegistry.asset";

        public const string LevelRootName = "Level_Generated";
        public const string MeltdownRootName = "State_Meltdown";
        public const string NormalRootName = "State_Normal";
        public const string MeltdownGenName = "Generated_Meltdown";
        public const string NormalGenName = "Generated_Normal";

        const float FloorThickness = 0.1f;

        [MenuItem("Nullflame/Build Facility")]
        static void BuildMenu()
        {
            var layout = AssetDatabase.LoadAssetAtPath<FacilityLayout>(LayoutPath);
            var tuning = AssetDatabase.LoadAssetAtPath<GameTuning>(TuningPath);
            var registry = AssetDatabase.LoadAssetAtPath<AssetRegistry>(RegistryPath);
            if (layout == null || tuning == null || registry == null)
            {
                Debug.LogError("FacilityBuilder: missing FacilityLayout, GameTuning or AssetRegistry asset.");
                return;
            }
            Scene scene = SceneManager.GetActiveScene();
            Build(scene, layout, tuning, registry);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        public static void Build(Scene scene, FacilityLayout layout, GameTuning tuning, AssetRegistry registry)
        {
            Transform level = Clear(FindOrCreateRoot(scene, LevelRootName));
            Transform meltdown = Clear(Group(FindOrCreateRoot(scene, MeltdownRootName), MeltdownGenName));
            Transform normal = Clear(Group(FindOrCreateRoot(scene, NormalRootName), NormalGenName));
            float height = tuning.ceilingHeight;

            Transform floors = Group(level, "Floors");
            Transform ceilings = Group(level, "Ceilings");
            foreach (var area in layout.areas)
            {
                Rect r = area.xz;
                var center = new Vector3(r.center.x, -FloorThickness * 0.5f, r.center.y);
                Block("Floor_" + area.id, floors, center, new Vector3(r.width, FloorThickness, r.height), 0f,
                    registry.floorTile, solid: false, visible: true);
                // Ceiling at T-LVL-CEIL: the floor tile flipped. Solid, so the orbit camera stays under it.
                var top = new Vector3(r.center.x, height + FloorThickness * 0.5f, r.center.y);
                GameObject ceiling = Block("Ceiling_" + area.id, ceilings, top, new Vector3(r.width, FloorThickness, r.height), 0f,
                    registry.floorTile, solid: true, visible: true);
                ceiling.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            }

            Transform walls = Group(level, "Walls");
            var cuts = new List<Vector2>();
            for (int i = 0; i < layout.walls.Length; i++)
            {
                WallSpec w = layout.walls[i];
                Vector2 ab = w.b - w.a;
                float length = ab.magnitude;
                Vector2 dir = ab / length;
                float yaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + 90f;

                cuts.Clear();
                foreach (var o in layout.openings)
                {
                    float t = Vector2.Dot(o.center - w.a, dir);
                    if (t < 0f || t > length || Vector2.Distance(w.a + dir * t, o.center) > 0.01f) continue;
                    cuts.Add(new Vector2(t - o.width * 0.5f, t + o.width * 0.5f));
                    BuildOpening(o, w.a + dir * t, yaw, height, layout.wallThickness, level, meltdown, normal, registry);
                }
                cuts.Sort((p, q) => p.x.CompareTo(q.x));

                float half = layout.wallThickness * 0.5f;
                float start = -half; // extend wall ends by half a thickness so corners close
                int span = 0;
                foreach (Vector2 cut in cuts)
                {
                    WallSpan(walls, i, span++, w.a, dir, start, cut.x, yaw, height, layout.wallThickness, registry);
                    start = cut.y;
                }
                WallSpan(walls, i, span, w.a, dir, start, length + half, yaw, height, layout.wallThickness, registry);
            }

            Transform blockers = Group(level, "Blockers");
            foreach (var b in layout.blockers)
            {
                var center = new Vector3(b.center.x, height * 0.5f, b.center.y);
                Block(b.id, blockers, center, new Vector3(b.size.x, height, b.size.y), 0f, null, solid: true, visible: false);
            }

            Transform props = Group(level, "Props");
            foreach (var p in layout.props)
                Block(p.id, props, p.center, p.size, p.yaw, PropPrefab(registry, p.kind), solid: false, visible: true);

            BuildMeltdownFlames(meltdown, layout, tuning, registry, height);

            Transform lights = Group(level, "CeilingLights");
            foreach (Vector2 c in layout.ceilingLights)
            {
                var go = new GameObject("CeilingLight");
                go.transform.SetParent(lights, false);
                go.transform.position = new Vector3(c.x, height - 0.2f, c.y);
                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = layout.ceilingLightRange;
                light.intensity = layout.ceilingLightIntensity;
                light.shadows = LightShadows.Soft; // real silhouettes are a look only; gameplay reads ShadowArea (design doc 7.1)
                if (registry.ceilingLight == null) continue;
                // The fixture hangs from the ceiling plane; the builder's point light is the only light source
                // (the pack prefab's own Light is switched off so fixtures don't double up, EN-058).
                GameObject fixture = Instantiate(registry.ceilingLight, go.transform);
                fixture.transform.position = new Vector3(c.x, height, c.y);
                foreach (Light packLight in fixture.GetComponentsInChildren<Light>(true)) packLight.enabled = false;
            }
        }

        // Alarm lights (cone light + floor-circle damage) and the Final Corridor wall fire (instant-kill box),
        // Meltdown only (design doc §7.7, §10).
        static void BuildMeltdownFlames(Transform meltdown, FacilityLayout layout, GameTuning tuning, AssetRegistry registry, float height)
        {
            int hazard = LayerMask.NameToLayer("Hazard");
            Transform alarms = Group(meltdown, "AlarmLights");
            foreach (Vector2 p in layout.alarmLights)
            {
                var go = new GameObject("AlarmLight");
                go.transform.SetParent(alarms, false);
                go.transform.position = new Vector3(p.x, 0f, p.y);
                go.layer = hazard;
                var capsule = go.AddComponent<CapsuleCollider>();
                capsule.isTrigger = true;
                capsule.height = height;
                capsule.center = new Vector3(0f, height * 0.5f, 0f);
                capsule.radius = tuning.alarmFloorRadius;
                Flame(go, FlameKind.Alarm, tuning);
                var lamp = new GameObject("Lamp");
                lamp.transform.SetParent(go.transform, false);
                lamp.transform.localPosition = new Vector3(0f, height - 0.2f, 0f);
                lamp.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                var light = lamp.AddComponent<Light>();
                light.type = LightType.Spot;
                light.color = layout.alarmColor;
                light.range = height + 1f;
                light.spotAngle = 2f * Mathf.Atan(tuning.alarmFloorRadius / (height - 0.2f)) * Mathf.Rad2Deg;
                light.intensity = 6f;
                light.shadows = LightShadows.Soft;
                if (registry.alarmLight != null) Instantiate(registry.alarmLight, lamp.transform);
            }

            Transform fire = Group(meltdown, "WallFireLanterns");
            if (layout.TryGetArea("FinalCorridor", out Rect corridor))
            {
                var box = new GameObject("WallFireKillVolume");
                box.transform.SetParent(fire, false);
                box.transform.position = new Vector3(corridor.center.x, height * 0.5f, corridor.center.y);
                box.layer = hazard;
                var col = box.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(corridor.width, height, corridor.height);
                Flame(box, FlameKind.WallFire, tuning);
            }
            foreach (Vector3 p in layout.wallFireLanterns)
            {
                var go = new GameObject("WallFireLantern");
                go.transform.SetParent(fire, false);
                go.transform.position = p;
                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = layout.fireColor;
                light.range = 4f;
                light.intensity = 2f;
                light.shadows = LightShadows.Soft;
                if (registry.wallFireLantern != null) Instantiate(registry.wallFireLantern, go.transform);
                if (registry.fireSmallVfx != null) Instantiate(registry.fireSmallVfx, go.transform);
            }
        }

        static void Flame(GameObject go, FlameKind kind, GameTuning tuning)
        {
            var f = go.AddComponent<FlameVolume>();
            f.kind = kind;
            var so = new SerializedObject(f);
            so.FindProperty("tuning").objectReferenceValue = tuning;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildOpening(OpeningSpec o, Vector2 c, float yaw, float height, float thickness,
            Transform level, Transform meltdown, Transform normal, AssetRegistry registry)
        {
            var panelCenter = new Vector3(c.x, height * 0.5f, c.y);
            var panelSize = new Vector3(o.width, height, thickness);
            switch (o.kind)
            {
                case OpeningKind.ContainmentDoor:
                    // Meltdown: doorway open (no panel). Normal: closed, blocks jumps and sight.
                    Block(o.id + "_Closed", normal, panelCenter, panelSize, yaw, registry.containmentDoor, solid: true, visible: true);
                    break;
                case OpeningKind.ShowOnlyDoor:
                    Block(o.id, Group(level, "Doorways"), panelCenter, panelSize, yaw, registry.showOnlyDoor, solid: true, visible: true);
                    break;
                case OpeningKind.OpenFrame:
                case OpeningKind.Door:
                    if (registry.doorFrame != null)
                        Block(o.id + "_Frame", Group(level, "Doorways"), panelCenter, panelSize, yaw, registry.doorFrame, solid: false, visible: true);
                    break;
            }
        }

        static void WallSpan(Transform parent, int wall, int span, Vector2 a, Vector2 dir, float t0, float t1,
            float yaw, float height, float thickness, AssetRegistry registry)
        {
            if (t1 - t0 < 0.01f) return;
            Vector2 mid = a + dir * ((t0 + t1) * 0.5f);
            Block("Wall_" + wall + "_" + span, parent, new Vector3(mid.x, height * 0.5f, mid.y),
                new Vector3(t1 - t0, height, thickness), yaw, registry.wallSegment, solid: true, visible: true);
        }

        // A box at center/size/yaw. solid = LevelSolid BoxCollider. The visual is the prefab fitted to the box,
        // or a cube when the registry slot is empty.
        static GameObject Block(string name, Transform parent, Vector3 center, Vector3 size, float yaw,
            GameObject prefab, bool solid, bool visible)
        {
            GameObject go;
            if (prefab == null && visible)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
            }
            else
            {
                go = new GameObject();
                if (solid) go.AddComponent<BoxCollider>();
            }
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(center, Quaternion.Euler(0f, yaw, 0f));
            go.transform.localScale = size;
            if (solid) go.layer = LayerMask.NameToLayer("LevelSolid");
            if (prefab != null && visible) FitVisual(prefab, go.transform);
            return go;
        }

        // Instantiates prefab as a child and scales it so its renderer bounds fill the parent's unit box.
        static void FitVisual(GameObject prefab, Transform box)
        {
            var visual = Instantiate(prefab, box);
            var renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            Bounds b = LocalBounds(box, renderers);
            Vector3 s = new Vector3(Safe(1f / b.size.x), Safe(1f / b.size.y), Safe(1f / b.size.z));
            visual.transform.localScale = s;
            visual.transform.localPosition = -Vector3.Scale(b.center, s);
        }

        static Bounds LocalBounds(Transform space, Renderer[] renderers)
        {
            var b = new Bounds(space.InverseTransformPoint(renderers[0].bounds.center), Vector3.zero);
            foreach (var r in renderers)
            {
                Vector3 min = r.bounds.min, max = r.bounds.max;
                for (int i = 0; i < 8; i++)
                {
                    var corner = new Vector3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
                    b.Encapsulate(space.InverseTransformPoint(corner));
                }
            }
            return b;
        }

        static float Safe(float v) => float.IsInfinity(v) || float.IsNaN(v) ? 1f : v;

        // Pack prefabs keep their demo-scene position as the local offset; every visual sits on its parent.
        static GameObject Instantiate(GameObject prefab, Transform parent)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }

        static GameObject PropPrefab(AssetRegistry r, PropKind kind)
        {
            switch (kind)
            {
                case PropKind.Crate: return r.crate;
                case PropKind.Cart: return r.cart;
                case PropKind.Cabinet: return r.cabinet;
                case PropKind.Bunk: return r.bunk;
                case PropKind.Locker: return r.locker;
                case PropKind.LanternRack: return r.lanternRack;
                default: return r.labBench;
            }
        }

        static Transform Group(Transform parent, string name)
        {
            Transform t = parent.Find(name);
            if (t != null) return t;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static Transform FindOrCreateRoot(Scene scene, string name)
        {
            foreach (var go in scene.GetRootGameObjects())
                if (go.name == name) return go.transform;
            var root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root.transform;
        }

        static Transform Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) Object.DestroyImmediate(t.GetChild(i).gameObject);
            return t;
        }
    }
}
