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
        c_bender.maxCornerAngleRadians = maxCornerAngleDegrees * Mathf.Deg2Rad;

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

    public void Bend(float L1, float L2, float A1, float A2, float theta1, float theta2,
                    Vector3 fixedPosition, 
                    Vector3 movableEndPosition, 
                    Vector3 targetPostion, 
                    Vector3 targetDirection, 
                    ref Vector3[] creasePositions)
    {
        c_bender.Bend(L1, L2, A1, A2, theta1, theta2,
                    fixedPosition, 
                    movableEndPosition, 
                    targetPostion, 
                    targetDirection, 
                    ref creasePositions);
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
        angle = Mathf.Acos(Vector3.Dot(movableEndPosition, targetPostion) / (movableEndPosition.magnitude * targetPostion.magnitude));
        rotationAxis = Vector3.Cross(movableEndPosition, targetPostion).normalized;
    }
}

public class VertexBenderLogic
{
    

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
//        Debug.Log("thetaRadians: " + thetaRadians);
        if (thetaRadians > Mathf.PI / 2)
        {
            scale = 1;
            normalizedLength = -1;
            return;
        }

        normalizedLength = Mathf.Sqrt(thetaRadians);

        if (normalizedLength == 0)
        {
            normalizedLength = 0.5f;
            scale = targetPostion.magnitude;
            return;
        }

        float xL = FresnelMath.FresnelC(normalizedLength);
        float yL = FresnelMath.FresnelS(normalizedLength);

        float xA = xL * (1 + Mathf.Cos(thetaRadians * 2)) + yL * Mathf.Sin(thetaRadians * 2);

        scale = (targetPostion.magnitude * Mathf.Cos(thetaRadians)) / xA;

