using UnityEngine;
using System.Collections;
using System;

public struct XY
{
    public int x, y;
}

public interface ITerrainData
{
    int GetAlphamapHeight();
    int GetAlphamapWidth();
}

public interface IToolSelector
{
    Effect GetBrushSize();
}

public class TerrainController : MonoBehaviour, IHeightmapOwner, ITerrainData, IToolSelector
{    
	private TerrainData terrainData;
    private Brush brush;
    private TerrainControllerLogic logic;

    public int[] brushSizeRadii = {5, 10, 20};
    public int[] smoothingFactors = {5, 10, 20};
    public float heightAdjustmentSensitivity = 0.0003f;
    public float highlightOpacity = 0.5f;
    public float rockyThreshold = 2.0f;
    public float cliffThreshold = 5.0f;

    private XY startSquare;
    private XY endSquare;

    private bool[,] area;
    private XY areaCorner;
    private float[,,] areaAlphamap;

    private enum Tool
    {
        Level,
        Smooth
    }

    private Tool currentTool;

    void Awake()
    {
        terrainData = GetComponent<Terrain>().terrainData;

        brush = new Brush();
        brush.SetToolSelector(this);
        brush.SetTerrainData(this);
        brush.SetSizeRadii(brushSizeRadii);

        logic = new TerrainControllerLogic();
        logic.heightmapOwner = this;
    }

    void Update()
    {
        if (areaAlphamap != null) SetAlphaMap(areaCorner, false);
        areaAlphamap = null;
    }

    public void UpdateRaiseLower()
    {
        SetTerrainHighlights();
        if (area != null)
            RaiseLowerHeightMap();
    }

    public void UpdateLevel()
    {
        currentTool = Tool.Level;
        UpdateRectangle();
    }

    public void UpdateSmooth()
    {
        currentTool = Tool.Smooth;
        UpdateRectangle();
    }

    private void UpdateRectangle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseHit;
            if (CameraController.GetMouseHitLocation(out mouseHit))
            {
                WorldToTerrain(mouseHit, out startSquare);
            }
            else
            {
                startSquare.x = startSquare.y = -1;
            }
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseHit;
            if (CameraController.GetMouseHitLocation(out mouseHit))
            {
                if (WorldToTerrain(mouseHit, out endSquare))
                {
                    SetSquareArea();
                    SetAlphaMap(areaCorner, true);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            SetSquareArea();
            AdjustHeightMap();
        }
    }

    private void SetTerrainHighlights()
    {
        Vector3 mouseHit;

        if (CameraController.GetMouseHitLocation(out mouseHit))
        {
            XY brushLocation;
            if (WorldToTerrain(mouseHit, out brushLocation))
            {
                XY corner, dimension;

                brush.GetDetails(brushLocation, out corner, out dimension);
                area = brush.GetAffectedArea(brushLocation, corner, dimension);
                areaCorner = corner;

                SetAlphaMap(corner, true);
            }
        }

    }

