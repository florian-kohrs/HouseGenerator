using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INavigatable<T,J>
{

    T[] GetCircumjacent(T field);

    float DistanceToTarget(T from, J to);

    float DistanceToField(T from, T to);

    bool ReachedTarget(T current, J destination);

    bool IsEqual(T t1, T t2);

}
