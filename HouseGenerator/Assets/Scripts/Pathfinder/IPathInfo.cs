using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathInfo<T>
{

    IEnumerable<T> GetPathParts();

    Vector3 ToPosition(T next);

}
