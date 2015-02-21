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
}
