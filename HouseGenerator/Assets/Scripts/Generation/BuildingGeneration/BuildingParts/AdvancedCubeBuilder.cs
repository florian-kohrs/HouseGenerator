using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedCubeBuilder : CubeBuilder
{

    public AdvancedCubeBuilder(Vector2 maxDimensions,  Vector3 ownDemensions, Vector2 offset, Transform parent, Material m)
        : base(ownDemensions.x, ownDemensions.y, ownDemensions.z, parent, m) 
    {
        this.maxDimensions = maxDimensions;
        this.offset = offset;
    }


    protected Vector2 maxDimensions;

    protected Vector2 offset;

    protected override Vector3 GetCurrentVertexPosition(int x, int z)
    {
        Vector3 result = base.GetCurrentVertexPosition(x, z);

        float zOffset = Length / 2- maxDimensions.x / 2   + offset.x;

        float yOffset =   Height / 2 - maxDimensions.y / 2 + offset.y;

        result += new Vector3(0, yOffset, zOffset);

        return result;
    }

    protected override Vector2 GetColorAt(float xProgress, float zProgress, float height)
    {

        return LocalProgressToGlobal(xProgress, zProgress);
    }

    protected Vector2 LocalProgressToGlobal(float xProgress, float zProgress)
    {
        int xIndex = ProgressToIndex(xProgress);
        int zIndex = ProgressToIndex(zProgress);

        float length = GetCurrentZ(xIndex, zIndex);
        length += Length / 2; 

        float height = GetCurrentY(xIndex, zIndex);
        height += Height / 2;

        float globalXProgress = (length + offset.x) / maxDimensions.x;
        float globalYProgress = (height + offset.y) / maxDimensions.y;

        Vector2 globalProgress = new Vector2(globalXProgress, globalYProgress);

        Vector2 scaledProgress = new Vector2
            (globalProgress.x * maxDimensions.x, 
            globalProgress.y * maxDimensions.y);

        return scaledProgress;
    }

}
