﻿using UnityEngine;
using System.Collections;

public static class TrainsMath {

    private const float c_fudgeFactor = 0.000001f;

    public static bool AreApproximatelyEqual(float a, float b, float margin = -1)
    {
        if (margin < 0) margin = c_fudgeFactor;

        float diff = a - b;
        if (diff < margin && diff > -margin) return true;

        return false;
    }

    public static bool AreApproximatelyEqual(Vector3 a, Vector3 b, float margin = -1)
    {
        if (margin < 0) margin = c_fudgeFactor;

        if (AreApproximatelyEqual(a.x, b.x, margin) &&
            AreApproximatelyEqual(a.y, b.y, margin) &&
            AreApproximatelyEqual(a.z, b.z, margin))
            return true;

        return false;
    }

    public static Vector3 RotateVector(Vector3 vector, Vector3 axis, float angleRadians)
    {
        // Rodrigues' rotation forumla
        axis.Normalize();
        Vector3 vRot = vector * Mathf.Cos(angleRadians) 
            + Vector3.Cross(axis, vector) * Mathf.Sin(angleRadians)
            + axis * Vector3.Dot(axis, vector) * (1 - Mathf.Cos(angleRadians));
        return vRot;
    }
}


public static class FresnelMath
{
    private const float c_errorMargin = 0.000001f;

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
        for (float i = 2; i < x + 1; i++)
        {
            factorial *= i;
        }

