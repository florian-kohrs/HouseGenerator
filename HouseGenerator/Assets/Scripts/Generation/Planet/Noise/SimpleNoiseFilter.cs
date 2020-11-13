using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleNoiseFilter : INoiseFilter
{
    
    public SimpleNoiseFilter(NoiseSettings settings)
    {
        this.settings = settings;
        noise = new Noise3D();
    }

    protected int seed;

    public NoiseSettings settings;

    protected Noise3D noise;

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequenzy = settings.baseRoughness;
        float amplitude = 1;
        for (int i = 0; i < settings.layers; i++)
        {
            float v = noise.Evaluate(point * frequenzy + settings.centre);
            noiseValue += (v + 1) * .5f * amplitude;
            frequenzy *= settings.roughness;
            amplitude *= settings.persitence;
        }
        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }

}
