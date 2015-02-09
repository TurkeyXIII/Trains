using UnityEngine;
using System.Collections;
using System;

public class VertexBender : MonoBehaviour, IMeshOwner
{
    private VertexBenderLogic c_bender;
    private Mesh m_mesh;

    void Awake()
    {
        c_bender = new VertexBenderLogic();
        c_bender.meshOwner = this;

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

        Vector3[] newVerts = new Vector3[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vert = verts[i] - fixedPosition;

            float L = Vector3.Dot(vert, unitXdash) * normalizedLength * 2 / movableEndPosition.magnitude;

            Vector3 vy = vert - Vector3.Dot(vert, unitXdash) * unitXdash;
            if (L <= normalizedLength) // the first half, 'transition in' part of the curve
            {
                float xvdash = FresnelC(L) * scale;
                float yvdash = FresnelS(L) * scale;

                float thetaRadiansAtVert = Mathf.Pow(L, 2);
                Vector3 lineOffset = Mathf.Cos(thetaRadiansAtVert) * vy + Mathf.Sin(thetaRadiansAtVert) * Vector3.Cross(rotationAxis, vy) + (1 - Mathf.Cos(thetaRadiansAtVert)) * Vector3.Dot(rotationAxis, vy) * rotationAxis;
                newVerts[i] = xvdash * unitXdash + yvdash * unitYdash + lineOffset + fixedPosition;
            }
            else // the second half, 'transition out' part of the curve
            {
                float xvdoubledash = FresnelC((2 * normalizedLength) - L) * scale;
                float yvdoubledash = FresnelS((2 * normalizedLength) - L) * scale;

                float thetaRadiansAtVert = thetaRadians * 2 - Mathf.Pow(2 * normalizedLength - L, 2);
                Vector3 lineOffset = Mathf.Cos(thetaRadiansAtVert) * vy + Mathf.Sin(thetaRadiansAtVert) * Vector3.Cross(rotationAxis, vy) + (1 - Mathf.Cos(thetaRadiansAtVert)) * Vector3.Dot(rotationAxis, vy) * rotationAxis;

                newVerts[i] = targetPosition + xvdoubledash * unitXdoubledash + yvdoubledash * unitYdoubledash + lineOffset + fixedPosition;

            }

        }

        meshOwner.SetMeshInfo(newVerts, uvs, tris);

        return normalizedLength * scale * 2;
    }

    public float Bend(Vector3 movableEndPosition, Vector3 targetPosition) //assumes the fixed point is the origin
    {
        return Bend(Vector3.zero, movableEndPosition, targetPosition);
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