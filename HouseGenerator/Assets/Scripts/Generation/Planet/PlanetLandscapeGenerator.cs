
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public abstract class PlanetLandscapeGenerator<T, V> : BasePlanet<T, V>, INavigatable<PlanetTriangle, TriangleInfo> where T : ITriangle where V : IVertex, new()
{

    public NoiseLayer[] noiseLayers;

    protected INoiseFilter[] noiseFilters;
    
    protected MinMax minMax = new MinMax();

    protected abstract Color ComputeSurfaceForVector(Vector3 v, V data);
  
    protected virtual void OnValidate()
    {
        minMax = new MinMax();
        vertexCount = 0;
        noiseFilters = new INoiseFilter[noiseLayers.Length];

        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(noiseLayers[i].noiseSettings);
        }
        if (Application.isPlaying)
        {
            if (!builded)
            {
                BuildPlanet();
            }
        }
        else
        {
            BuildPlanet();
        }

        // base.OnValidate();
    }

    bool builded = false;
    protected int vertexCount;

    protected override Vector3 EditPointOnPlanet(Vector3 point)
    {
        float elevation = 0;

        float firstMaskValue = 0;
        
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            if (noiseLayers[i].enabled)
            {
                if (i == 0)
                {
                    firstMaskValue = noiseFilters[i].Evaluate(point);
                    elevation += firstMaskValue;
                }
                else
                {
                    float mask = 1;
                    if (noiseLayers[i].useFirstLayerAs == NoiseLayer.UseFirstLayerAs.Mask)
                    {
                        mask = firstMaskValue;
                    }
                    else if (noiseLayers[i].useFirstLayerAs == NoiseLayer.UseFirstLayerAs.InverseMask)
                    {
                        if(firstMaskValue == 0)
                        {
                            mask = 1;
                        }
                        else
                        {
                            mask = 0;
                        }
                    }
                    elevation += noiseFilters[i].Evaluate(point) * mask;
                }
                
            }
        }
        elevation = radius * (1 + elevation);
        minMax.AddValue(elevation);
        return point * elevation;
    }

 
    protected override void BuildUvs()
    {
        normalColorData = new Color[vertices.Length];
        
        for (int i = 0; i < normalColorData.Length; i++)
        {
            ///maybe replace .magnitude with array of length so sqrtRoot doesnt have to be called
            float heightProgress = Mathf.InverseLerp(minMax.Min, minMax.Max, vertices[i].Vertex.magnitude);

            float latitude = vertices[i].Vertex.y;
            normalColorData[i] = ComputeSurfaceForVector(vertices[i].Vertex, vertices[i]);
        }

    }

    public PlanetTriangle[] GetCircumjacent(PlanetTriangle field)
    {
        return field.GetAccessableCircumjacent();
    }
    
    public float DistanceToField(PlanetTriangle from, PlanetTriangle to)
    {
        return Vector3.Distance(from.RotatedMiddlePointOfTriangle, to.RotatedMiddlePointOfTriangle);
    }

    public bool IsEqual(PlanetTriangle t1, PlanetTriangle t2)
    {
        return t1 == t2;
    }
    
    public float DistanceToTarget(PlanetTriangle from, TriangleInfo to)
    {
        float distance = Vector3.Distance(from.RotatedMiddlePointOfTriangle + transform.position, to.position);
        float angle3 = Mathf.Acos(1 - Mathf.Pow(distance, 2) / (2 * Mathf.Pow(radius, 2)));
        float factor2 = angle3 / (Mathf.PI * 2);
        float circumference = 2 * Mathf.PI * radius;
        return circumference * factor2;
    }

    public bool ReachedTarget(PlanetTriangle current, TriangleInfo destination)
    {
        return current.RotatedNormal == destination.normal;
    }

}
