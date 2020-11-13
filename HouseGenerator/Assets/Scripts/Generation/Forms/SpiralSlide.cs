using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralSlide : TerrainBuilder<Vector2>
{

    public float spiralHeight = 10;

    [Range(0,5)]
    public float centerSpace = 0;
    
    protected override int XSize => 1;

    protected override int ZSize => terrainLength - 1;

    public int spiralSmoothness = 50;

    public float EvaluatePlainHeight(Vector2 progress)
    {
        return progress.y * spiralHeight;
    }

    private void Reset()
    {
        terrainWidth = 5;
        terrainLength = 100;
        spiralSmoothness = 50;
        spiralHeight = 10;
    }

    protected override Vector3 GetCurrentVertexPosition(int x, int z)
    {
        Vector2 progress = ToScaledProgress(x, z);
        Vector3 result = Vector3.zero;
        float currentRad = 2 * Mathf.PI * ((float)z / spiralSmoothness);

        float cos = Mathf.Cos(currentRad);
        float sin = Mathf.Sin(currentRad);
        if (progress.x == 0)
        {
            result.x = (cos * (centerSpace) - sin * 0);
            result.z = (sin * (centerSpace) + cos * 0);
        }
        else
        {
            result.x = (cos * (1) - sin * (0));
            result.z = (sin * (1) + cos * (0));
            result *= TerrainWidth;
        }
        result.y = EvaluatePlainHeight(progress);
        //result += transform.position;
        return result;
    }

    protected void OnValidate()
    {
        ShowTerrain(true);
    }

    protected override Vector2 GetColorAt(float xProgress, float zProgress, float height)
    {
        return new Vector2(xProgress, zProgress);
    }

    protected override float GetCurrentY(int x, int z)
    {
        return EvaluatePlainHeight(new Vector2(x, z));
    }
}
