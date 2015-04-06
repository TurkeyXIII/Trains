using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnitTest
{
    [TestFixture]
    [Category("Rail")]
    internal class RailVectorTests
    {
        [Test]
        public void TestStraightTrack()
        {
            Vector3[] rail;
            Vector3 start = Vector3.zero;
            Vector3 end = Vector3.right;

            rail = RailVectorCreator.CreateRailVectors(0, 0, 0, 0, 0, 0, start, end, 0);

            Assert.AreEqual(2, rail.Length);
            Assert.True(TrainsMath.AreApproximatelyEqual(rail[0], start), (rail[0] + ", " + start));
            Assert.True(TrainsMath.AreApproximatelyEqual(rail[1], end), (rail[1] + ", " + end));
        }

        [Test]
        public void Test45DegCurve10DegRailNumbers()
        {
            Vector3[] rail;
            Vector3 start = Vector3.zero;
            Vector3 end = new Vector3(1, 0, Mathf.Tan(Mathf.PI / 8));
            Vector3 startDir = Vector3.right;
            Vector3 endDir = (new Vector3(1, 0, 1)).normalized;

            float length;
            rail = RunTest(start, end, startDir, endDir, 10, out length);

            Assert.AreEqual(6, rail.Length);
            Assert.True(TrainsMath.AreApproximatelyEqual(rail[0], start), (rail[0] + ", " + start));
            Assert.True(TrainsMath.AreApproximatelyEqual(rail[rail.Length-1], new Vector3(length, 0, 0)), (rail[rail.Length-1] + ", " + end));
        }

        [Test]
        public void Test45DegCurve10DegRailPlacement()
        {
            Vector3[] rail;
            Vector3 start = Vector3.zero;
            Vector3 end = new Vector3(1, 0, Mathf.Tan(Mathf.PI / 8));
            Vector3 startDir = Vector3.right;
            Vector3 endDir = (new Vector3(1, 0, 1)).normalized;
            float maxAngleDeg = 10f;
            float maxAngle = maxAngleDeg * Mathf.Deg2Rad;

            float L1, L2, A1, A2, theta1, theta2;
            float length;
            
            FresnelMath.FindTheta(out theta1, out theta2, start, end, startDir, -endDir);

            Debug.Log("theta1 = " + theta1 + ", theta2 = " + theta2);

            float x = Vector3.Dot((end - start), startDir.normalized);

            float phi = theta1 + theta2;

            A2 = FresnelMath.A2(theta1, phi, x);
            A1 = FresnelMath.A1(A2, theta1, phi);

            L1 = Mathf.Sqrt(theta1);
            L2 = Mathf.Sqrt(theta2);

            length = L1 / A1 + L2 / A2;

            rail = RailVectorCreator.CreateRailVectors(L1, L2, A1, A2, theta1, theta2, start, end, maxAngleDeg);

            float angleOfLast = Mathf.Pow(rail[0].x * A1, 2);

            float lastDifference = -1;

            for (int i = 1; i < rail.Length; i++)
            {
                float angleOfCurrent;
                if (rail[i].x < L1 / A1)
                {
                    angleOfCurrent = Mathf.Pow(rail[i].x * A1, 2);
                }
                else
                {
                    angleOfCurrent = phi - Mathf.Pow((length - rail[i].x) * A2, 2);
                }

                float difference = angleOfCurrent - angleOfLast;

                Assert.LessOrEqual(difference, maxAngle);

                if (lastDifference > 0)
                    Assert.That(difference, Is.EqualTo(lastDifference).Within(1).Percent);

                angleOfLast = angleOfCurrent;
                lastDifference = difference;
            }
        }

        private static Vector3[] RunTest(Vector3 start, Vector3 end, Vector3 startDir, Vector3 endDir, float maxAngle, out float length)
        {
            float L1, L2, A1, A2, theta1, theta2;

            FresnelMath.FindTheta(out theta1, out theta2, start, end, startDir, -endDir);

            Debug.Log("theta1 = " + theta1 + ", theta2 = " + theta2);

            float x = Vector3.Dot((end - start), startDir.normalized);

            float phi = theta1 + theta2;

            A2 = FresnelMath.A2(theta1, phi, x);
            A1 = FresnelMath.A1(A2, theta1, phi);

            L1 = Mathf.Sqrt(theta1);
            L2 = Mathf.Sqrt(theta2);

            length = L1 / A1 + L2 / A2;

            return RailVectorCreator.CreateRailVectors(L1, L2, A1, A2, theta1, theta2, start, end, maxAngle);
        }
    }
}