using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class VertexBender : MonoBehaviour, IMeshOwner
{
    private VertexBenderLogic c_bender;
    private Mesh m_mesh;

    public float maxCornerAngleDegrees = 5;

    void Awake()
    {
        c_bender = new VertexBenderLogic();
        c_bender.meshOwner = this;
        c_bender.maxCornerAngleRadians = maxCornerAngleDegrees * Mathf.Rad2Deg;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        m_mesh = meshFilter.mesh;
    }

    public void Bend(Vector3 movableEndPosition, Vector3 targetPostion)
    {
        c_bender.Bend(movableEndPosition, targetPostion);
    }

    public void Bend(Vector3 fixedPosition, Vector3 movableEndPosition, Vector3 targetPostion)
    {
        c_bender.Bend(fixedPosition, movableEndPosition, targetPostion);
    }

    public void GetMeshInfo(out Vector3[] verts, out Vector2[] uv, out int[] triangles)
    {
        verts = m_mesh.vertices;
        uv = m_mesh.uv;
        triangles = m_mesh.triangles;
    }

    public void SetMeshInfo(Vector3[] verts, Vector2[] uv, int[] triangles)
    {
        m_mesh.Clear();

        m_mesh.vertices = verts;
        m_mesh.uv = uv;
        m_mesh.triangles = triangles;

        m_mesh.RecalculateNormals();
    }

    public static void GetBentLengthAndRotation(Vector3 movableEndPosition, Vector3 targetPostion, out float length, out Vector3 rotationAxis, out float angle)
    {
        length = VertexBenderLogic.GetBentLength(movableEndPosition, targetPostion);
        angle = Mathf.Acos(Vector3.Dot(movableEndPosition, targetPostion) / (movableEndPosition.magnitude * targetPostion.magnitude)) * 2 * Mathf.Rad2Deg;
        rotationAxis = Vector3.Cross(movableEndPosition, targetPostion).normalized;
    }
}

public class VertexBenderLogic
{
    private const float c_errorMargin = 0.0001f;

    public IMeshOwner meshOwner { set; private get; }

    public float maxCornerAngleRadians { set; private get; }

    public VertexBenderLogic()
    {
        maxCornerAngleRadians = Mathf.PI/6;
    }

    public static float GetBentLength(Vector3 movableEndPosition, Vector3 targetPostion)
    {
        float scale, normalizedLength, thetaRadius;
        GetBendProperties(movableEndPosition, targetPostion, out scale, out normalizedLength, out thetaRadius);

        return normalizedLength * scale * 2;
    }

    private static void GetBendProperties(Vector3 movableEndPosition, Vector3 targetPostion, out float scale, out float normalizedLength, out float thetaRadians)
    {
        thetaRadians = Mathf.Acos(Vector3.Dot(movableEndPosition, targetPostion) / (movableEndPosition.magnitude * targetPostion.magnitude));
        normalizedLength = Mathf.Sqrt(thetaRadians);

        if (normalizedLength == 0)
        {
            normalizedLength = 0.5f;
            scale = targetPostion.magnitude;
            return;
        }

        float xL = FresnelC(normalizedLength);
        float yL = FresnelS(normalizedLength);

        float xA = xL * (1 + Mathf.Cos(thetaRadians * 2)) + yL * Mathf.Sin(thetaRadians * 2);

        scale = (targetPostion.magnitude * Mathf.Cos(thetaRadians)) / xA;

        if (xA == 0)
        {
            scale = targetPostion.magnitude / (2 * yL);
        }

    }

