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
