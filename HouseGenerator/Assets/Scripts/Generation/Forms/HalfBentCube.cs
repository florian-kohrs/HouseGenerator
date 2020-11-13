using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfBentCube : BentCube
{

    public float maxHeight = 1;

    protected override float GetCurrentY(int x, int z)
    {
        float height = Mathf.Min(maxHeight, base.GetCurrentY(x, z));

        return height;
    }

}
