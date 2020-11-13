using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    
    public enum FilterType { Simple, Rigid };

    public FilterType filtertype;

    public float strength = 1;
    
    public float roughness = 1;

    public float baseRoughness = 1;

    public float weightMultiplier = 0.8f;

    public Vector3 centre;

    [Range(1,8)]
    public int layers = 1;

    [Range(0.01f,1)]
    public float persitence = .5f;

    public float minValue;
    
}
