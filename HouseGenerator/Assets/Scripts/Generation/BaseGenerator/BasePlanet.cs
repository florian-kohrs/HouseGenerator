using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// code modified from http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html
/// </summary>

///fix redundant classes "BasePlanet" "IcoSphere
public abstract class BasePlanet<T, V> : SaveableMonoBehaviour  where T : ITriangle where V : IVertex, new()
{

    #region constants

    public const int PLANET_LAYER_INDEX = 12;

    public const int PLANET_REACHABLE_INDEX = 16;

    protected const int VERTICES_AT_LOD_ZERO = 2;

    protected const int TRIANGLE_AT_LOD_ZERO = 15;
    #endregion

    #region abstracts
    public abstract IEnumerable<Vector3Int> GetTriangles(T triData);

    public abstract IEnumerable<T> GetChildTris(T triData);

    public abstract T CreateTriangle(int x, int y, int z);

    protected abstract void BuildUvs();

    #endregion

    #region virtuals
    protected virtual Vector3 EditPointOnPlanet(Vector3 point) => point * radius;
    protected virtual void OnRootTrianglesDone() { }

    /// <summary>
    /// this method is called when all Triangles are created just before
    /// their actual vertex position are calculated
    /// </summary>
    protected virtual void OnAllPlainTrianglesDone() { }
    #endregion

    #region properties

    protected int TrianglesAtLod(int lod) => TRIANGLE_AT_LOD_ZERO * (int)Mathf.Pow(4, levelOfDetail);

    protected int VerticesAtLod(int lod) => vertexCountsForLods[lod - 1];

    public IEnumerable<T> GetAllBuildedTriangles
    {
        get
        {
            foreach (T root in mainTriangles)
            {
                foreach (T tri in GetChildTris(root))
                {
                    yield return tri;
                }
            }
        }
    }

    #endregion 

    #region inspector values 

    [Save]
    public float radius = 10;

    [Save]
    [Range(1, 6)]
    public int levelOfDetail = 2;

    #endregion region

    #region planet mesh and triangle info

    protected V[] vertices;

    protected List<T> mainTriangles;

    protected Color[] normalColorData;

    protected Color[] flatColorData;

    protected int[] normalTriangles;

    protected int[] flatTriangles;

    protected int usedVertexCount;

    protected Dictionary<long, int> middlePointIndexCache;

    protected Mesh m;

    private int[] vertexCountsForLods;

    #endregion

    public void AlignToPlanet(Transform t, Vector3 forward)
    {
        t.rotation = Quaternion.LookRotation(t.position- transform.position , -forward);
        t.Rotate(new Vector3(90, 0, 0), Space.Self);
    }


    public virtual void BuildPlanet()
    {
        mainTriangles = new List<T>();
        vertexCountsForLods = new int[] { 12, 42, 162, 642, 2562, 10242, 41912 };
        PlanetTriangle.totalNeighborCount = 0;
        vertices = new V[VerticesAtLod(levelOfDetail)];
        normalTriangles = new int[TrianglesAtLod(levelOfDetail)];
        CreatePlanetData(levelOfDetail - 1);
        //CreateDistribution(levelOfDetail - 1);
        BuildUvs();
        vertices = ShadePlanetFlat();
        //Debug.Log("Used Vertices: " + usedVertexCount + ". Vertices after flatshading: " + vertices.Length);
        m = new Mesh();
        m.vertices = vertices.Select(v => v.Vertex).ToArray();
        m.triangles = flatTriangles;
        m.colors = flatColorData;
        m.RecalculateNormals();
        MeshCollider meshC = GetComponent<MeshCollider>();
        if (meshC != null)
        {
            GetComponent<MeshCollider>().sharedMesh = m;
        }
        this.GetOrAddComponent<MeshFilter>().mesh = m;
    }

