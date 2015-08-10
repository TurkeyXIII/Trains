using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnitTest
{
    [TestFixture]
    [Category("Vertex Bending")]
    internal class VertexBenderTests
    {
        [Test]
        public void TestEndpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            float length = logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, 1));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(1).Within(0.001));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(1).Within(0.001));

            Assert.That(length, Is.EqualTo(0.886227 * 2 / 1.055089).Within(0.001));

        }

        [Test]
        public void TestMidpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(2, 0, 0), new Vector3(2, 0, 2));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(0.833099 / 1.055089 * 2).Within(0.0001));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(0.22199 / 1.055089 * 2).Within(0.0001));
        }

        [Test]
        public void TestPartwaypoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(0.25f, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, 1));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(0.441408 / 1.055089).Within(0.0001));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(0.0289219 / 1.055089).Within(0.0001));
        }

        [Test]
        public void TestInsideCurveStartpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = new Vector3(0, 0, 0.5f);
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, 1));

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(0).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].z, Is.EqualTo(0.5).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].y, Is.EqualTo(0).Within(0.1).Percent);
        }

        [Test]
        public void TestOutsideCurveStartpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;

            meshOwner.uvs = new Vector2[3];
            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = new Vector3(0, 0, -0.5f);
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, 1));

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(0).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].z, Is.EqualTo(-0.5).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].y, Is.EqualTo(0).Within(0.1).Percent);
        }

        [Test]
        public void TestInsideCurveEndpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = new Vector3(1, 0, 0.5f);
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, 1));

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(0.5).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].z, Is.EqualTo(1).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].y, Is.EqualTo(0).Within(0.1).Percent);
        }

        [Test]
        public void TestOutsideCurveEndpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = new Vector3(1, 0, -0.5f);
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, 1));

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(1.5).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].z, Is.EqualTo(1).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].y, Is.EqualTo(0).Within(0.1).Percent);
        }

        [Test]
        public void TestInsideCurveMidpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = new Vector3(0.5f, 0, 0.5f);
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, 1));

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(0.833099 / 1.055089 - 0.5 * 0.7071).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].z, Is.EqualTo(0.22199 / 1.055089 + 0.5 * 0.7071).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].y, Is.EqualTo(0).Within(0.1).Percent);
        }

        [Test]
        public void TestOutsideCurveMidpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = new Vector3(0.5f, 0, -0.5f);
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, 1));

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(0.833099 / 1.055089 + 0.5 * 0.7071).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].z, Is.EqualTo(0.22199 / 1.055089 - 0.5 * 0.7071).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].y, Is.EqualTo(0).Within(0.1).Percent);
        }

        [Test]
        public void TestStartOffsetStartpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(-0.5f, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(0.5f, 0, 1));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(-0.5).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(0).Within(0.1).Percent);
        }

        [Test]
        public void TestStartOffsetEndpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(0.5f, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(0.5f, 0, 1));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(0.5).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(1).Within(0.1).Percent);
        }

        [Test]
        public void TestStartOffsetMidpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(0.5f, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(0.5f, 0, 1));

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(0.833099 / 1.055089 - 0.5).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[0].z, Is.EqualTo(0.22199 / 1.055089).Within(0.1).Percent);
        }
        
        [Test]
        public void TestEndPoint30deg()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            float z = Mathf.Tan(Mathf.PI/6f); //for a 30 degree bend
            float length = logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, z));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(1).Within(0.0001));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(z).Within(0.0001));

            Assert.That(length, Is.EqualTo(2 * 0.7236 / 1.1633).Within(0.0001));
        }

        [Test]
        public void TestMidPoint30deg()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(0.5f, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            float z = Mathf.Tan(Mathf.PI / 6f); //for a 30 degree bend
            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, z));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(0.704012 / 1.1633).Within(0.0001));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(0.123840 / 1.1633).Within(0.0001));
        }

        [Test]
        public void TestMidpointClockwise()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;

            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            float length = logic.Bend(new Vector3(2, 0, 0), new Vector3(2, 0, -2));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(0.833099 / 1.055089 * 2).Within(0.0001));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(-0.22199 / 1.055089 * 2).Within(0.0001));

            Assert.That(length, Is.EqualTo(0.886227 * 4 / 1.055089).Within(0.001));
        }

        [Test]
        public void TestPartwaypointClockwise()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;

            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = Vector3.zero;
            meshOwner.vertices[2] = new Vector3(0.25f, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, -1));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(0.441408 / 1.055089).Within(0.0001));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(-0.0289219 / 1.055089).Within(0.0001));
        }

        [Test]
        public void TestNewTrianglesMidpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;

            //use a subtle bend theta = 30 degrees
            //the midpoint should always be split
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = new Vector3(0, -1, 0);
            meshOwner.vertices[1] = new Vector3(0, 1, 0);
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            float z = Mathf.Tan(Mathf.PI / 6f); //for a 30 degree bend
            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, z));

            Assert.AreEqual(9, meshOwner.tris.Length);
            Assert.AreEqual(5, meshOwner.vertices.Length);
            Assert.That(meshOwner.vertices[3].x, Is.EqualTo(0.704012 / 1.1633).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[3].z, Is.EqualTo(0.123840 / 1.1633).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[4].x, Is.EqualTo(0.704012 / 1.1633).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[4].z, Is.EqualTo(0.123840 / 1.1633).Within(0.1).Percent);
        }

        [Test]
        public void TestNewTrianglesSharedVerts()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;

            meshOwner.uvs = new Vector2[5];

            meshOwner.vertices = new Vector3[5];
            meshOwner.vertices[0] = new Vector3(0, -1, 0);
            meshOwner.vertices[1] = new Vector3(0, 1, 0);
            meshOwner.vertices[2] = new Vector3(1, -1, 0);
            meshOwner.vertices[3] = new Vector3(1, 1, 0);
            meshOwner.vertices[4] = new Vector3(1, 1, 1);

            meshOwner.tris = new int[9];
            for (int i = 0; i < 3; i++)
            {
                meshOwner.tris[i] = i;
                meshOwner.tris[i + 3] = 3 - i;
            }
            meshOwner.tris[6] = 1;
            meshOwner.tris[7] = 3;
            meshOwner.tris[8] = 4;

            // when bent, this quad should have 7 verts total
            // the triangle in z should have an additional 3
            float z = Mathf.Tan(Mathf.PI / 6f); //for a 30 degree bend
            logic.maxCornerAngleRadians = Mathf.PI;
            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, z));

            Assert.AreEqual(10, meshOwner.vertices.Length);
            Assert.AreEqual(27, meshOwner.tris.Length);

            for (int i = 0; i > 27; i++)
            {
                Assert.Less(meshOwner.tris[i], 9, "Tris referencing vert " + meshOwner.tris[i]);
            }
        }

        [Test]
        public void TestNewTrianglesTrisCreaseInPlane()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;

            meshOwner.uvs = new Vector2[4];

            meshOwner.vertices = new Vector3[4];
            meshOwner.vertices[0] = new Vector3(0, -1, 0);
            meshOwner.vertices[1] = new Vector3(0, 1, 0);
            meshOwner.vertices[2] = new Vector3(1, -1, 0);
            meshOwner.vertices[3] = new Vector3(1, 1, 0);

            meshOwner.tris = new int[6];
            for (int i = 0; i < 3; i++)
            {
                meshOwner.tris[i] = i;
                meshOwner.tris[i + 3] = 3 - i;
            }

            // when bent, this quad should have 7 verts total
            float z = Mathf.Tan(Mathf.PI / 6f); //for a 30 degree bend
            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, z));

            float creaseXpos = 0.704012f / 1.1633f;
            float fudgeMargin = 0.001f;
            for (int i = 0; i < 18; i+=3)
            {
                bool anyLessThan = false;
                bool anyGreaterThan = false;
                for (int j = 0; j < 3; j++)
                {
                    if (meshOwner.vertices[meshOwner.tris[i + j]].x < creaseXpos - fudgeMargin)
                    {
                        anyLessThan = true;
                    }
                    else if (meshOwner.vertices[meshOwner.tris[i + j]].x > creaseXpos + fudgeMargin)
                    {
                        anyGreaterThan = true;
                    }
                }


                Assert.False(anyGreaterThan && anyLessThan, "Triangle over crease " + creaseXpos + " bent incorrectly: " +
                    meshOwner.vertices[meshOwner.tris[i]] + ", " + meshOwner.vertices[meshOwner.tris[i + 1]] + ", " + meshOwner.vertices[meshOwner.tris[i + 2]]);
                
            }
            
        }

        [Test]
        public void TestNewTrianglesTrisCreaseOrthogonal()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;
            meshOwner.uvs = new Vector2[3];

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = new Vector3(0, 0, 1);
            meshOwner.vertices[1] = new Vector3(0, 0, -1);
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;
            logic.maxCornerAngleRadians = Mathf.PI;
            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, 1));

            Assert.That(meshOwner.vertices[3].x, Is.EqualTo(0.833099 / 1.055089 - 0.5 * 0.7071).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[3].z, Is.EqualTo(0.22199 / 1.055089 + 0.5 * 0.7071).Within(0.1).Percent);

            Assert.That(meshOwner.vertices[4].x, Is.EqualTo(0.833099 / 1.055089 + 0.5 * 0.7071).Within(0.1).Percent);
            Assert.That(meshOwner.vertices[4].z, Is.EqualTo(0.22199 / 1.055089 - 0.5 * 0.7071).Within(0.1).Percent);

            Assert.AreEqual(9, meshOwner.tris.Length);

            for (int i = 0; i < 9; i+=3)
            {
                bool anyXzero = false;
                bool anyXone = false;
                for (int j = 0; j < 3; j++)
                {
                    if (meshOwner.tris[i+j] == 2)
                        anyXone = true;
                    else if (meshOwner.tris[i+j] < 2)
                        anyXzero = true;
                }

                Assert.False(anyXzero && anyXone, "triangle not creased: " + meshOwner.vertices[meshOwner.tris[i]] + ", " + meshOwner.vertices[meshOwner.tris[i+1]] + ", " + meshOwner.vertices[meshOwner.tris[i+2]]);
            }

        }

        [Test]
        public void TestNewTrianglesUVs()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;

            //use a subtle bend theta = 30 degrees
            //the midpoint should always be split

            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = new Vector3(0, 0, 0);
            meshOwner.vertices[1] = new Vector3(0, 1, 0);
            meshOwner.vertices[2] = new Vector3(1, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < 3; i++) meshOwner.tris[i] = i;

            meshOwner.uvs = new Vector2[3];
            meshOwner.uvs[0] = new Vector2(0, 0);
            meshOwner.uvs[1] = new Vector2(0, 1);
            meshOwner.uvs[2] = new Vector2(1, 0);

            float z = Mathf.Tan(Mathf.PI / 6f); //for a 30 degree bend
            logic.Bend(new Vector3(1, 0, 0), new Vector3(1, 0, z));

            Assert.AreEqual(5, meshOwner.uvs.Length);
            if (meshOwner.vertices[3].y == 0)
            {
                Assert.That(meshOwner.uvs[3].y, Is.EqualTo(0).Within(0.1).Percent);
                Assert.That(meshOwner.uvs[4].y, Is.EqualTo(0.5).Within(0.1).Percent);
            }
            else
            {
                Assert.That(meshOwner.uvs[3].y, Is.EqualTo(0.5).Within(0.1).Percent);
                Assert.That(meshOwner.uvs[4].y, Is.EqualTo(0).Within(0.1).Percent);
            }
            Assert.That(meshOwner.uvs[3].x, Is.EqualTo(0.5).Within(0.1).Percent);
            Assert.That(meshOwner.uvs[4].x, Is.EqualTo(0.5).Within(0.1).Percent);
        }

        [Test]
        public void TestBendLengths()
        {
            // theta = 0
            float length;
            length = GetBentLength(new Vector3(1, 0, 0), new Vector3(1, 0, 0));
            Assert.That(length, Is.EqualTo(1).Within(0.001f));

            length = GetBentLength(new Vector3(1, 0, 0), new Vector3(2, 0, 0));
            Assert.That(length, Is.EqualTo(2).Within(0.001f));

            // theta = 60 deg
            float targetLength = 1 / (0.5f * FresnelMath.FresnelC(1.02333f) + 0.866f * FresnelMath.FresnelS(1.02333f)) * 2 * 1.02333f;
            length = GetBentLength(new Vector3(1, 0, 0), new Vector3(1, 0, Mathf.Tan(Mathf.PI / 3)));
            Assert.That(length, Is.EqualTo(targetLength).Within(0.001f));

            length = GetBentLength(new Vector3(1, 0, 0), new Vector3(1, 0, -Mathf.Tan(Mathf.PI / 3)));
            Assert.That(length, Is.EqualTo(targetLength).Within(0.001f));

            // theta = 90 deg
            targetLength = 1 / (2 * FresnelMath.FresnelS(1.2533f)) * 2 * 1.2533f;
            length = GetBentLength(new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            Assert.That(length, Is.EqualTo(targetLength).Within(0.001f));

            length = GetBentLength(new Vector3(1, 0, 0), new Vector3(0, 0, -1));
            Assert.That(length, Is.EqualTo(targetLength).Within(0.001f));

            // theta = 120 deg
            // as of iteration 4, angles greater than 90 degrees (pi/2) will return negative length
            float L = 1.4472f;
            targetLength = -1 / (0.5f * FresnelMath.FresnelC(L) - 0.866f * FresnelMath.FresnelS(L)) * 2 * L;
            length = GetBentLength(new Vector3(1, 0, 0), new Vector3(-1, 0, Mathf.Tan(Mathf.PI / 3)));
            //Assert.That(length, Is.EqualTo(targetLength).Within(0.1f).Percent);
            Assert.Less(length, 0);

            length = GetBentLength(new Vector3(1, 0, 0), new Vector3(-1, 0, -Mathf.Tan(Mathf.PI / 3)));
            //Assert.That(length, Is.EqualTo(targetLength).Within(0.1f).Percent);
            Assert.Less(length, 0);

            // theta = 150 deg
            length = GetBentLength(new Vector3(1, 0, 0), new Vector3(-1, 0, Mathf.Tan(Mathf.PI / 6)));
            Assert.Less(length, 0);

            length = GetBentLength(new Vector3(1, 0, 0), new Vector3(-1, 0, -Mathf.Tan(Mathf.PI / 6)));
            Assert.Less(length, 0);
        }

        private static float GetBentLength(Vector3 endPos, Vector3 targetPos)
        {
            return VertexBenderLogic.GetBentLength(endPos, targetPos);
        }
    }

    internal class VertexBenderCircularTests
    {
        [Test]
        public void TestQuarterCircleRadius1()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = CreateLineMeshOwner(Mathf.PI/2);
            logic.meshOwner = meshOwner;

            Vector3[] creases = new Vector3[0];

            Vector3 centre = new Vector3(0, 0, 1);

            logic.Bend(centre, ref creases);

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(0).Within(0.001f));
            
            Assert.That(meshOwner.vertices[1].x, Is.EqualTo(0.7071).Within(0.01f));
            Assert.That(meshOwner.vertices[1].z, Is.EqualTo(1 - 0.7071).Within(0.01f));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(1).Within(0.001f));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(1).Within(0.001f));
        }

        [Test]
        public void TestHalfCircleRadius5()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = CreateLineMeshOwner(Mathf.PI * 5);
            logic.meshOwner = meshOwner;

            Vector3[] creases = new Vector3[0];

            Vector3 centre = new Vector3(0, 0, 5);

            logic.Bend(centre, ref creases);

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(0).Within(0.001f));

            Assert.That(meshOwner.vertices[1].x, Is.EqualTo(5).Within(0.001f));
            Assert.That(meshOwner.vertices[1].z, Is.EqualTo(5).Within(0.001f));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(0).Within(0.001f));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(10).Within(0.001f));
        }

        [Test]
        public void TestQuarterCircleRadius1Outside()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = CreateLineMeshOwner(Mathf.PI / 2);
            logic.meshOwner = meshOwner;

            for (int i = 0; i < meshOwner.vertices.Length; i++)
            {
                meshOwner.vertices[i] += new Vector3(0, 0, -0.5f);
            }

            Vector3[] creases = new Vector3[0];

            Vector3 centre = new Vector3(0, 0, 1);

            logic.Bend(centre, ref creases);

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(0).Within(0.001f));
            Assert.That(meshOwner.vertices[0].z, Is.EqualTo(-0.5f).Within(0.001f));

            Assert.That(meshOwner.vertices[1].x, Is.EqualTo(0.7071 * 1.5).Within(0.01f));
            Assert.That(meshOwner.vertices[1].z, Is.EqualTo(1 - (0.7071 * 1.5)).Within(0.01f));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(1.5).Within(0.001f));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(1).Within(0.001f));
        }

        [Test]
        public void TestQuarterCircleRadius1Inside()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = CreateLineMeshOwner(Mathf.PI / 2);
            logic.meshOwner = meshOwner;

            for (int i = 0; i < meshOwner.vertices.Length; i++)
            {
                meshOwner.vertices[i] += new Vector3(0, 0, 0.5f);
            }

            Vector3[] creases = new Vector3[0];

            Vector3 centre = new Vector3(0, 0, 1);

            logic.Bend(centre, ref creases);

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(0).Within(0.001f));
            Assert.That(meshOwner.vertices[0].z, Is.EqualTo(0.5f).Within(0.001f));

            Assert.That(meshOwner.vertices[1].x, Is.EqualTo(0.7071 * 0.5).Within(0.01f));
            Assert.That(meshOwner.vertices[1].z, Is.EqualTo(1 - (0.7071 * 0.5)).Within(0.01f));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(0.5).Within(0.001f));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(1).Within(0.001f));
        }

        [Test]
        public void TestQuaterCircleRadius1Offset()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = CreateLineMeshOwner(Mathf.PI / 2);
            logic.meshOwner = meshOwner;

            Vector3[] creases = new Vector3[0];

            Vector3 centre = new Vector3(-Mathf.PI/2, 0, 1);

            logic.Bend(centre, ref creases);

            Assert.That(meshOwner.vertices[0].x, Is.EqualTo(1 - Mathf.PI/2).Within(0.001f));
            Assert.That(meshOwner.vertices[0].z, Is.EqualTo(1).Within(0.001f));

            Assert.That(meshOwner.vertices[2].x, Is.EqualTo(-Mathf.PI/2).Within(0.001f));
            Assert.That(meshOwner.vertices[2].z, Is.EqualTo(2).Within(0.001f));
        }

        [Test]
        public void TestCreasePositionsChange()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = CreateLineMeshOwner(Mathf.PI / 3);
            logic.meshOwner = meshOwner;

            Vector3[] creases = new Vector3[2];
            creases[0] = new Vector3(Mathf.PI / 4, 0, 0);
            creases[1] = new Vector3(Mathf.PI / 2, 0, 0);

            Vector3 centre = new Vector3(0, 0, 1);

            logic.Bend(centre, ref creases);

            Assert.That(creases[0].x, Is.EqualTo(0.7071).Within(0.01f));
            Assert.That(creases[0].z, Is.EqualTo(1 - 0.7071).Within(0.01f));

            Assert.That(creases[1].x, Is.EqualTo(1).Within(0.001f));
            Assert.That(creases[1].z, Is.EqualTo(1).Within(0.001f));
        }

        [Test]
        public void TestNewTrianglesVertsExist()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = CreateTriangleMeshOwner(Mathf.PI / 2);
            logic.meshOwner = meshOwner;

            Vector3[] creases = new Vector3[1];
            creases[0] = new Vector3(Mathf.PI/4, 0, 0);

            Vector3 centre = new Vector3(0, 0, 1);

            logic.Bend(centre, ref creases);

            Assert.AreEqual(5, meshOwner.vertices.Length);
        }

        [Test]
        public void TestNewTrianglesTrianglesExist()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = CreateTriangleMeshOwner(Mathf.PI / 2);
            logic.meshOwner = meshOwner;

            Vector3[] creases = new Vector3[1];
            creases[0] = new Vector3(Mathf.PI/4, 0, 0);

            Vector3 centre = new Vector3(0, 0, 1);

            logic.Bend(centre, ref creases);

            Assert.AreEqual(9, meshOwner.tris.Length);
        }

        [Test]
        public void TestNewTrianglesPositionsQuaterCircleRadius1()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = CreateTriangleMeshOwner(Mathf.PI / 2);
            logic.meshOwner = meshOwner;

            Vector3[] creases = new Vector3[1];
            creases[0] = new Vector3(Mathf.PI / 4, 0, 0);

            Vector3 centre = new Vector3(0, 0, 1);

            logic.Bend(centre, ref creases);

            Assert.That(meshOwner.vertices[3].x, Is.EqualTo(0.7071).Within(0.01f));
            float magnitudeY = (meshOwner.vertices[3].y < 0) ? -meshOwner.vertices[3].y : meshOwner.vertices[3].y;
            Assert.That(magnitudeY, Is.EqualTo(0.5).Within(0.01f));

            Assert.That(meshOwner.vertices[4].x, Is.EqualTo(0.7071).Within(0.01f));
            Assert.That(meshOwner.vertices[4].z, Is.EqualTo(1-0.7071).Within(0.01f));
        }

        [Test]
        public void TestNewTrianglesUV()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = CreateTriangleMeshOwner(Mathf.PI / 2);
            logic.meshOwner = meshOwner;

            Vector3[] creases = new Vector3[1];
            creases[0] = new Vector3(Mathf.PI / 4, 0, 0);

            meshOwner.uvs = new Vector2[3];
            meshOwner.uvs[0] = new Vector2(0, 1);
            meshOwner.uvs[1] = new Vector2(0, -1);
            meshOwner.uvs[2] = new Vector2(1, 0);

            Vector3 centre = new Vector3(0, 0, 1);
            
            logic.Bend(centre, ref creases);

            Assert.AreEqual(5, meshOwner.uvs.Length);

            Assert.That(meshOwner.uvs[3].x, Is.EqualTo(0.5).Within(0.001f));
            Assert.That(meshOwner.uvs[4].x, Is.EqualTo(0.5).Within(0.001f));

            bool is3negative = meshOwner.vertices[3].y < 0;
            Assert.That(meshOwner.uvs[3].y, Is.EqualTo(is3negative ? -0.5f : 0.5f).Within(0.001f));
            Assert.That(meshOwner.uvs[4].y, Is.EqualTo(is3negative ? 0.5f : -0.5f).Within(0.001f));
        }

        private MeshOwnerStub CreateTriangleMeshOwner(float scale)
        {
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = new Vector3(0, 1, 0);
            meshOwner.vertices[1] = new Vector3(0, -1, 0);
            meshOwner.vertices[2] = new Vector3(scale, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < meshOwner.tris.Length; i++)
                meshOwner.tris[i] = i;

            return meshOwner;
        }

        private MeshOwnerStub CreateLineMeshOwner(float scale)
        {
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            meshOwner.vertices = new Vector3[3];
            meshOwner.vertices[0] = Vector3.zero;
            meshOwner.vertices[1] = new Vector3(scale / 2, 0, 0);
            meshOwner.vertices[2] = new Vector3(scale, 0, 0);

            meshOwner.tris = new int[3];
            for (int i = 0; i < meshOwner.tris.Length; i++)
                meshOwner.tris[i] = i;

            return meshOwner;
        }

    }

    internal class MeshOwnerStub : IMeshOwner
    {
        public Vector3[] vertices;
        public Vector2[] uvs;
        public int[] tris;

        public void GetMeshInfo(out Vector3[] verts, out Vector2[] uv, out int[] triangles)
        {
            verts = vertices;
            uv = uvs;
            triangles = tris;
        }

        public void SetMeshInfo(Vector3[] verts, Vector2[] uv, int[] triangles)
        {
            vertices = verts;
            uvs = uv;
            tris = triangles;
        }
    }
}