    public float Bend(Vector3 fixedPosition, Vector3 movableEndPosition, Vector3 targetPosition)
    {
        float thetaRadians;
        float normalizedLength;
        float scale;
        int i;

        movableEndPosition -= fixedPosition;
        targetPosition -= fixedPosition;

        GetBendProperties(movableEndPosition, targetPosition, out scale, out normalizedLength, out thetaRadians);
        
        if (thetaRadians == 0)
        {
            return scale * normalizedLength * 2;
        }
        /*
        Debug.Log("NormalizedLength: " + normalizedLength);
        Debug.Log("Fixed end:" + fixedPosition);
        Debug.Log("movableEnd" + movableEndPosition);
        Debug.Log("Target" + targetPosition);
        Debug.Log("Scale: " + scale);
        */
        // orthogonal unit vectors describing the x and y directions of a co-ordinate system where x is the original direction and y is the lateral movement
        Vector3 unitXdash = movableEndPosition / movableEndPosition.magnitude;
        Vector3 unitYdash = targetPosition - (targetPosition.magnitude * Mathf.Cos(thetaRadians) * unitXdash);
        unitYdash /= unitYdash.magnitude;

        // orthogonal unit vectors describing x and y directions of co-ordinate system where x is direction away from target point along straigh line and y is lateral movement to origin
        Vector3 rotationAxis = Vector3.Cross(unitXdash, unitYdash);
        // Rodrigues' rotation forumla
        Vector3 unitXdoubledash = -(Mathf.Cos(2 * thetaRadians) * unitXdash +
                                Mathf.Sin(2 * thetaRadians) * Vector3.Cross(rotationAxis, unitXdash) +
                                (1 - Mathf.Cos(2 * thetaRadians)) * Vector3.Dot(rotationAxis, unitXdash) * rotationAxis); //rotate 2 * theta;
        Vector3 unitYdoubledash = Mathf.Cos(2 * thetaRadians) * unitYdash +
                                Mathf.Sin(2 * thetaRadians) * Vector3.Cross(rotationAxis, unitYdash) +
                                (1 - Mathf.Cos(2 * thetaRadians)) * Vector3.Dot(rotationAxis, unitYdash) * rotationAxis; //rotate 2 * theta

        Vector3[] verts;
        Vector2[] uvs;
        int[] tris;

        meshOwner.GetMeshInfo(out verts, out uvs, out tris);

        //create new triangles where it's necessary to fit them onto the curve

        //these array sizes are an arbitrary guess at how big they might need to be
        Vector3[] newVerts = new Vector3[verts.Length * 10 / 3];
        int[] newTris = new int[tris.Length * 9];
        float[] Ls = new float[verts.Length * 10 / 3];
        int nNewTris = tris.Length;
        int nNewVerts = verts.Length;

        
        for (i = 0; i < verts.Length; i++)
        {
            Vector3 newVert = verts[i] - fixedPosition;
            newVerts[i] = newVert;
            Ls[i] = Vector3.Dot(newVert, unitXdash) * normalizedLength * 2 / movableEndPosition.magnitude;
        }
        
        for (int t = 0; t < tris.Length; t++)
        {
            newTris[t] = tris[t];
        }
        

        //do this for each crease
        bool[] greaterThanCrease = new bool[3];

        float crease = normalizedLength;
        int trisLength = tris.Length; //store this because trisLength is likely to change but we only want to look at the tris that exist before this crease
        
        for (int t = 0; t < trisLength; t += 3)
        {
            bool anyGreaterThanCrease = false;
            bool anyLessThanCrease = false;
            for (int u = 0; u < 3; u++)
            {
                greaterThanCrease[u] = Ls[tris[t + u]] > crease;
                if (greaterThanCrease[u]) anyGreaterThanCrease = true;
                else anyLessThanCrease = true;
            }

            
            if (anyGreaterThanCrease && anyLessThanCrease) //this triangle crosses the crease; 
            {
                int indexIsolatedVert = 0, indexPairedVert1 = 0, indexPairedVert2 = 0;
                for (int u = 0; u < 3; u++)
                {
                    indexIsolatedVert = u;
                    indexPairedVert1 = (u+1)%3;
                    indexPairedVert2 = (u+2)%3;

                    if ((greaterThanCrease[indexPairedVert1] && greaterThanCrease[indexPairedVert2]) || (!greaterThanCrease[indexPairedVert1] && !greaterThanCrease[indexPairedVert2]))
                        break;
                }
                
                //we acquire two new verts by moving the paired verts towards the isolated one such that L == crease;
                float a = (crease - Ls[tris[t+indexPairedVert1]]) / (Ls[tris[t+indexIsolatedVert]] - Ls[tris[t+indexPairedVert1]]);
                Vector3 newVert1 = verts[tris[t+indexPairedVert1]] + a * (verts[tris[t+indexIsolatedVert]] - verts[tris[t+indexPairedVert1]]);
                Debug.Log(a + " from " + verts[tris[t + indexPairedVert1]] + " to " + verts[tris[t + indexIsolatedVert]] + ": " + newVert1);
                a = (crease - Ls[tris[t + indexPairedVert2]]) / (Ls[tris[t + indexIsolatedVert]] - Ls[tris[t + indexPairedVert2]]);
                Vector3 newVert2 = verts[tris[t + indexPairedVert2]] + a * (verts[tris[t + indexIsolatedVert]] - verts[tris[t + indexPairedVert2]]);
                Debug.Log(a + " from " + verts[tris[t + indexPairedVert2]] + " to " + verts[tris[t + indexIsolatedVert]] + ": " + newVert2);

                //insert the two new verts
                int indexNewVert1;
                int indexNewVert2;

                indexNewVert1 = FindVertInArray(newVert1, newVerts, nNewVerts);
                Debug.Log("Finding " + newVert1 + " in array: " + indexNewVert1);
                if (indexNewVert1 == -1)
                {
                    Debug.Log("Creating new vert at: " + nNewVerts);
                    newVerts[nNewVerts] = newVert1;
                    Ls[nNewVerts] = crease; // by definition
                    indexNewVert1 = nNewVerts;
                    nNewVerts++;
                }

                indexNewVert2 = FindVertInArray(newVert2, newVerts, nNewVerts);
                Debug.Log("Finding " + newVert2 + " in array: " + indexNewVert2);
                if (indexNewVert2 == -1)
                {
                    Debug.Log("Creating new vert at: " + nNewVerts);
                    newVerts[nNewVerts] = newVert2;
                    Ls[nNewVerts] = crease;
                    indexNewVert2 = nNewVerts;
                    nNewVerts++;
                }

                //reassign the old triangle to use the new verts
                newTris[t+indexPairedVert1] = indexNewVert1;
                newTris[t+indexPairedVert2] = indexNewVert2;
                //create two new triangles to fill the gap
                newTris[nNewTris] = indexNewVert2;
                newTris[nNewTris+1] = indexNewVert1;
                newTris[nNewTris+2] = t+indexPairedVert1;
                newTris[nNewTris+3] = indexNewVert2;
                newTris[nNewTris+4] = t+indexPairedVert1;
                newTris[nNewTris+5] = t+indexPairedVert2;

                nNewTris += 6;
            }

        }
        

        tris = new int[nNewTris];
        for (i = 0; i < nNewTris; i++)
        {
            tris[i] = newTris[i];
        }

        //bend the verts with the curve
        verts = new Vector3[nNewVerts];

//        Debug.Log("nNewVerts: " + nNewVerts + " nNewTris: " + nNewTris);

        
        for (i = 0; i < nNewVerts; i++)
        {
            Vector3 vy = newVerts[i] - Vector3.Dot(newVerts[i], unitXdash) * unitXdash;
            if (Ls[i] <= normalizedLength) // the first half, 'transition in' part of the curve
            {
                float xvdash = FresnelC(Ls[i]) * scale;
                float yvdash = FresnelS(Ls[i]) * scale;

                float thetaRadiansAtVert = Mathf.Pow(Ls[i], 2);
                Vector3 lineOffset = Mathf.Cos(thetaRadiansAtVert) * vy + Mathf.Sin(thetaRadiansAtVert) * Vector3.Cross(rotationAxis, vy) + (1 - Mathf.Cos(thetaRadiansAtVert)) * Vector3.Dot(rotationAxis, vy) * rotationAxis;
                verts[i] = xvdash * unitXdash + yvdash * unitYdash + lineOffset + fixedPosition;
            }
            else // the second half, 'transition out' part of the curve
            {
                float xvdoubledash = FresnelC((2 * normalizedLength) - Ls[i]) * scale;
                float yvdoubledash = FresnelS((2 * normalizedLength) - Ls[i]) * scale;

                float thetaRadiansAtVert = thetaRadians * 2 - Mathf.Pow(2 * normalizedLength - Ls[i], 2);
                Vector3 lineOffset = Mathf.Cos(thetaRadiansAtVert) * vy + Mathf.Sin(thetaRadiansAtVert) * Vector3.Cross(rotationAxis, vy) + (1 - Mathf.Cos(thetaRadiansAtVert)) * Vector3.Dot(rotationAxis, vy) * rotationAxis;

                verts[i] = targetPosition + xvdoubledash * unitXdoubledash + yvdoubledash * unitYdoubledash + lineOffset + fixedPosition;

            }

//            Debug.Log("Moving vert " + i + " from " + newVerts[i] + " to " + verts[i]);
        }
        
        meshOwner.SetMeshInfo(verts, uvs, tris);

        return normalizedLength * scale * 2;
    }

