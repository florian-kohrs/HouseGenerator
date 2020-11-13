using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializeableDictionary : ISerializationCallbackReceiver
{

    public List<Vector2> keys = new List<Vector2>();
    public List<float> values = new List<float>();

    public Dictionary<Vector2, float> dict = new Dictionary<Vector2, float>();


    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        dict = new Dictionary<Vector2, float>();

        for (var i = 0; i != Math.Min(keys.Count, values.Count); i++)
        {
            dict.Add(keys[i], values[i]);
        }
    }

}