        if (xA == 0)
        {
            scale = targetPostion.magnitude / (2 * yL);
        }

    }

    public static float GetLengthOfCompoundCurve(Vector3 startPosition, Vector3 targetPosition, Vector3 startDirection, Vector3 targetDirection)
    {
        float A1, L1, A2, L2;
        float theta1, theta2;
        Debug.Log("startPos: " + startPosition + ", endPos: " + targetPosition + ", startDir: " + startDirection + ", endDir: " + targetDirection);

        FresnelMath.FindTheta(out theta1, out theta2, startPosition, targetPosition, startDirection, -targetDirection);
        if (theta1 < 0) return -1;

        float x = Vector3.Dot((targetPosition - startPosition), startDirection.normalized);

        float phi = theta1 + theta2;

        A2 = FresnelMath.A2(theta1, phi, x);
        A1 = FresnelMath.A1(A2, theta1, phi);

        L1 = Mathf.Sqrt(theta1);
        L2 = Mathf.Sqrt(theta2);

        return L1/A1 + L2/A2;
    }

    //this has the precondition that the start direction (movableEndPosition - fixedPosition) is positive x.
    public void Bend(float L1, float L2, float A1, float A2, float theta1, float theta2,
                    Vector3 fixedPosition, 
                    Vector3 movableEndPosition, 
                    Vector3 targetPosition, 
                    Vector3 targetDirection, 
                    ref Vector3[] creasePositions)
    {
        movableEndPosition -= fixedPosition;
        targetPosition -= fixedPosition;
        if (creasePositions == null) creasePositions = new Vector3[0];
        for (int i = 0; i < creasePositions.Length; i++)
        {
            creasePositions[i] -= fixedPosition;
        }

//        Debug.Log("TargetPosition: " + targetPosition + ", TargetDirection: " + targetDirection);

        float phi = theta1 + theta2;

        // orthogonal unit vectors describing the x and y directions of a co-ordinate system where x is the original direction and y is the lateral movement
        Vector3 unitXdash = new Vector3(1,0,0);
        Vector3 unitYdash = targetPosition - unitXdash * targetPosition.x;
        unitYdash.Normalize();

        Vector3 rotationAxis;
        Vector3 unitXdoubledash;
        Vector3 unitYdoubledash;
        UnitVectorsFromRotation(phi, unitXdash, unitYdash, out rotationAxis, out unitXdoubledash, out unitYdoubledash);

        Vector3[] verts;
        Vector2[] uvs;
        int[] tris;

        meshOwner.GetMeshInfo(out verts, out uvs, out tris);

        //create new triangles where it's necessary to fit them onto the curve

        //these array sizes are an arbitrary guess at how big they might need to be
        Vector3[] newVerts = new Vector3[verts.Length * 100];
        Vector3[] normals = new Vector3[verts.Length * 100];
        Vector2[] newUVs = new Vector2[verts.Length * 100];
        int[] newTris = new int[tris.Length * 270];
        float[] Ls = new float[verts.Length * 100];
        int nNewTris = tris.Length;
        int nNewVerts = verts.Length;

        //    Debug.Log("starting tris: " + tris.Length + ", starting verts: " + verts.Length);
        //    Debug.Log("Maximum tris: " + newTris.Length + ", maximum verts: " + newVerts.Length);

        // set up normals
        for (int t = 0; t < tris.Length; t += 3)
        {
            Vector3 normal = Vector3.Cross(verts[tris[t + 2]] - verts[tris[t + 1]], verts[tris[t]] - verts[tris[t + 1]]);
            for (int u = 0; u < 3; u++)
            {
                normals[tris[t + u]] += normal;
            }
        }

        float L1Real = L1 / A1;
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 newVert = verts[i] - fixedPosition;
            newVerts[i] = newVert;
            float positionAlongLength = Vector3.Dot(newVert, unitXdash);
            
            if (positionAlongLength <= L1 / A1) //we are in L1 territory
            {
                Ls[i] = positionAlongLength * A1;
            }
            else // L2 territory
            {
                Ls[i] = L1 + ((positionAlongLength - L1Real) * A2);
            }
            newUVs[i] = uvs[i];

            normals[i].Normalize();
            //         Debug.Log("Vert " + i + ": " + newVert);
        }

        for (int t = 0; t < tris.Length; t++)
        {
            newTris[t] = tris[t];
        }

        #region creases
        // Set up creases: creases should be applied as evenly as possible

        // each crease is in terms of normalised length. This list does not need to be ordered; in fact it is better to have the middle one first, then the n/4th and 3n/4th, and so-on
        float[] creases = new float[creasePositions.Length];
        bool[] done = new bool[creasePositions.Length];
        float[] creaseLs = new float[creasePositions.Length]; // this is the same as creases, but in order so creaseLs[i] corresponds to creasePositions[i]

        for (int i = 0; i < done.Length; i++)
        {
            done[i] = false;
        }
        int n = 0;

        recursiveCreases(ref creases, ref done, creasePositions.Length/2, ref n, creasePositions, L1, A1, A2, creasePositions.Length/2, ref creaseLs);

        
        //do this for each crease
        bool[] greaterThanCrease = new bool[3];
        foreach (float crease in creases)
        {

            int trisLength = nNewTris; //store this because nNewTris is likely to change but we only want to look at the tris that exist before this crease

            for (int t = 0; t < trisLength; t += 3)
            {
                bool anyGreaterThanCrease = false;
                bool anyLessThanCrease = false;
                for (int u = 0; u < 3; u++)
                {
                    greaterThanCrease[u] = Ls[newTris[t + u]] > crease;
                    if (greaterThanCrease[u]) anyGreaterThanCrease = true;
                    else anyLessThanCrease = true;
                }

                if (anyGreaterThanCrease && anyLessThanCrease) //this triangle crosses the crease; 
                {
                    //         Debug.Log("Triangle creasing: " + newVerts[tris[t]] + ", " + newVerts[tris[t + 1]] + ", " + newVerts[tris[t + 2]]);

                    int indexIsolatedVert = 0, indexPairedVert1 = 0, indexPairedVert2 = 0;
                    for (int u = 0; u < 3; u++)
                    {
                        indexIsolatedVert = u;
                        indexPairedVert1 = (u + 1) % 3;
                        indexPairedVert2 = (u + 2) % 3;

                        if ((greaterThanCrease[indexPairedVert1] && greaterThanCrease[indexPairedVert2]) || (!greaterThanCrease[indexPairedVert1] && !greaterThanCrease[indexPairedVert2]))
                            break;
                    }

                    //            Debug.Log("isolatedIndex: " + indexIsolatedVert + " paired1index: " + indexPairedVert1 + " paired2index: " + indexPairedVert2);

                    //we acquire two new verts by moving the paired verts towards the isolated one such that L == crease;
                    Vector3 normal1, normal2;

                    float a = (crease - Ls[newTris[t + indexPairedVert1]]) / (Ls[newTris[t + indexIsolatedVert]] - Ls[newTris[t + indexPairedVert1]]);
                    Vector3 newVert1 = newVerts[newTris[t + indexPairedVert1]] + a * (newVerts[newTris[t + indexIsolatedVert]] - newVerts[newTris[t + indexPairedVert1]]);
                    Vector2 newUV1 = newUVs[newTris[t + indexPairedVert1]] + a * (newUVs[newTris[t + indexIsolatedVert]] - newUVs[newTris[t + indexPairedVert1]]);
                    normal1 = ((1 - a) * normals[newTris[t + indexPairedVert1]] + a * (normals[newTris[indexIsolatedVert]])).normalized;

                    a = (crease - Ls[newTris[t + indexPairedVert2]]) / (Ls[newTris[t + indexIsolatedVert]] - Ls[newTris[t + indexPairedVert2]]);
                    Vector3 newVert2 = newVerts[newTris[t + indexPairedVert2]] + a * (newVerts[newTris[t + indexIsolatedVert]] - newVerts[newTris[t + indexPairedVert2]]);
                    Vector2 newUV2 = newUVs[newTris[t + indexPairedVert2]] + a * (newUVs[newTris[t + indexIsolatedVert]] - newUVs[newTris[t + indexPairedVert2]]);
                    normal2 = ((1 - a) * normals[newTris[t + indexPairedVert2]] + a * (normals[newTris[indexIsolatedVert]])).normalized;

                    //insert the two new verts
                    int indexNewVert1;
                    int indexNewVert2;

                    indexNewVert1 = FindVertInArray(newVert1, normal1, newVerts, normals, nNewVerts);
                    if (indexNewVert1 == -1)
                    {
                        newVerts[nNewVerts] = newVert1;
                        newUVs[nNewVerts] = newUV1;
                        Ls[nNewVerts] = crease; // by definition
                        normals[nNewVerts] = normal1;
                        indexNewVert1 = nNewVerts;
                        nNewVerts++;

                        //             Debug.Log("Requiring new vert at " + newVert1);
                    }
                    //             else
                    //               Debug.Log("Using existing vert at " + newVert1);

                    indexNewVert2 = FindVertInArray(newVert2, normal2, newVerts, normals, nNewVerts);
                    if (indexNewVert2 == -1)
                    {
                        newVerts[nNewVerts] = newVert2;
                        newUVs[nNewVerts] = newUV2;
                        Ls[nNewVerts] = crease;
                        normals[nNewVerts] = normal2;
                        indexNewVert2 = nNewVerts;
                        nNewVerts++;

                        //                Debug.Log("Requiring new vert at " + newVert2);
                    }
                    //             else
                    //                  Debug.Log("Using existing vert at " + newVert2);


                    //create two new triangles to fill the gap
                    newTris[nNewTris] = indexNewVert2;
                    newTris[nNewTris + 1] = indexNewVert1;
                    newTris[nNewTris + 2] = newTris[t + indexPairedVert2];
                    newTris[nNewTris + 3] = indexNewVert1;
                    newTris[nNewTris + 4] = newTris[t + indexPairedVert1];
                    newTris[nNewTris + 5] = newTris[t + indexPairedVert2];

                    //reassign the old triangle to use the new verts
                    newTris[t + indexPairedVert1] = indexNewVert1;
                    newTris[t + indexPairedVert2] = indexNewVert2;

                    //                Debug.Log("Adding 2 new triangles at " + nNewTris + ", " + (nNewTris+1) + ", " + (nNewTris+2) + ", " + (nNewTris+3) + ", " + (nNewTris+4) + ", " + (nNewTris+5) + ".");
                    //                Debug.Log(newVerts[indexNewVert2] + ", " + newVerts[indexNewVert1] + ", " + newVerts[t+indexPairedVert1] + ", " + newVerts[indexNewVert2] + ", " + newVerts[t+indexPairedVert1] + ", " + newVerts[t+indexPairedVert2]);

                    nNewTris += 6;
                }

            }
        }
