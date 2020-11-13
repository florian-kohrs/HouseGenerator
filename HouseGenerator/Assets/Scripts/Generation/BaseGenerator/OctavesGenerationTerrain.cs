using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OctavesGenerationTerrain<T> : TerrainBuilder<T>
{

    [Range(0,1)]
    public float minHeight;

    protected override int XSize => TerrainWidth;

    protected override int ZSize => terrainLength;

    [Save]
    public int octaves = 4;

    [Save]
    [Range(0, 1)]
    public float persistance = 0.5f;

    [Save]
    public float lacunarity = 2;

    [Save]
    public float terrainHeightMultiplier = 5;

    [Tooltip("How much should the height multiplier be applied to different terrain heights")]
    [Save]
    public SAnimationCurve terrainHeightCurve = new SAnimationCurve();

    [Save]
    [Range(0.001f, 100)]
    public float scale = 20;

    [Save]
    public int seed;

    public bool fixedSeed = true;

    //[Save]
    public Serializable2DVector terrainOffset = new Serializable2DVector(0, 0);

    protected float[][] noiseMap;

    protected float maxNoiseHeight;
    protected float minNoiseHeight;

    protected virtual void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
        if (terrainHeightMultiplier < 1)
        {
            terrainHeightMultiplier = 1;
        }
        noiseMap = null;
        InitializeWithCollider();
    }

    protected override void OnFirstTimeBehaviourAwakend()
    {
        if (!fixedSeed)
        {
            seed = Random.Range(100000, -100000);
        }
        base.OnFirstTimeBehaviourAwakend();
    }

    protected virtual void Start()
    {
        noiseMap = null;
        ShowTerrain(true);
    }

    private void AddColliderIfExisting()
    {
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = mesh;
        }
    }

    protected virtual void GenerateHeightMap()
    {
        noiseMap = new float[VerticesXCount][];
        float normalizedMaxNoiseHeight = float.MinValue;
        float normalizedMinNoiseHeight = float.MaxValue;
        maxNoiseHeight = float.MinValue;
        minNoiseHeight = float.MaxValue;

        System.Random terrainRNG = new System.Random(seed);
        Vector2[] octavesOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = terrainRNG.Next(-100000, 100000) + terrainOffset.v.x;
            float offsetZ = terrainRNG.Next(-100000, 100000) + terrainOffset.v.y;
            octavesOffsets[i] = new Vector2(offsetX, offsetZ);
        }

        float halfWidth = VerticesXCount / 2f;
        float halfLength = VerticesZCount / 2f;

        for (int x = 0; x < VerticesXCount; x++)
        {
            noiseMap[x] = new float[BaseBuilder.VerticesZCount];
            for (int z = 0; z < BaseBuilder.VerticesZCount; z++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octavesOffsets[i].x;
                    float sampleY = (z - halfLength) / scale * frequency + octavesOffsets[i].y;

                    float currentNoise = GetPerlinNoiseAt(sampleX, sampleY);
                    noiseHeight += currentNoise * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                noiseHeight *= GetHeightMultiplierForProgress((float)x / XSize, (float)z / ZSize);

                if (noiseHeight > normalizedMaxNoiseHeight)
                {
                    normalizedMaxNoiseHeight = noiseHeight;
                }
                if (noiseHeight < normalizedMinNoiseHeight)
                {
                    normalizedMinNoiseHeight = noiseHeight;
                }

                noiseMap[x][z] = noiseHeight;

            }
        }

        for (int z = 0; z < VerticesZCount; z++)
        {
            for (int x = 0; x < VerticesXCount; x++)
            {
                float newHeight = Mathf.InverseLerp(normalizedMinNoiseHeight, normalizedMaxNoiseHeight, noiseMap[x][z]);
                newHeight = terrainHeightCurve.Evaluate(newHeight);
                newHeight = Mathf.Sign(newHeight) * Mathf.Max(0, Mathf.Abs(newHeight) - minHeight);
                newHeight *= terrainHeightMultiplier;

                noiseMap[x][z] = newHeight;
                if (newHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = newHeight;
                }
                if (newHeight < minNoiseHeight)
                {
                    minNoiseHeight = newHeight;
                }
            }
        }
    }

    protected virtual float GetHeightMultiplierForProgress(float x, float z)
    {
        return 1;
    }

    protected virtual float GetPerlinNoiseAt(float x, float z)
    {
        return Mathf.PerlinNoise(x, z) * 2 - 1;
    }

    private float[][] NoiseMap
    {
        get
        {
            if (noiseMap == null)
            {
                GenerateHeightMap();
            }
            return noiseMap;
        }
    }

    protected override float GetCurrentY(int x, int z)
    {
        return NoiseMap[x][z];
    }
}
