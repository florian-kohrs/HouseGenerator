using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// code modified from http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html
/// </summary>

public class IcoSphere : MonoBehaviour
{

    protected const int VERTICES_AT_LOD_ZERO = 2;

    protected const int TRIANGLE_AT_LOD_ZERO = 15;

    public float radius;

    protected int[] vertexCountsForLods;

    protected int TrianglesAtLod(int lod) => TRIANGLE_AT_LOD_ZERO * (int)Mathf.Pow(4, levelOfDetail);

    protected int VerticesAtLod(int lod) => vertexCountsForLods[lod - 1];

    [Range(1, 6)]
    public int levelOfDetail = 3;

    protected virtual Vector3 EditPointOnPlanet(Vector3 point) => point * radius;

    protected Color[] colorData;

    protected virtual void BuildUvs()
    {
        colorData = new Color[VerticesAtLod(levelOfDetail)];

        int index = 0;
        for (int i = 0; i < colorData.Length; i++)
        {
            colorData[index] = Color.gray;
            index++;
        }
    }

    protected Vector3[] vertices;
    protected int[] triangles;
    protected int index;
    private Dictionary<long, int> middlePointIndexCache;

    protected virtual void OnValidate()
    {
        BuildPlanet();
    }


    public void BuildPlanet()
    {
        vertexCountsForLods = new int[] { 12, 42, 162, 642, 2562, 10242, 41912 };
        vertices = new Vector3[VerticesAtLod(levelOfDetail)];
        triangles = new int[TrianglesAtLod(levelOfDetail)];
        Create(levelOfDetail - 1);
        BuildUvs();
        BaseMeshBuilder<Color>.ShadeFlat(ref vertices, triangles, ref colorData);
        Mesh m = new Mesh();
        m.vertices = vertices;
        m.triangles = triangles;
        m.colors = colorData;
        m.RecalculateNormals();

        this.GetOrAddComponent<MeshFilter>().mesh = m;
    }

    // add vertex to mesh, fix position to be on unit sphere, return index
    private int AddVertex(Vector3 p)
    {
        float length = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
        Vector3 vertex = new Vector3(p.x / length, p.y / length, p.z / length);
        vertex = EditPointOnPlanet(vertex);
        vertices[index] = vertex;
        return index++;
    }

    // return index of point in the middle of p1 and p2
    private int GetMiddlePoint(int p1, int p2)
    {
        // first check if we have it already
        bool firstIsSmaller = p1 < p2;
        int smallerIndex = firstIsSmaller ? p1 : p2;
        int greaterIndex = firstIsSmaller ? p2 : p1;
        long key = ((long)smallerIndex << 32) + greaterIndex;

        int ret;
        if (middlePointIndexCache.TryGetValue(key, out ret))
        {
            return ret;
        }

        // not in cache, calculate it
        Vector3 point1 = this.vertices[p1];
        Vector3 point2 = this.vertices[p2];
        Vector3 middle = new Vector3(
            (point1.x + point2.x) / 2.0f,
            (point1.y + point2.y) / 2.0f,
            (point1.z + point2.z) / 2.0f);

        // add vertex makes sure point is on unit sphere
        int i = AddVertex(middle);

        // store it, return index
        middlePointIndexCache.Add(key, i);
        return i;
    }

    public void Create(int recursionLevel)
    {
        middlePointIndexCache = new Dictionary<long, int>();
        index = 0;

        // create 12 vertices of a icosahedron
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        AddVertex(new Vector3(-1, t, 0));
        AddVertex(new Vector3(1, t, 0));
        AddVertex(new Vector3(-1, -t, 0));
        AddVertex(new Vector3(1, -t, 0));

        AddVertex(new Vector3(0, -1, t));
        AddVertex(new Vector3(0, 1, t));
        AddVertex(new Vector3(0, -1, -t));
        AddVertex(new Vector3(0, 1, -t));

        AddVertex(new Vector3(t, 0, -1));
        AddVertex(new Vector3(t, 0, 1));
        AddVertex(new Vector3(-t, 0, -1));
        AddVertex(new Vector3(-t, 0, 1));


        // create 20 triangles of the icosahedron
        List<Vector3Int> faces = new List<Vector3Int>();

        ///5 faces around point 0
        faces.Add(new Vector3Int(0, 11, 5));
        faces.Add(new Vector3Int(0, 5, 1));
        faces.Add(new Vector3Int(0, 1, 7));
        faces.Add(new Vector3Int(0, 7, 10));
        faces.Add(new Vector3Int(0, 10, 11));


        ///5 adjacent faces
        faces.Add(new Vector3Int(1, 5, 9));
        faces.Add(new Vector3Int(5, 11, 4));
        faces.Add(new Vector3Int(11, 10, 2));
        faces.Add(new Vector3Int(10, 7, 6));
        faces.Add(new Vector3Int(7, 1, 8));


        /// 5 faces around point 3
        faces.Add(new Vector3Int(3, 9, 4));
        faces.Add(new Vector3Int(3, 4, 2));
        faces.Add(new Vector3Int(3, 2, 6));
        faces.Add(new Vector3Int(3, 6, 8));
        faces.Add(new Vector3Int(3, 8, 9));


        /// 5 adjacent faces 
        faces.Add(new Vector3Int(4, 9, 5));
        faces.Add(new Vector3Int(2, 4, 11));
        faces.Add(new Vector3Int(6, 2, 10));
        faces.Add(new Vector3Int(8, 6, 7));
        faces.Add(new Vector3Int(9, 8, 1));

        for (int i = 0; i < recursionLevel; i++)
        {
            List<Vector3Int> faces2 = new List<Vector3Int>();
            foreach (Vector3Int tri in faces)
            {
                int a = GetMiddlePoint(tri.x, tri.y);
                int b = GetMiddlePoint(tri.y, tri.z);
                int c = GetMiddlePoint(tri.z, tri.x);

                faces2.Add(new Vector3Int(tri.x, a, c));
                faces2.Add(new Vector3Int(tri.y, b, a));
                faces2.Add(new Vector3Int(tri.z, c, b));
                faces2.Add(new Vector3Int(a, b, c));
            }
            faces = faces2;
        }

        int count = 0;
        /// done, now add triangles to mesh
        foreach (var tri in faces)
        {
            triangles[count] = tri.x;
            triangles[count + 1] = tri.y;
            triangles[count + 2] = tri.z;
            count += 3;
        }
    }
    
}
