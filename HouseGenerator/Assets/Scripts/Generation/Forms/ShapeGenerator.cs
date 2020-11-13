using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator : TerrainGenerator, ITerrainHeightEvaluator
{

    protected override Vector3 GetCenteredMesh()
    {
        return new Vector3(0,0, terrainLength / 2);
    }

    public AnimationCurve pipeShape =
       new AnimationCurve()
       {
           keys =
           new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 0) }
       };
    
    public float shapeScale = 2;

    protected override ITerrainHeightEvaluator HeightEvaluater => this;

    protected override float GetCurrentY(int x, int z)
    {
        float progressZ = ToScaledZProgress(z);

        return pipeShape.Evaluate(progressZ) * shapeScale;
    }

    public virtual float EvaluatePlainHeight(Vector2 progress)
    {
        return pipeShape.Evaluate(progress.y) * shapeScale;
    }
}
