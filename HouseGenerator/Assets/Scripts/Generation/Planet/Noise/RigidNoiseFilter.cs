using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidNoiseFilter : INoiseFilter
{
    
    public RigidNoiseFilter(NoiseSettings settings)
    {
        this.settings = settings;
        noise = new Noise3D();
    }
    
    public NoiseSettings settings;

    protected Noise3D noise;

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequenzy = settings.baseRoughness;
        float amplitude = 1;
        float weight = 1;
        for (int i = 0; i < settings.layers; i++)
        {
            float v = 1-Mathf.Abs(noise.Evaluate(point * frequenzy + settings.centre));
            v *= v;
            v *= weight;
            weight = Mathf.Clamp01(v * settings.weightMultiplier);
            noiseValue += v * amplitude;
            frequenzy *= settings.roughness;
            amplitude *= settings.persitence;
        }
        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }


}
