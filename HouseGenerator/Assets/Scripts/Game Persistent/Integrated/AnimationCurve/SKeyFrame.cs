using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class SKeyFrame
{

    public SKeyFrame(Keyframe frame)
    {
        inTangent = frame.inTangent;
        outTangent = frame.outTangent;
        inWeight = frame.inWeight;
        outWeight = frame.outWeight;
        time = frame.time;
        value = frame.value;
    }

    public float inTangent;
    public float outTangent;
    public float inWeight;
    public float outWeight;
    public float time;
    public float value;

    public Keyframe ToFrame()
    {
        return new Keyframe(time, value, inTangent, outTangent, inWeight, outWeight);
    }
}