    private void RaiseLowerHeightMap()
    {
        if (area == null) return;

        XY dimension;
        dimension.x = area.GetLength(1);
        dimension.y = area.GetLength(0);

        float[,] heightmap = terrainData.GetHeights(areaCorner.x, areaCorner.y, dimension.x, dimension.y);

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            for (int i = 0; i < dimension.y; i++)
            {
                for (int j = 0; j < dimension.x; j++)
                {
                    if (area[i, j])
                    {
                        if (Input.GetMouseButton(0))
                            heightmap[i, j] += heightAdjustmentSensitivity;

                        if (Input.GetMouseButton(1))
                            heightmap[i, j] -= heightAdjustmentSensitivity;
                    }
                }
            }
            terrainData.SetHeights(areaCorner.x, areaCorner.y, heightmap);
            OnHeightsChanged();
        }
    }

    private void AdjustHeightMap()
    {
        bool heightsChanged = false;
        if (area == null) return;

        XY dimension;
        dimension.x = area.GetLength(1);
        dimension.y = area.GetLength(0);

        float[,] heightmap = terrainData.GetHeights(areaCorner.x, areaCorner.y, dimension.x, dimension.y);

        
        if (currentTool == Tool.Level)
        {
            float targetHeight = terrainData.GetHeight(startSquare.x, startSquare.y) / terrainData.size.y;
            for (int i = 0; i < dimension.y; i++)
            {
                for (int j = 0; j < dimension.x; j++)
                {
                    if (area[i, j])
                    {
                        heightmap[i,j] = targetHeight;
                    }
                }
            }
            terrainData.SetHeights(areaCorner.x, areaCorner.y, heightmap);
            heightsChanged = true;
        }
        else if (currentTool == Tool.Smooth)
        {
            float[,] newHeightmap = new float[dimension.y, dimension.x];
            int averagingRadius = smoothingFactors[(int)Control.GetControl().GetToolSelector().GetEffect() - (int)Effect.Small];

            //there must be more efficient smoothing algorithms out there
            for (int i = 0; i < dimension.y; i++)
            {
                for (int j = 0; j < dimension.x; j++)
                {
                    float average = heightmap[i,j];
                    int nPoints = 1;
                    for (int y = i - averagingRadius; y <= i + averagingRadius; y++)
                    {
                        for (int x = j - averagingRadius; x <= j + averagingRadius; x++)
                        {
                            if (x >= 0 && x < dimension.x && y >= 0 && y < dimension.y)
                            {
                                if ((y - i) * (y - i) + (x - j) * (x - j) < averagingRadius * averagingRadius) //only average over a circle
                                {
                                    average += heightmap[y, x];
                                    nPoints++;
                                }
                            }
                        }
                    }

                    average /= nPoints;

                    //Only allow the terrain to deviate a lot if it has a lot of points to average
                    float deviability = (float)nPoints / ((float)((averagingRadius) * averagingRadius) * Mathf.PI); //so middle of the selection will be 1.0, corner will be 0.25

                    deviability = Mathf.Min(deviability * deviability, 1.0f);

                    newHeightmap[i,j] = (average * deviability) + (heightmap[i,j] * (1-deviability));
                }
            }

            terrainData.SetHeights(areaCorner.x, areaCorner.y, newHeightmap);
            heightsChanged = true;
        }
        
        
        if (heightsChanged)
        {
            OnHeightsChanged();
        }

    }

    private void OnHeightsChanged()
    {
        //always keep the terrain boundary at height zero
        float[,] heightmap;

        if (areaCorner.x == 0)
        {
            Debug.Log("Clamping terrain edge along x=0");
            heightmap = new float[area.GetLength(0), 1];
            terrainData.SetHeights(0, areaCorner.y, heightmap);
        }
        if (areaCorner.y == 0)
        {
            Debug.Log("Clamping terrain edge along y=0");
            heightmap = new float[1, area.GetLength(1)];
            terrainData.SetHeights(areaCorner.x, 0, heightmap);
        }

        if (areaCorner.x + area.GetLength(1) == terrainData.heightmapWidth - 1)
        {
            Debug.Log("Clamping terrain edge along x=" + (terrainData.heightmapWidth - 1).ToString());
            heightmap = new float[area.GetLength(0), 1];
            terrainData.SetHeights(terrainData.heightmapWidth - 1, areaCorner.y, heightmap);
        }
        if (areaCorner.y + area.GetLength(0) == terrainData.heightmapHeight - 1)
        {
            Debug.Log("Clamping terrain edge along y=" + (terrainData.heightmapHeight - 1).ToString());
            heightmap = new float[1, area.GetLength(1)];
            terrainData.SetHeights(areaCorner.x, terrainData.heightmapHeight - 1, heightmap);
        }

        //Set alpha maps according to steepness
        areaAlphamap = new float[area.GetLength(0), area.GetLength(1), terrainData.splatPrototypes.Length];
        for (int i = 0; i < area.GetLength(0); i++)
        {
            for (int j = 0; j < area.GetLength(1); j++)
            {

                float normalizedX = (float)(areaCorner.x + j) / (float)terrainData.alphamapWidth;
                float normalizedY = (float)(areaCorner.y + i) / (float)terrainData.alphamapHeight;
                float steepness = terrainData.GetSteepness(normalizedX, normalizedY);
                //Debug.Log("Steep: " + steepness.ToString() + ", X: " + (areaCorner.x + j).ToString() + ", Y: " + (areaCorner.y + i).ToString());
                if (steepness < rockyThreshold)
                {
                    areaAlphamap[i, j, 0] = 1f;
                }
                else if (steepness < cliffThreshold)
                {
                    areaAlphamap[i, j, 1] = 1f;
                }
                else
                {
                    areaAlphamap[i, j, 2] = 1f;
                }

            }
        }
        Control.GetControl().GetFileHandler().LevelHasChanged();
    }

    private bool WorldToTerrain(Vector3 world, out XY terrain)
    {
        terrain.x = -1;
        terrain.y = -1;
        //transform from real world co-ordinates to terrainData alphamap co-ords
        float localX = world.x - transform.position.x;
        float localZ = world.z - transform.position.z;

        if (localX < 0 || localZ < 0) return false;
        if (localX > terrainData.size.x) return false;
        if (localZ > terrainData.size.z) return false;

        terrain.x = Mathf.RoundToInt(localX / terrainData.size.x * terrainData.alphamapHeight);
        terrain.y = Mathf.RoundToInt(localZ / terrainData.size.z * terrainData.alphamapWidth);

        return true;
        
    }

    private bool WorldToTerrain(Vector3 world, out XY terrain, out float height)
    {
        if (!WorldToTerrain(world, out terrain))
        {
            height = float.NaN;
            return false;
        }

        height = (world.y - transform.position.y) / terrainData.size.y;

        return true;
    }

    private void SetSquareArea()
    {
        if (startSquare.x < 0)
        {
            area = null;
            return;
        }

        areaCorner.x = Mathf.Min(startSquare.x, endSquare.x);
        areaCorner.y = Mathf.Min(startSquare.y, endSquare.y);

        XY dimension;
        dimension.x = Mathf.Max(startSquare.x, endSquare.x) - areaCorner.x;
        dimension.y = Mathf.Max(startSquare.y, endSquare.y) - areaCorner.y;

        area = new bool[dimension.y, dimension.x];
        for (int i = 0; i < dimension.y; i++)
        {
            for (int j = 0; j < dimension.x; j++)
            {
                area[i,j] = true;
            }
        }

    }

    private void SetAlphaMap(XY corner, bool highlight)
    {
        if (area == null) return;

        if (!highlight)
        {
            terrainData.SetAlphamaps(areaCorner.x, areaCorner.y, areaAlphamap);
            area = null;
            areaAlphamap = null;
            return;
        }

        areaAlphamap = terrainData.GetAlphamaps(areaCorner.x, areaCorner.y, area.GetLength(1), area.GetLength(0));

        float[,,] map;
        int nTextures = terrainData.splatPrototypes.Length;

        //y -> i; x -> j
        map = new float[area.GetLength(0), area.GetLength(1), nTextures];

        for (int i = 0; i < area.GetLength(0); i++)
        {
            for (int j = 0; j < area.GetLength(1); j++)
            {
                if (area[i, j]) //only paint in the circle
                {
                    for (int t = 0; t < nTextures - 1; t++)
                    {
                        map[i, j, t] = areaAlphamap[i, j, t] * (1-highlightOpacity);
                    }
                    map[i, j, nTextures-1] = highlightOpacity;
                }
                else
                {
                    for (int t = 0; t < nTextures-1; t++)
                    {
                        map[i,j,t] = areaAlphamap[i,j,t];
                    }
                }
            }
        }

        terrainData.SetAlphamaps(corner.x, corner.y, map);
    }

    public void SetLineHeight(Vector3 from, Vector3 to, float width)
    {
        XY terrainFrom, terrainTo;
        float fromHeight, toHeight;
        if (!WorldToTerrain(from, out terrainFrom, out fromHeight) || !WorldToTerrain(to, out terrainTo, out toHeight))
        {
            throw new Exception("Error: setting line height outside terrain boundary");
        }
        float terrainWidth = ((float)terrainData.heightmapHeight / terrainData.size.z) * width; 

        logic.SetLineHeight(terrainFrom, terrainTo, terrainWidth, fromHeight, toHeight);
    }

    public float[,] GetHeightmap(int xbase, int ybase, int width, int height)
    {
        return terrainData.GetHeights(xbase, ybase, width, height);
    }

    public void SetHeightmap(int xbase, int ybase, float[,] heightmap)
    {
        terrainData.SetHeights(xbase, ybase, heightmap);

        areaCorner.x = xbase;
        areaCorner.y = ybase;
        area = new bool[heightmap.GetLength(0), heightmap.GetLength(1)];
        OnHeightsChanged();

    }

    public int GetAlphamapHeight()
    {
        return terrainData.alphamapHeight;
    }

    public int GetAlphamapWidth()
    {
        return terrainData.alphamapWidth;
    }

    public Effect GetBrushSize()
    {
        Effect effect = Control.GetControl().GetToolSelector().GetEffect();
        return (Effect)((int)effect - (int)Effect.Small); // Effect starts with 'none'; 'Small' is the first brush size
    }
}

