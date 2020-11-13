using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BentHeightDiffCube : BentCube
{

    public AnimationCurve heightMultiplier;

    protected override float GetCurrentY(int x, int z)
    {
        float result = base.GetCurrentY(x, z);
        if (z > 0 && z < ZSize)
        {

            float heightChange = heightMultiplier.Evaluate(Mathf.InverseLerp(1,ZSize - 1,z));

            switch (x)
            {
                ///cases for upper height
                case 0:
                case 1:
                case 4:
                    {
                        result -= Height * ((1 - heightChange) / 2);
                        break;
                    }
                ///cases for lower height
                case 2:
                case 3:
                    {
                        result += Height * ((1 - heightChange) / 2);
                        break;
                    }
            }
        }
        return result;
    }

}
