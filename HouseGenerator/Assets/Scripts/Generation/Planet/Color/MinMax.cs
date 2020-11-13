using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMax
{

    public float Min { get; private set; } = float.MaxValue;

    public float Max { get; private set; } = float.MinValue;
    
    public void AddValue(float f)
    {
        if (f < Min)
        {
            Min = f;
        }
        if(f > Max)
        {
            Max = f;
        }
    }

}
