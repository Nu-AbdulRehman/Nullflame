using NUnit.Framework;
using UnityEngine;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    public sealed class ShadowMathTests
    {
        static readonly Vector2 Size = new Vector2(4f, 2f);

        [Test]
        public void Contains_RotatedRect()
        {
            // Rotated 90°: local X (4 m) runs along world Z.
            Assert.IsTrue(ShadowMath.Contains(Vector3.zero, 90f, Size, new Vector3(0f, 0f, 1.9f)));
            Assert.IsFalse(ShadowMath.Contains(Vector3.zero, 90f, Size, new Vector3(1.9f, 0f, 0f)));
            Assert.IsTrue(ShadowMath.Contains(Vector3.zero, 0f, Size, new Vector3(1.9f, 0f, 0f)));
        }

        [Test]
        public void ClosestPoint_OutsideCorner()
        {
            Vector3 p = ShadowMath.ClosestPoint(Vector3.zero, 0f, Size, new Vector3(5f, 0f, 5f));
            Assert.AreEqual(2f, p.x, 1e-4f);
            Assert.AreEqual(1f, p.z, 1e-4f);
        }

        [Test]
        public void ClampInside_SmallerThanEntity_ReturnsCenter_TPLYSIZE()
        {
            // T-PLY-SIZE: a 1.2 m entity (margin 0.6) can't slide in a 1 m shadow.
            var c = new Vector3(3f, 0f, 3f);
            Vector3 p = ShadowMath.ClampInside(c, 30f, new Vector2(1f, 1f), 0.6f, new Vector3(10f, 0f, -4f));
            Assert.AreEqual(c.x, p.x, 1e-4f);
            Assert.AreEqual(c.z, p.z, 1e-4f);
        }

        [Test]
        public void ClampInside_KeepsMarginFromEdge()
        {
            Vector3 p = ShadowMath.ClampInside(Vector3.zero, 0f, Size, 0.6f, new Vector3(9f, 0f, 0f));
            Assert.AreEqual(1.4f, p.x, 1e-4f);
        }
    }

    public sealed class JumpArcTests
    {
        [Test]
        public void Evaluate_ApexAtHalf_TJMPARC()
        {
            Vector3 p = JumpArc.Evaluate(Vector3.zero, new Vector3(6f, 0f, 0f), 1.5f, 0.5f);
            Assert.AreEqual(1.5f, p.y, 1e-4f, "T-JMP-ARC");
            Assert.AreEqual(3f, p.x, 1e-4f);
        }

        [Test]
        public void Evaluate_Endpoints()
        {
            var from = new Vector3(1f, 0f, 2f);
            var to = new Vector3(4f, 0f, -1f);
            Assert.AreEqual(from, JumpArc.Evaluate(from, to, 1.5f, 0f));
            Assert.AreEqual(to, JumpArc.Evaluate(from, to, 1.5f, 1f));
        }
    }
}
