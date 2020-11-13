using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public abstract class MeshBuilder<T> : SaveableMonoBehaviour, IMeshInfo
{

    protected BaseMeshBuilder<T> baseBuilder;

    protected BaseMeshBuilder<T> BaseBuilder
    {
        get
        {
            if (baseBuilder == null)
            {
                baseBuilder = new BaseMeshBuilder<T>(CreateShape, GetColorAt, () => XSize, () => ZSize, GetCurrentVertexPosition);
            }
            return baseBuilder;
        }
    }

    private MeshFilter filter;

    protected Mesh mesh;


    protected abstract float GetCurrentY(int x, int z);


    protected abstract T GetColorAt(float xProgress, float zProgress, float height);


    protected abstract int XSize { get; }

    protected abstract int ZSize { get; }

    protected abstract Vector3 GetCurrentVertexPosition(int x, int z);

    protected int VerticesXCount => BaseBuilder.VerticesXCount;

    protected int VerticesZCount => BaseBuilder.VerticesZCount;

    protected virtual void BuildMesh()
    {
        mesh = new Mesh();
        BaseBuilder.BuildMesh();
        UpdateMesh();
        MeshFilter.mesh = mesh;
    }

    private MeshFilter meshFilter;

    protected MeshFilter MeshFilter
    {
        get
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }
            return meshFilter;
        }
    }

    private SMeshRenderer meshRenderer;

    protected SMeshRenderer MeshRenderer
    {
        get
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<SMeshRenderer>();
            }
            return meshRenderer;
        }
    }

    public Vector3[] Vertices => BaseBuilder.Vertices;

    protected void UpdateVertices()
    {
        mesh.vertices = Vertices;
        ///may result in worse graphic
        mesh.RecalculateNormals();
    }

    protected virtual void DisplayTexture() { }
    
    public void RebuildMesh()
    {
        BuildMesh();
        UpdateMesh();
    }

    protected virtual void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = Vertices;
        mesh.triangles = BaseBuilder.triangles;
        DisplayTexture();

        mesh.RecalculateNormals();
    }

    protected void DisplayVertexChanges()
    {
        UpdateVertices();
        mesh.RecalculateNormals();
    }

    protected  void CreateShape(out Vector3[] shape)
    {
        shape = new Vector3[BaseBuilder.SimpleVerticesCount];

        int currentPointIndex = 0;
        for (int z = 0; z < BaseBuilder.VerticesZCount; z++)
        {
            for (int x = 0; x < BaseBuilder.VerticesXCount; x++)
            {
                shape[currentPointIndex] = GetCurrentVertexPosition(x, z);
                currentPointIndex++;
            }
        }
    }

    public void ModifyShape()
    {
        for (int z = 0; z < BaseBuilder.VerticesZCount ; z++)
        {
            for (int x = 0; x < BaseBuilder.VerticesXCount; x++)
            {
                BaseBuilder.Vertices[z * BaseBuilder.VerticesXCount + x].y = GetCurrentVertexPosition(x, z).y;
            }
        }

        DisplayVertexChanges();
    }

    //private void OnDrawGizmos()
    //{
    //    if (vertices != null)
    //    {
    //        for (int i = 0; i < vertices.Length; i++)
    //        {
    //            Gizmos.DrawSphere(vertices[i], 0.1f);
    //        }
    //    }
    //}

}