public class TerrainControllerLogic
{
    public IHeightmapOwner heightmapOwner { set; private get; }


    public void SetLineHeight(XY terrainFrom, XY terrainTo, float terrainWidth, float fromHeight, float toHeight)
    {
        XY corner, lineDimension, lineArea;
        Vector2 delta;

        terrainWidth -= 0.5f;

        lineDimension.x = terrainFrom.x - terrainTo.x;
        if (lineDimension.x < 0) lineDimension.x = -lineDimension.x;
        lineDimension.y = terrainFrom.y - terrainTo.y;
        if (lineDimension.y < 0) lineDimension.y = -lineDimension.y;


        if (lineDimension.y == 0)
        {
            corner.x = Mathf.Min(terrainFrom.x, terrainTo.x);
            corner.y = terrainFrom.y - Mathf.RoundToInt(terrainWidth);

            lineArea.x = lineDimension.x + 1;
            lineArea.y = Mathf.RoundToInt(terrainWidth * 2) + 1;

            delta.x = 0;
            delta.y = terrainWidth;
        }
        else
        {
            delta.x = terrainWidth / Mathf.Sqrt(1 + Mathf.Pow(lineDimension.x, 2) / Mathf.Pow(lineDimension.y, 2));
            delta.y = (float)(delta.x * lineDimension.x) / (float)lineDimension.y;
            corner.x = Mathf.Min(terrainFrom.x, terrainTo.x) - Mathf.RoundToInt(delta.x);
            corner.y = Mathf.Min(terrainFrom.y, terrainTo.y) - Mathf.RoundToInt(delta.y);

            if (corner.x < 0) corner.x = 0;
            if (corner.y < 0) corner.y = 0;

            lineArea.x = lineDimension.x + Mathf.RoundToInt(delta.x * 2) + 1;
            lineArea.y = lineDimension.y + Mathf.RoundToInt(delta.y * 2) + 1;

        }

        float[,] heightmap = heightmapOwner.GetHeightmap(corner.x, corner.y, lineArea.x, lineArea.y);

        float totalLength = Mathf.Sqrt(Mathf.Pow(terrainFrom.x - terrainTo.x, 2) + Mathf.Pow(terrainFrom.y - terrainTo.y, 2));

        bool positiveSlope = (terrainFrom.x - terrainTo.x) * (terrainFrom.y - terrainTo.y) > 0;

        for (int i = 0; i < heightmap.GetLength(0); i++)
        {
            int jstart, jend;

            float x1, x2;
            if (positiveSlope)
            {
                x1 = -(delta.x / delta.y) * i + 2 * delta.x;
                x2 = ((float)lineDimension.x / (float)lineDimension.y) * (i - 2 * delta.y);

                jstart = Mathf.Max(Mathf.RoundToInt(x1), Mathf.RoundToInt(x2), 0);

                x1 = -(delta.x / delta.y) * (i - lineDimension.y) + 2 * delta.x + lineDimension.x;
                x2 = ((float)lineDimension.x / (float)lineDimension.y) * i + 2 * delta.x;
            }
            else //negative or zero slope
            {
                x1 = (delta.x / delta.y) * (i - lineDimension.y);
                x2 = -((float)lineDimension.x / (float)lineDimension.y) * i + lineDimension.x;

                jstart = Mathf.Max(Mathf.RoundToInt(x1), Mathf.RoundToInt(x2), 0);

                x1 = (delta.x / delta.y) * i + lineDimension.x;
                x2 = -((float)lineDimension.x / (float)lineDimension.y) * (i - 2 * delta.y) + lineDimension.x + 2 * delta.x;

                //check for infinities and NaNs, which don't RoundToInt properly
#pragma warning disable
                if (x1 > lineArea.x || x1 != x1) x1 = lineArea.x-1;
                if (x2 > lineArea.x || x2 != x2) x2 = lineArea.x-1;
#pragma warning enable
            }

            jend = Mathf.Min(Mathf.RoundToInt(x1), Mathf.RoundToInt(x2), heightmap.GetLength(1) - 1);

            for (int j = jstart; j <= jend; j++)
            {
                float targetHeight, dist;

                Vector2 pointFromFrom = new Vector2((terrainFrom.x - corner.x) - j, (terrainFrom.y - corner.y) - i);

                if (positiveSlope)
                    dist = pointFromFrom.magnitude * Mathf.Cos(Mathf.Atan(pointFromFrom.y / pointFromFrom.x) - Mathf.Atan((float)lineDimension.y / (float)lineDimension.x));
                else
                    dist = pointFromFrom.magnitude * Mathf.Cos(Mathf.Atan(pointFromFrom.y / pointFromFrom.x) + Mathf.Atan((float)lineDimension.y / (float)lineDimension.x));

                if (pointFromFrom.magnitude <= 0.5f) dist = 0;

                if (dist < 0) dist = -dist;

                targetHeight = (dist / totalLength) * (toHeight - fromHeight) + fromHeight;
                heightmap[i, j] = targetHeight;
            }
        }
        heightmapOwner.SetHeightmap(corner.x, corner.y, heightmap);
    }
}

