using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathfinder<T>
{

    void StartPath(T to);
    
}
