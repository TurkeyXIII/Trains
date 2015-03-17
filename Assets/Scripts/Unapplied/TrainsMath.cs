using UnityEngine;
using System.Collections;

public class TrainsMath {

    private static float c_fudgeFactor = 0.0001f;

    public static bool AreApproximatelyEqual(float a, float b)
    {
        return AreApproximatelyEqual(a, b, c_fudgeFactor * 10);
    }

    public static bool AreApproximatelyEqual(float a, float b, float margin)
    {
        float diff = a - b;
        if (diff < margin && diff > -margin) return true;

        return false;
    }

    public static bool AreApproximatelyEqual(Vector3 a, Vector3 b, float margin)
    {
        if (AreApproximatelyEqual(a.x, b.x, margin) &&
            AreApproximatelyEqual(a.y, b.y, margin) &&
            AreApproximatelyEqual(a.z, b.z, margin))
            return true;

        return false;
    }
}


public class FresnelMath
{
    private const float c_errorMargin = 0.0001f;

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
        float phi = Mathf.Acos(Vector3.Dot(startDir, -endDir));

        float xd = Vector3.Dot((endPos - startPos), startDir);
        float yd = ((endPos - startPos) - xd * startDir).magnitude;

        Debug.Log("phi = " + phi + ", xd = " + xd + ", yd = " + yd);


        // this should catch all impossible scenarios, making maxIterations redundant
        float maximumRatio = FresnelC(Mathf.Sqrt(phi)) / FresnelS(Mathf.Sqrt(phi));
        float xdd = Vector3.Dot((startPos - endPos), endDir);
        float ydd = ((startPos - endPos) - xdd * endDir).magnitude;

        Debug.Log("Ratio: " + maximumRatio + ", xd/yd: " + (xd/yd) + ", xdd/ydd: " + (xdd/ydd));
        
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
        Debug.Log("Theta guess: " + theta1);

        int maxIterations = 10;

        for (int i = 0; i < maxIterations; i++)
        {
            float aDerivative = DeltaADerivative(theta1, phi, xd, yd);
//            Debug.Log("aDerivative = " + aDerivative);
            float difference = DeltaA(theta1, phi, xd, yd) / aDerivative;

            theta1 -= difference;

            Debug.Log("Theta guess: " + theta1);

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
}