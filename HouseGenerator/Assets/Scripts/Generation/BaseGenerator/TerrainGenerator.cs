using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TerrainGenerator : TerrainBuilder<Vector2>
{

    public bool generateNoiseSeed = true;

    public enum ChangesDisplay { Never, InPlayMode, InEditor, Always }

    [Tooltip("Defines how slow the height of the terrain changes")]
    [Range(1.01f, 40)]
    [Save]
    public float landscapeSmoothness = 5;

    
    [Tooltip("Defines how much the terrain varies in height")]
    [Range(0.0f, 20)]
    [Save]
    public float landscapeHeightScale = 2;

    [Tooltip("The size of each rectangle in the generated terrain. " +
        "The bigger the more angular does the landscape look.")]
    [Range(1, 20)]
    [Save]
    public int rectangleScale = 1;
    
    public ChangesDisplay changesPreview = ChangesDisplay.InEditor;

    protected bool isInPlayMode;

    protected override void BuildMesh()
    {
        if (generateNoiseSeed)
        {
            noiseSeed = Random.Range(-10000f, 10000);
        }
        base.BuildMesh();
    }
    
    protected void Start()
    {
        isInPlayMode = true;
        ShowTerrain(true);
    }

    protected override void DisplayTexture()
    {
        mesh.uv = BaseBuilder.colorData;
    }

    protected void OnValidate()
    {
        switch (changesPreview)
        {
            case (ChangesDisplay.InPlayMode):
                {
                    if (isInPlayMode)
                    {
                        ShowTerrain(false);
                    }
                    break;
                }
            case (ChangesDisplay.Always):
                {
                    ShowTerrain(false);
                    break;
                }
            case (ChangesDisplay.InEditor):
                {
                    if (!isInPlayMode)
                    {
                        ShowTerrain(false);
                    }
                    break;
                }
            case (ChangesDisplay.Never):
                {
                    break;
                }
        }
    }

    protected override Vector2 GetColorAt(float xProgress, float zProgress, float height)
    {
        return new Vector2(xProgress, zProgress);
    }
    
    protected abstract ITerrainHeightEvaluator HeightEvaluater { get; }
    
    protected float noiseSeed;

    protected void ReshapeMesh()
    {
        SetOffset();
        CreateShape(out BaseBuilder.vertices);
        if (mesh.vertices.Length != Vertices.Length)
        {
            BaseBuilder.DrawCurrentVertices();
            BaseBuilder.BuildUvs();
            UpdateMesh();
        }
        UpdateVertices();
        mesh.RecalculateNormals();
    }
    
    
    protected override int XSize
    {
        get
        {
            float scaledSize = TerrainWidth / rectangleScale;
            int result = (int)scaledSize;
            if(result < scaledSize)
            {
                result++;
            }
            return result;
        }
    }

    public int GetXSize()
    {
        return XSize;
    }
    
    protected override int ZSize
    {
        get
        {
            float scaledSize = terrainLength / rectangleScale;
            int result = (int)scaledSize;
            if (result < scaledSize)
            {
                result++;
            }
            return result;
        }
    }

    public int GetZSize()
    {
        return ZSize;
    }

    protected override float GetCurrentPlainX(int x, int z)
    {
        return x;
    }

    protected override float GetCurrentPlainZ(int x, int z)
    {
        return z;
    }

    /// <summary>
    /// contains the main logic for computing the height from the given values.
    /// Care with overriding
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    protected override float GetCurrentY(int x, int z)
    {
        float result = GetPerlinNoise(x, z);
        float extraHeight = BuildPlainHeightOnIndex(x, z);
        result += extraHeight;

        return result;
    }

    protected float BuildPlainHeightOnIndex(int x, int z)
    {
        float lengthProgress = ToScaledZProgress(z);
        float widthProgress = ToScaledXProgress(x);

        return HeightEvaluater.EvaluatePlainHeight(new Vector2(widthProgress, lengthProgress));
    }

    protected float GetPerlinNoise(int x, int z)
    {
        return (Mathf.PerlinNoise(TranslateIntoCurrentHeigthNoise(x), TranslateIntoCurrentHeigthNoise(z)) - 0.5f) * landscapeHeightScale;
    }
    
    protected float TranslateIntoCurrentHeigthNoise(int value)
    {
        return value * (1 / landscapeSmoothness) * rectangleScale + noiseSeed;
    }

}
