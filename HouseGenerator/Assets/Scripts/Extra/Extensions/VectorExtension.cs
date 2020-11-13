using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtension
{

    //public static IEnumerator GetSmoothTowards(this Vector3 original, Vector3 target, System.Action<Vector3> set, float inTime)
    //{
    //    IEnumerator result = SmoothVectorTransformation.GetStoppable(() => new Vector3[] { original }, set, target, inTime);
    //    return result;
    //}

    public static Vector2 RotateVector(this Vector2 original, float rad, Vector2 offset = default(Vector2))
    {
        Vector2 result = Vector2.zero;
        float cos = Mathf.Cos(rad * Mathf.PI);
        float sin = Mathf.Sin(rad * Mathf.PI);
        Vector2 rotateCenter = original - offset;
        result.x = (cos * (rotateCenter.x) - sin * rotateCenter.y) + offset.x;
        result.y = (sin * (rotateCenter.x) + cos * rotateCenter.y) + offset.y;
        return result;
    }
    
    public static Vector3 RotateTowardsAngle(this Vector3 from, Vector3 to, float speed)
    {
        return new Vector3(
            TowardsAngle(from.x,to.x,speed), 
            TowardsAngle(from.y, to.y, speed), 
            TowardsAngle(from.z, to.z, speed)
        );
    }

    private static float TowardsAngle(float from, float to, float speed)
    {
        float deltaEuler = from - to;
        if (deltaEuler > 180)
        {
            deltaEuler -= 360;
        }
        else if (deltaEuler < -180)
        {
            deltaEuler += 360;
        }
        float maxMagnitudeDelta = deltaEuler;
        float sign = Mathf.Sign(maxMagnitudeDelta);
        float deltaX = sign * Mathf.Min(Mathf.Abs(maxMagnitudeDelta), speed);
        from += deltaX;
        return from;
    }

    public static Vector3 LerpAngle(this Vector3 a, Vector3 b, float t)
    {
        return new Vector3(
            Mathf.LerpAngle(a.x, b.x, t), 
            Mathf.LerpAngle(a.y, b.y, t), 
            Mathf.LerpAngle(a.z, b.z, t)
        );
    }

    public static Vector3 LerpAngleFunc(Vector3 a, Vector3 b, float t)
    {
        return a.LerpAngle(b, t);
    }

}
