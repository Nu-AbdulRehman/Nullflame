using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nullflame.Tests
{
    // Design doc §4.1 coordinates, read from the real asset so edits to it are checked too.
    public sealed class FacilityLayoutTests
    {
        const string LayoutPath = "Assets/_Project/Settings/FacilityLayout.asset";

        static FacilityLayout Load()
        {
            var layout = AssetDatabase.LoadAssetAtPath<FacilityLayout>(LayoutPath);
            Assert.IsNotNull(layout, LayoutPath);
            return layout;
        }

        [TestCase("TimeRoom", 0f, 7.5f, 0f, 6f)]
        [TestCase("ScpRoomA", 7.5f, 15f, 0f, 6f)]
        [TestCase("ScpRoomB", 15f, 22.5f, 0f, 6f)]
        [TestCase("ScpRoomC", 22.5f, 30f, 0f, 6f)]
        [TestCase("ScpRoomD", 30f, 37.5f, 0f, 6f)]
        [TestCase("AccessCorridor", 0f, 37.5f, 6f, 12f)]
        [TestCase("MainCorridor", 15.75f, 21.75f, 12f, 40f)]
        [TestCase("PlayerRoom", 8.25f, 15.75f, 22.25f, 29.75f)]
        [TestCase("Lab", 21.75f, 32f, 19.5f, 29.75f)]
        [TestCase("Barracks", 21.75f, 32f, 29.75f, 40f)]
        [TestCase("SleepingAlcove", 32f, 33.75f, 34.85f, 40f)]
        [TestCase("SemiFinalGate", 15.75f, 21.75f, 40f, 41.5f)]
        [TestCase("FinalCorridor", 15.75f, 21.75f, 41.5f, 56f)]
        [TestCase("WestSideCorridor", 9.75f, 15.75f, 53f, 56f)]
        [TestCase("EastSideCorridor", 21.75f, 27.75f, 53f, 56f)]
        [TestCase("FinalGate", 15.75f, 21.75f, 56f, 57.5f)]
        public void Areas_MatchDesignTable(string id, float x0, float x1, float z0, float z1)
        {
            Assert.IsTrue(Load().TryGetArea(id, out Rect r), id);
            Assert.AreEqual(x0, r.xMin, 1e-4f, id + " xMin");
            Assert.AreEqual(x1, r.xMax, 1e-4f, id + " xMax");
            Assert.AreEqual(z0, r.yMin, 1e-4f, id + " zMin");
            Assert.AreEqual(z1, r.yMax, 1e-4f, id + " zMax");
        }

        [Test]
        public void EveryOpening_LiesOnAWall()
        {
            var layout = Load();
            foreach (var o in layout.openings)
            {
                bool onWall = false;
                foreach (var w in layout.walls)
                {
                    Vector2 ab = w.b - w.a;
                    float t = Vector2.Dot(o.center - w.a, ab) / ab.sqrMagnitude;
                    if (t >= 0f && t <= 1f && Vector2.Distance(w.a + ab * t, o.center) < 0.01f) { onWall = true; break; }
                }
                Assert.IsTrue(onWall, o.id);
            }
        }
    }
}
