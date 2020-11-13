using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazyComponent<T> where T : Component
{

    public T GetValue(MonoBehaviour source)
    {
        if (value == null)
        {
            value = source.GetComponent<T>();
        }
        return value;
    }

    private T value;

}
