using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlanetEntity
{
   
    bool HasTriangleAssigned { get; }

    PlanetPathfinder Planet { get; }

    PlanetTriangle CurrentTriangle { get; }

}
