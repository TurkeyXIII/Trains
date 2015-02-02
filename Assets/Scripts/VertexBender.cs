using UnityEngine;
using System.Collections;
using System;

public class VertexBender : MonoBehaviour, IMeshOwner
{
    private VertexBenderLogic c_bender;
    private Mesh m_mesh;

    private Vector3[] c_originalVerts;
    private Vector2[] c_originalUV;
    private int[] c_originalTriangles;


    void Awake()
    {
        c_bender = new VertexBenderLogic();
        c_bender.meshOwner = this;

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

    public float GetBentLength(Vector3 movableEndPosition, Vector3 targetPostion)
    {
        return c_bender.GetBentLength(movableEndPosition, targetPostion);
    }
}

public class VertexBenderLogic
{
    private const float c_errorMargin = 0.0001f;

    public IMeshOwner meshOwner { set; private get; }

    public float GetBentLength(Vector3 movableEndPosition, Vector3 targetPostion)
    {
        float scale, normalizedLength, thetaRadius;
        GetBendProperties(movableEndPosition, targetPostion, out scale, out normalizedLength, out thetaRadius);

        return normalizedLength * scale * 2;
    }

    public void GetBendProperties(Vector3 movableEndPosition, Vector3 targetPostion, out float scale, out float normalizedLength, out float thetaRadians)
    {
        thetaRadians = Mathf.Acos(Vector3.Dot(movableEndPosition, targetPostion) / (movableEndPosition.magnitude * targetPostion.magnitude));
        normalizedLength = Mathf.Sqrt(thetaRadians);

        Debug.Log("Normalized length: " + normalizedLength);

        float xL = FresnelC(normalizedLength);
        float yL = FresnelS(normalizedLength);

        float xA = xL * (1 + Mathf.Cos(thetaRadians * 2)) + yL * Mathf.Sin(thetaRadians * 2);

        Debug.Log("xL: " + xL + " yL: " + yL + " xA: " + xA);

        scale = (targetPostion.magnitude * Mathf.Cos(thetaRadians)) / xA;

        Debug.Log("Scale: " + scale);
    }

    public float Bend(Vector3 movableEndPosition, Vector3 targetPostion) //assumes the fixed point is the origin
    {
        float thetaRadians;
        float normalizedLength;
        float scale;

        GetBendProperties(movableEndPosition, targetPostion, out scale, out normalizedLength, out thetaRadians);

        // orthogonal unit vectors describing the x and y directions of a co-ordinate system where x is the original direction and y is the lateral movement
        Vector3 unitXdash = movableEndPosition / movableEndPosition.magnitude;
        Vector3 unitYdash = targetPostion - (targetPostion.magnitude * Mathf.Cos(thetaRadians) * unitXdash);
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

        for (int i = 0; i < verts.Length; i++)
        {
            float L = Vector3.Dot(verts[i], unitXdash) * normalizedLength * 2 / movableEndPosition.magnitude;

            if (L <= normalizedLength) // the first half, 'transition in' part of the curve
            {
                float xvdash = FresnelC(L) * scale;
                float yvdash = FresnelS(L) * scale;

                verts[i] = xvdash * unitXdash + yvdash * unitYdash;
            }
            else // the second half, 'transition out' part of the curve
            {
                float xvdoubledash = FresnelC((2 * normalizedLength) - L) * scale;
                float yvdoubledash = FresnelS((2 * normalizedLength) - L) * scale;

                verts[i] = targetPostion + xvdoubledash * unitXdoubledash + yvdoubledash * unitYdoubledash;
            }

        }

        meshOwner.SetMeshInfo(verts, uvs, tris);

        return normalizedLength * scale * 2;
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