using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;
using System;
using UnityEngine.XR;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class WallWithHoles : MonoBehaviour
{


    public float wallLength;

    public float wallHeight;

    public float wallThickness;

    public Material m;

    public List<WallObstacle> obstacles;

    protected List<AdvancedCubeBuilder> wallParts = new List<AdvancedCubeBuilder>();

    public List<WallObstacleScriptableObject> obstaclesObjects;
   
    protected MeshFilter meshFilter;

    protected IEnumerable<WallObstacle> allObstaclesSorted =>
        obstaclesObjects
        .Select(o => new WallObstacle(o)).Concat(obstacles)
        .OrderBy((h) => h.bottomLeftAnchorPosition.x)
        .ToList();

    protected MeshFilter MeshFilter
    {
        get
        {
            if (meshFilter == null)
            {
                meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
            }
            return meshFilter;
        }
    }

    protected MeshRenderer meshRenderer;

    protected MeshRenderer MeshRenderer
    {
        get
        {
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>();
            }
            return meshRenderer;
        }
    }

    protected Mesh mesh;

    protected Tuple<int[], Vector2[], Vector3[]> EmptyMesh =>
        Tuple.Create(new int[0], new Vector2[0], new Vector3[0]);

    protected void ApplyTupleToMesh(Tuple<int[], Vector2[], Vector3[]> t)
    {
        if (mesh != null)
        {
            mesh?.Clear();
        }
        mesh = new Mesh();
        mesh.vertices = t.Item3;
        mesh.triangles = t.Item1;

        mesh.uv = t.Item2;

        MeshFilter.mesh = mesh;
        MeshRenderer.material = m;
        mesh.RecalculateNormals();
    }

    protected float wallLengthDone;

    protected Vector2 MaxSize => new Vector2(wallLength, wallHeight);

    private void Start()
    {
        BuildWall();
    }

    private void OnValidate()
    {
        BuildWall();
    }
    
    protected void ResetWall()
    {
        if (mesh != null)
        {
            mesh.Clear();
        }
        wallLengthDone = 0;
        wallParts.Clear();
    }

    protected void BuildWall()
    {
        ResetWall();
        CreateWall();
    }

    protected void CombineWallMeshes()
    {
        ApplyTupleToMesh(wallParts
            .Select(w => w.baseMeshBuilder)
            .Aggregate(EmptyMesh, (t, meshB) =>
               BaseMeshBuilder<Vector2>.MergeMesh(
                   t.Item1, t.Item2, t.Item3, 
                   meshB.triangles, meshB.colorData, meshB.vertices)));
    }

    //private void OnValidate()
    //{
    //    foreach (var item in wallParts)
    //    {
    //        Destroy(item.g);
    //    }
    //    Start();
    //}

    protected void CreateWall()
    {
        foreach (WallObstacle o in allObstaclesSorted)
        {
            CreateWallToObstacle(o);
            wallLengthDone = o.bottomLeftAnchorPosition.x;
            CreateWallsAroundObstacle(o);
            wallLengthDone = o.bottomLeftAnchorPosition.x + o.obstacleSize.x;
        }
        BuildLastWallPart();

        CombineWallMeshes();
    }

    protected void BuildLastWallPart()
    {
        CreateWallToObstacle(
            new WallObstacle() 
            { bottomLeftAnchorPosition = new Vector2(wallLength, 0) }
        );
    }

    protected void CreateWallToObstacle(WallObstacle o)
    {
        Vector2 offset = new Vector2(wallLengthDone, 0);
        Vector3 wallSize = new Vector3(wallThickness, wallHeight, o.bottomLeftAnchorPosition.x - wallLengthDone);
        AdvancedCubeBuilder b = new AdvancedCubeBuilder (MaxSize, wallSize, offset, transform, m);
        HandleCreatedWall(b);
    }

    protected void CreateWallsAroundObstacle(WallObstacle o)
    {
        CreateWallUnderObstacle(o);
        CreateWallOverObstacle(o);
    }

    protected void CreateWallUnderObstacle(WallObstacle o)
    {
        if (o.bottomLeftAnchorPosition.y > 0)
        {
            float upperYPos = 
                Mathf.Min(o.bottomLeftAnchorPosition.y, wallHeight);

            Vector2 offset = new Vector2(wallLengthDone, 0);
            Vector3 wallSize = new Vector3(
                wallThickness, 
                upperYPos, 
                o.bottomLeftAnchorPosition.x - wallLengthDone + o.obstacleSize.x);
            AdvancedCubeBuilder b = new AdvancedCubeBuilder(MaxSize, wallSize, offset, transform, m);
            HandleCreatedWall(b);
        }
    }

    protected void CreateWallOverObstacle(WallObstacle o)
    {
        float upperYPos = Mathf.Clamp(o.bottomLeftAnchorPosition.y + o.obstacleSize.y, 0, wallHeight);
        if (upperYPos < wallHeight)
        {
            Vector2 offset = new Vector2(wallLengthDone, upperYPos);
            Vector3 wallSize = new Vector3(
                wallThickness, 
                wallHeight - upperYPos, 
                o.bottomLeftAnchorPosition.x - wallLengthDone + o.obstacleSize.x);
            AdvancedCubeBuilder b = new AdvancedCubeBuilder(MaxSize, wallSize, offset, transform, m);
            HandleCreatedWall(b);
        }
    }

    protected void HandleCreatedWall(AdvancedCubeBuilder b) 
    {
        wallParts.Add(b);
        b.Build();
    }

}
