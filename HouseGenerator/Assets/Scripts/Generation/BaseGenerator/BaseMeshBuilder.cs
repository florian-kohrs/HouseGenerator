using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMeshBuilder<T>
{

    public delegate void ShapeCreator(out Vector3[] shape);


    public static Tuple<int[],T[], Vector3[]> MergeMesh(Mesh m1, Mesh m2, Func<Mesh,T[]> getColor)
    {
        return MergeMesh(m1.triangles, getColor(m1), m1.vertices,  m2.triangles, getColor(m2), m2.vertices);
    }

    public static Tuple<int[], T[], Vector3[]> MergeMesh
        (int[] tris1, T[] colors1, Vector3[] vertices1, 
        int[] tris2, T[] colors2, Vector3[] vertices2)
    {
        int[] newTris = new int[tris1.Length + tris2.Length];
        T[] newColor = new T[colors1.Length + colors2.Length];
        Vector3[] newVertices = new Vector3[vertices1.Length + vertices2.Length];

        //cant compile since size fo struct is unsafe
        //Buffer.BlockCopy(vertices1, 0, newVertices, 0, sizeof(Vector3) * vertices1.Length);

        for (int i = 0; i < vertices1.Length; i++)
        {
            newVertices[i] = vertices1[i];
        }
        for (int i = 0; i < vertices2.Length; i++)
        {
            newVertices[vertices1.Length + i] = vertices2[i];
        }

        for (int i = 0; i < tris1.Length; i++)
        {
            newTris[i] = tris1[i];
        }
        for (int i = 0; i < tris2.Length; i++)
        {
            newTris[tris1.Length + i] = tris2[i] + vertices1.Length;
        }

        for (int i = 0; i < colors1.Length; i++)
        {
            newColor[i] = colors1[i];
        }
        for (int i = 0; i < colors2.Length; i++)
        {
            newColor[colors1.Length + i] = colors2[i];
        }

        return Tuple.Create(newTris, newColor, newVertices);
    }

    public BaseMeshBuilder(ShapeCreator shapeCreator,
        Func<float,float,float,T> GetColorAt, 
        Func<int> XSize, 
        Func<int> ZSize,
        Func<int,int, Vector3> GetCurrentVertexPosition, bool shadeFlat = true)
    {
        this.shapeCreator = shapeCreator;
        this.XSize = XSize;
        this.ZSize = ZSize;
        this.GetColorAt = GetColorAt;
        this.GetCurrentVertexPosition = GetCurrentVertexPosition;
        this.useFlatShading = shadeFlat;
    }

    protected ShapeCreator shapeCreator;

    private const int POINTS_PER_TRIANGLE = 3;

    public Func<float, float, float, T> GetColorAt;

    public Func<int> XSize;

    public Func<int> ZSize;

    public Func<int, int, Vector3> GetCurrentVertexPosition;

    public Vector3[] vertices;

    public Vector3[] Vertices => vertices;
    
    public T[] colorData;

    public int[] triangles;

    public bool useFlatShading = false;

    public int VerticesXCount
    {
        get
        {
            return XSize.Invoke() + 1;
        }
    }

    public int VerticesZCount
    {
        get
        {
            return ZSize.Invoke() + 1;
        }
    }

    public int SimpleVerticesCount
    {
        get
        {
            return VerticesXCount * VerticesZCount;
        }
    }

    public int SimpleTriangleCount
    {
        get
        {
            return ZSize.Invoke() * XSize.Invoke() * 2;
        }
    }

    
    public virtual void BuildMesh()
    {
        shapeCreator(out vertices);
        DrawCurrentVertices();
        BuildUvs();
        if (useFlatShading)
        {
            ShadeFlat();
        }
    }

    private void ShadeFlat()
    {
        ShadeFlat(ref vertices, triangles, ref colorData);
    }

    public static void ShadeFlat<K>(ref Vector3[] vertices, int[] triangles, ref K[]colorData)
    {
        Vector3[] flatshededVertices = new Vector3[triangles.Length];
        K[] flatshededUVs = new K[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            flatshededVertices[i] = vertices[triangles[i]];
            flatshededUVs[i] = colorData[triangles[i]];
            triangles[i] = i;
        }
        colorData = flatshededUVs;
        vertices = flatshededVertices;
    }


    public void BuildUvs()
    {
        colorData = new T[vertices.Length];

        int index = 0;
        for (int z = 0; z < VerticesZCount; z++)
        {
            for (int x = 0; x < VerticesXCount; x++)
            {
                colorData[index] = GetColorAt((float)x / (XSize.Invoke()), (float)z / (ZSize.Invoke() ), vertices[index].y);
                index++;
            }
        }
    }


    public virtual void DrawCurrentVertices()
    {
        triangles = new int[SimpleTriangleCount * POINTS_PER_TRIANGLE];

        int currentPointIndex = 0;
        int finishedRectangleCount = 0;
        for (int z = 0; z < ZSize.Invoke(); z++)
        {
            for (int x = 0; x < XSize.Invoke(); x++)
            {
                DrawRectangle(ref currentPointIndex, ref finishedRectangleCount);
            }
            ///skip one rectangle so no connection is build
            ///from the most right to the most left node
            finishedRectangleCount++;
        }
    }

    /// <summary>
    /// will draw the next 2 triangles to finish the rectangle
    /// </summary>
    private void DrawRectangle(ref int currentPointIndex, ref int finishedRectangle)
    {
        triangles[currentPointIndex++] = finishedRectangle;
        triangles[currentPointIndex++] = finishedRectangle + VerticesXCount;
        triangles[currentPointIndex++] = finishedRectangle + 1;
        triangles[currentPointIndex++] = finishedRectangle + 1;
        triangles[currentPointIndex++] = finishedRectangle + VerticesXCount;
        triangles[currentPointIndex++] = finishedRectangle + VerticesXCount + 1;
        finishedRectangle++;
    }

}
