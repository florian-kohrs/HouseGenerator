using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BentSizeDiffCube : BentHeightDiffCube
{
    
    public AnimationCurve zMultiplier;

    protected float YMultiplierAtIndex(int z)
    {
        float progress = ToScaledZProgress(z);
        float sign = Mathf.Sign(progress - 0.5f);
        return sign;
    }

    protected override float GetCurrentPlainZ(int x, int z)//
    {
        float lengthChange = zMultiplier.Evaluate(Mathf.InverseLerp(1, XSize / 2, x % (XSize/ 2)));

        float result = base.GetCurrentPlainX(x, z);
        if (z > 0 && z < ZSize)
        {
            switch (x)
            {
                ///cases for upper height
                case 0:
                case 3:
                case 4:
                    {
                        result += ((1 - lengthChange) / 2);
                        break;
                    }
                ///cases for lower height
                case 1:
                case 2:
                    {
                        result -= ((1 - lengthChange) / 2);
                        break;
                    }
            }
        }
        return result;
    }
    
}
