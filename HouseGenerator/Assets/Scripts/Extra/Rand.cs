using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rand
{
    
    public static int Variance(int center, int variance)
    {
        return Random.Range(center - variance, center + variance + 1);
    }

    public static int Variance(float center, float variance)
    {
        return Mathf.RoundToInt(Random.Range(center - variance, center + variance + 1));
    }

    public static T PickOne<T>(IList<T> ts)
    {
        return ts[Random.Range(0, ts.Count)];
    }

    public static T PickOne<T>(IList<T> ts, out int index)
    {
        index = Random.Range(0, ts.Count);
        return ts[index];
    }

    public static IEnumerable<T> GetRandomUniqueSet<T>(IList<T> set, int count)
    {
        if(count > set.Count)
        {
            throw new System.ArgumentException();
        }
        List<T> newSet = new List<T>();
        newSet.AddRange(set);
        int counter = 0;
        while(counter < count) {
            int current = Random.Range(0, newSet.Count);
            T next = newSet[current];
            newSet.RemoveAt(current);
            counter++;
            yield return next;
        }
    }

}
