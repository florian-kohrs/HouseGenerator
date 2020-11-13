using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexInfo
{

    public int index;

    /// <summary>
    /// the value the x-index needs to add to get the actual position
    /// </summary>
    public int xIndex;

    public int zIndex;

    public float OriginalXDiff
    {
        set
        {
            diffToOriginal.x = value;
        }
        get
        {
            return diffToOriginal.x;
        }
    }

    public float OriginalZDiff
    {
        set
        {
            diffToOriginal.y = value;
        }
        get
        {
            return diffToOriginal.y;
        }
    }

    public Vector2 diffToOriginal;

    public float DistanceToOriginal
    {
        get
        {
            return diffToOriginal.magnitude; 
        }
    }

}
