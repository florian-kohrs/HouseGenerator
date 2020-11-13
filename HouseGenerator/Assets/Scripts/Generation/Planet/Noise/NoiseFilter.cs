using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NoiseFilter : ScriptableObject, INoiseFilter
{

    public abstract float Evaluate(Vector3 point);

}