#endregion

        tris = new int[nNewTris];
        for (int i = 0; i < nNewTris; i++)
        {
            tris[i] = newTris[i];
        }

        uvs = new Vector2[nNewVerts];
        for (int i = 0; i < nNewVerts; i++)
        {
            uvs[i] = newUVs[i];
        }

        //bend the verts with the curve
        verts = new Vector3[nNewVerts];

        for (int i = 0; i < nNewVerts; i++)
        {
            Vector3 vy = newVerts[i] - Vector3.Dot(newVerts[i], unitXdash) * unitXdash;
            if (Ls[i] <= L1) // the first half, 'transition in' part of the curve
            {
                float xvdash = FresnelMath.FresnelC(Ls[i]) / A1;
                float yvdash = FresnelMath.FresnelS(Ls[i]) / A1;

                float thetaRadiansAtVert = Mathf.Pow(Ls[i], 2);
                Vector3 lineOffset = Mathf.Cos(thetaRadiansAtVert) * vy + Mathf.Sin(thetaRadiansAtVert) * Vector3.Cross(rotationAxis, vy) + (1 - Mathf.Cos(thetaRadiansAtVert)) * Vector3.Dot(rotationAxis, vy) * rotationAxis;
                verts[i] = xvdash * unitXdash + yvdash * unitYdash + lineOffset + fixedPosition;
            }
            else // the second half, 'transition out' part of the curve
            {
                float xvdoubledash = FresnelMath.FresnelC((L1+L2) - Ls[i]) / A2;
                float yvdoubledash = FresnelMath.FresnelS((L1+L2) - Ls[i]) / A2;

                float thetaRadiansAtVert = phi - Mathf.Pow(L1 + L2 - Ls[i], 2);
                Vector3 lineOffset = Mathf.Cos(thetaRadiansAtVert) * vy + Mathf.Sin(thetaRadiansAtVert) * Vector3.Cross(rotationAxis, vy) + (1 - Mathf.Cos(thetaRadiansAtVert)) * Vector3.Dot(rotationAxis, vy) * rotationAxis;

                verts[i] = targetPosition + xvdoubledash * unitXdoubledash + yvdoubledash * unitYdoubledash + lineOffset + fixedPosition;

            }

            //            Debug.Log("Moving vert " + i + " from " + newVerts[i] + " to " + verts[i]);
        }

        meshOwner.SetMeshInfo(verts, uvs, tris);

        for (int i = 0; i < creasePositions.Length; i++)
        {
            if (creaseLs[i] <= L1) // the first half, 'transition in' part of the curve
            {
                float xvdash = FresnelMath.FresnelC(creaseLs[i]) / A1;
                float yvdash = FresnelMath.FresnelS(creaseLs[i]) / A1;

                creasePositions[i] = xvdash * unitXdash + yvdash * unitYdash;
            }
            else // the second half, 'transition out' part of the curve
            {
                float xvdoubledash = FresnelMath.FresnelC((L1 + L2) - creaseLs[i]) / A2;
                float yvdoubledash = FresnelMath.FresnelS((L1 + L2) - creaseLs[i]) / A2;

                creasePositions[i] = targetPosition + xvdoubledash * unitXdoubledash + yvdoubledash * unitYdoubledash;
            }
        }
    }

    internal static void UnitVectorsFromRotation(float phi, Vector3 unitXdash, Vector3 unitYdash, out Vector3 unitXdoubledash, out Vector3 unitYdoubledash)
    {
        Vector3 dummyAxis;
        UnitVectorsFromRotation(phi, unitXdash, unitYdash, out dummyAxis, out unitXdoubledash, out unitYdoubledash);
    }

    internal static void UnitVectorsFromRotation(float phi, Vector3 unitXdash, Vector3 unitYdash, out Vector3 rotationAxis, out Vector3 unitXdoubledash, out Vector3 unitYdoubledash)
    {
        // orthogonal unit vectors describing x and y directions of co-ordinate system where x is direction away from target point along straigh line and y is lateral movement to origin
        rotationAxis = Vector3.Cross(unitXdash, unitYdash);
        // Rodrigues' rotation forumla
        unitXdoubledash = -(Mathf.Cos(phi) * unitXdash +
                                Mathf.Sin(phi) * Vector3.Cross(rotationAxis, unitXdash) +
                                (1 - Mathf.Cos(phi)) * Vector3.Dot(rotationAxis, unitXdash) * rotationAxis); //rotate phi
        unitYdoubledash = Mathf.Cos(phi) * unitYdash +
                Mathf.Sin(phi) * Vector3.Cross(rotationAxis, unitYdash) +
                (1 - Mathf.Cos(phi)) * Vector3.Dot(rotationAxis, unitYdash) * rotationAxis; //rotate phi
    }

    private void recursiveCreases(ref float[] creases, ref bool[] done, int i, ref int n, Vector3[] creasePositions, float L1, float A1, float A2, int lastDifference, ref float[] creaseLs)
    {
        if (creaseLs.Length <= i) return;
        if (creasePositions[i].x <= L1 / A1) //we are in L1 territory
        {
            creaseLs[i] = creasePositions[i].x * A1;
        }
        else // L2 territory
        {
            creaseLs[i] = L1 + ((creasePositions[i].x - L1/A1) * A2);
        }

        // Debug.Log("CreasePosition: " + creasePositions[i] + ", creaseL: " + creaseLs[i]);

        creases[n] = creaseLs[i];         
        done[i] = true;
        n++;

        int difference = lastDifference / 2;
        if (difference == 0) difference = 1;

        int newI = i - difference;
        if (newI >= 0 && !done[newI])
            recursiveCreases(ref creases, ref done, newI, ref n, creasePositions, L1, A1, A2, difference, ref creaseLs);
        newI = i + difference;
        if (newI < done.Length && !done[newI])
            recursiveCreases(ref creases, ref done, newI, ref n, creasePositions, L1, A1, A2, difference, ref creaseLs);
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

        Debug.Log("ThetaRadians(3arg): " + thetaRadians);

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
        Vector3[] newVerts = new Vector3[verts.Length * 100];
        Vector3[] normals = new Vector3[verts.Length * 100];
        Vector2[] newUVs = new Vector2[verts.Length * 100];
        int[] newTris = new int[tris.Length * 270];
        float[] Ls = new float[verts.Length * 100];
        int nNewTris = tris.Length;
        int nNewVerts = verts.Length;

        //    Debug.Log("starting tris: " + tris.Length + ", starting verts: " + verts.Length);
        //    Debug.Log("Maximum tris: " + newTris.Length + ", maximum verts: " + newVerts.Length);

        // set up normals
        for (int t = 0; t < tris.Length; t += 3)
        {
            Vector3 normal = Vector3.Cross(verts[tris[t + 2]] - verts[tris[t + 1]], verts[tris[t]] - verts[tris[t + 1]]);
            for (int u = 0; u < 3; u++)
            {
                normals[tris[t + u]] += normal;
            }
        }

        for (i = 0; i < verts.Length; i++)
        {
            Vector3 newVert = verts[i] - fixedPosition;
            newVerts[i] = newVert;
            Ls[i] = Vector3.Dot(newVert, unitXdash) * normalizedLength * 2 / movableEndPosition.magnitude;
            newUVs[i] = uvs[i];

            normals[i].Normalize();
            //         Debug.Log("Vert " + i + ": " + newVert);
        }

        for (int t = 0; t < tris.Length; t++)
        {
            newTris[t] = tris[t];
        }



        //debug logging
        /*
        for (int t = 0; t < tris.Length; t += 3)
        {
            Debug.Log("Triangle " + tris[t] + ", " + tris[t+1] + ", " + tris[t+2]);
        }
        */

        // Set up creases: creases should be applied as evenly as possible
        List<float> creases = new List<float>();
        creases.Add(normalizedLength);
        float minAngle = normalizedLength * normalizedLength;

        while (minAngle > maxCornerAngleRadians)
        {
            minAngle = minAngle / 2;
            float angle = minAngle;
            while (angle < normalizedLength * normalizedLength)
            {
                float crease = Mathf.Sqrt(angle);
                creases.Add(crease);
                creases.Add(normalizedLength * 2 - crease);
                angle += angle * 2;
            }
        }

        //do this for each crease


        bool[] greaterThanCrease = new bool[3];


        foreach (float crease in creases)
        {

            int trisLength = nNewTris; //store this because nNewTris is likely to change but we only want to look at the tris that exist before this crease

            for (int t = 0; t < trisLength; t += 3)
            {
                bool anyGreaterThanCrease = false;
                bool anyLessThanCrease = false;
                for (int u = 0; u < 3; u++)
                {
                    greaterThanCrease[u] = Ls[newTris[t + u]] > crease;
                    if (greaterThanCrease[u]) anyGreaterThanCrease = true;
                    else anyLessThanCrease = true;
                }

                if (anyGreaterThanCrease && anyLessThanCrease) //this triangle crosses the crease; 
                {
                    //         Debug.Log("Triangle creasing: " + newVerts[tris[t]] + ", " + newVerts[tris[t + 1]] + ", " + newVerts[tris[t + 2]]);

                    int indexIsolatedVert = 0, indexPairedVert1 = 0, indexPairedVert2 = 0;
                    for (int u = 0; u < 3; u++)
                    {
                        indexIsolatedVert = u;
                        indexPairedVert1 = (u + 1) % 3;
                        indexPairedVert2 = (u + 2) % 3;

                        if ((greaterThanCrease[indexPairedVert1] && greaterThanCrease[indexPairedVert2]) || (!greaterThanCrease[indexPairedVert1] && !greaterThanCrease[indexPairedVert2]))
                            break;
                    }

                    //            Debug.Log("isolatedIndex: " + indexIsolatedVert + " paired1index: " + indexPairedVert1 + " paired2index: " + indexPairedVert2);

                    //we acquire two new verts by moving the paired verts towards the isolated one such that L == crease;
                    Vector3 normal1, normal2;

                    float a = (crease - Ls[newTris[t + indexPairedVert1]]) / (Ls[newTris[t + indexIsolatedVert]] - Ls[newTris[t + indexPairedVert1]]);
                    Vector3 newVert1 = newVerts[newTris[t + indexPairedVert1]] + a * (newVerts[newTris[t + indexIsolatedVert]] - newVerts[newTris[t + indexPairedVert1]]);
                    Vector2 newUV1 = newUVs[newTris[t + indexPairedVert1]] + a * (newUVs[newTris[t + indexIsolatedVert]] - newUVs[newTris[t + indexPairedVert1]]);
                    normal1 = ((1 - a) * normals[newTris[t + indexPairedVert1]] + a * (normals[newTris[indexIsolatedVert]])).normalized;

                    a = (crease - Ls[newTris[t + indexPairedVert2]]) / (Ls[newTris[t + indexIsolatedVert]] - Ls[newTris[t + indexPairedVert2]]);
                    Vector3 newVert2 = newVerts[newTris[t + indexPairedVert2]] + a * (newVerts[newTris[t + indexIsolatedVert]] - newVerts[newTris[t + indexPairedVert2]]);
                    Vector2 newUV2 = newUVs[newTris[t + indexPairedVert2]] + a * (newUVs[newTris[t + indexIsolatedVert]] - newUVs[newTris[t + indexPairedVert2]]);
                    normal2 = ((1 - a) * normals[newTris[t + indexPairedVert2]] + a * (normals[newTris[indexIsolatedVert]])).normalized;

                    //insert the two new verts
                    int indexNewVert1;
                    int indexNewVert2;

                    indexNewVert1 = FindVertInArray(newVert1, normal1, newVerts, normals, nNewVerts);
                    if (indexNewVert1 == -1)
                    {
                        newVerts[nNewVerts] = newVert1;
                        newUVs[nNewVerts] = newUV1;
                        Ls[nNewVerts] = crease; // by definition
                        normals[nNewVerts] = normal1;
                        indexNewVert1 = nNewVerts;
                        nNewVerts++;

                        //             Debug.Log("Requiring new vert at " + newVert1);
                    }
                    //             else
                    //               Debug.Log("Using existing vert at " + newVert1);

                    indexNewVert2 = FindVertInArray(newVert2, normal2, newVerts, normals, nNewVerts);
                    if (indexNewVert2 == -1)
                    {
                        newVerts[nNewVerts] = newVert2;
                        newUVs[nNewVerts] = newUV2;
                        Ls[nNewVerts] = crease;
                        normals[nNewVerts] = normal2;
                        indexNewVert2 = nNewVerts;
                        nNewVerts++;

                        //                Debug.Log("Requiring new vert at " + newVert2);
                    }
                    //             else
                    //                  Debug.Log("Using existing vert at " + newVert2);


                    //create two new triangles to fill the gap
                    newTris[nNewTris] = indexNewVert2;
                    newTris[nNewTris + 1] = indexNewVert1;
                    newTris[nNewTris + 2] = newTris[t + indexPairedVert2];
                    newTris[nNewTris + 3] = indexNewVert1;
                    newTris[nNewTris + 4] = newTris[t + indexPairedVert1];
                    newTris[nNewTris + 5] = newTris[t + indexPairedVert2];

                    //reassign the old triangle to use the new verts
                    newTris[t + indexPairedVert1] = indexNewVert1;
                    newTris[t + indexPairedVert2] = indexNewVert2;

                    //                Debug.Log("Adding 2 new triangles at " + nNewTris + ", " + (nNewTris+1) + ", " + (nNewTris+2) + ", " + (nNewTris+3) + ", " + (nNewTris+4) + ", " + (nNewTris+5) + ".");
                    //                Debug.Log(newVerts[indexNewVert2] + ", " + newVerts[indexNewVert1] + ", " + newVerts[t+indexPairedVert1] + ", " + newVerts[indexNewVert2] + ", " + newVerts[t+indexPairedVert1] + ", " + newVerts[t+indexPairedVert2]);

                    nNewTris += 6;
                }

            }
        }

        tris = new int[nNewTris];
        for (i = 0; i < nNewTris; i++)
        {
            tris[i] = newTris[i];
        }

        uvs = new Vector2[nNewVerts];
        for (i = 0; i < nNewVerts; i++)
        {
            uvs[i] = newUVs[i];
        }
        /*
        Debug.Log("nNewVerts: " + nNewVerts + " nNewTris: " + nNewTris);

        for (i = 0; i < nNewVerts; i++)
        {
            Debug.Log("newVert " + i + ": " + newVerts[i]);
        }

        //debug logging
        for (int t = 0; t < tris.Length; t += 3)
        {
            Debug.Log("Triangle " + t +"-"+(t+1)+"-"+(t+2)+ ": " + tris[t] + ", " + tris[t + 1] + ", " + tris[t + 2]);
        }
        */
        //bend the verts with the curve
        verts = new Vector3[nNewVerts];

        for (i = 0; i < nNewVerts; i++)
        {
            Vector3 vy = newVerts[i] - Vector3.Dot(newVerts[i], unitXdash) * unitXdash;
            if (Ls[i] <= normalizedLength) // the first half, 'transition in' part of the curve
            {
                float xvdash = FresnelMath.FresnelC(Ls[i]) * scale;
                float yvdash = FresnelMath.FresnelS(Ls[i]) * scale;

                float thetaRadiansAtVert = Mathf.Pow(Ls[i], 2);
                Vector3 lineOffset = Mathf.Cos(thetaRadiansAtVert) * vy + Mathf.Sin(thetaRadiansAtVert) * Vector3.Cross(rotationAxis, vy) + (1 - Mathf.Cos(thetaRadiansAtVert)) * Vector3.Dot(rotationAxis, vy) * rotationAxis;
                verts[i] = xvdash * unitXdash + yvdash * unitYdash + lineOffset + fixedPosition;
            }
            else // the second half, 'transition out' part of the curve
            {
                float xvdoubledash = FresnelMath.FresnelC((2 * normalizedLength) - Ls[i]) * scale;
                float yvdoubledash = FresnelMath.FresnelS((2 * normalizedLength) - Ls[i]) * scale;

                float thetaRadiansAtVert = thetaRadians * 2 - Mathf.Pow(2 * normalizedLength - Ls[i], 2);
                Vector3 lineOffset = Mathf.Cos(thetaRadiansAtVert) * vy + Mathf.Sin(thetaRadiansAtVert) * Vector3.Cross(rotationAxis, vy) + (1 - Mathf.Cos(thetaRadiansAtVert)) * Vector3.Dot(rotationAxis, vy) * rotationAxis;

                verts[i] = targetPosition + xvdoubledash * unitXdoubledash + yvdoubledash * unitYdoubledash + lineOffset + fixedPosition;

            }

            //            Debug.Log("Moving vert " + i + " from " + newVerts[i] + " to " + verts[i]);
        }
        /*
        Debug.Log("finsished tris: " + tris.Length + ", finshed verts: " + verts.Length);
        for (i = 0; i < tris.Length; i++)
        {
            if (tris[i] < 0 || tris[i] >= verts.Length)
            {
                Debug.Log("Invalid tri found: " + tris[i] + " in tri " + i);
            }
        }
        */
        meshOwner.SetMeshInfo(verts, uvs, tris);


        return normalizedLength * scale * 2;
    }

    public float Bend(Vector3 movableEndPosition, Vector3 targetPosition) //assumes the fixed point is the origin
    {
        return Bend(Vector3.zero, movableEndPosition, targetPosition);
    }

    public static void BendVectors(Vector3[] originals, out Vector3[] bent, Vector3 movableEndPosition, Vector3 targetPosition)
    {
        bent = new Vector3[originals.Length];

        float scale, normalizedLength, thetaRadians;

        GetBendProperties(movableEndPosition, targetPosition, out scale, out normalizedLength, out thetaRadians);

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



        for (int i = 0; i < originals.Length; i++)
        {
            Vector3 vy = originals[i] - Vector3.Dot(originals[i], unitXdash) * unitXdash;

            float L = Vector3.Dot(originals[i], unitXdash) * normalizedLength * 2 / movableEndPosition.magnitude;

            if (L <= normalizedLength) // the first half, 'transition in' part of the curve
            {
                float xvdash = FresnelMath.FresnelC(L) * scale;
                float yvdash = FresnelMath.FresnelS(L) * scale;

                float thetaRadiansAtVert = Mathf.Pow(L, 2);
                Vector3 lineOffset = Mathf.Cos(thetaRadiansAtVert) * vy + Mathf.Sin(thetaRadiansAtVert) * Vector3.Cross(rotationAxis, vy) + (1 - Mathf.Cos(thetaRadiansAtVert)) * Vector3.Dot(rotationAxis, vy) * rotationAxis;
                bent[i] = xvdash * unitXdash + yvdash * unitYdash + lineOffset;
            }
            else // the second half, 'transition out' part of the curve
            {
                float xvdoubledash = FresnelMath.FresnelC((2 * normalizedLength) - L) * scale;
                float yvdoubledash = FresnelMath.FresnelS((2 * normalizedLength) - L) * scale;

                float thetaRadiansAtVert = thetaRadians * 2 - Mathf.Pow(2 * normalizedLength - L, 2);
                Vector3 lineOffset = Mathf.Cos(thetaRadiansAtVert) * vy + Mathf.Sin(thetaRadiansAtVert) * Vector3.Cross(rotationAxis, vy) + (1 - Mathf.Cos(thetaRadiansAtVert)) * Vector3.Dot(rotationAxis, vy) * rotationAxis;

                bent[i] = targetPosition + xvdoubledash * unitXdoubledash + yvdoubledash * unitYdoubledash + lineOffset;

            }

            //            Debug.Log("Moving vert " + i + " from " + newVerts[i] + " to " + verts[i]);
        }

    }

    private static int FindVertInArray(Vector3 vert, Vector3 normal, Vector3[] vertArray, Vector3[] normalArray, int arraySize)
    {
        for (int i = 0; i < arraySize; i++)
        {
            if (TrainsMath.AreApproximatelyEqual(vert, vertArray[i], 0.0001f) &&
                TrainsMath.AreApproximatelyEqual(normal, normalArray[i], 0.0001f))
                return i;
        }
        return -1;
    }

}