        return factorial;
    }

    public static float A2(float theta1, float phi, float x)
    {
        return ( FresnelC(Mathf.Sqrt(theta1)) / Mathf.Sqrt(phi/theta1 - 1) + FresnelC(Mathf.Sqrt(phi - theta1)) * Mathf.Cos(phi) + FresnelS(Mathf.Sqrt(phi - theta1)) * Mathf.Sin(phi) ) / x;
    }

    public static float A1(float A2, float theta1, float phi)
    {
        return A2 * Mathf.Sqrt((phi-theta1) / theta1);
    }

    public static float DeltaA(float theta1, float phi, float x, float y)
    {
        float equation1;
        float equation2;
//        Debug.Log("phi = " + phi + ", theta1 = " + theta1);
        float rootTheta = Mathf.Sqrt(theta1);
        float rootPhiMinusTheta = Mathf.Sqrt(phi - theta1);
        float rootPhiOnThetaMinus1 = rootPhiMinusTheta / rootTheta;

        equation1 = (FresnelC(rootTheta) / rootPhiOnThetaMinus1 + FresnelC(rootPhiMinusTheta) * Mathf.Cos(phi) + FresnelS(rootPhiMinusTheta) * Mathf.Sin(phi)) / x;
//        Debug.Log("Equation1 = " + equation1);

        equation2 = (FresnelS(rootTheta) / rootPhiOnThetaMinus1 + FresnelC(rootPhiMinusTheta) * Mathf.Sin(phi) - FresnelS(rootPhiMinusTheta) * Mathf.Cos(phi)) / y;

//        Debug.Log("DeltaA = " + (equation1 - equation2));

        return equation1 - equation2;
    }

    private static float DeltaADerivative(float theta1, float phi, float x, float y)
    {
        float f = Mathf.Sqrt(theta1);
        float fd = 1 / (2 * f);
        float g = FresnelC(f);
        float gd = Mathf.Cos(theta1) / (2 * f);
        float h = Mathf.Sqrt(phi - theta1);
        float hd = -1 / (2 * h);      

        float ad1 = (((fd * g + f * gd) * h) - (f * g * hd)) / (h*h); // first term

        ad1 += hd * Mathf.Cos(phi - theta1) * Mathf.Cos(phi); // second term

        ad1 += hd * Mathf.Sin(phi - theta1) * Mathf.Sin(phi); // third term

        g = FresnelS(f);
        gd = Mathf.Sin(theta1) / (2 * f);

        float ad2 = (((fd * g + f * gd) * h) - (f * g * hd)) / (h*h); // fourth term

        ad2 += hd * Mathf.Cos(phi - theta1) * Mathf.Sin(phi); // fifth term

        ad2 -= hd * Mathf.Sin(phi - theta1) * Mathf.Cos(phi); // sixth

        return ad1/x - ad2/y;
    }


    // uses newton's method of numerical analysis to find theta from simultaneous equations of A2
    public static void FindTheta(out float theta1, out float theta2, Vector3 startPos, Vector3 endPos, Vector3 startDir, Vector3 endDir, float initialGuess = -1)
    {
        float phi = Mathf.Acos(Vector3.Dot(startDir, -endDir) / (startDir.magnitude * endDir.magnitude));

        float xd = Vector3.Dot((endPos - startPos), startDir);
        float yd = ((endPos - startPos) - xd * startDir).magnitude;

//        Debug.Log("phi = " + phi + ", xd = " + xd + ", yd = " + yd);


        // this should catch all impossible scenarios, making maxIterations redundant
        float maximumRatio = FresnelC(Mathf.Sqrt(phi)) / FresnelS(Mathf.Sqrt(phi));
        float xdd = Vector3.Dot((startPos - endPos), endDir);
        float ydd = ((startPos - endPos) - xdd * endDir).magnitude;

//        Debug.Log("Ratio: " + maximumRatio + ", xd/yd: " + (xd/yd) + ", xdd/ydd: " + (xdd/ydd));
        
        if (xd / yd > maximumRatio || xdd / ydd > maximumRatio)
        {
            theta1 = -1;
            theta2 = -1;
            return;
        }
        

        if (initialGuess == -1)
        {
            initialGuess = phi / 2;
        }

        theta1 = initialGuess;
//        Debug.Log("Theta guess: " + theta1);

        int maxIterations = 10;

        for (int i = 0; i < maxIterations; i++)
        {
            float aDerivative = DeltaADerivative(theta1, phi, xd, yd);
//            Debug.Log("aDerivative = " + aDerivative);
            float deltaA = DeltaA(theta1, phi, xd, yd);
//            Debug.Log("deltaA = " + deltaA);
            float difference = deltaA / aDerivative;

            theta1 -= difference;

//            Debug.Log("Theta guess: " + theta1);

            //constrain theta
            if (theta1 < 0.001f*phi) theta1 = 0.001f*phi;
            if (theta1 > 0.999f*phi) theta1 = 0.999f*phi;

            if (difference < c_errorMargin && difference > -c_errorMargin)
            {
                theta2 = phi - theta1;
                return;
            }
        }

        // failed to converge
        theta1 = -1;
        theta2 = -1;
    }

    public static void FindAForPartialTransitionOut(out float a, out float theta, float R, float xp, float yp)
    {
        float dummy;
        FindAForPartialTransitionOut(out a, out theta, out dummy, R, xp, yp);
    }

    public static void FindAForPartialTransitionOut(out float a, out float theta, out float fractionOut, float R, float xp, float yp)
    {
        FindAForSingleTransition(out a, out theta, R, xp, yp);
        if (a > 0)
        {
            fractionOut = 0;
            return;
        }

        // if R -> infinity compared to the scale of the curve, consider it a full transition
        // this may not be used; the check for curvature will likely happen before this function is called
        if (R > (xp + yp) / c_errorMargin)
        {
            theta = Mathf.Atan2(yp, xp);

            float CRootTheta = FresnelC(Mathf.Sqrt(theta));

            a = (CRootTheta + CRootTheta * Mathf.Cos(2 * theta) + FresnelS(Mathf.Sqrt(theta)) * Mathf.Sin(2 * theta)) / xp;
            fractionOut = 1;
            return;
        }

        // if we haven't returned yet, we're somewhere in the middle of the out transition
        // the following is a false position method numerical analysis to find a

        // find upper and lower bounds for A
        // these are very broad
        float aUpper = 2 * 0.97745f / xp;
        float aLower = 0.1f / xp; // corresponds to an angle less than 1 degree
        if (aLower < 1 / (2 * R * 1.535f)) aLower = 1 / (2 * R * 1.535f);
        

//        Debug.Log("aUpper: " + aUpper + ", aLower: " + aLower);

        int maxIterations = 25;

        float fUpper = FunctionOfAForPartialTransition(aUpper, R, xp, yp);
        float fLower = FunctionOfAForPartialTransition(aLower, R, xp, yp);

        if (Mathf.Sign(fUpper) == Mathf.Sign(fLower))
        {
            Debug.Log("Require different sign for fUpper: " + fUpper + ", fLower: " + fLower);
            a = -1;
            theta = -1;
            fractionOut = -1;
            return;
        }

        float upperInARow = 0;
        float lowerInARow = 0;

        for (int i = 0; i < maxIterations; i++)
        {
//            Debug.Log("aUpper: " + aUpper + ", aLower: " + aLower + ", fUpper: " + fUpper + ", fLower: " + fLower);

            float aFalse;

            // the Illonois algorithm is modified to increase weighting for longer periods of one-sidedness. This converges faster.
            if (upperInARow > 1)
                aFalse = (fUpper * aLower - Mathf.Pow(3f, -upperInARow) * fLower * aUpper) / (fUpper - Mathf.Pow(3f, -upperInARow) * fLower);
            else if (lowerInARow > 1)
                aFalse = (Mathf.Pow(3f, -lowerInARow) * fUpper * aLower - fLower * aUpper) / (Mathf.Pow(3f, -lowerInARow) * fUpper - fLower);
            else
                aFalse = (fUpper * aLower - fLower * aUpper) / (fUpper - fLower);

            float fFalse = FunctionOfAForPartialTransition(aFalse, R, xp, yp);

            if (TrainsMath.AreApproximatelyEqual(0, fFalse, 0.00001f))
            {
                a = aFalse;
                float yq = FresnelS(1 / (2 * a * R)) / a;
                float xq = FresnelC(1 / (2 * a * R)) / a;

                theta = Mathf.Atan2(yp - yq, xp - xq);

//                Debug.Log("Solution found in " + i + " iterations: a = " + a + ", theta = " + theta);

                fractionOut = 1 - (1/(2*a*R) / Mathf.Sqrt(theta));

                return;
            }

            if (Mathf.Sign(fFalse) == Mathf.Sign(fUpper))
            {
                aUpper = aFalse;
                fUpper = fFalse;
                upperInARow++;
                lowerInARow = 0;
            }
            else if (Mathf.Sign(fFalse) == Mathf.Sign(fLower))
            {
                aLower = aFalse;
                fLower = fFalse;
                lowerInARow++;
                upperInARow = 0;
            }
            else
                Debug.Log("Shouldn't be here!");
        }

        a = -1;
        theta = -1;
        fractionOut = -1;
    }

    public static void FindAForPartialTransitionIn(out float a, out float theta, out float fractionIn, float R, float xp, float yp)
    {
        // intial guesses
        a = 1 / Mathf.Sqrt(xp * xp + yp * yp);
        theta = Mathf.Atan2(yp, xp);

        int maxIterations = 15;
        bool errorMinimised = false;

        // this is the Newton-Raphson algorithm in two dimensions
        for (int i = 0; i < maxIterations; i++)
        {
            float thetap = 1 / (4 * a * a * R * R);
            theta = Mathf.Min(Mathf.PI / 2, Mathf.Max(thetap, theta));

            float rtTheta = Mathf.Sqrt(theta);

            float sRtTheta = FresnelS(rtTheta);
            float cRtTheta = FresnelC(rtTheta);


            float sRtThetap = FresnelS(Mathf.Sqrt(thetap));
            float cRtThetap = FresnelC(Mathf.Sqrt(thetap));

            float sin2Theta = Mathf.Sin(2 * theta - thetap);
            float cos2Theta = Mathf.Cos(2 * theta - thetap);

            float sinThetap = Mathf.Sin(thetap);
            float cosThetap = Mathf.Cos(thetap);

            // The evaluation of each equation with the current guess
            float f0 = sRtTheta * sin2Theta + cRtTheta * cos2Theta
                    + (sRtTheta - sRtThetap) * sinThetap + (cRtTheta - cRtThetap) * cosThetap - a * xp;
            float g0 = -sRtTheta * cos2Theta + cRtTheta * sin2Theta
                    + (sRtTheta - sRtThetap) * cosThetap - (cRtTheta - cRtThetap) * sinThetap - a * yp;

            float sinSqRtTheta = Mathf.Pow(Mathf.Sin(rtTheta), 2);
            float cosSqRtTheta = Mathf.Pow(Mathf.Cos(rtTheta), 2);

            // the evaluation of each derivative wrt theta with the current guess
            float ftheta = 2 * sRtTheta * cos2Theta + sin2Theta * sinSqRtTheta / rtTheta
                    - 2 * cRtTheta * sin2Theta + cos2Theta * cosSqRtTheta / rtTheta
                    + sinSqRtTheta * sinThetap / rtTheta
                    + cosSqRtTheta * cosThetap / rtTheta;

            float gtheta = 2 * sRtTheta * sin2Theta - cos2Theta * sinSqRtTheta / rtTheta
                    + 2 * cRtTheta * cos2Theta + sin2Theta * cosSqRtTheta / rtTheta
                    + sinSqRtTheta * cosThetap / rtTheta
                    - cosSqRtTheta * sinThetap / rtTheta;

            float aCubed = 2 * Mathf.Pow(R, 2) * Mathf.Pow(a, 3);
            float aSq = 2 * R * a * a;
            float sinSqRtThetap = Mathf.Pow(Mathf.Sin(Mathf.Sqrt(thetap)), 2);
            float cosSqRtThetap = Mathf.Pow(Mathf.Cos(Mathf.Sqrt(thetap)), 2);

            // the evaluation of each derivative wrt a with the current guess
            float fa = sRtTheta * cos2Theta / aCubed
                    - cRtTheta * sin2Theta / aCubed
                    - (sRtTheta - sRtThetap) * cosThetap / aCubed + sinSqRtThetap * sinThetap / aSq
                    + (cRtTheta - cRtThetap) * sinThetap / aCubed + cosSqRtThetap * cosThetap / aSq
                    - xp;

            float ga = sRtTheta * sin2Theta / aCubed
                    + cRtTheta * cos2Theta / aCubed
                    + (sRtTheta - sRtThetap) * sinThetap / aCubed + sinSqRtThetap * cosThetap / aSq
                    + (cRtTheta - cRtThetap) * cosThetap / aCubed - cosSqRtThetap * sinThetap / aSq
                    - yp;

            float fk = f0 - ftheta * theta - fa * a;
            float gk = g0 - gtheta * theta - ga * a;

            a = (fk * gtheta - ftheta * gk) / (ftheta * ga - fa * gtheta);
            theta = (-fa * a - fk) / ftheta;

            float error = Mathf.Sqrt(f0 * f0 + g0 * g0);

            if (error < c_errorMargin)
            {
                errorMinimised = true;
                break;
            }
        }

        // check the solution is valid
        float finalThetap = 1 / (4 * a * a * R * R);
        if (errorMinimised && theta > finalThetap && theta < Mathf.PI / 2)
        {
            fractionIn = 1 - 1 / (2 * a * R * Mathf.Sqrt(theta));
            return;
        }

        a = -1;
        theta = -1;
        fractionIn = -1;
    }

    private static float FunctionOfAForPartialTransition(float a, float r, float xp, float yp)
    {
        float f;

        float C1on2ar = FresnelC(1/(2*a*r));
        float S1on2ar = FresnelS(1/(2*a*r));

//        Debug.Log("a = " + a + ", r = " + r + ", 1/2ar = " + (1/(2*a*r)) + ", C1on2ar = " + C1on2ar + ", S1on2ar = " + S1on2ar);

        float xq = C1on2ar / a;
        float yq = S1on2ar / a;
       
//        Debug.Log("yp - yq: " + (yp - yq) + ", xp - xq: " + ( xp - xq));

        float theta = Mathf.Atan2((yp - yq), (xp - xq));

//        Debug.Log("theta = " + theta);

        if (theta < 0) theta = 0;

        float x1 = FresnelC(Mathf.Sqrt(theta))/a;
        float y1 = FresnelS(Mathf.Sqrt(theta))/a;
        
        Vector2 mp = new Vector2(xp - x1, yp - y1);
        Vector2 qm = new Vector2(x1 - xq, y1 - yq);

        f = mp.magnitude - qm.magnitude;

        return f;
    }

    public static void FindAForSingleTransition(out float a, out float theta, float R, float xp, float yp)
    {
        //intial guess
        theta = 2 * Mathf.Atan(yp/xp);
        a = FresnelC(Mathf.Sqrt(theta)) / xp;
        //Debug.Log("Initial guess: " + a);
        // Newton-Raphson method again
        int maxIterations = 10;
        float L;
        for (int i = 0; i < maxIterations; i++)
        {
            
            L = 1 / (2 * a * R);

            float f = FresnelC(L) - (xp / yp) * FresnelS(L);
            float dfda = (xp / yp) * Mathf.Pow(Mathf.Sin(L), 2) / (2 * a * a * R) - Mathf.Pow(Mathf.Cos(L), 2) / (2 * a * a * R);

            float difference = f / dfda;

            a -= difference;
            theta = 1 / (4 * a * a * R * R);

            //Debug.Log("Diff: " + difference + ", a: " + a + ", theta: " + theta);
            if (difference < c_errorMargin && difference > -c_errorMargin) break;
#pragma warning disable
            if (difference != difference) break; // check for NaN
#pragma warning enable
            if (theta < 0 || theta > Mathf.PI / 2) break;
        }

        // check if the found solution is valid
        L = Mathf.Sqrt(theta);
        if (TrainsMath.AreApproximatelyEqual(xp, FresnelC(L) / a, 0.0001f) && TrainsMath.AreApproximatelyEqual(yp, FresnelS(L) / a, 0.0001f))
        {
            // all is well
            return;
        }
        a = -1;
        theta = -1;
    }


    public static void FindAForSingleTransition(out float a, out float theta, float R, float dist)
    {
        //initial guess
        a = Mathf.Sqrt(1/(2 * dist * R));
        theta = 0;
        // Newton Raphson again

        int maxIterations = 10;
        float L;

        for (int i = 0; i < maxIterations; i++)
        {
            L = 1 / (2 * a * R);
            float CL = FresnelC(L);
            float SL = FresnelS(L);

            float f = CL * CL + SL * SL - (dist * a) * (dist * a);
            float dfda = -CL * Mathf.Pow(Mathf.Cos(L), 2) / (a*R) - SL * Mathf.Pow(Mathf.Sin(L), 2) / (a*R) - 2 * dist * dist * a;

            float difference = f / dfda;

            a -= difference;
            theta = 1 / (4 * a * a * R * R);

            //Debug.Log("Diff: " + difference + ", a: " + a + ", theta: " + theta);
            if (difference < c_errorMargin && difference > -c_errorMargin) break;
#pragma warning disable
            if (difference != difference) break; // check for NaN
#pragma warning enable
            if (theta < 0 || theta > Mathf.PI / 2) break;
        }

        L = Mathf.Sqrt(theta);
        Vector2 v = new Vector2 (FresnelC(L) / a, FresnelS(L) / a);
        if (TrainsMath.AreApproximatelyEqual(dist, v.magnitude, 0.001f)) return;

        theta = -1;
        a = -1;
    }


    public static void FindAForSinglePartialTransition(out float a, out float theta, out float fraction, float dist, float rSmall, float rLarge)
    {
        //intial guess
        //a = Mathf.Sqrt(1 / (2 * rSmall * dist)) * 0.72f;
        theta = Mathf.PI / 4;
        a = Mathf.Sqrt(theta) / dist;
        fraction = 1;

        //Debug.Log("Initial Guess: " + a);

        // Newton-Raphson method again
        int maxIterations = 200;
        float Lq, Lp, CLp, CLq, SLp, SLq, x, y;
        for (int i = 0; i < maxIterations; i++)
        {

            Lq = 1 / (2 * a * rSmall);
            Lp = 1 / (2 * a * rLarge);

            CLp = FresnelC(Lp);
            CLq = FresnelC(Lq);
            SLp = FresnelS(Lp);
            SLq = FresnelS(Lq);

            x = (CLq - CLp);
            y = (SLq - SLp);

            float f = x * x + y * y - dist * dist * a * a;
            float dfda = 2 * x * (-Lq * Mathf.Pow(Mathf.Cos(Lq), 2) + Lp * Mathf.Pow(Mathf.Cos(Lp), 2)) + 2 * y * (-Lq * Mathf.Pow(Mathf.Sin(Lq), 2) + Lp * Mathf.Pow(Mathf.Sin(Lp), 2)) - 2 * dist * dist * a;

            float difference = f / dfda;

            a -= difference;
            theta = 1 / (4 * a * a * rSmall * rSmall);

            //Debug.Log("Diff: " + difference + ", a: " + a + ", theta: " + theta + ", i: " + i);
            if (difference < c_errorMargin && difference > -c_errorMargin) break;
#pragma warning disable
            if (difference != difference) break; // check for NaN
#pragma warning enable
            if (theta < 0.001f) theta = 0.001f;
            if (theta > Mathf.PI / 2) theta = Mathf.PI / 2;
        }

        // check if the found solution is valid
        Lq = Mathf.Sqrt(theta);
        Lp = 1 / (2 * a * rLarge);

        CLp = FresnelC(Lp);
        CLq = FresnelC(Lq);
        SLp = FresnelS(Lp);
        SLq = FresnelS(Lq);

        x = (CLq - CLp);
        y = (SLq - SLp);
        if (TrainsMath.AreApproximatelyEqual(0, x * x + y * y - dist * dist * a * a, 0.001f) && theta <= Mathf.PI / 2)
        {
            // all is well
            fraction = 1 - (Lp / Lq);
            return;
        }
        a = -1;
        theta = -1;
    }
}