public class Brush
{
    private XY m_location;
    private IToolSelector toolSelector;
    private ITerrainData terrainData;
    private int[] brushSizeRadii;

    public void SetTerrainData(ITerrainData td)
    {
        terrainData = td;
    }

    public void SetToolSelector(IToolSelector ts)
    {
        toolSelector = ts;
    }

    public void SetSizeRadii(int[] bsr)
    {
        brushSizeRadii = bsr;
    }

    public bool[,] GetAffectedArea(XY brushLocation, XY corner, XY dimension)
    {
        XY localCenter;
        localCenter.x = brushLocation.x - corner.x;
        localCenter.y = brushLocation.y - corner.y;

        int radius = GetRadius();

        bool[,] area = new bool[dimension.y, dimension.x];

        for (int i = 0; i < dimension.y; i++)
        {
            for (int j = 0; j < dimension.x; j++)
            {
                if ((i - localCenter.y) * (i - localCenter.y) + (j - localCenter.x) * (j - localCenter.x) < radius * radius) //only paint in the circle
                {
                    area[i, j] = true;
                }
            }
        }

        return area;
    }

    public void GetDetails(XY brushLocation, out XY corner, out XY dimension)
    {
        XY offset;
        int radius = GetRadius();

        offset.x = 0;
        offset.y = 0;
        dimension.x = radius * 2;
        dimension.y = radius * 2;

        //limit the size of the map if the point is near the terrain edge
        if (brushLocation.x < radius)
        {
            offset.x = radius - brushLocation.x;
            dimension.x = radius * 2 - offset.x;
        }
        else if (brushLocation.x > terrainData.GetAlphamapHeight() - radius)
        {
            dimension.x = radius * 2 + terrainData.GetAlphamapHeight() - radius - brushLocation.x;
        }
        if (brushLocation.y < radius)
        {
            offset.y = radius - brushLocation.y;
            dimension.y = radius * 2 - offset.y;
        }
        else if (brushLocation.y > terrainData.GetAlphamapWidth() - radius)
        {
            dimension.y = radius * 2 + terrainData.GetAlphamapWidth() - radius - brushLocation.y;
        }
        corner.x = brushLocation.x - radius + offset.x;
        corner.y = brushLocation.y - radius + offset.y;
    }

    private int GetRadius()
    {
        return brushSizeRadii[(int)toolSelector.GetBrushSize()];
    }
}

public interface IHeightmapOwner
{
    float[,] GetHeightmap(int xbase, int ybase, int width, int height);
    void SetHeightmap(int xbase, int ybase, float[,] heightmap);
}