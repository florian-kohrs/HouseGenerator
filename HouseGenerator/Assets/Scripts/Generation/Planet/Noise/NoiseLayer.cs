using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseLayer
{

    public bool enabled = true;

    public UseFirstLayerAs useFirstLayerAs;
    
    public NoiseSettings noiseSettings;

    public enum UseFirstLayerAs { Mask, InverseMask, Nothing}

}
