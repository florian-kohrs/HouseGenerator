using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlanetTriangle : ITriangle
{

    public PlanetTriangle(Transform planet, Vector3Int indizes, SurfaceVertex[] vertices, Dictionary<long, int> middlePointIndexTable) : this(planet, indizes, vertices, 0, middlePointIndexTable) { }

    public static int totalNeighborCount = 0;

    public PlanetTriangle(Transform planet, Vector3Int indizes, SurfaceVertex[] vertices, int depth, Dictionary<long, int> middlePointIndexTable)
    {
        this.planet = planet;
        this.depth = depth;
        this.tri = indizes;
        this.vertices = vertices;
        this.middlePointIndexTable = middlePointIndexTable;
    }

    public PlanetTriangle ClosestFromNeighboursAt(Vector3 point)
    {
        PlanetTriangle result = this;
        float distance = (RotatedMiddlePointOfTriangle.normalized - point.normalized).sqrMagnitude;

        foreach (PlanetTriangle tri in neighbors)
        {
            float newDistance = (tri.RotatedMiddlePointOfTriangle.normalized - point.normalized).sqrMagnitude;
            if (newDistance < distance)
            {
                distance = newDistance;
                result = tri;
            }
        }
        return result;
    }

    /// <summary>
    /// use fog on all triangles (or vertices) the player hasnt discorvered yet
    /// </summary>
    //public bool discovered;

    Dictionary<long, int> middlePointIndexTable;

    public const int SAME_INDEX_COUNT_EXPECTED_FOR_NEIGHBORS = 1;

    protected Transform planet;

    public Vector3Int tri;

    public int[] TriIndizes => new int[] { tri.x, tri.y, tri.z };

    public IList<PlanetTriangle> neighbors = new List<PlanetTriangle>();

    private int neighborCount = 0;

    //public float heightDiff { get; set; }

    private int childCount = 0;

    public int depth;

    protected float steepness;

    public float Steepness
    {
        get
        {
            if(steepness == 0)
            {
                steepness = Vector3.Angle(RotatedNormal, RotatedMiddlePointOfTriangle - planet.position);
            }
            return steepness;
        }
    }

    public bool CanWalkOnTriangle = true;

    protected Vector3 normalNormal;
   
    public Vector3 RotatedNormal
    {
        get
        {
            if (normalNormal == default)
            {
                Vector3 side1 = vertices[tri.y].position - vertices[tri.x].position;
                Vector3 side2 = vertices[tri.z].position - vertices[tri.x].position;

                normalNormal = Vector3.Cross(side1, side2).normalized;
            }

            Vector3 rotatedNormal = Quaternion.Euler(planet.transform.eulerAngles) * normalNormal;
            return rotatedNormal;
        }
    }

    public Vector3 UnrotatedMiddlePointOfTriangle
    {
        get
        {
            if (middlePointOfTriangle == default)
            {
                middlePointOfTriangle = (vertices[tri.x].position + vertices[tri.y].position + vertices[tri.z].position) / 3;
            }

            return Vector3.Scale(middlePointOfTriangle, planet.lossyScale);
        }
    }

    public Vector3 RotatedMiddlePointOfTriangle => Quaternion.Euler(planet.transform.eulerAngles) * UnrotatedMiddlePointOfTriangle;

    public bool CanEnterTriangleFrom(PlanetTriangle t)
    {
        IList<int> entrances = GetNeighborVertices(t);
        bool found = false;
        for (int i = 0; i < entrances.Count && !found; i++)
        {
            found = vertices[entrances[i]].surface.canMoveOnTerrain;
        }
        return found;
    }

    public IList<int> GetNeighborVertices(PlanetTriangle t)
    {
        return GetNeighborVertices(t.TriIndizes);
    }

    public bool IsLastParent => children != null && children[0].IsLeaf;

   
    public IList<int> GetNeighborVertices(int[] vertices)
    {
        IList<int> result = new List<int>(3);
        foreach (int i in vertices)
        {
            if (TriIndizes.Contains(i))
            {
                result.Add(i);
            }
        }
        return result;
    }

    public PlanetTriangle[] children;

    public SurfaceVertex[] vertices;

    public PlanetTriangle parent;


    public bool IsNeighborWith(PlanetTriangle t)
    {
        int[] tris = t.TriIndizes;
        int count = 0;
        for (int i = 0; i < tris.Length && count < SAME_INDEX_COUNT_EXPECTED_FOR_NEIGHBORS; i++)
        {
            if (tris[i] == tri.x || tris[i] == tri.y || tris[i] == tri.z)
            {
                count++;
            }
        }
        return count >= SAME_INDEX_COUNT_EXPECTED_FOR_NEIGHBORS;
    }

    public void SetChildNeighborsWith(PlanetTriangle target)
    {
        if (!IsLeaf)
        {
            List<PlanetTriangle> children = Children.ToList();
            children = children.Where(c => c.IsNeighborWith(target)).ToList();
            foreach (PlanetTriangle child in children)
            {
                if (!child.neighbors.Contains(target))
                {
                    child.AddNeighborsTwoWay(target);
                }
            }
            //NeighborRelevantChildren.Where(c => c.IsNeighborWith(target)).ToList().ForEach(c => c.AddNeighborTwoWay(target));
        }
        else
        {
            Debug.LogError("trianlge has higher lod  then neighbor. Holes  in mesh needs to be fixed");
        }
    }

    public IEnumerable<PlanetTriangle> NeighborRelevantChildren => Children/*.Take(3)*/.Where(c => c.HasAllNeighborsSet == false);

    public int MAX_TRI_NEIGHBOR_COUNT = 12;

    public bool HasAllNeighborsSet => neighborCount == MAX_TRI_NEIGHBOR_COUNT;

    public PlanetTriangle[] Children
    {
        get
        {
            if (children == null)
            {
                children = new PlanetTriangle[4];
            }
            return children;
        }
    }

    public void AddChildren(params PlanetTriangle[] childs)
    {
        foreach (PlanetTriangle child in childs)
        {
            AddChild(child);
        }
    }

    public void AddChild(PlanetTriangle child)
    {
        Children[childCount] = child;
        child.parent = this;
        childCount++;
    }

    public bool IsLeaf => children == null || children[0] == null;

    public void UpdateTransformToTriangle(Transform t, Vector3 forward)
    {
        AlignToTriangle(t, forward);
        PutOnTriangle(t);
    }

    public void AlignToTriangle(Transform t, Vector3 forward)
    {
        t.rotation = Quaternion.LookRotation(RotatedNormal, -forward);
        t.Rotate(new Vector3(90, 0, 0), Space.Self);
    }

    public void PutOnTriangle(Transform t)
    {
        Plane p = new Plane(RotatedNormal, GlobalMiddlePointOfTriangle);
        Vector3 pos = p.ClosestPointOnPlane(t.position);
        t.position = pos;
    }

    public void SetChildNeighbors()
    {
        if (!IsLeaf)
        {
            List<PlanetTriangle> children = NeighborRelevantChildren.ToList();
            for (int i = 0; i < children.Count; i++)
            {
                PlanetTriangle t = children[i];
                for (int x = 0; x < neighbors.Count/* && currentNeighbor == null*/; x++)
                {
                    neighbors[x].SetChildNeighborsWith(t);
                }
            }
            foreach (PlanetTriangle t in Children)
            {
                t.SetChildNeighbors();
            }
        }

    }

    public void AddNeighborTwoWay(PlanetTriangle Neighbor)
    {
        Neighbor.AddNeighbor(this);
        AddNeighbor(Neighbor);
    }

    public void AddNeighborsTwoWay(params PlanetTriangle[] Neighbors)
    {
        foreach (PlanetTriangle n in Neighbors)
        {
            n.AddNeighbor(this);
            AddNeighbor(n);
        }
    }

    public void AddNeighbor(PlanetTriangle neighbor)
    {
        //neighbors[neighborCount] = Neighbor;
        neighbors.Add(neighbor);
        neighborCount++;
        totalNeighborCount++;
    }

    public IEnumerable<Vector3Int> GetAllTriangles()
    {
        if (IsLeaf)
        {
            yield return tri;
        }
        else
        {
            foreach (PlanetTriangle c in children)
            {
                foreach (Vector3Int v in c.GetAllTriangles())
                {
                    yield return v;
                }
            }
        }
    }

    public IEnumerable<PlanetTriangle> GetAllPlanetTriangles()
    {
        if (IsLeaf)
        {
            yield return this;
        }
        else
        {
            foreach (PlanetTriangle c in children)
            {
                foreach (PlanetTriangle v in c.GetAllPlanetTriangles())
                {
                    yield return v;
                }
            }
        }
    }

    public void SplitTriangle(int splitAmount, ref int vertexCount)
    {
        SplitTriangle(0, splitAmount, ref vertexCount);
    }

    public void SplitTriangle(int count, int splitAmount, ref int vertexCount)
    {
        if (count < splitAmount)
        {
            /// replace triangle by 4 triangles
            int a = GetMiddlePoint(tri.x, tri.y, ref vertexCount);
            int b = GetMiddlePoint(tri.y, tri.z, ref vertexCount);
            int c = GetMiddlePoint(tri.z, tri.x, ref vertexCount);

            PlanetTriangle t1 = new PlanetTriangle(planet, new Vector3Int(tri.x, a, c), vertices, depth + 1, middlePointIndexTable);
            t1.SplitTriangle(count + 1, splitAmount, ref vertexCount);

            PlanetTriangle t2 = new PlanetTriangle(planet, new Vector3Int(tri.y, b, a), vertices, depth + 1, middlePointIndexTable);
            t2.SplitTriangle(count + 1, splitAmount, ref vertexCount);

            PlanetTriangle t3 = new PlanetTriangle(planet, new Vector3Int(tri.z, c, b), vertices, depth + 1, middlePointIndexTable);
            t3.SplitTriangle(count + 1, splitAmount, ref vertexCount);

            PlanetTriangle t4 = new PlanetTriangle(planet, new Vector3Int(a, b, c), vertices, depth + 1, middlePointIndexTable);
            t4.SplitTriangle(count + 1, splitAmount, ref vertexCount);

            t4.AddNeighborsTwoWay(t1, t2, t3);
            t1.AddNeighborsTwoWay(t2, t3);
            t2.AddNeighborsTwoWay(t3);

            AddChildren(t1, t2, t3, t4);
        }
    }

    private int GetMiddlePoint(int p1, int p2, ref int vertexCount)
    {
        // first check if we have it already
        bool firstIsSmaller = p1 < p2;
        int smallerIndex = firstIsSmaller ? p1 : p2;
        int greaterIndex = firstIsSmaller ? p2 : p1;
        long key = ((long)smallerIndex << 32) + greaterIndex;

        int ret;
        if (middlePointIndexTable.TryGetValue(key, out ret))
        {
            return ret;
        }

        // not in cache, calculate it
        Vector3 point1 = vertices[p1].position;
        Vector3 point2 = vertices[p2].position;
        Vector3 middle = new Vector3(
            (point1.x + point2.x) / 2.0f,
            (point1.y + point2.y) / 2.0f,
            (point1.z + point2.z) / 2.0f);

        // add vertex makes sure point is on unit sphere
        int i = AddVertex(middle, ref vertexCount);

        /// store it, return index
        middlePointIndexTable.Add(key, i);
        return i;
    }

    private int AddVertex(Vector3 p, ref int vertexCount)
    {
        float length = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
        SurfaceVertex vertex = new SurfaceVertex();
        vertex.position = new Vector3(p.x / length, p.y / length, p.z / length);
        vertices[vertexCount] = vertex;
        return vertexCount++;
    }

    public PlanetTriangle[] GetAccessableCircumjacent()
    {
        return neighbors.Where(n => n.CanEnterTriangleFrom(this)).ToArray();
    }

    private Vector3 middlePointOfTriangle;

    public Vector3 GlobalMiddlePointOfTriangle => RotatedMiddlePointOfTriangle + planet.transform.position;

    public int V1 => tri.x;

    public int V2 => tri.y;

    public int V3 => tri.z;

    public IEnumerable<int> Corners
    {
        get
        {
            yield return tri.x;
            yield return tri.y;
            yield return tri.z;
        }
    }

    public Vector3Int TriIndices => tri;


    /// <summary>
    /// modifies the middle point as if it lies on a perfect sphere
    /// </summary>
    public Vector3 GetSphericalMiddlePointOfTriangle(float radius)
    {
        return RotatedMiddlePointOfTriangle.normalized * radius;
    }

}
