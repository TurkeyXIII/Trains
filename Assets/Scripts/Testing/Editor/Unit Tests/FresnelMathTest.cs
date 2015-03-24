using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnitTest
{
    [TestFixture]
    [Category("Fresnel Integrals")]
    internal class FresnelMathTests
    {
        [Test]
        public void TestFresnelC()
        {
            Assert.That(FresnelMath.FresnelC(0.3f), Is.EqualTo(0.299757).Within(0.1f).Percent);
            Assert.That(FresnelMath.FresnelC(0.8f), Is.EqualTo(0.767848).Within(0.1f).Percent);
            Assert.That(FresnelMath.FresnelC(1.44f), Is.EqualTo(0.932543).Within(0.01f).Percent);
        }

        [Test]
        public void TestFresnelS()
        {
            Assert.That(FresnelMath.FresnelS(0.3f), Is.EqualTo(0.00899479).Within(0.1f).Percent);
            Assert.That(FresnelMath.FresnelS(0.8f), Is.EqualTo(0.165738).Within(0.1f).Percent);
            Assert.That(FresnelMath.FresnelS(1.44f), Is.EqualTo(0.728459).Within(0.01f).Percent);
        }

    }

    [Category("Asymmetrical Curves")]
    internal class CurveCalculationTests
    {
        [Test]
        public void TestA90DegSymmetrical()
        {
            float a2 = FresnelMath.DeltaA(Mathf.PI/4, Mathf.PI/2, 1, 1);

            Assert.That(a2, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void TestTheta90DegSymmetrical()
        {
            float theta1, theta2;

            Vector3 startPoint, endPoint, startDirection, endDirection;
            startPoint = Vector3.zero;
            startDirection = new Vector3(1, 0, 0);
            endPoint = new Vector3(1, 1, 0);
            endDirection = new Vector3(0, -1, 0);

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, endDirection);

            Assert.That(theta1, Is.EqualTo(Mathf.PI / 4).Within(0.001f));
            Assert.That(theta2, Is.EqualTo(Mathf.PI / 4).Within(0.001f));

            float L1 = Mathf.Sqrt(theta1);
            float L2 = Mathf.Sqrt(theta2);

            float phi = Mathf.PI/2;
            float A2 = FresnelMath.A2(theta1, phi, endPoint.x);
            float A1 = FresnelMath.A1(A2, theta1, phi);
            Debug.Log("A1: " + A1 + ", L1: " + L1 + ", A2: " + A2 + ", L2: " + L2 + ", LTot: " + (L1 / A1 + L2 / A2) + ", R: " + (1/(2*A1*L1)));
        }

        [Test]
        public void TestTheta90DegSymmetricalBadGuess()
        {
            float theta1, theta2;

            Vector3 startPoint, endPoint, startDirection, endDirection;
            startPoint = Vector3.zero;
            startDirection = new Vector3(1, 0, 0);
            endPoint = new Vector3(1, 1, 0);
            endDirection = new Vector3(0, -1, 0);

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, endDirection, 0.01f);

            Assert.That(theta1, Is.EqualTo(Mathf.PI / 4).Within(0.001f));
            Assert.That(theta2, Is.EqualTo(Mathf.PI / 4).Within(0.001f));

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, endDirection, Mathf.PI/2 - 0.01f);

            Assert.That(theta1, Is.EqualTo(Mathf.PI / 4).Within(0.001f));
            Assert.That(theta2, Is.EqualTo(Mathf.PI / 4).Within(0.001f));
        }

        [Test]
        public void TestTheta90Deg57to100Asymmetrical()
        {
            float theta1, theta2;

            Vector3 startPoint, endPoint, startDirection, endDirection;
            startPoint = Vector3.zero;
            startDirection = new Vector3(1, 0, 0);
            endPoint = new Vector3(0.57f, 1, 0);
            endDirection = new Vector3(0, -1, 0);

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, endDirection);

            Assert.Greater(theta1, 0);
            Assert.Greater(theta2, 0);

            float L1 = Mathf.Sqrt(theta1);
            float L2 = Mathf.Sqrt(theta2);

            float phi = Mathf.PI/2;

            float A2 = FresnelMath.A2(theta1, phi, endPoint.x);
            float A1 = FresnelMath.A1(A2, theta1, phi);

            Assert.That(A1*L1, Is.EqualTo(A2*L2).Within(0.001f));
            Debug.Log("A1: " + A1 + ", L1: " + L1 + ", A2: " + A2 + ", L2: " + L2 + ", LTot: " + (L1/A1+L2/A2) + ", R: " + (1/(2*A1*L1)));
        }

        [Test]
        public void TestA30DegSymmetrical()
        {
            float a2 = FresnelMath.DeltaA(Mathf.PI / 12, Mathf.PI / 6, 1, Mathf.Tan(Mathf.PI/12));

            Assert.That(a2, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void TestTheta30DegSymmetrical()
        {
            float theta1, theta2;

            Vector3 startPoint, endPoint, startDirection, endDirection;
            startPoint = Vector3.zero;
            startDirection = new Vector3(1, 0, 0);
            endPoint = new Vector3(1, Mathf.Tan(Mathf.PI/12), 0);
            endDirection = new Vector3(-Mathf.Cos(Mathf.PI/6), -Mathf.Sin(Mathf.PI/6), 0);

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, endDirection);

            Assert.That(theta1, Is.EqualTo(Mathf.PI / 12).Within(0.001f));
            Assert.That(theta2, Is.EqualTo(Mathf.PI / 12).Within(0.001f));
        }

        [Test]
        public void TestTheta30DegSymmetricalBadGuess()
        {
            float theta1, theta2;

            Vector3 startPoint, endPoint, startDirection, endDirection;
            startPoint = Vector3.zero;
            startDirection = new Vector3(1, 0, 0);
            endPoint = new Vector3(1, Mathf.Tan(Mathf.PI / 12), 0);
            endDirection = new Vector3(-Mathf.Cos(Mathf.PI / 6), -Mathf.Sin(Mathf.PI / 6), 0);

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, endDirection, 0.01f);

            Assert.That(theta1, Is.EqualTo(Mathf.PI / 12).Within(0.001f));
            Assert.That(theta2, Is.EqualTo(Mathf.PI / 12).Within(0.001f));

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, endDirection, Mathf.PI/6 - 0.01f);

            Assert.That(theta1, Is.EqualTo(Mathf.PI / 12).Within(0.001f));
            Assert.That(theta2, Is.EqualTo(Mathf.PI / 12).Within(0.001f));
        }

        [Test]
        public void TestTheta30Deg5To1Asymmetrical()
        {
            float theta1, theta2;

            Vector3 startPoint, endPoint, startDirection, endDirection;
            startPoint = Vector3.zero;
            startDirection = new Vector3(1, 0, 0);
            endPoint = new Vector3(2.75f, 1, 0);
            endDirection = new Vector3(-Mathf.Cos(Mathf.PI / 6), -Mathf.Sin(Mathf.PI / 6), 0);

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, endDirection);

            Assert.Greater(theta1, 0);
            Assert.Greater(theta2, 0);

            float L1 = Mathf.Sqrt(theta1);
            float L2 = Mathf.Sqrt(theta2);

            float phi = Mathf.PI / 6;

            float A2 = FresnelMath.A2(theta1, phi, endPoint.x);
            float A1 = FresnelMath.A1(A2, theta1, phi);

            Assert.That(A1 * L1, Is.EqualTo(A2 * L2).Within(0.001f));
            Debug.Log("A1: " + A1 + ", L1: " + L1 + ", A2: " + A2 + ", L2: " + L2 + ", LTot: " + (L1 / A1 + L2 / A2) + ", R: " + (1 / (2 * A1 * L1)));

            Assert.That(theta1 + theta2, Is.EqualTo(Mathf.Acos(Vector3.Dot(startDirection, -endDirection))).Within(0.001f));
        }
    }
}