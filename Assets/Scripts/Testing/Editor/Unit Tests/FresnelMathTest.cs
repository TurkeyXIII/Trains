﻿using System;
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

            Debug.Log("A1: " + A1 + ", L1: " + L1 + ", A2: " + A2 + ", L2: " + L2 + ", LTot: " + (L1 / A1 + L2 / A2) + ", R: " + (1 / (2 * A1 * L1)));
            Assert.That(A1*L1, Is.EqualTo(A2*L2).Within(0.001f));
            Assert.That(theta1 + theta2, Is.EqualTo(Mathf.Acos(Vector3.Dot(startDirection, -endDirection))).Within(0.001f));
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

        [Test]
        public void TestThetaArbitrary1()
        {
            float theta1, theta2;

            Vector3 startPoint, endPoint, startDirection, endDirection;
            startPoint = Vector3.zero;
            startDirection = Vector3.right;
            endPoint = new Vector3(15.6f, 6.8f, 0);
            endDirection = new Vector3(0.8f, 0.6f, 0);

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, -endDirection);

            Assert.Greater(theta1, 0);
            Assert.Greater(theta2, 0);

            float L1 = Mathf.Sqrt(theta1);
            float L2 = Mathf.Sqrt(theta2);

            float phi = theta1 + theta2;

            float A2 = FresnelMath.A2(theta1, phi, endPoint.x);
            float A1 = FresnelMath.A1(A2, theta1, phi);


            Debug.Log("A1: " + A1 + ", L1: " + L1 + ", A2: " + A2 + ", L2: " + L2 + ", LTot: " + (L1 / A1 + L2 / A2) + ", R: " + (1 / (2 * A1 * L1)));
            Assert.That(A1 * L1, Is.EqualTo(A2 * L2).Within(0.001f));
            Assert.That(theta1 + theta2, Is.EqualTo(Mathf.Acos(Vector3.Dot(startDirection, endDirection))).Within(0.001f));
        }

        [Test]
        public void TestThetaArbitrary2()
        {
            float theta1, theta2;

            Vector3 startPoint, endPoint, startDirection, endDirection;
            startPoint = Vector3.zero;
            startDirection = Vector3.right;
            endPoint = new Vector3(21.2f, 11.3f, 0);
            endDirection = new Vector3(0.6f, 0f, -0.8f);

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, -endDirection);

            Assert.Greater(theta1, 0);
            Assert.Greater(theta2, 0);

            float L1 = Mathf.Sqrt(theta1);
            float L2 = Mathf.Sqrt(theta2);

            float phi = theta1 + theta2;

            float A2 = FresnelMath.A2(theta1, phi, endPoint.x);
            float A1 = FresnelMath.A1(A2, theta1, phi);


            Debug.Log("A1: " + A1 + ", L1: " + L1 + ", A2: " + A2 + ", L2: " + L2 + ", LTot: " + (L1 / A1 + L2 / A2) + ", R: " + (1 / (2 * A1 * L1)));
            Assert.That(A1 * L1, Is.EqualTo(A2 * L2).Within(0.001f));
            Assert.That(theta1 + theta2, Is.EqualTo(Mathf.Acos(Vector3.Dot(startDirection, endDirection))).Within(0.001f));
        }

        [Test]
        public void TestThetaArbitrary3()
        {
            float theta1, theta2;

            Vector3 startPoint, endPoint, startDirection, endDirection;
            startPoint = new Vector3(-4.5f, 1.0f, -2.5f);
            startDirection = new Vector3(1.0f, 0, -0.1f);
            endPoint = new Vector3(-3.7f, 1.0f, -2.2f);
            endDirection = new Vector3(0.6f, 0, 0.8f);

            FresnelMath.FindTheta(out theta1, out theta2, startPoint, endPoint, startDirection, -endDirection);

            Assert.Greater(theta1, 0);
            Assert.Greater(theta2, 0);

            float L1 = Mathf.Sqrt(theta1);
            float L2 = Mathf.Sqrt(theta2);

            float phi = theta1 + theta2;
            Debug.Log("phi = " + phi);

            float A2 = FresnelMath.A2(theta1, phi, Vector3.Dot((endPoint - startPoint), startDirection.normalized));
            float A1 = FresnelMath.A1(A2, theta1, phi);


            Debug.Log("A1: " + A1 + ", L1: " + L1 + ", A2: " + A2 + ", L2: " + L2 + ", LTot: " + (L1 / A1 + L2 / A2) + ", R: " + (1 / (2 * A1 * L1)));
            Assert.That(A1 * L1, Is.EqualTo(A2 * L2).Within(0.001f));
            Assert.That(theta1 + theta2, Is.EqualTo(Mathf.Acos(Vector3.Dot(startDirection, endDirection) / (startDirection.magnitude * endDirection.magnitude))).Within(0.001f));
        }
    }

    [Category("Partial Transitions")]
    internal class PartialCurveCalculationTests
    {
        [Test]
        public void TestSingleTransitionNormalised30Degrees()
        {
            float theta, a;

            FresnelMath.FindAForSingleTransition(out a, out theta, 0.691f, 0.704f, 0.12384f);

            Assert.That(a, Is.EqualTo(1).Within(0.0001f));
            Assert.That(theta, Is.EqualTo(Mathf.PI/6).Within(0.0001f));
        }

        [Test]
        public void TestSingleTransitionScaled30Degrees()
        {
            float theta, a;

            FresnelMath.FindAForSingleTransition(out a, out theta, 2.3033f, 2.34667f, 0.4128f);

            Assert.That(a, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(theta, Is.EqualTo(Mathf.PI / 6).Within(0.0001f));
        }

        [Test]
        public void TestSingleTransitionInvalid()
        {
            float theta, a;

            FresnelMath.FindAForSingleTransition(out a, out theta, 2.4f, 2.34667f, 0.4128f);

            Assert.Less(a, 0);
        }

        [Test]
        public void TestFullTransition45Degrees()
        {
            float theta, a;

            FresnelMath.FindAForPartialTransitionOut(out a, out theta, float.PositiveInfinity, 2.11f, 2.11f);

            Assert.That(a, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(theta, Is.EqualTo(Mathf.PI/4).Within(0.0001f));
        }

        [Test]
        public void TestFullTransition30Degrees()
        {
            float theta, a;

            FresnelMath.FindAForPartialTransitionOut(out a, out theta, float.PositiveInfinity, 1.163245f, 0.6716f);

            Assert.That(a, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(theta, Is.EqualTo(Mathf.PI / 6).Within(0.0001f));
        }

        [Test]
        public void TestPartialTransitionNormalised30Degrees()
        {
            TestPartialTransition(1, Mathf.PI / 6, 0.5f);
        }

        [Test]
        public void TestPartialTransitionScaled30Degrees()
        {
            TestPartialTransition(12, Mathf.PI / 6, 0.5f);
        }

        [Test]
        public void TestPartialTransitionNearMidpoint30Degrees()
        {
            TestPartialTransition(1, Mathf.PI / 6, 0.99f);
        }

        [Test]
        public void TestPartialTransitionNearEnd30Degrees()
        {
            TestPartialTransition(1, Mathf.PI / 6, 0.01f);
        }

        [Test]
        public void TestPartialTransitionScaled60Degrees()
        {
            TestPartialTransition(7, Mathf.PI / 3, 0.5f);
        }

        [Test]
        public void TestPartialTransitionNearMidpoint1Degree()
        {
            TestPartialTransition(1, Mathf.PI/180, 0.99f);
        }

        private void TestPartialTransition(float targetA, float targetTheta, float fractionFromEnd)
        {
            float theta, a;

            float xp, yp, radius;

            float rootTheta = Mathf.Sqrt(targetTheta);

            radius = 1 / (2 * rootTheta * targetA * fractionFromEnd);

            float xFull = FresnelMath.FresnelC(rootTheta) / targetA;
            float yFull = FresnelMath.FresnelS(rootTheta) / targetA;
            float xHalf = FresnelMath.FresnelC(fractionFromEnd * rootTheta) / targetA;
            float yHalf = FresnelMath.FresnelS(fractionFromEnd * rootTheta) / targetA;

            xp = xFull + Mathf.Cos(targetTheta * 2) * (xFull - xHalf) + Mathf.Sin(targetTheta * 2) * (yFull - yHalf);
            yp = yFull + Mathf.Sin(targetTheta * 2) * (xFull - xHalf) - Mathf.Cos(targetTheta * 2) * (yFull - yHalf);

            Debug.Log("xp = " + xp + ", yp = " + yp + ", radius = " + radius);

            FresnelMath.FindAForPartialTransitionOut(out a, out theta, radius, xp, yp);

            Assert.That(a, Is.EqualTo(targetA).Within(0.01f).Percent);
            Assert.That(theta, Is.EqualTo(targetTheta).Within(0.01f).Percent);
        }
    }
}