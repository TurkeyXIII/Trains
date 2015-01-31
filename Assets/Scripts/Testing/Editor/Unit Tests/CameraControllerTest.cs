using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace UnitTest
{
    [TestFixture]
    [Category("Polar Coorinates")]
    internal class PolarStructTests
    {
        [Test]
        public void IdentityConversion()
        {
            Vector3 v = new Vector3(1f, 1f, 1f);
            Polar p = new Polar(v);

            Assert.AreEqual(Mathf.Sqrt(3f), p.radius);
            Assert.AreEqual(45f, p.rotation);
            Assert.AreEqual(Mathf.Asin(1/Mathf.Sqrt(3f)) * Mathf.Rad2Deg, p.elevation);

            Vector3 rebuiltV = p.ToVector3();

            Assert.That(rebuiltV.x, Is.EqualTo(v.x).Within(0.01f).Percent);
            Assert.That(rebuiltV.y, Is.EqualTo(v.y).Within(0.01f).Percent);
            Assert.That(rebuiltV.z, Is.EqualTo(v.z).Within(0.01f).Percent);
            
        }

        [Test]
        public void InvertedIdentityConversion()
        {
            Vector3 v = new Vector3(-1f, -1f, -1f);
            Polar p = new Polar(v);

            Assert.AreEqual(Mathf.Sqrt(3f), p.radius);
            Assert.AreEqual(225f, p.rotation);
            Assert.AreEqual(-Mathf.Asin(1 / Mathf.Sqrt(3f)) * Mathf.Rad2Deg, p.elevation);

            Vector3 rebuiltV = p.ToVector3();

            Assert.That(rebuiltV.x, Is.EqualTo(v.x).Within(0.01f).Percent);
            Assert.That(rebuiltV.y, Is.EqualTo(v.y).Within(0.01f).Percent);
            Assert.That(rebuiltV.z, Is.EqualTo(v.z).Within(0.01f).Percent);

        }

        [Test]
        public void FourthQuadConversion()
        {
            Vector3 v = new Vector3(1f, 0f, -1f);
            Polar p = new Polar(v);

            Assert.AreEqual(Mathf.Sqrt(2f), p.radius);
            Assert.AreEqual(315f, p.rotation);
            Assert.AreEqual(0, p.elevation);

            Vector3 rebuiltV = p.ToVector3();

            Assert.That(rebuiltV.x, Is.EqualTo(v.x).Within(0.01f).Percent);
            Assert.That(rebuiltV.y, Is.EqualTo(v.y).Within(0.01f).Percent);
            Assert.That(rebuiltV.z, Is.EqualTo(v.z).Within(0.01f).Percent);

        }

        [Test]
        public void GimbalLockedConversion()
        {
            Vector3 v = new Vector3(0f, 1f, 0f);
            Polar p = new Polar(v);

            Assert.AreEqual(1f, p.radius);
            Assert.AreEqual(90f, p.elevation);

            Vector3 rebuiltV = p.ToVector3();

            Assert.That(rebuiltV.x, Is.EqualTo(v.x).Within(0.01f).Percent);
            Assert.That(rebuiltV.y, Is.EqualTo(v.y).Within(0.01f).Percent);
            Assert.That(rebuiltV.z, Is.EqualTo(v.z).Within(0.01f).Percent);

        }
    }

    [TestFixture]
    [Category("Clamper")]
    internal class ClamperTests
    {
        [Test]
        public void MiddleOfTerrain()
        {
            CameraViewStub cameraView = new CameraViewStub();
            cameraView.angleFromVertical = 10;
            cameraView.lookingAtWorldPoint = new Vector3(0, 0, 0);

            Clamper c = new Clamper(new Vector3(-10, 0, -10), new Vector3(10, 0, 10), 0);
            c.cameraView = cameraView;

            Vector3 pos = new Vector3(0, 9, 0);
            Assert.False(c.ClampPosition(ref pos));
            Assert.AreEqual(0, pos.x);
            Assert.AreEqual(9, pos.y);
            Assert.AreEqual(0, pos.z);
        }

        [Test]
        public void XShunt()
        {
            CameraViewStub cameraView = new CameraViewStub();
            cameraView.angleFromVertical = 10;
            cameraView.lookingAtWorldPoint = new Vector3(-10, 0, 0);

            Clamper c = new Clamper(new Vector3(-10, 0, -10), new Vector3(10, 0, 10), 0);
            c.cameraView = cameraView;

            Vector3 pos = new Vector3(0, 5, 0);
            Assert.True(c.ClampPosition(ref pos));
            Assert.Greater(pos.x, 4);
            Assert.AreEqual(pos.z, 0);
        }

        [Test]
        public void XZShunt()
        {
            CameraViewStub cameraView = new CameraViewStub();
            cameraView.angleFromVertical = 10;
            cameraView.lookingAtWorldPoint = new Vector3(-10, 0, -10);

            Clamper c = new Clamper(new Vector3(-10, 0, -10), new Vector3(10, 0, 10), 0);
            c.cameraView = cameraView;

            Vector3 pos = new Vector3(0, 5, 0);
            Assert.True(c.ClampPosition(ref pos));
            Assert.Greater(pos.x, 4);
            Assert.Greater(pos.z, 4);
        }

        [Test]
        public void YShunt()
        {
            CameraViewStub cameraView = new CameraViewStub();
            cameraView.angleFromVertical = 10;
            cameraView.lookingAtWorldPoint = new Vector3(-10, 0, -10);

            Clamper c = new Clamper(new Vector3(-10, 0, -10), new Vector3(10, 0, 10), 0);
            c.cameraView = cameraView;

            Vector3 pos = new Vector3(0, 15, 0);
            Assert.True(c.ClampPosition(ref pos));
            Assert.LessOrEqual(pos.y, 10);
        }

        [Test]
        public void DistToMidCorrect()
        {
            CameraViewStub cameraView = new CameraViewStub();
            cameraView.angleFromVertical = 45;
            cameraView.lookingAtWorldPoint = new Vector3(0, 0, 0);

            Clamper c = new Clamper(new Vector3(-10, 0, -10), new Vector3(10, 0, 10), 0);
            c.cameraView = cameraView;

            Vector3 pos = new Vector3(5, 5, 0);
            c.ClampPosition(ref pos);
            Assert.That(cameraView.distToMid, Is.EqualTo(Mathf.Sqrt(50)).Within(0.0001));
        }
    }

    internal class CameraViewStub : ICameraView
    {
        public Vector3 lookingAtWorldPoint;
        public float angleFromVertical;
        public float distToMid;

        public Vector3 ViewportToWorldPoint(Vector3 position, Vector3 viewPort)
        {
            distToMid = viewPort.z;
            return lookingAtWorldPoint;
        }

        public float AngleFromVertical()
        {
            return angleFromVertical;
        }

        public float FieldOfView()
        {
            return 90f;
        }

        public float GetTerrainHeight(Vector3 position)
        {
            return 0;
        }
    }
}

