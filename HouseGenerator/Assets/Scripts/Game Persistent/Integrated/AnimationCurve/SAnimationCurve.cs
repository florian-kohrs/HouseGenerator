using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class SAnimationCurve : ISerializable
{

    public SAnimationCurve()
    {
        curve = new AnimationCurve();
        curve.AddKey(0, 1);
        curve.AddKey(1, 1);
    }

    public  float Evaluate(float time)
    {
        return curve.Evaluate(time);
    }

    protected SAnimationCurve(SerializationInfo info, StreamingContext context)
    {
        keys = (List < SKeyFrame > )info.GetValue("keys", typeof(List<SKeyFrame>));

        curve = new AnimationCurve();
        foreach (SKeyFrame key in keys)
        {
            curve.AddKey(key.ToFrame());
        }
    }

    public AnimationCurve curve;

    private List<SKeyFrame> keys = new List<SKeyFrame>();

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        keys.Clear();
        foreach (Keyframe k in curve.keys) 
        {
            keys.Add(new SKeyFrame(k));
        }
        info.AddValue("keys", keys);
    }

}
