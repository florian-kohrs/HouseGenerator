using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameObjectExtension
{

    public static T GetInterface<T>(this GameObject g) where T : class
    {
        MonoBehaviour[] ms = g.GetComponents<MonoBehaviour>();
        foreach(MonoBehaviour m in ms)
        {
            if(m is T)
            {
                return m as T;
            }
        }
        return null;
    }

    public static List<T> GetInterfaces<T>(this GameObject g) where T : class
    {
        List<T> result = new List<T>();
        MonoBehaviour[] ms = g.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour m in ms)
        {
            if (m is T)
            {
                result.Add(m as T);
            }
        }
        return result;
    }

    public static T GetInterface<T>(this MonoBehaviour mono) where T : class
    {
        return mono.gameObject.GetInterface<T>();
    }

    public static List<T> GetInterfaces<T>(this MonoBehaviour mono) where T : class
    {
        return mono.gameObject.GetInterfaces<T>();
    }

    public static T GetOrAddComponent<T>(this Component c, Action<T> setupOnCreate = null) where T : Component
    {
        return c.gameObject.GetOrAddComponent<T>(setupOnCreate);
    }

    public static T GetOrAddComponent<T>(this GameObject g, Action<T> setupOnCreate = null) where T : Component
    {
        T c = g.GetComponent<T>();
        if (c == null)
        {
            c = g.AddComponent<T>();
            setupOnCreate?.Invoke(c);
        }
        return c;
    }

}