    public void CreatePlanetData(int recursionLevel)
    {
        middlePointIndexCache = new Dictionary<long, int>();
        usedVertexCount = 0;

        /// create 12 vertices of a icosahedron
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

        /// create 20 triangles of the icosahedron

        T t1 = CreateTriangle(0, 11, 5);
        T t2 = CreateTriangle(0, 5, 1);
        T t3 = CreateTriangle(0, 1, 7);
        T t4 = CreateTriangle(0, 7, 10);
        T t5 = CreateTriangle(0, 10, 11);

        ///5 adjacent faces
        T t6 = CreateTriangle(1, 5, 9);
        T t7 = CreateTriangle(5, 11, 4);
        T t8 = CreateTriangle(11, 10, 2);
        T t9 = CreateTriangle(10, 7, 6);
        T t10 = CreateTriangle(7, 1, 8);

        /// 5 faces around point 3
        T t11 = CreateTriangle(3, 9, 4);
        T t12 = CreateTriangle(3, 4, 2);
        T t13 = CreateTriangle(3, 2, 6);
        T t14 = CreateTriangle(3, 6, 8);
        T t15 = CreateTriangle(3, 8, 9);

        /// 5 adjacent faces 
        T t16 = CreateTriangle(4, 9, 5);
        T t17 = CreateTriangle(2, 4, 11);
        T t18 = CreateTriangle(6, 2, 10);
        T t19 = CreateTriangle(8, 6, 7);
        T t20 = CreateTriangle(9, 8, 1);

        ///setup neighbours

        mainTriangles.AddRange(new T[] { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16, t17, t18, t19, t20 });

        OnRootTrianglesDone();

        mainTriangles.ForEach(tri => tri.SplitTriangle(recursionLevel, ref usedVertexCount));
        //mainTriangles.ForEach(tri => tri.SplitTriangle(UnityEngine.Random.Range(2,recursionLevel + 1), ref usedVertexCount));

        OnAllPlainTrianglesDone();

        //Debug.Log("Expected Vertices: " + vertices.Length + ". Actual Needed: " + usedVertexCount);


        //Debug.Log("total neighbors: " + PlanetTriangle.totalNeighborCount + ". Expected was: " + TrianglesAtLod(levelOfDetail));

        for (int i = 0; i < usedVertexCount; i++)
        {
            vertices[i].Vertex = EditPointOnPlanet(vertices[i].Vertex);
        }

        vertices = vertices.Take(usedVertexCount).ToArray();

        int count = 0;

        foreach (T root in mainTriangles)
        {
            foreach (Vector3Int tri in GetTriangles(root))
            {
                normalTriangles[count] = tri.x;
                normalTriangles[count + 1] = tri.y;
                normalTriangles[count + 2] = tri.z;
                count += 3;
            }
        }
    }

    // add vertex to mesh, fix position to be on unit sphere, return index
    private int AddVertex(Vector3 p)
    {
        float length = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
        V vertex = new V();
        vertex.Vertex = new Vector3(p.x / length, p.y / length, p.z / length);
        vertices[usedVertexCount] = vertex;
        return usedVertexCount++;
    }


    public V[] ShadePlanetFlat()
    {
        V[] flatshededVertices = new V[normalTriangles.Length];
        flatColorData = new Color[normalTriangles.Length];
        flatTriangles = new int[normalTriangles.Length];
        for (int i = 0; i < normalTriangles.Length; i++)
        {
            flatshededVertices[i] = vertices[normalTriangles[i]];
            flatColorData[i] = normalColorData[normalTriangles[i]];
            flatTriangles[i] = i;
        }
        return flatshededVertices;
    }

    protected void FlattenUvs()
    {
        flatColorData = new Color[normalTriangles.Length];
        for (int i = 0; i < normalTriangles.Length; i++)
        {
            flatColorData[i] = normalColorData[normalTriangles[i]];
            flatTriangles[i] = i;
        }
    }

    public void FlattenPlanetColor()
    {
        //BuildUvs();
        FlattenUvs();
        m.colors = flatColorData;
    }

}
