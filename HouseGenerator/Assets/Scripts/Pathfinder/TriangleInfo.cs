using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleInfo 
{

    public TriangleInfo() { }
    public TriangleInfo(RaycastHit hit) 
    {
        normal = hit.normal;
        position = hit.point;
    }

    public TriangleInfo(Vector3 pos, Vector3 normal)
    {
        this.normal = normal;
        position = pos;
    }


    public Vector3 normal;

    public Vector3 position;

}
