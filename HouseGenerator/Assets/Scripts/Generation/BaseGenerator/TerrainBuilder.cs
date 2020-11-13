using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TerrainBuilder<T> : MeshBuilder<T>, IHeightInfo
{

    [Tooltip("The width of the terrain.")]
    [Range(1, 2000)]
    [Save]
    [SerializeField]
    public int terrainWidth = 50;

    public virtual int TerrainWidth => terrainWidth;

    [Tooltip("The length of the terrain.")]
    [Range(1, 2000)]
    [Save]
    public int terrainLength = 50;

    protected MeshCollider meshCollider;

    protected Vector3 offset;

    public Vector3 Offset
    {
        get
        {
            return offset;
        }
    }
    
    protected virtual void ShowTerrain(bool updateCollider)
    {
        if (updateCollider)
        {
            InitializeWithCollider();
        }
        else
        {
            BuildMesh();
        }
    }

    public void InitializeWithCollider()
    {
        BuildMesh();
        SetMeshAsCollision(mesh);
    }

    protected override void BuildMesh()
    {
        SetOffset();
        base.BuildMesh();
    }

    protected MeshCollider MeshCollider
    {
        get
        {
            if (meshCollider == null)
            {
                meshCollider = GetComponent<MeshCollider>();
            }
            return meshCollider;
        }
    }

    protected void SetMeshAsCollision(Mesh mesh)
    {
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = mesh;
        }
    }

    public void UpdateMeshWithCollider()
    {
        base.UpdateMesh();
        SetMeshAsCollision(mesh);
    }

    protected void SetOffset()
    {
        offset = GetCenteredMesh();
    }

    protected override Vector3 GetCurrentVertexPosition(int x, int z)
    {
        return new Vector3(GetCurrentPlainX(x, z) - offset.x, GetCurrentY(x, z), GetCurrentPlainZ(x, z) - offset.z);
    }

    protected virtual float GetCurrentPlainX(int x, int z)
    {
        return x;
    }

    protected virtual float GetCurrentPlainZ(int x, int z)
    {
        return z;
    }

    protected virtual Vector3 GetCenteredMesh()
    {
        return new Vector3(TerrainWidth / 2, 0, terrainLength / 2);
    }

    protected float ToScaledXProgress(float x)
    {
        return x / XSize;
    }

    public Vector2 ToScaledProgress(float x, float z)
    {
        return ToScaledProgress(new Vector2(x, z));
    }

    public Vector2 ToScaledProgress(Vector2 position)
    {
        return new Vector2(ToScaledXProgress(position.x), ToScaledZProgress(position.y));
    }

    /// <summary>
    /// transforms a local position to the x and z progress on the map
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2 LocalPosToProgress(Vector2 position)
    {
        return new Vector2(ToXProgress(position.x), ToZProgress(position.y));
    }

    public Vector2Int ProgressToXAndZIndex(Vector2 progress)
    {
        return new Vector2Int(Mathf.RoundToInt(progress.x * XSize), Mathf.RoundToInt(progress.y * ZSize));
    }

    /// <summary>
    /// transforms a global position to the x and z progress on the map
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2 GlobalPosToProgress(Vector3 position)
    {
        Vector3 localPosition = TransformToAnkorPosition(position);
        return new Vector2(ToXProgress(localPosition.x), ToZProgress(localPosition.z));
    }

    public bool TryGlobalPosToProgress(Vector3 position, out Vector2 progress)
    {
        progress = GlobalPosToProgress(position);
        bool result = progress.x <= 1 && progress.x >= 0 && progress.y <= 1 && progress.y >= 0;
        return result;
    }

    public Vector2 GlobalPosToProgress(Vector2 position)
    {
        return GlobalPosToProgress(new Vector3(position.x, 0, position.y));
    }

    /// <summary>
    /// not working!
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2 GlobalPosToRotatedProgress(Vector3 position)
    {
        Vector3 distance = transform.position - position;
        distance = transform.TransformDirection(distance) * -1 + offset;
        return LocalPosToProgress(new Vector2(distance.x, distance.z));
    }

    protected virtual Vector3 TransformToAnkorPosition(Vector3 pos)
    {
        return pos - (transform.position - offset);
    }

    protected virtual Vector3 TransformToLocalPosition(Vector3 pos)
    {
        return pos - transform.position;
    }

    /// <summary>
    /// returns local x and z position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    protected virtual Vector2 TransformToLocal2DPosition(Vector3 pos)
    {
        Vector3 result = TransformToLocalPosition(pos);
        return new Vector2(result.x, result.z);
    }

    protected virtual Vector2 TransformToAnkor2DPosition(Vector3 pos)
    {
        Vector3 result = TransformToAnkorPosition(pos);
        return new Vector2(result.x, result.z);
    }

    protected float ToXProgress(float z)
    {
        return z / TerrainWidth;
    }

    protected float ToScaledZProgress(float z)
    {
        return z / ZSize;
    }

    protected float ToZProgress(float z)
    {
        return z / terrainLength;
    }

    //public List<IndexInfo> GetNearestIndicies(Vector2 progress, int range, bool calculateCircular)
    //{
    //    List<IndexInfo> result = new List<IndexInfo>();
    //    List<IndexInfo> latelyAdded = new List<IndexInfo>();



    //    return result;
    //}

    //protected void ExtendIndex(System.Func<Vector2> direction, List<IndexInfo> indicies)
    //{
    //    int count = indicies.Count;
    //    for(int i = 0; i < count; i++)
    //    {
    //        indicies.Add(MoveOnX);
    //    }
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="calculateCircular">will transition between outermost indicies</param>
    /// <returns></returns>
    public IndexInfo[] GetNearestIndicies(Vector2 progress, bool calculateCircular = false)
    {
        IndexInfo[] result = new IndexInfo[4];

        int xVerticesLenght = VerticesXCount;

        result[0] = GetNearestIndexInfo(progress);

        IndexInfo first = GetNearestIndexInfo(progress);
        result[0] = first;

        if (result[0].OriginalXDiff != 0)
        {
            result[1] = MoveOnX(first, calculateCircular);
        }
        if (result[0].OriginalZDiff != 0)
        {
            result[2] = MoveOnZ(first);
        }
        if (result[0].OriginalXDiff != 0 && result[0].OriginalZDiff != 0)
        {
            result[3] = MoveOnZ(result[1]);
        }

        float zIndex = ((Vertices.Length - xVerticesLenght)) * progress.y;
        float modDiff = zIndex % xVerticesLenght;


        zIndex = (zIndex - modDiff) + Mathf.Round(modDiff / xVerticesLenght) * xVerticesLenght;
        return result;
    }

    private IndexInfo MoveOnX(IndexInfo previous, bool calculateCircular = false)
    {
        if (previous == null)
        {
            return null;
        }

        IndexInfo result = new IndexInfo();

        result.OriginalXDiff = 1 - Mathf.Abs(previous.OriginalXDiff);

        if (!calculateCircular && previous.OriginalXDiff % VerticesXCount
            > result.OriginalXDiff % VerticesXCount)
        {
            ///skip index on next row
            result = null;
        }
        else
        {
            int delta = (int)Mathf.Sign(previous.OriginalXDiff);
            result.index = previous.index + delta;
            result.OriginalZDiff = previous.OriginalZDiff;
            result.xIndex = previous.xIndex + delta;
            result.zIndex = previous.zIndex;
        }

        return result;
    }

    private IndexInfo MoveOnZ(IndexInfo previous)
    {
        if (previous == null)
        {
            return null;
        }

        IndexInfo result = new IndexInfo();

        int delta = (int)Mathf.Sign(previous.OriginalZDiff) * VerticesXCount;
        result.index = previous.index + delta;
        result.OriginalZDiff = 1 - Mathf.Abs(previous.OriginalZDiff);
        result.OriginalXDiff = previous.OriginalXDiff;
        result.xIndex = previous.xIndex;
        result.zIndex = previous.zIndex + delta;

        return result;
    }

    private IndexInfo MoveOnZTowards(IndexInfo previous, int direction)
    {
        if (previous == null)
        {
            return null;
        }

        IndexInfo result = new IndexInfo();
        int directionSign = (int)Mathf.Sign(direction);
        result.index = previous.index + directionSign * VerticesXCount;
        result.OriginalZDiff = directionSign - Mathf.Abs(previous.OriginalZDiff);
        result.OriginalXDiff = previous.OriginalXDiff;

        return result;
    }

    public IndexInfo GetNearestIndexInfo(Vector2 progress)
    {
        IndexInfo result = new IndexInfo();
        int xVerticesLenght = VerticesXCount;
        float zIndex = (Vertices.Length - xVerticesLenght) * progress.y;

        float indexMod = zIndex % xVerticesLenght;

        float modDiffZAxis = indexMod / xVerticesLenght;

        zIndex -= indexMod;

        result.OriginalZDiff = modDiffZAxis - Mathf.Round(modDiffZAxis);

        float xIndex = progress.x * XSize;
        result.xIndex = (int)xIndex;

        float modDiffXAxis = (xIndex % 1);
        result.OriginalXDiff = progress.x - Mathf.RoundToInt(progress.x);
        zIndex = zIndex + Mathf.Round(modDiffZAxis) * xVerticesLenght;
        result.zIndex = (int)zIndex;
        result.index = result.zIndex + result.xIndex;
        return result;
    }

    protected int GetNearestIndex(Vector2 progress)
    {
        return GetNearestIndexInfo(progress).index;
    }

    public float GetAccurateLocalHeightOnProgress(Vector2 progress)
    {
        float height = 0;

        foreach (IndexInfo info in GetTriangleIndiciesFor(progress))
        {
            if (info != null)
            {
                height += Vertices[info.index].y * Mathf.Max(0, (1 - (info.DistanceToOriginal)));
            }
        }

        return height;
    }

    public Vector2 ShapeVec(Vector3 pos)
    {
        return new Vector2(pos.x, pos.z);
    }

    public IndexInfo[] GetCircleIndiciesAroundPosition(Vector2 position, int radius)
    {
        return GetIndiciesAroundPosition(position, radius * 2, radius * 2, (x, z) => x * x + z * z <= radius * radius);
    }

    public IndexInfo[] GetBoxIndiciesAroundPosition(Vector2 position, int width, int length)
    {
        return GetIndiciesAroundPosition(position, width, length, (_, __) => true);
    }

    public IndexInfo[] GetIndiciesAroundPosition(Vector2 position, int width, int length, System.Func<int, int, bool> useIndex)
    {
        IndexInfo[] result = new IndexInfo[width * length];

        IndexInfo startInfo = GetNearestIndexInfo(GlobalPosToProgress(position));

        int startX = startInfo.xIndex - (width / 2);

        int middleZ = startInfo.zIndex / VerticesXCount;

        int startZ = middleZ - length / 2;

        int startIndex = startX + startZ;
        int currentIndex = startIndex;
        IndexInfo info;
        int count = 0;
        for (int z = startZ; z < length + startZ; z++)
        {
            for (int x = startX; x < width + startX; x++)
            {
                info = null;
                int originalXDiff = Mathf.Abs(startInfo.xIndex - x);
                int originalZDiff = Mathf.Abs(middleZ - z);
                if (x >= 0 && x < VerticesXCount && z >= 0 && z * VerticesXCount < Vertices.Length && useIndex(originalXDiff, originalZDiff))
                {
                    info = new IndexInfo();
                    info.OriginalXDiff = originalXDiff;
                    info.OriginalZDiff = originalZDiff;
                    info.xIndex = x;
                    info.zIndex = z * VerticesXCount;
                    info.index = info.zIndex + x;
                }
                result[count] = info;
                count++;
            }
        }
        return result;
    }

    //public float GetAccurateLocalHeightOnProgress(Vector2 progress)
    //{
    //    float height = 0;
    //    float totalDistanceToOriginal = 0;
    //    IndexInfo[] infos = GetTriangleIndiciesFor(progress);
    //    IndexInfo maxDistance info = -1;
    //    float maxDistance = 0;
    //    foreach (IndexInfo info in infos)
    //    {
    //        if (info != null)
    //        {
    //            totalDistanceToOriginal += info.DistanceToOriginal;
    //            if (info.DistanceToOriginal >= maxDistance)
    //            {
    //                maxDistance = info.DistanceToOriginal;
    //            }
    //        }
    //    }
    //    foreach (IndexInfo info in infos)
    //    {
    //        if (info != null)
    //        {
    //            height += vertices[info.index].y * 1 - (info.DistanceToOriginal / totalDistanceToOriginal);
    //        }
    //    }

    //    return height;
    //}

    protected IndexInfo[] GetTriangleIndiciesFor(Vector2 progress)
    {
        IndexInfo[] result = new IndexInfo[3];

        IndexInfo first = GetNearestIndexInfo(progress);
        result[0] = first;

        if (first.OriginalXDiff == 0)
        {
            if (first.OriginalZDiff != 0)
            {
                result[1] = MoveOnZ(first);
            }
        }
        else
        {
            if (first.OriginalZDiff == 0)
            {
                result[1] = MoveOnX(first);
            }
            else
            {
                #region totalProgress
                float totalLocalZProgess;
                if (first.OriginalZDiff < 0)
                {
                    totalLocalZProgess = 1 + first.OriginalZDiff;
                }
                else
                {
                    totalLocalZProgess = first.OriginalZDiff;
                }
                float totalLocalXProgess;
                if (first.OriginalXDiff < 0)
                {
                    totalLocalXProgess = 1 + first.OriginalXDiff;
                }
                else
                {
                    totalLocalXProgess = first.OriginalXDiff;
                }
                #endregion
                if (totalLocalZProgess > totalLocalXProgess)
                {
                    ///is upper left triangle
                    if (first.OriginalXDiff > 0 && first.OriginalZDiff < 0)
                    {
                        ///startPoint is top left
                        result[1] = MoveOnZ(first);
                        result[2] = MoveOnX(first);
                    }
                    else if (first.OriginalXDiff < 0)
                    {
                        ///startPoint is top right
                        result[1] = MoveOnX(first);
                        result[2] = MoveOnZ(result[1]);
                    }
                    else
                    {
                        ///startPoint is bottom left
                        result[1] = MoveOnZ(first);
                        result[2] = MoveOnX(result[1]);
                    }
                }
                else
                {
                    ///is lower right triangle
                    if (first.OriginalXDiff < 0 && first.OriginalZDiff > 0)
                    {
                        ///startPoint is bottom right
                        result[1] = MoveOnZ(first);
                        result[2] = MoveOnX(first);
                    }
                    else if (first.OriginalXDiff > 0)
                    {
                        ///startPoint is bottom left
                        result[1] = MoveOnX(first);
                        result[2] = MoveOnZ(result[1]);
                    }
                    else
                    {
                        ///startPoint is top right
                        result[1] = MoveOnZ(first);
                        result[2] = MoveOnX(result[1]);
                    }
                }
            }
        }

        return result;
    }

    public float HeightOnPosition(Vector2 position)
    {
        return GetAccurateLocalHeightOnProgress(GlobalPosToProgress(position)) /*+ transform.position.y*/;
    }

    public float AbsoluteHeightOnNearestIndex(Vector2 position)
    {
        return Vertices[GetNearestIndex(GlobalPosToProgress(position))].y + transform.position.y;
    }

    public float RelativeHeightOnNearestIndex(Vector2 position)
    {
        return Vertices[GetNearestIndex(GlobalPosToProgress(position))].y;
    }
}
