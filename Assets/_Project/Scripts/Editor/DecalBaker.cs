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
        public const int QueueShadow = 3000, QueueCone = 3002, QueueOutline = 3003, QueueEntity = 3004;
        const int ConeSegments = 24;

        [MenuItem("Nullflame/Bake Decals")]
        public static void Bake()
        {
            Material(Texture("PH_ShadowDecal", (u, v) => new Color(0.32f, 0.32f, 0.34f, 0.6f * Edge(u, v, 0.08f))),
                "PH_ShadowDecal", QueueShadow);
            Material(Texture("PH_EntityBlob", (u, v) => new Color(0f, 0f, 0f, Radial(u, v, 0.75f))),
                "PH_EntityBlob", QueueEntity);
            Material(null, "PH_Outline", QueueOutline);
            Material(null, "PH_Teardrop", QueueEntity, Color.black);
            Material(null, "PH_ViewCone", QueueCone, new Color(1f, 1f, 1f, 0.12f));
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

        static Texture2D Texture(string name, System.Func<float, float, Color> pixel)
        {
            var tex = new Texture2D(Res, Res, TextureFormat.RGBA32, false);
            for (int y = 0; y < Res; y++)
                for (int x = 0; x < Res; x++)
                    tex.SetPixel(x, y, pixel((x + 0.5f) / Res, (y + 0.5f) / Res));
            string path = $"{Folder}/{name}.png";
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
        static void Material(Texture2D tex, string name, int queue, Color? color = null)
        {
            string path = $"{Folder}/{name}.mat";
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
