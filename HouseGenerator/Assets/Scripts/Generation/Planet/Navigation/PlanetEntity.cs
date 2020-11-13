using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetEntity : SaveableMonoBehaviour, IPlanetEntity
{

    protected virtual void Start()
    {
        UpdateToTriangle(transform.forward);
    }

    protected PlanetTriangle currentTriangle;

    protected PlanetPathfinder planet;

    public PlanetPathfinder Planet
    {
        get
        {
            if (planet == null)
            {
                planet = PlanetPathfinder.ClosestPlanetAt(transform.position);
            }
            return planet;
        }
    }

    public bool HasTriangleAssigned => currentTriangle != null;

    public PlanetTriangle CurrentTriangle
    {
        get
        {
            if (currentTriangle == null)
            {
                Planet.GetPlanetTriangleFor(transform.position, out currentTriangle);
                CurrentTriangle = currentTriangle;
            }
            return currentTriangle;
        }
        set
        {
            currentTriangle = value;
            OnTriangleSet(value);
        }
    }

    protected virtual void OnTriangleSet(PlanetTriangle t) { }

    /// <summary>
    /// distance calculations bugged on scaled objects
    /// </summary>
    /// <param name="forward"></param>
    public virtual void AlignToTriangle(Vector3 forward)
    {
        CurrentTriangle.AlignToTriangle(transform, forward);
    }


    public virtual void UpdateToTriangle(Vector3 forward)
    {
        AlignToTriangle(forward);
        PutOnTriangle();
    }

    public virtual void PutOnTriangle()
    {
        CurrentTriangle.PutOnTriangle(transform);
    }

}
