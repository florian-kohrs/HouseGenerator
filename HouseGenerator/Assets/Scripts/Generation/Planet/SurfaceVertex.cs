using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceVertex : IVertex
{

    public Vector3 position;

    public Surface surface;

    public Vector3 Vertex { get => position; set => position = value; }

}
