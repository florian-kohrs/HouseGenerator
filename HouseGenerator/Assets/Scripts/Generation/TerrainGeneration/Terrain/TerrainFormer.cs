using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainFormer : TerrainGenerator, ITerrainHeightEvaluator
{

    [Tooltip("Multiplied with the current slopecurve position.")]
    [Range(-100, 1000)]
    public float slopeXFactor = 2;

    [Tooltip("Will higher the side of the map")]
    public AnimationCurve xSlopeHeightCurve;

    //[Tooltip("Sets the slope of the map")]
    //[Range(-0.9f, 0.9f)]
    //public float slope = 0.05f;

    [Tooltip("Multiplied with the current slopecurve position.")]
    [Range(-100, 1000)]
    public float slopeZFactor = 2;

    public AnimationCurve zSlopeHeightCurve;
    
    protected override ITerrainHeightEvaluator HeightEvaluater
    {
        get
        {
            return this;
        }
    }

    public float EvaluatePlainHeight(Vector2 progress)
    {
        float result = 0;
        if(xSlopeHeightCurve != null)
        {
            result += xSlopeHeightCurve.Evaluate(progress.x) * slopeXFactor;
        }
        if (zSlopeHeightCurve != null)
        {
            result += zSlopeHeightCurve.Evaluate(progress.y) * slopeZFactor;
        }
        return result;
    }

}
