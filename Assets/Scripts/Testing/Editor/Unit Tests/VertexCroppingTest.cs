using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnitTest
{
    [TestFixture]
    [Category("Vertex Truncation")]
    internal class VertexCropperTests
    {
        [Test]
        public void NoVertsBeyondBounds()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();

            Bounds bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(20, 4, 4));

            //check there aren't any triangle refences out of bounds of the vert array
            foreach (int i in meshOwner.m_triangles)
            {
                Assert.Less(i, meshOwner.m_verts.Length);
            }
            //This bounding box is large enough to encapsulate whole model; check it's set up properly
            foreach (Vector3 v in meshOwner.m_verts)
            {
                Assert.True(bounds.Contains(v));
            }

            cropper.meshOwner = meshOwner;

            Bounds smallbounds = new Bounds(new Vector3(0, 0, 0), new Vector3(12, 4, 4));

            //this bounding box should be to small to contain the model, make sure
            bool outsideBounds = false;
            foreach (Vector3 v in meshOwner.m_verts)
            {
                if (!smallbounds.Contains(v)) outsideBounds = true;
            }
            Assert.True(outsideBounds);

            cropper.CropVerts(smallbounds);

            //it should now be small enough to fit inside smallBounds
            foreach (Vector3 v in meshOwner.m_verts)
            {
                Assert.True(smallbounds.Contains(v));
            }
        }

        [Test]
        public void TriangleMatchUp()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;

            //store the vectors that make up the triangles
            Vector3[] triangles = new Vector3[meshOwner.m_triangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = meshOwner.m_verts[meshOwner.m_triangles[i]];
            }


            Bounds smallbounds = new Bounds(new Vector3(0, 0, 0), new Vector3(12, 4, 4));

            cropper.CropVerts(smallbounds);

            //check there aren't any triangle references out of bounds of the vert array
            foreach (int i in meshOwner.m_triangles)
            {
                Assert.Less(i, meshOwner.m_verts.Length);
            }

            //verify that the new meshowner triangles all existed before
            Vector3[] newTriangles = new Vector3[meshOwner.m_triangles.Length];
            for (int i = 0; i < newTriangles.Length; i++)
            {
                newTriangles[i] = meshOwner.m_verts[meshOwner.m_triangles[i]];
            }

            for (int i = 0; i < newTriangles.Length; i += 3)
            {
                bool verified = false;
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    if (newTriangles[i] == triangles[j] &&
                        newTriangles[i + 1] == triangles[j + 1] &&
                        newTriangles[i + 2] == triangles[j + 2])
                    {
                        verified = true;
                        break;
                    }
                }
                Assert.True(verified);
            }
        }

        [Test]
        public void UVremoved()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;

            Bounds smallbounds = new Bounds(new Vector3(0, 0, 0), new Vector3(12, 4, 4));

            cropper.CropVerts(smallbounds);

            Assert.AreEqual(8*3, meshOwner.m_verts.Length);
            Assert.AreEqual(8*3, meshOwner.m_uv.Length);
        }

        [Test]
        public void NoVertsBeyondBoundsHalfTriangle()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();

            cropper.meshOwner = meshOwner;

            Bounds smallbounds = new Bounds(new Vector3(0, 0, 0), new Vector3(16, 4, 4));

            cropper.CropVerts(smallbounds);

            //it should now be small enough to fit inside smallBounds
            foreach (Vector3 v in meshOwner.m_verts)
            {
                Assert.True(smallbounds.Contains(v));
            }
        }

        [Test]
        public void ArraysCorrectLengthHalfTriangle()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();

            cropper.meshOwner = meshOwner;

            Bounds smallbounds = new Bounds(new Vector3(0, 0, 0), new Vector3(16, 4, 4));

            cropper.CropVerts(smallbounds);

            foreach (int t in meshOwner.m_triangles)
            {
                Assert.Less(t, meshOwner.m_verts.Length);
            }

            Assert.AreEqual(meshOwner.m_verts.Length, meshOwner.m_uv.Length);
        }
        
        [Test]
        public void TriangleMatchUpHalfTriangle()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;

            int nTriangles = meshOwner.m_triangles.Length / 3;

            Bounds smallbounds = new Bounds(new Vector3(0, 0, 0), new Vector3(16, 4, 4));

            cropper.CropVerts(smallbounds);

            Assert.AreEqual(nTriangles + 4, meshOwner.m_triangles.Length / 3);

            int nSmooshedTrianglesNegativeX = 0;
            for (int i = 0; i < meshOwner.m_triangles.Length; i+=3)
            {
                if (meshOwner.m_verts[meshOwner.m_triangles[i]].x < -6 &&
                    meshOwner.m_verts[meshOwner.m_triangles[i + 1]].x < -6 &&
                    meshOwner.m_verts[meshOwner.m_triangles[i + 2]].x < -6)
                {
                    nSmooshedTrianglesNegativeX++;
                }
            }

            Assert.AreEqual(6, nSmooshedTrianglesNegativeX);

            int nSmooshedTrianglesPositiveX = 0;
            for (int i = 0; i < meshOwner.m_triangles.Length; i += 3)
            {
                if (meshOwner.m_verts[meshOwner.m_triangles[i]].x > 6 &&
                    meshOwner.m_verts[meshOwner.m_triangles[i + 1]].x > 6 &&
                    meshOwner.m_verts[meshOwner.m_triangles[i + 2]].x > 6)
                {
                    nSmooshedTrianglesPositiveX++;
                }
            }

            Assert.AreEqual(6, nSmooshedTrianglesPositiveX);
        }

        [Test]
        public void TestSingleVertInsideBounds()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;

            meshOwner.m_verts = new Vector3[3];
            meshOwner.m_verts[0] = new Vector3(0, 0, 0);
            meshOwner.m_verts[1] = new Vector3(0, 2, 0);
            meshOwner.m_verts[2] = new Vector3(0, 0, 2);

            meshOwner.m_uv = new Vector2[3];

            meshOwner.m_triangles = new int[3];
            for (int i = 0; i < 3; i++)
            {
                meshOwner.m_triangles[i] = i;
            }

            Bounds b = new Bounds(new Vector3(0, 0, 0), new Vector3(2, 2, 2));

            cropper.CropVerts(b);

            Assert.AreEqual(3, meshOwner.m_triangles.Length);
            Assert.That(meshOwner.m_verts[1].y, Is.EqualTo(1).Within(0.001));
            Assert.That(meshOwner.m_verts[2].z, Is.EqualTo(1).Within(0.001));
        }

        [Test]
        public void TestDoubleVertInsideBounds()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;

            meshOwner.m_verts = new Vector3[3];
            meshOwner.m_verts[0] = new Vector3(0, -0.5f, 0);
            meshOwner.m_verts[1] = new Vector3(0, 0.5f, 0);
            meshOwner.m_verts[2] = new Vector3(0, 0, 5);

            meshOwner.m_uv = new Vector2[3];

            meshOwner.m_triangles = new int[3];
            for (int i = 0; i < 3; i++)
            {
                meshOwner.m_triangles[i] = i;
            }

            Bounds b = new Bounds(new Vector3(0, 0, 0), new Vector3(2, 2, 2));

            cropper.CropVerts(b);

            Assert.AreEqual(6, meshOwner.m_triangles.Length);
            Assert.AreEqual(4, meshOwner.m_verts.Length);

            int nVertsWithZ1 = 0;
            for (int i = 1; i < 4; i++)
            {
                if (1 - meshOwner.m_verts[i].z < 0.001) nVertsWithZ1++;
            }
            Assert.AreEqual(2, nVertsWithZ1);
        }

        [Test]
        public void EverythingIsZero()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;

            Bounds b = new Bounds(new Vector3(0, 0, 0), new Vector3(0.1f, 0.1f, 0.1f));

            cropper.CropVerts(b);

            Assert.AreEqual(0, meshOwner.m_triangles.Length);
            Assert.AreEqual(0, meshOwner.m_verts.Length);
            Assert.AreEqual(0, meshOwner.m_uv.Length);
        }

        [Test]
        public void ShortenQuad()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;

            meshOwner.m_verts = new Vector3[4];
            meshOwner.m_verts[0] = new Vector3(-5, 1, 0);
            meshOwner.m_verts[1] = new Vector3(5, 1, 0);
            meshOwner.m_verts[2] = new Vector3(5, -1, 0);
            meshOwner.m_verts[3] = new Vector3(-5, -1, 0);

            meshOwner.m_uv = new Vector2[4];

            meshOwner.m_triangles = new int[6];
            meshOwner.m_triangles[0] = 0;
            meshOwner.m_triangles[1] = 1;
            meshOwner.m_triangles[2] = 2;
            meshOwner.m_triangles[3] = 2;
            meshOwner.m_triangles[4] = 3;
            meshOwner.m_triangles[5] = 0;

            Vector3[] oldNormals = meshOwner.GetNormals();

            Assert.That(oldNormals[0].z, Is.EqualTo(-1.0f).Within(0.001f));

            Bounds b = new Bounds(new Vector3(0,0,0), new Vector3(4, 3, 1));

            cropper.CropVerts(b);

            Assert.AreEqual(6, meshOwner.m_verts.Length);
            Assert.AreEqual(12, meshOwner.m_triangles.Length);

            //Test that the triangle normals are the same
            Vector3[] newNormals = meshOwner.GetNormals();
            Assert.That(newNormals[0].z, Is.EqualTo(oldNormals[0].z).Within(0.001f));
            Assert.That(newNormals[0].y, Is.EqualTo(oldNormals[0].y).Within(0.001f));
            Assert.That(newNormals[0].x, Is.EqualTo(oldNormals[0].x).Within(0.001f));

        }

        [Test]
        public void CornerTriangle()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;

            meshOwner.m_verts = new Vector3[3];
            meshOwner.m_verts[0] = new Vector3(1.5f, -0.5f, 0);
            meshOwner.m_verts[1] = new Vector3(-0.5f, 1.5f, 0);
            meshOwner.m_verts[2] = new Vector3(1.5f, 1.5f, 0);

            meshOwner.m_uv = new Vector2[3];
            meshOwner.m_triangles = new int[3];
            for (int i = 0; i < 3; i++)
            {
                meshOwner.m_triangles[i] = i;
            }

            Bounds b = new Bounds(new Vector3(0, 0, 0), new Vector3(2, 2, 2));

            cropper.CropVerts(b);

            Assert.That(meshOwner.m_verts[2].x, Is.EqualTo(1).Within(0.0001));
            Assert.That(meshOwner.m_verts[2].y, Is.EqualTo(1).Within(0.0001));

            foreach (Vector3 v in meshOwner.m_verts)
            {
                Assert.True(b.Contains(v));
            }

            Assert.AreEqual(3, meshOwner.m_triangles.Length);

            // now on a different corner
            meshOwner.m_verts = new Vector3[3];
            meshOwner.m_verts[0] = new Vector3(0, -1.5f, 0);
            meshOwner.m_verts[1] = new Vector3(-1.5f, 1f, 0);
            meshOwner.m_verts[2] = new Vector3(-1.5f, -1.5f, 0);

            meshOwner.m_uv = new Vector2[3];
            meshOwner.m_triangles = new int[3];
            for (int i = 0; i < 3; i++)
            {
                meshOwner.m_triangles[i] = i;
            }

            cropper.CropVerts(b);

            Assert.That(meshOwner.m_verts[2].x, Is.EqualTo(-1).Within(0.0001));
            Assert.That(meshOwner.m_verts[2].y, Is.EqualTo(-1).Within(0.0001));

            foreach (Vector3 v in meshOwner.m_verts)
            {
                Assert.True(b.Contains(v));
            }

            Assert.AreEqual(3, meshOwner.m_triangles.Length);
        }

        [Test]
        public void UVmappedunmovedVerts()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;
            
            Bounds b = new Bounds(new Vector3(0, 0, 0), new Vector3(12, 4, 4));

            cropper.CropVerts(b);
            
            for (int i = 0; i < meshOwner.m_uv.Length; i += 8)
            {
                Assert.That(meshOwner.m_uv[i + 0] == new Vector2(0, 0));
                Assert.That(meshOwner.m_uv[i + 1] == new Vector2(0, 1));
                Assert.That(meshOwner.m_uv[i + 2] == new Vector2(1, 1));
                Assert.That(meshOwner.m_uv[i + 3] == new Vector2(1, 0));

                Assert.That(meshOwner.m_uv[i + 4] == new Vector2(1, 0));
                Assert.That(meshOwner.m_uv[i + 5] == new Vector2(1, 1));
                Assert.That(meshOwner.m_uv[i + 6] == new Vector2(0, 1));
                Assert.That(meshOwner.m_uv[i + 7] == new Vector2(0, 0));
            }
        }

        [Test]
        public void UVmappedmovedVerts()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;

            Bounds b = new Bounds(new Vector3(0, 0, 0), new Vector3(16, 4, 4));

            cropper.CropVerts(b);

            Assert.AreEqual(meshOwner.m_verts.Length, meshOwner.m_uv.Length);

            int uvsChecked = 0;
            for (int i = 0; i < meshOwner.m_verts.Length; i++)
            {
                if (VertexCropperLogic.AreApproximatelyEqual(meshOwner.m_verts[i].y, 0))
                {
                    Assert.That(meshOwner.m_uv[i].y, Is.EqualTo(0.5f).Within(0.001f));
                    
                    Assert.That(meshOwner.m_uv[i].x, Is.EqualTo(0.5f).Within(0.001f));

                    uvsChecked++;
                }
            }

            Assert.AreEqual(4, uvsChecked);


        }

        [Test]
        public void UVDiagonalTriangleAcross()
        {
            VertexCropperLogic cropper = new VertexCropperLogic();
            MeshOwnerCropStub meshOwner = new MeshOwnerCropStub();
            cropper.meshOwner = meshOwner;

            Bounds b = new Bounds(Vector3.zero, Vector3.one);

            meshOwner.m_verts = new Vector3[3];
            meshOwner.m_uv = new Vector2[3];
            meshOwner.m_triangles = new int[3];

            meshOwner.m_verts[0] = new Vector3(-1.5f, 0.5f, 1f);
            meshOwner.m_verts[1] = new Vector3(-1.5f, -0.5f, 1f);
            meshOwner.m_verts[2] = new Vector3(1.5f, 0, -1f);

            meshOwner.m_uv[0] = new Vector2(0, 1);
            meshOwner.m_uv[1] = new Vector2(0, 0);
            meshOwner.m_uv[2] = new Vector2(1, 0.5f);

            for (int i = 0; i < 3; i++)
            {
                meshOwner.m_triangles[i] = i;
            }

            cropper.CropVerts(b);

            Assert.AreEqual(6, meshOwner.m_triangles.Length);
            Assert.AreEqual(4, meshOwner.m_verts.Length);

            int assertionCounter = 0;
            for (int i = 0; i < 4; i++)
            {
                if (VertexCropperLogic.AreApproximatelyEqual(0.3333333f, meshOwner.m_verts[i].y))
                {
                    Assert.That(meshOwner.m_uv[i].x, Is.EqualTo(0.33333f).Within(0.001f));
                    Assert.That(meshOwner.m_uv[i].y, Is.EqualTo(0.83333f).Within(0.001f));
                    assertionCounter++;
                }
                else if (VertexCropperLogic.AreApproximatelyEqual(0.1666667f, meshOwner.m_verts[i].y))
                {
                    Assert.That(meshOwner.m_uv[i].x, Is.EqualTo(0.66667f).Within(0.001f));
                    Assert.That(meshOwner.m_uv[i].y, Is.EqualTo(0.66667f).Within(0.001f));
                    assertionCounter++;
                }
                else if (VertexCropperLogic.AreApproximatelyEqual(-0.3333333f, meshOwner.m_verts[i].y))
                {
                    Assert.That(meshOwner.m_uv[i].x, Is.EqualTo(0.33333f).Within(0.001f));
                    Assert.That(meshOwner.m_uv[i].y, Is.EqualTo(0.16667f).Within(0.001f));
                    assertionCounter++;
                }
                else if (VertexCropperLogic.AreApproximatelyEqual(-0.1666667f, meshOwner.m_verts[i].y))
                {
                    Assert.That(meshOwner.m_uv[i].x, Is.EqualTo(0.66667f).Within(0.001f));
                    Assert.That(meshOwner.m_uv[i].y, Is.EqualTo(0.33333f).Within(0.001f));
                    assertionCounter++;
                }

            }

            Assert.AreEqual(4, assertionCounter);
        }


        [Test]
        public void MovingVerts()
        {
            Bounds b = new Bounds(new Vector3(0, 0, 0), new Vector3(20, 20, 20));

            Vector3 vert = VertexCropperLogic.GetMovedVert(b, new Vector3(0, 5, 0), new Vector3(15, 20, 0));

            Assert.That(vert.x, Is.EqualTo(5).Within(0.0001));
            Assert.That(vert.y, Is.EqualTo(10).Within(0.0001));
            Assert.That(vert.z, Is.EqualTo(0).Within(0.0001));

            vert = VertexCropperLogic.GetMovedVert(b, new Vector3(-20, -15, 0), new Vector3(15, 20, 0));

            Assert.That(vert.x, Is.EqualTo(5).Within(0.0001));
            Assert.That(vert.y, Is.EqualTo(10).Within(0.0001));
            Assert.That(vert.z, Is.EqualTo(0).Within(0.0001));

            vert = VertexCropperLogic.GetMovedVert(b, new Vector3(20, -15, 0), new Vector3(15, 20, 0));

            Assert.False(b.Contains(vert));

            vert = VertexCropperLogic.GetMovedVert(b, new Vector3(20, -5, 0), new Vector3(20, 5, 0));

            Assert.False(b.Contains(vert));

            vert = VertexCropperLogic.GetMovedVert(b, new Vector3(15, 0, 0), new Vector3(20, 0, 0));

            Assert.False(b.Contains(vert));
        }
    }




    internal class MeshOwnerCropStub : IMeshOwner
    {
        public Vector3[] m_verts;
        public Vector2[] m_uv;
        public int[] m_triangles;
        
        public MeshOwnerCropStub()
        {
            m_verts = new Vector3[8 * 5];
            m_uv = new Vector2[8 * 5];

            for (int box = 0; box < 5; box++)
            {
                float offset = -8 + 4 * box;
                int indexOffset = box * 8;

                m_verts[indexOffset] = new Vector3(offset - 1, -1, 1);
                m_verts[indexOffset + 1] = new Vector3(offset - 1, 1, 1);
                m_verts[indexOffset + 2] = new Vector3(offset + 1, 1, 1);
                m_verts[indexOffset + 3] = new Vector3(offset + 1, -1, 1);

                m_verts[indexOffset + 4] = new Vector3(offset + 1, -1, -1);
                m_verts[indexOffset + 5] = new Vector3(offset + 1, 1, -1);
                m_verts[indexOffset + 6] = new Vector3(offset - 1, 1, -1);
                m_verts[indexOffset + 7] = new Vector3(offset - 1, -1, -1);


                m_uv[indexOffset] = new Vector2(0, 0);
                m_uv[indexOffset + 1] = new Vector2(0, 1);
                m_uv[indexOffset + 2] = new Vector2(1, 1);
                m_uv[indexOffset + 3] = new Vector2(1, 0);

                m_uv[indexOffset + 4] = new Vector2(1, 0);
                m_uv[indexOffset + 5] = new Vector2(1, 1);
                m_uv[indexOffset + 6] = new Vector2(0, 1);
                m_uv[indexOffset + 7] = new Vector2(0, 0);
            }

            m_triangles = new int[4 * 5 * 3];

            for (int i = 0; i < (4 * 5 * 3) / 6; i++)
            {
                int indexBase = i * 6;
                int vertIndBase = i * 4;

                m_triangles[indexBase] = vertIndBase;
                m_triangles[indexBase + 1] = vertIndBase + 1;
                m_triangles[indexBase + 2] = vertIndBase + 2;
                m_triangles[indexBase + 3] = vertIndBase;
                m_triangles[indexBase + 4] = vertIndBase + 2;
                m_triangles[indexBase + 5] = vertIndBase + 3;
            }
        }

        public void GetMeshInfo(out Vector3[] verts, out Vector2[] uv, out int[] triangles)
        {
            verts = m_verts;
            uv = m_uv;
            triangles = m_triangles;
        }

        public void SetMeshInfo(Vector3[] verts, Vector2[] uv, int[] triangles)
        {
            m_verts = verts;
            m_uv = uv;
            m_triangles = triangles;
        }

        public Vector3[] GetNormals()
        {
            Vector3[] normals = new Vector3[m_triangles.Length / 3];

            for (int i = 0; i < normals.Length; i++)
            {
                int triangleIndex = i * 3;
                normals[i] = Vector3.Cross((m_verts[m_triangles[triangleIndex + 1]] - m_verts[m_triangles[triangleIndex]]),
                    (m_verts[m_triangles[triangleIndex + 2]] - m_verts[m_triangles[triangleIndex + 1]]));

                normals[i].Normalize();
            }

            return normals;
        }
    }
}