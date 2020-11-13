using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITerrainHeightEvaluator
{

    float EvaluatePlainHeight(Vector2 progress);
    
}