    public float Bend(Vector3 movableEndPosition, Vector3 targetPosition) //assumes the fixed point is the origin
    {
        return Bend(Vector3.zero, movableEndPosition, targetPosition);
    }

    private static int FindVertInArray(Vector3 vert, Vector3[] vertArray, int arraySize)
    {
        for (int i = 0; i < arraySize; i++)
        {
            if (VertexCropperLogic.AreApproximatelyEqual(vert.x, vertArray[i].x, 0.0001f) &&
                VertexCropperLogic.AreApproximatelyEqual(vert.y, vertArray[i].y, 0.0001f) &&
                VertexCropperLogic.AreApproximatelyEqual(vert.z, vertArray[i].z, 0.0001f))
                return i;
        }
        return -1;
    }

    public static float FresnelS(float x)
    {
        float lastTerm = float.PositiveInfinity;
        float FresnelS = 0;
        int n = 0;

        while (lastTerm / FresnelS > c_errorMargin || lastTerm / FresnelS < -c_errorMargin)
        {
            lastTerm = Mathf.Pow(x, 4 * n + 3) / Factorial(2 * n + 1) / (float)(4 * n + 3);

            if (n % 2 == 1) lastTerm = -lastTerm;

            FresnelS += lastTerm;
            n++;

            if (n > 20)
            {
                return -1;
            }
        }

        return FresnelS;
    }

    public static float FresnelC(float x)
    {
        float lastTerm = float.PositiveInfinity;
        float FresnelC = 0;
        int n = 0;

        while (lastTerm / FresnelC > c_errorMargin || lastTerm / FresnelC < -c_errorMargin)
        {
            lastTerm = Mathf.Pow(x, 4 * n + 1) / Factorial(2 * n) / (float)(4 * n + 1);

            if (n % 2 == 1) lastTerm = -lastTerm;

            FresnelC += lastTerm;
            n++;

            if (n > 20)
            {
                return -1;
            }
        }
        return FresnelC;
    }

    public static float Factorial(int x)
    {
        float factorial = 1;
        for (float i = 2; i < x+1; i++)
        {
            factorial *= i;
        }

        return factorial;
    }
}