// Author: Phillip Boyack 2015

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class VertexCropper : MonoBehaviour , IMeshOwner
{
    private VertexCropperLogic c_cropper;
    private Mesh m_mesh;
    private Vector3[] c_originalVerts;
    private Vector2[] c_originalUV;
    private int[] c_originalTriangles;

    void Awake()
    {
        c_cropper = new VertexCropperLogic();
        c_cropper.meshOwner = this;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        m_mesh = meshFilter.mesh;


        c_originalVerts = new Vector3[m_mesh.vertices.Length];
        c_originalUV = new Vector2[m_mesh.uv.Length];
        for (int i = 0; i < c_originalVerts.Length; i++)
        {
            c_originalVerts[i] = m_mesh.vertices[i];
            c_originalUV[i] = m_mesh.uv[i];
        }

        c_originalTriangles = new int[m_mesh.triangles.Length];
        for (int i = 0; i < c_originalTriangles.Length; i++)
        {
            c_originalTriangles[i] = m_mesh.triangles[i];
        }
    }

    public void Crop(Bounds bounds)
    {
        try
        {
            c_cropper.CropVerts(bounds);
        }
        catch (Exception e)
        {
            Debug.Log("Exception caught: " + e.Message);
        }
    }

    public void GetMeshInfo(out Vector3[] verts, out Vector2[] uv, out int[] triangles)
    {
        verts = c_originalVerts;
        uv = c_originalUV;
        triangles = c_originalTriangles;
    }

    public void SetMeshInfo(Vector3[] verts, Vector2[] uv, int[] triangles)
    {
        m_mesh.Clear();
        m_mesh.vertices = verts;
        m_mesh.uv = uv;
        m_mesh.triangles = triangles;
        m_mesh.RecalculateNormals();
    }

    public void Restore()
    {
        SetMeshInfo(c_originalVerts, c_originalUV, c_originalTriangles);
    }
}

public class VertexCropperLogic
{
    public IMeshOwner meshOwner { set; private get; }

    private static float c_fudgeFactor = 0.000001f; //used to compensate for floating point errors; should be very small.

    public void CropVerts(Bounds bounds)
    {
        Vector3[] verts;
        Vector2[] uv;
        int[] triangles;
        int i;

        meshOwner.GetMeshInfo(out verts, out uv, out triangles);

        if (verts.Length == 0) return;
                
        if (Mathf.Min(bounds.size.x, bounds.size.y, bounds.size.z) <= 0.01f)
        {
            verts = new Vector3[0];
            uv = new Vector2[0];
            triangles = new int[0];
            meshOwner.SetMeshInfo(verts, uv, triangles);
            return;
        }
        
        List<int>[] movedVertIndices = new List<int>[verts.Length];
        for (i = 0; i < verts.Length; i++)
        {
            movedVertIndices[i] = new List<int>();
        }

        Vector3[] newVerts = new Vector3[verts.Length * 2 - 1]; //maximum possible size of new vert array;
        Vector2[] newUVs = new Vector2[verts.Length * 2 - 1];
        int newVertsCount = 0;
        List<int> newTriangles = new List<int>();

        for (i = 0; i < triangles.Length; i += 3)
        {
            int nVertsInBounds = 0;
            bool[] vertsInBounds = new bool[3];
            for (int j = 0; j < 3; j++)
            {
                vertsInBounds[j] = bounds.Contains(verts[triangles[i + j]]);
                if (vertsInBounds[j]) nVertsInBounds++;
            }

            switch (nVertsInBounds)
            {
                case 0:
                    {
                        //figure out if and how the triangle intersects the bounds
                        
                        int intersectingLines = 0;
                        Vector3 firstVert = Vector3.zero;
                        Vector3 secondVert = Vector3.zero;
                        Vector2 firstUV = Vector2.zero;
                        Vector2 secondUV = Vector2.zero;
                        int firstVertIndex = -1;
                        bool[] hasBeenMoved = new bool[3];
                        for (int j = 0; j < 3; j++)
                        {
                            Vector3 testVert;
                            Vector2 testUV;
                            testVert = GetMovedVert(bounds, verts[triangles[i + (j + 1) % 3]], verts[triangles[i + j]], uv[triangles[i+ (j + 1) % 3]], uv[triangles[i+j]], out testUV);

                            if (bounds.Contains(testVert))
                            {
                                if (firstVertIndex == -1)
                                {
                                    firstVert = testVert;
                                    firstUV = testUV;
                                    firstVertIndex = triangles[i+j];
                                }

                                AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, testVert, movedVertIndices[triangles[i + j]], newUVs, testUV);

                                secondVert = GetMovedVert(bounds, verts[triangles[i + j]], verts[triangles[i + (j + 1) % 3]], uv[triangles[i+j]], uv[triangles[i + (j + 1) % 3]], out secondUV);

                                AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, secondVert, movedVertIndices[triangles[i + (j + 1) % 3]], newUVs, secondUV);

                                hasBeenMoved[j] = true;
                                hasBeenMoved[(j+1)%3] = true;

                                intersectingLines++;
                                if (intersectingLines == 2)
                                {
                                    AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, firstVert, movedVertIndices[firstVertIndex], newUVs, firstUV);
                                    AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, testVert, movedVertIndices[triangles[i + j]], newUVs, testUV);
                                }
                            }
                        }
                        
