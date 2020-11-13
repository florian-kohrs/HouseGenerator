using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeGenerator : TerrainGenerator, ITerrainHeightEvaluator
{

    public int seed = 0;

    protected override void BuildMesh()
    {
        generateNoiseSeed = false;
        noiseSeed = seed;
        base.BuildMesh();
    }

    protected override ITerrainHeightEvaluator HeightEvaluater
    {
        get { return this; }
    }

    protected override Vector3 GetCenteredMesh()
    {
        return new Vector3(0,0, (terrainLength / 2f));
    }

    public bool showInside;
    
    [Range(0.1f, 50)]
    public float caveRadius = 1;

    public AnimationCurve caveRadiusDistribution = 
        new AnimationCurve()
        {
            keys =
            new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) }
        };
    
    private float firstXPos;

    private float firstYPos;

    protected override float GetCurrentPlainX(int x, int z)
    {
        float progress = ToScaledXProgress(x);
        float zProgress = ToScaledZProgress(z);
        float result;
        if (progress == 1)
        {
            result = firstXPos;
        }
        else
        {
            if (showInside)
            {
                result = Mathf.Cos(Mathf.PI * 2 * ToScaledXProgress(x)) * caveRadius
                    * caveRadiusDistribution.Evaluate(zProgress);
            }
            else
            {
                result = Mathf.Sin(Mathf.PI * 2 * ToScaledXProgress(x)) * caveRadius
                    * caveRadiusDistribution.Evaluate(zProgress);
            }
        }
        if(progress == 0)
        {
            firstXPos = result;
        }
        return result;
    }

    protected override float GetCurrentY(int x, int z)
    {
        float progress = ToScaledXProgress(x);
        float result;
        if (progress == 1)
        {
            result = firstYPos;
        }
        else
        {
            result = base.GetCurrentY(x, z);
        }
        if (progress == 0)
        {
            firstYPos = result;
        }
        return result;
    }

    public float EvaluatePlainHeight(Vector2 progress)
    {
        if (showInside)
        {
            return Mathf.Sin(Mathf.PI * 2 * progress.x) * 
                caveRadiusDistribution.Evaluate(progress.y) * caveRadius;
        }
        else
        {
            return Mathf.Cos(Mathf.PI * 2 * progress.x) * 
                caveRadiusDistribution.Evaluate(progress.y) * caveRadius;
        }
    }
}
