using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITriangle
{

    int V1 { get; }

    int V2 { get; }

    int V3 { get; }

    IEnumerable<int> Corners { get; }

    Vector3Int TriIndices { get; }

    void SplitTriangle(int recursionLevel, ref int usedVertexCount);

}