                        if (intersectingLines == 1)
                        {

                            Vector3 cornerVert = new Vector3(float.NaN, float.NaN, float.NaN);
                            Vector2 cornerUV = new Vector2();
                            Vector3 unmovedVert = Vector3.zero;
                            Vector2 unmovedUV = Vector2.zero;
                            int unmovedVertIndex = -1;
                            for (int j = 0; j < 3; j++)
                            {
                                if (!hasBeenMoved[j])
                                {
                                    unmovedVertIndex = triangles[i + j];
                                    unmovedUV = uv[unmovedVertIndex];
                                    unmovedVert = verts[unmovedVertIndex];
                                    break;
                                }
                                if (j == 2) throw new Exception("Error: no unmoved verts found");
                            }
                            float a = 0, b = 0;
                            Vector3 A = unmovedVert - firstVert;
                            Vector3 B = secondVert - firstVert;
                            Vector2 uvA = unmovedUV - firstUV;
                            Vector2 uvB = secondUV - firstUV;

                            //This block of if-elseifs assigns two of the three dimensions to the cornerVert based on where the intersecting line meets the bounds
                            float equation1Total = 0;
                            float A1, A2, B1, B2;
                            {
                                if (TrainsMath.AreApproximatelyEqual(firstVert.x, bounds.max.x) || TrainsMath.AreApproximatelyEqual(firstVert.x, bounds.min.x))
                                {
                                    cornerVert.x = firstVert.x;
                                    A2 = A.x;
                                    B2 = B.x;
                                }
                                else if (TrainsMath.AreApproximatelyEqual(firstVert.y, bounds.max.y) || TrainsMath.AreApproximatelyEqual(firstVert.y, bounds.min.y))
                                {
                                    cornerVert.y = firstVert.y;
                                    A2 = A.y;
                                    B2 = B.y;
                                }
                                else
                                {
                                    if (TrainsMath.AreApproximatelyEqual(firstVert.z, bounds.max.z) || TrainsMath.AreApproximatelyEqual(firstVert.z, bounds.min.z))
                                    {
                                        cornerVert.z = firstVert.z;
                                        A2 = A.y;
                                        B2 = B.y;
                                    }
                                    else
                                    {
                                        throw new Exception("Error: firstVert not on boundary");
                                    }
                                }

                                if (TrainsMath.AreApproximatelyEqual(secondVert.x, bounds.max.x) || TrainsMath.AreApproximatelyEqual(secondVert.x, bounds.min.x))
                                {
                                    cornerVert.x = secondVert.x;
                                    equation1Total = secondVert.x - firstVert.x;
                                    A1 = A.x;
                                    B1 = B.x;
                                }
                                else if (TrainsMath.AreApproximatelyEqual(secondVert.y, bounds.max.y) || TrainsMath.AreApproximatelyEqual(secondVert.y, bounds.min.y))
                                {
                                    cornerVert.y = secondVert.y;
                                    equation1Total = secondVert.y - firstVert.y;
                                    A1 = A.y;
                                    B1 = B.y;
                                }
                                else
                                {
                                    if (TrainsMath.AreApproximatelyEqual(secondVert.z, bounds.max.z) || TrainsMath.AreApproximatelyEqual(secondVert.z, bounds.min.z))
                                    {
                                        cornerVert.z = secondVert.z;
                                        equation1Total = secondVert.z - firstVert.z;
                                        A1 = A.y;
                                        B1 = B.y;
                                    }
                                    else
                                    {
                                        throw new Exception("Error: secondVert not on boundary");
                                    }
                                }
                            } // end if-elseif block

                            //this is determined by the simultaneous equations needed to make cornerVert = firstVert+a*A+b*B;
                            a = equation1Total / (A1 - (A2 * B1 / B2));
                            b = -a * A2 / B2;

                            cornerVert = firstVert + a * A + b * B;
                            cornerUV = firstUV + a * uvA + b * uvB;
                            
                            AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, cornerVert, movedVertIndices[unmovedVertIndex], newUVs, cornerUV);
                        }
                        break;
                    }
                case 1:
                    {
                        //move the other two verts towards the bounded one and keep the triangle
                        Vector3 vertInBounds = new Vector3();
                        Vector2 uvInBounds = new Vector2();
                        for (int j = 0; j < 3; j++)
                        {
                            if (vertsInBounds[j])
                            {
                                vertInBounds = verts[triangles[i+j]];
                                uvInBounds = uv[triangles[i+j]];
                                break;
                            }
                        }

                        for (int j = 0; j < 3; j++)
                        {
                            if (!vertsInBounds[j])
                            {
                                Vector3 newVert;
                                Vector3 oldVert = verts[triangles[i + j]];
                                Vector2 newUV;
                                Vector2 oldUV = uv[triangles[i+j]];

                                newVert = GetMovedVert(bounds, vertInBounds, oldVert, uvInBounds, oldUV, out newUV);

                                List<int> movedVertsInds = movedVertIndices[triangles[i + j]];

                                AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, newVert, movedVertsInds, newUVs, newUV);
                            }
                            else //(vertInBounds[j])
                            {
                                List<int> movedVertsInds = movedVertIndices[triangles[i + j]];

                                AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, vertInBounds, movedVertsInds, newUVs, uvInBounds);
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        //create two new verts closer to each of the inside verts
                        Vector3 vertOutsideBounds = new Vector3();
                        int vertOutsideBoundsIndex = 0;
                        for (int j = 0; j < 3; j++)
                        {
                            if (!vertsInBounds[j])
                            {
                                vertOutsideBounds = verts[triangles[i + j]];
                                vertOutsideBoundsIndex = j;
                                break;
                            }
                        }

                        {
                            int j = (vertOutsideBoundsIndex + 2) % 3;
                            Vector3 vert = verts[triangles[i+j]];
                            Vector2 uv1 = uv[triangles[i+j]];
                            Vector2 uv2;
                            List<int> movedVertInds = movedVertIndices[triangles[i+j]];
                            AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, vert, movedVertInds, newUVs, uv1);

                            Vector3 newVert = GetMovedVert(bounds, vert, vertOutsideBounds, uv1, uv[triangles[i + vertOutsideBoundsIndex]], out uv2);
                            movedVertInds = movedVertIndices[triangles[i + vertOutsideBoundsIndex]];
                            AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, newVert, movedVertInds, newUVs, uv2);

                            j = (j + 2) % 3;
                            vert = verts[triangles[i+j]];
                            uv1 = uv[triangles[i+j]];
                            movedVertInds = movedVertIndices[triangles[i + j]];
                            AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, vert, movedVertInds, newUVs, uv1);

                            AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, vert, movedVertInds, newUVs, uv1);

                            movedVertInds = movedVertIndices[triangles[i + vertOutsideBoundsIndex]];
                            AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, newVert, movedVertInds, newUVs, uv2);

                            newVert = GetMovedVert(bounds, vert, vertOutsideBounds, uv1, uv[triangles[i + vertOutsideBoundsIndex]], out uv2);
                            AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, newVert, movedVertInds, newUVs, uv2);
                        }
                        break;
                    }
                case 3:
                    {
                        //the triangle should stay as it is
                        for (int j = 0; j < 3; j++)
                        {
                            Vector3 vert = verts[triangles[i+j]];
                            List<int> movedVertsInds = movedVertIndices[triangles[i+j]];
                            AddVertToNewVerts(newVerts, ref newVertsCount, newTriangles, vert, movedVertsInds, newUVs, uv[triangles[i+j]]);
                        }
                        break;
                    }
            }
        }

        verts = new Vector3[newVertsCount];
        uv = new Vector2[newVertsCount];
        for (i = 0; i < newVertsCount; i++)
        {
            verts[i] = newVerts[i];
            uv[i] = newUVs[i];
        }
        triangles = new int[newTriangles.Count];
        i = 0;
        foreach (int index in newTriangles)
        {
            triangles[i] = index;
            i++;
        }
        meshOwner.SetMeshInfo(verts, uv, triangles);
    }

    public static Vector3 GetMovedVert(Bounds b, Vector3 towards, Vector3 oldVert)
    {
        Vector2 dummyuv = new Vector2();
        return GetMovedVert(b, towards, oldVert, dummyuv, dummyuv, out dummyuv);
    }

    public static Vector3 GetMovedVert(Bounds b, Vector3 towards, Vector3 oldVert, Vector2 towardsUV, Vector2 oldUV, out Vector2 newUV)
    {
        float alpha, beta, minAoverB = 1;

        beta = (towards.x - oldVert.x);
        if (beta != 0)
        {
            if (beta > 0)
            {
                alpha = (towards.x - b.min.x);
            }
            else
            {
                beta = -beta;
                alpha = (b.max.x - towards.x);
            }
            minAoverB = Mathf.Min(minAoverB, alpha / beta);
        }

        beta = (towards.y - oldVert.y);
        if (beta != 0)
        {
            if (beta > 0)
            {
                alpha = (towards.y - b.min.y);
            }
            else
            {
                beta = -beta;
                alpha = (b.max.y - towards.y);
            }
            minAoverB = Mathf.Min(minAoverB, alpha / beta);
        }

        beta = (towards.z - oldVert.z);
        if (beta != 0)
        {
            if (beta > 0)
            {
                alpha = (towards.z - b.min.z);
            }
            else
            {
                beta = -beta;
                alpha = (b.max.z - towards.z);
            }
            minAoverB = Mathf.Min(minAoverB, alpha / beta);
        }

        minAoverB = Mathf.Max(0, minAoverB);
        newUV = oldUV + (1 - minAoverB) * (towardsUV - oldUV);
        Vector3 newVert = oldVert + (1 - minAoverB) * (towards - oldVert);

        if (!b.Contains(newVert)) //we've got some floating point error keeping it just outside the box
        {
            if (b.Contains(oldVert))
            {
                //budge it closer to oldVert
                newVert += (oldVert - newVert) * c_fudgeFactor;
            }
            else
            {
                //budge it closer to towards
                newVert += (towards - newVert) * c_fudgeFactor;
            }
        }
        return newVert;
    }

    private static void AddVertToNewVerts(Vector3[] newVerts, ref int newVertsCount, List<int> newTriangles, Vector3 vert, List<int> movedVertsInds, Vector2[] newUVs, Vector2 newUV)
    {
        bool exists = false;
        foreach (int element in movedVertsInds)
        {
            if (newVerts[element] == vert)
            {
                exists = true;
                newTriangles.Add(element);
            }
        }
        if (exists == false)
        {
            movedVertsInds.Add(newVertsCount);
            newVerts[newVertsCount] = vert;
            newUVs[newVertsCount] = newUV;
            newTriangles.Add(newVertsCount);
            newVertsCount++;
        }
    }

}

public interface IMeshOwner
{
    void GetMeshInfo(out Vector3[] verts, out Vector2[] uv, out int[] triangles);
    void SetMeshInfo(Vector3[] verts, Vector2[] uv, int[] triangles);
}