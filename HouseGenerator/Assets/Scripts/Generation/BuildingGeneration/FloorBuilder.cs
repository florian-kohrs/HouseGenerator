using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorBuilder
{
    
    public FloorBuilder(Transform parent,
        float widht, 
        float height,
        float thickness, 
        WallObstacleScriptableObject door, 
        params WallObstacleScriptableObject[] windows)
    {
        floorParent = new GameObject().transform;
        floorParent.SetParent(parent, false);
    }

    protected Transform floorParent;

    protected List<Transform> childs = new List<Transform>();




}
