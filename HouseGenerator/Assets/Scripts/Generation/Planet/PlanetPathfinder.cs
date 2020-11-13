using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlanetPathfinder : PlanetBiomGenerator
{

    public static List<PlanetPathfinder> allPlanets = new List<PlanetPathfinder>();

    public override void OnAwake()
    {
        base.OnAwake();
        allPlanets.Add(this);
    }

    public static PlanetPathfinder ClosestPlanetAt(Vector3 dir)
    {
        float distance = float.MaxValue;
        PlanetPathfinder result = null;
        foreach (PlanetPathfinder p in allPlanets)
        {
            float currentDistance = (p.transform.position - dir).sqrMagnitude;
            if (currentDistance < distance)
            {
                currentDistance = distance;
                result = p;
            }
        }
        return result;
    }

    protected override void OnRootTrianglesDone()
    {
        for (int i = 0; i < mainTriangles.Count; i++)
        {
            for (int x = i + 1; x < mainTriangles.Count; x++)
            {
                if (mainTriangles[i].IsNeighborWith(mainTriangles[x]))
                {
                    mainTriangles[i].AddNeighborTwoWay(mainTriangles[x]);
                }
            }
        }
    }

    protected override void OnAllPlainTrianglesDone()
    {
        mainTriangles.ForEach(tri => tri.SetChildNeighbors());
    }

    public bool GetPlanetTriangleFor(Vector3 from, out PlanetTriangle tri)
    {
        RaycastHit hit;
        tri = null;
        Vector3 dir = transform.position - from;
        //Debug.DrawRay(from + (-dir.normalized * 5), dir, Color.red, 1);
        if (Physics.Raycast(from + (-dir.normalized * 5), dir, out hit, dir.magnitude, 1 << 12))
        {
            tri = GetTriangleWithData(new TriangleInfo(hit));

        }
        return tri != null;
    }

    public PlanetTriangle GetTriangleWithData(TriangleInfo triInfo)
    {
        Vector3 localPosition = triInfo.position - transform.position;
        PlanetTriangle result;
        if (levelOfDetail == 1)
        {
            result = mainTriangles.Where(t => t.RotatedNormal == triInfo.normal).First();
        }
        else
        {
            IEnumerable<PlanetTriangle> tris = mainTriangles;

            do
            {
                result = ClosestTriangleAtSphericalPosition(tris, localPosition);
                tris = result.Children;

            } while (!result.IsLastParent);


            result = result.Children.Where(t => t.RotatedNormal == triInfo.normal).FirstOrDefault();

            if (result == null)
            {
                // Debug.LogWarning("Couldnt find triangle with same normal. Searched in neighbors of closest.");
                result = ClosestTriangleAtSphericalPosition(tris, localPosition);

                PlanetTriangle normalInNeighbor = result.neighbors.Where(t => t.RotatedNormal == triInfo.normal).FirstOrDefault();
                if (normalInNeighbor == null)
                {
                    throw new System.Exception("Couldnt find triangle with hit normal!");
                }
                else
                {
                    result = normalInNeighbor;
                }
            }
        }

        return result;
    }


    public PlanetTriangle ClosestTriangleAtSphericalPosition(IEnumerable<PlanetTriangle> possibilities, Vector3 globalPoint)
    {
        Vector3 sphericalPoint = globalPoint = globalPoint.normalized * radius;
        PlanetTriangle result = null;

        float minDistance = float.MaxValue;

        foreach (PlanetTriangle tri in possibilities)
        {
            float distance = (tri.GetSphericalMiddlePointOfTriangle(radius) - sphericalPoint).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                result = tri;
            }
        }

        return result;
    }
}
