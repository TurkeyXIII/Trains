using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace UnitTest
{
    internal class StubToolSelector : IToolSelector
    {

        public EffectSize GetBrushSize()
        {
            return EffectSize.Small;
        }
    }
    internal class StubTerrainData : ITerrainData
    {

        public int GetAlphamapHeight()
        {
            return 16;
        }

        public int GetAlphamapWidth()
        {
            return GetAlphamapHeight();
        }
    }

    [TestFixture]
    [Category("TerrainControllerBrush")]
    internal class TerrainBrushTest
    {
        private Brush SetUpBrush()
        {
            Brush brush = new Brush();
            int[] brushRadius = { 4 };
            StubToolSelector ts = new StubToolSelector();
            StubTerrainData td = new StubTerrainData();
            brush.SetSizeRadii(brushRadius);
            brush.SetToolSelector(ts);
            brush.SetTerrainData(td);

            return brush;
        }

        [Test]
        public void TestBrushCircle()
        {
            Brush brush = SetUpBrush();
            XY location, corner, dimension;
            location.x = 8;location.y = 8;
            corner.x = 4; corner.y = 4;
            dimension.x = 8; dimension.y = 8;

            bool[,] area = brush.GetAffectedArea(location, corner, dimension);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i-4)*(i-4) + (j-4)*(j-4) < 16)
                    {
                        Assert.True(area[i,j]);
                    }
                    else
                    {
                        Assert.False(area[i,j]);
                    }
                }
            }
        }

        [Test]
        public void TestBrushSemiCircle()
        {
            Brush brush = SetUpBrush();
            XY location, corner, dimension;
            location.x = 8; location.y = 0;
            corner.x = 4; corner.y = 0;
            dimension.x = 8; dimension.y = 4;

            bool[,] area = brush.GetAffectedArea(location, corner, dimension);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i) * (i) + (j - 4) * (j - 4) < 16)
                    {
                        Assert.True(area[i, j]);
                    }
                    else
                    {
                        Assert.False(area[i, j]);
                    }
                }
            }
        }

        [Test]
        public void TestBrushQuaterCircle()
        {
            Brush brush = SetUpBrush();
            XY location, corner, dimension;
            location.x = 0; location.y = 0;
            corner.x = 0; corner.y = 0;
            dimension.x = 4; dimension.y = 4;

            bool[,] area = brush.GetAffectedArea(location, corner, dimension);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if ((i) * (i) + (j) * (j) < 16)
                    {
                        Assert.True(area[i, j]);
                    }
                    else
                    {
                        Assert.False(area[i, j]);
                    }
                }
            }
        }

        [Test]
        public void TestDetails()
        {
            Brush brush = SetUpBrush();
            XY location, corner, dimension;
            location.x = 8; location.y = 8;

            brush.GetDetails(location, out corner, out dimension);

            Assert.AreEqual(4, corner.x);
            Assert.AreEqual(4, corner.y);
            Assert.AreEqual(8, dimension.x);
            Assert.AreEqual(8, dimension.y);
        }

        [Test]
        public void TestDetailsCorner()
        {
            Brush brush = SetUpBrush();
            XY location, corner, dimension;
            location.x = 15; location.y = 15;

            brush.GetDetails(location, out corner, out dimension);

            Assert.AreEqual(11, corner.x);
            Assert.AreEqual(11, corner.y);
            Assert.AreEqual(5, dimension.x);
            Assert.AreEqual(5, dimension.y);

            location.x = 0; location.y = 0;
            brush.GetDetails(location, out corner, out dimension);
            Assert.AreEqual(0, corner.x);
            Assert.AreEqual(0, corner.y);
            Assert.AreEqual(4, dimension.x);
            Assert.AreEqual(4, dimension.y);

        }

    }

    [TestFixture]
    [Category("TerrainControllerBrush")]
    internal class TerrainControllerTest
    {
        [Test]
        public void Horizontal5by5()
        {
            HeightmapOwnerStub owner = new HeightmapOwnerStub();
            TerrainControllerLogic logic = new TerrainControllerLogic();
            logic.heightmapOwner = owner;

            owner.IntialiseHeightmap(5);

            XY from, to;

            from.x = 1;
            from.y = 2;
            to.x = 3;
            to.y = 2;

            logic.SetLineHeight(from, to, 1.5f, 0, 0);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (j == 0 || j == 4 || i == 0 || i == 4)
                    {
                        Assert.AreEqual(1, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                    else
                    {
                        Assert.AreEqual(0, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                }
            }
        }

        [Test]
        public void Vertical5by5()
        {
            HeightmapOwnerStub owner = new HeightmapOwnerStub();
            TerrainControllerLogic logic = new TerrainControllerLogic();
            logic.heightmapOwner = owner;

            owner.IntialiseHeightmap(5);

            XY from, to;

            from.x = 2;
            from.y = 1;
            to.x = 2;
            to.y = 3;

            logic.SetLineHeight(from, to, 1.5f, 0, 0);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (j == 0 || j == 4 || i == 0 || i == 4)
                    {
                        Assert.AreEqual(1, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                    else
                    {
                        Assert.AreEqual(0, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                }
            }
        }

        [Test]
        public void Horizontal5by5Raised()
        {
            HeightmapOwnerStub owner = new HeightmapOwnerStub();
            TerrainControllerLogic logic = new TerrainControllerLogic();
            logic.heightmapOwner = owner;

            owner.IntialiseHeightmap(5);

            XY from, to;

            from.x = 1;
            from.y = 2;
            to.x = 3;
            to.y = 2;

            logic.SetLineHeight(from, to, 1.5f, 2, 2);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (j == 0 || j == 4 || i == 0 || i == 4)
                    {
                        Assert.AreEqual(1, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                    else
                    {
                        Assert.AreEqual(2, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                }
            }
        }

        [Test]
        public void Horizontal5by5Ramp()
        {
            HeightmapOwnerStub owner = new HeightmapOwnerStub();
            TerrainControllerLogic logic = new TerrainControllerLogic();
            logic.heightmapOwner = owner;

            owner.IntialiseHeightmap(5);

            XY from, to;

            from.x = 1;
            from.y = 2;
            to.x = 3;
            to.y = 2;

            logic.SetLineHeight(from, to, 1.5f, 1, 3);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (j == 0 || j == 4 || i == 0 || i == 4 || j == 1)
                    {
                        Assert.AreEqual(1, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                    else if (j == 1)
                    {
                        Assert.AreEqual(2, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                    else if (j == 3)
                    {
                        Assert.AreEqual(3, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                }
            }
        }

        [Test]
        public void Horizontal5by5ReverseRamp()
        {
            HeightmapOwnerStub owner = new HeightmapOwnerStub();
            TerrainControllerLogic logic = new TerrainControllerLogic();
            logic.heightmapOwner = owner;

            owner.IntialiseHeightmap(5);

            XY from, to;

            from.x = 3;
            from.y = 2;
            to.x = 1;
            to.y = 2;

            logic.SetLineHeight(from, to, 1.5f, 1, 3);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (j == 0 || j == 4 || i == 0 || i == 4 || j == 3)
                    {
                        Assert.AreEqual(1, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                    else if (j == 1)
                    {
                        Assert.AreEqual(3, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                    else if (j == 2)
                    {
                        Assert.AreEqual(2, owner.m_heightmap[i, j], "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                }
            }
        }

        [Test]
        public void Vertical5by5ReverseRamp()
        {
            HeightmapOwnerStub owner = new HeightmapOwnerStub();
            TerrainControllerLogic logic = new TerrainControllerLogic();
            logic.heightmapOwner = owner;

            owner.IntialiseHeightmap(5);

            XY from, to;

            from.x = 2;
            from.y = 3;
            to.x = 2;
            to.y = 1;

            logic.SetLineHeight(from, to, 1.5f, 1, 3);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (j == 0 || j == 4 || i == 0 || i == 4 || i == 3)
                    {
                        Assert.That(owner.m_heightmap[i, j], Is.EqualTo(1).Within(0.01), "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                    else if (i == 1)
                    {
                        Assert.That(owner.m_heightmap[i, j], Is.EqualTo(3).Within(0.01), "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                    else if (i == 2)
                    {
                        Assert.That(owner.m_heightmap[i, j], Is.EqualTo(2).Within(0.01), "(" + i.ToString() + ", " + j.ToString() + ")");
                    }
                }
            }
        }

        [Test]
        public void Diagonal5by5RampTLBR()
        {
            HeightmapOwnerStub owner = new HeightmapOwnerStub();
            TerrainControllerLogic logic = new TerrainControllerLogic();
            logic.heightmapOwner = owner;

            owner.IntialiseHeightmap(5);

            XY from, to;

            from.x = 0;
            from.y = 0;
            to.x = 4;
            to.y = 4;

            logic.SetLineHeight(from, to, 1, 1, 3);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (i == j)
                    {
                        Assert.That(owner.m_heightmap[i, j], Is.EqualTo(1 + (i * 0.5f)).Within(0.01), "(" + i.ToString() + ", " + j.ToString() + ")");
                        //Debug.Log("["+i.ToString()+","+j.ToString()+"]: "+owner.m_heightmap[i,j].ToString());
                    }
                }
            }
        }

        [Test]
        public void Diagonal5by5RampBLTR()
        {
            HeightmapOwnerStub owner = new HeightmapOwnerStub();
            TerrainControllerLogic logic = new TerrainControllerLogic();
            logic.heightmapOwner = owner;

            owner.IntialiseHeightmap(5);

            XY from, to;

            from.x = 0;
            from.y = 4;
            to.x = 4;
            to.y = 0;

            logic.SetLineHeight(from, to, 0.75f, 1, 3);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (i == (4 - j))
                    {
                        Assert.That(owner.m_heightmap[i, j], Is.EqualTo(1 + (j * 0.5f)).Within(0.01), "[" + i.ToString() + ", " + j.ToString() + "]");
                        //Debug.Log("[" + i.ToString() + "," + j.ToString() + "]: " + owner.m_heightmap[i, j].ToString());
                    }
                }
            }
        }
    }

    internal class HeightmapOwnerStub : IHeightmapOwner
    {
        public float[,] m_heightmap;

        public void IntialiseHeightmap(int dimensions)
        {
            m_heightmap = new float[dimensions,dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                for (int j = 0; j < dimensions; j++)
                {
                    m_heightmap[i,j] = 1;
                }
            }
        }

        public float[,] GetHeightmap(int xbase, int ybase, int width, int height)
        {
            float[,] map = new float[height, width];
            for (int i = 0; i < height; i++)
            {
                if (i + ybase < m_heightmap.GetLength(0) && i+ybase >= 0)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (j + xbase < m_heightmap.GetLength(1) && j + xbase >= 0)
                            map[i, j] = m_heightmap[i + ybase, j + xbase];
                    }
                }
            }
            return map;
        }

        public void SetHeightmap(int xbase, int ybase, float[,] heightmap)
        {
            for (int i = 0; i < heightmap.GetLength(0); i++)
            {
                if ((i + ybase) < m_heightmap.GetLength(0) && (i + ybase) >= 0)
                {
                    for (int j = 0; j < heightmap.GetLength(1); j++)
                    {
                        if ((j + xbase) < m_heightmap.GetLength(1) && (j + xbase) >= 0)
                            m_heightmap[i + ybase, j + xbase] = heightmap[i, j];
                    }
                }
            }
        }
    }
}