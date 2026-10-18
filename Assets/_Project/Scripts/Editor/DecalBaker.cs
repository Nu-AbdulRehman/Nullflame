using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nullflame.EditorTools
{
    // Bakes placeholder decal textures and their unlit transparent materials (IMPL §25.2, CLAUDE.md §5.3).
    // Overwrites its own outputs, so running it twice is safe.
    public static class DecalBaker
    {
        const string Folder = "Assets/_Project/Placeholders";
        const int Res = 64;

        // Render queues from IMPL §2.8: shadows under cones under the outline under the entity.
        public const int QueueShadow = 3000, QueueOil = 3001, QueueCone = 3002, QueueOutline = 3003, QueueEntity = 3004;
        const int ConeSegments = 24;

        // The SCP badge on every guard's back: the owner's logo on an alpha-blended unlit quad (AD-030).
        [MenuItem("Nullflame/Bake Badge Material")]
        public static void BakeBadge()
        {
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/scp_logo.png");
            Material(logo, "Assets/_Project/Art/M_ScpBadge.mat", QueueShadow, null, fullPath: true);
            AssetDatabase.SaveAssets();
        }

        // The closed SCP doors: the wall kit's own material, darkened (AD-031). Updated in place so the GUID holds.
        [MenuItem("Nullflame/Bake Door Material")]
        public static void BakeDoor()
        {
            var registry = AssetDatabase.LoadAssetAtPath<AssetRegistry>("Assets/_Project/Art/AssetRegistry.asset");
            var wallRenderer = registry.wallSegment != null ? registry.wallSegment.GetComponentInChildren<Renderer>() : null;
            if (wallRenderer == null) { Debug.LogError("AssetRegistry.wallSegment has no renderer; set the wall kit first."); return; }
            const string path = "Assets/_Project/Art/M_ScpDoor.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(wallRenderer.sharedMaterial);
                AssetDatabase.CreateAsset(mat, path);
            }
            else mat.CopyPropertiesFromMaterial(wallRenderer.sharedMaterial);
            mat.SetColor("_BaseColor", wallRenderer.sharedMaterial.GetColor("_BaseColor") * new Color(0.55f, 0.55f, 0.55f, 1f));
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
        }

        // The Time SCP stopwatch: the Sketchfab pocket watch's PBR textures on URP Lit materials, remapped onto the
        // four materials the FBX imports (Watch, Glass, Minutes, Text) (EN-063). The owner's model and textures
        // are never edited; only the normal map's import type and the FBX's material remap table change.
        [MenuItem("Nullflame/Bake Watch Material")]
        public static void BakeWatch()
        {
            const string tex = "Assets/vintage-pocket-watch/textures/";
            const string fbx = "Assets/vintage-pocket-watch/source/Pocketwatch_lowpoly.fbx";
            var normalImporter = (TextureImporter)AssetImporter.GetAtPath(tex + "Pocketwatch_lowpoly_Watch_Normal.png");
            if (normalImporter.textureType != TextureImporterType.NormalMap)
            {
                normalImporter.textureType = TextureImporterType.NormalMap;
                normalImporter.SaveAndReimport();
            }

            Material watch = Lit("M_PocketWatch");
            watch.SetTexture("_BaseMap", Tex(tex + "Pocketwatch_lowpoly_Watch_BaseColor.png"));
            watch.SetTexture("_BumpMap", Tex(tex + "Pocketwatch_lowpoly_Watch_Normal.png"));
            watch.SetTexture("_MetallicGlossMap", Tex(tex + "Pocketwatch_lowpoly_Watch_Metallic.png"));
            watch.SetTexture("_EmissionMap", Tex(tex + "Pocketwatch_lowpoly_Watch_Emissive.png"));
            watch.SetColor("_EmissionColor", Color.white);
            watch.SetFloat("_Smoothness", 0.6f);
            watch.EnableKeyword("_NORMALMAP");
            watch.EnableKeyword("_METALLICSPECGLOSSMAP");
            watch.EnableKeyword("_EMISSION");

            Material glass = Lit("M_PocketWatchGlass");
            glass.SetFloat("_Surface", 1f);
            glass.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            glass.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            glass.SetFloat("_ZWrite", 0f);
            glass.SetOverrideTag("RenderType", "Transparent");
            glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            glass.renderQueue = (int)RenderQueue.Transparent;
            glass.SetColor("_BaseColor", new Color(0.9f, 0.95f, 1f, 0.2f));
            glass.SetFloat("_Smoothness", 0.95f);

            Material hands = Cutout("M_PocketWatchHands", Tex(tex + "Minutes_Alpha.png"));
            Material text = Cutout("M_PocketWatchText", Tex(tex + "Text_alpha.png"));

            var importer = (ModelImporter)AssetImporter.GetAtPath(fbx);
            Remap(importer, "Watch", watch);
            Remap(importer, "Glass", glass);
            Remap(importer, "Minutes", hands);
            Remap(importer, "Text", text);
            importer.SaveAndReimport();
            AssetDatabase.SaveAssets();
        }

        static Texture2D Tex(string path) => AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        static Material Lit(string name)
        {
            string path = $"Assets/_Project/Art/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // Alpha-clipped dark detail (watch hands, dial text) from an alpha-only texture.
        static Material Cutout(string name, Texture2D alpha)
        {
            Material mat = Lit(name);
            mat.SetTexture("_BaseMap", alpha);
            mat.SetColor("_BaseColor", new Color(0.08f, 0.08f, 0.08f, 1f));
            mat.SetFloat("_AlphaClip", 1f);
            mat.SetFloat("_Cutoff", 0.5f);
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.renderQueue = (int)RenderQueue.AlphaTest;
            return mat;
        }

        static void Remap(ModelImporter importer, string sourceMaterial, Material target) =>
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), sourceMaterial), target);

        // Essence bar from docs/design/health_bar.png: a bar with the top-left and bottom-right corners cut at 45°.
        // Sized for the 420 x 56 HUD rect at 2x; Frame is the white outline, Fill the solid shape the Image tints.
        [MenuItem("Nullflame/Bake Essence Bar")]
        public static void BakeEssenceBar()
        {
            const int w = 840, h = 112;
            const float cut = h * 0.25f, stroke = 6f;
            float Dist(float u, float v)
            {
                float x = u * w, y = v * h;
                float d = Mathf.Min(Mathf.Min(x, w - x), Mathf.Min(y, h - y));
                d = Mathf.Min(d, (x + h - y - cut) * 0.7071f); // top-left cut
                return Mathf.Min(d, (w - x + y - cut) * 0.7071f); // bottom-right cut
            }
            Sprite(Texture("UI_EssenceBarFrame", (u, v) =>
            {
                float d = Dist(u, v);
                return new Color(1f, 1f, 1f, Mathf.Clamp01(d) * Mathf.Clamp01(stroke - d));
            }, w, h, "Assets/_Project/Art"));
            Sprite(Texture("UI_EssenceBarFill", (u, v) => new Color(1f, 1f, 1f, Mathf.Clamp01(Dist(u, v))), w, h, "Assets/_Project/Art"));
        }

        [MenuItem("Nullflame/Bake Decals")]
        public static void Bake()
        {
            // Near-black so it reads darker than the lit floor; ShadowArea tints soft shadows lighter.
            Material(Texture("PH_ShadowDecal", (u, v) => new Color(0.01f, 0.01f, 0.02f, 0.85f * Edge(u, v, 0.18f))),
                "PH_ShadowDecal", QueueShadow);
            Material(Texture("PH_EntityBlob", (u, v) => new Color(0f, 0f, 0f, Radial(u, v, 0.75f))),
                "PH_EntityBlob", QueueEntity);
            Material(null, "PH_Outline", QueueOutline);
            Material(null, "PH_Teardrop", QueueEntity, Color.black);
            Material(null, "PH_ViewCone", QueueCone, new Color(1f, 1f, 1f, 0.12f));
            Material(Texture("PH_OilDecal", (u, v) => new Color(0.02f, 0.02f, 0.03f, 0.92f * Radial(u, v, 0.8f))),
                "PH_OilDecal", QueueOil);
            Sprite(Texture("PH_Ring", (u, v) => new Color(1f, 1f, 1f, Ring(u, v, 0.78f, 0.98f))));
            ViewCone(AssetDatabase.LoadAssetAtPath<GameTuning>(FacilityBuilder.TuningPath));
            AssetDatabase.SaveAssets();
        }

        // Flat fan from the guard's feet: T-VIEW-ANGLE wide, T-VIEW-RANGE long, along +Z. Rebake after changing either.
        static void ViewCone(GameTuning tuning)
        {
            string path = $"{Folder}/PH_ViewCone.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = "PH_ViewCone" };
                AssetDatabase.CreateAsset(mesh, path);
            }
            var verts = new Vector3[ConeSegments + 2];
            var tris = new int[ConeSegments * 3];
            float half = tuning.viewAngle * 0.5f;
            for (int i = 0; i <= ConeSegments; i++)
            {
                float a = Mathf.Lerp(-half, half, i / (float)ConeSegments) * Mathf.Deg2Rad;
                verts[i + 1] = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * tuning.viewRange;
            }
            for (int i = 0; i < ConeSegments; i++)
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }
            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
        }

        // Alpha 1 inside, fading to 0 over `soft` of the half-size at each edge.
        static float Edge(float u, float v, float soft)
        {
            float d = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
            return Mathf.Clamp01(d / soft);
        }

        // Solid disc out to `inner` of the radius, fading to 0 at the rim.
        static float Radial(float u, float v, float inner)
        {
            float r = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
            return 1f - Mathf.Clamp01((r - inner) / (1f - inner));
        }

        // Opaque between inner and outer (fractions of the radius), soft one pixel at each edge.
        static float Ring(float u, float v, float inner, float outer)
        {
            float r = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
            float px = 2f / Res;
            return Mathf.Clamp01((r - inner) / px) * Mathf.Clamp01((outer - r) / px);
        }

        static void Sprite(Texture2D tex)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        static Texture2D Texture(string name, System.Func<float, float, Color> pixel, int w = Res, int h = Res, string folder = Folder)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, pixel((x + 0.5f) / w, (y + 0.5f) / h));
            string path = $"{folder}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // URP Unlit, transparent, alpha blended, no depth write.
        static void Material(Texture2D tex, string name, int queue, Color? color = null, bool fullPath = false)
        {
            string path = fullPath ? name : $"{Folder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", color ?? Color.white);
            mat.renderQueue = queue;
            EditorUtility.SetDirty(mat);
        }
    }
}
