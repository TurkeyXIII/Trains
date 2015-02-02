﻿using System;
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
        public void TestFresnelC()
        {
            Assert.That(VertexBenderLogic.FresnelC(0.3f), Is.EqualTo(0.299757).Within(0.1f).Percent);
            Assert.That(VertexBenderLogic.FresnelC(0.8f), Is.EqualTo(0.767848).Within(0.1f).Percent);
            Assert.That(VertexBenderLogic.FresnelC(1.44f), Is.EqualTo(0.932543).Within(0.01f).Percent);
        }

        [Test]
        public void TestFresnelS()
        {
            Assert.That(VertexBenderLogic.FresnelS(0.3f), Is.EqualTo(0.00899479).Within(0.1f).Percent);
            Assert.That(VertexBenderLogic.FresnelS(0.8f), Is.EqualTo(0.165738).Within(0.1f).Percent);
            Assert.That(VertexBenderLogic.FresnelS(1.44f), Is.EqualTo(0.728459).Within(0.01f).Percent);
        }

        [Test]
        public void TestEndpoint()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;

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
        public void TestEndPoint30deg()
        {
            VertexBenderLogic logic = new VertexBenderLogic();
            MeshOwnerStub meshOwner = new MeshOwnerStub();
            logic.meshOwner = meshOwner;

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
