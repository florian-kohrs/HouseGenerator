using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctavesColourGenerator : OctavesGenerationTerrain<Color>
{

    public Gradient colorGradient = new Gradient();

    protected override Color GetColorAt(float xProgress, float zProgress, float height)
    {
        return colorGradient.Evaluate(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, height));
    }

    protected override void DisplayTexture()
    {
        mesh.colors = BaseBuilder.colorData;
    }

    

}
