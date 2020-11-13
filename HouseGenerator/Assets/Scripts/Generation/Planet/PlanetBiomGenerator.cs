using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlanetBiomGenerator : PlanetLandscapeGenerator<PlanetTriangle, SurfaceVertex>
{

    public List<Biom> bioms;

    protected Surface GetSurfaceForVertex(Vector3 v)
    {
        return GetBiomForLat(0).GetSurfaceForProgress(Mathf.InverseLerp(minMax.Min, minMax.Max, v.magnitude));
    }

    protected override void OnValidate()
    {
        bioms.ForEach(b => b.SortSurfaces());
        base.OnValidate();
    }

    public override void BuildPlanet()
    {
        base.BuildPlanet();

        //GrowPlants();
    }

    protected Biom GetBiomForLat(float lat)
    {
        return bioms[0];
    }

    protected override Color ComputeSurfaceForVector(Vector3 v, SurfaceVertex data)
    {
        data.surface = GetSurfaceForVertex(v);
        return data.surface.color;
        //if (enteredVertices.ContainsKey(v))
        //{
        //}
        //else
        //{
        //    return new Color(0.2f, 0.2f, 0.2f);
        //}
    }

    public override IEnumerable<Vector3Int> GetTriangles(PlanetTriangle triData)
    {
        foreach (Vector3Int v in triData.GetAllTriangles())
        {
            yield return v;
        }
    }

    public override IEnumerable<PlanetTriangle> GetChildTris(PlanetTriangle triData)
    {
        foreach (PlanetTriangle t in triData.GetAllPlanetTriangles())
        {
            yield return t;
        }
    }

    public override PlanetTriangle CreateTriangle(int x, int y, int z)
    {
        return new PlanetTriangle(transform, new Vector3Int(x, y, z), vertices, middlePointIndexCache);
    }


}
