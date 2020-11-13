using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBuilder
{

    public CubeBuilder(float width, float height, float length, Transform parent, Material m)
    {
        this.width = width;
        this.height = height;
        this.length = length;
        //Cube.transform.SetParent(parent, false);
        //meshRenderer.material = m;
    }


    protected float width;

    public float Width => width;

    protected float height;

    public float Height => height;

    protected float length;

    public float Length => length;

    public BaseMeshBuilder<Vector2> baseMeshBuilder;

    public GameObject g;

    //protected MeshFilter meshFilter;

    //protected MeshRenderer meshRenderer;

    //protected Mesh mesh;

    //public GameObject Cube
    //{
    //    get
    //    {
    //        if (g == null)
    //        {
    //            g = new GameObject();
    //            meshRenderer = g.AddComponent<MeshRenderer>();
    //            meshFilter = g.AddComponent<MeshFilter>();
    //        }
    //        return g;
    //    }
    //}

    public void Build()
    {
        //mesh = new Mesh();
        BaseMeshBuilder.BuildMesh();
        //UpdateMesh();
        //meshFilter.mesh = mesh;
    }

    protected virtual void UpdateMesh()
    {
        //mesh.Clear();

        //mesh.vertices = BaseMeshBuilder.Vertices;
        //mesh.triangles = BaseMeshBuilder.triangles;

        //mesh.uv = BaseMeshBuilder.colorData;

        //mesh.RecalculateNormals();
    }

    protected BaseMeshBuilder<Vector2> BaseMeshBuilder
    {
        get
        {
            if (baseMeshBuilder == null)
            {
                baseMeshBuilder = new BaseMeshBuilder<Vector2>(
                    CreateShape,
                    GetColorAt,
                    GetXSize,
                    GetZSize,
                    GetCurrentVertexPosition) ;
            }
            return baseMeshBuilder;
        }
    }

    protected void CreateShape(out Vector3[] shape)
    {
        shape = new Vector3[BaseMeshBuilder.SimpleVerticesCount];

        int currentPointIndex = 0;
        for (int z = 0; z < BaseMeshBuilder.VerticesZCount; z++)
        {
            for (int x = 0; x < BaseMeshBuilder.VerticesXCount; x++)
            {
                shape[currentPointIndex] = GetCurrentVertexPosition(x, z);
                currentPointIndex++;
            }
        }
    }

    protected int ProgressToIndex(float progress)
    {
        return Mathf.RoundToInt(progress * GetZSize()); 
    }

    protected virtual float GetCurrentY(int x, int z)
    {
        float result;

        if (z == 0)
        {
            result = GetCurrentY((x + 1) % 5, z + 1);
        }
        else if (z == GetZSize())
        {
            result = GetCurrentY((x + 1) % 5, z - 1);
        }
        else
        {

            switch (x)
            {
                case 0:
                case 1:
                case 4:
                    {
                        result = height / 2;
                        break;
                    }
                case 2:
                case 3:
                    {
                        result = -height / 2;
                        break;
                    }
                default:
                    {
                        throw new System.ArgumentException("X can`t be higher than 4 yet it is: " + x);
                    }
            }
        }
        return result;
    }

    protected virtual float GetCurrentX(int x, int z)
    {

        float result = 0;


        switch (x)
        {
            case 0:
            case 3:
            case 4:
                {
                    result -= width / 2;
                    break;
                }
            case 1:
            case 2:
                result += width / 2;
                break;

            default:
                {
                    throw new System.ArgumentException("X must be within the range of 0 and 4 yet it is " + x);
                }

        }
        return result;

    }

    protected virtual float GetCurrentZ(int x, int z)
    {
        return ToZProgress(z) * length - length / 2;
    }

    protected float ToZProgress(int z)
    {
        if (z <= 1)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    protected virtual Vector3 GetCurrentVertexPosition(int x, int z)
    {
        return new Vector3(GetCurrentX(x, z), GetCurrentY(x, z), GetCurrentZ(x, z));
    }


    protected int GetXSize() => 4;

    protected int GetZSize() => 4;



    protected virtual Vector2 GetColorAt(float xProgress, float zProgress, float height)
    {
        return new Vector2(xProgress, zProgress);
    }

}
