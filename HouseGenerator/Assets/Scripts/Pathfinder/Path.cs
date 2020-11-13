using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Path<T, J>
{

    public Path(INavigatable<T, J> assistant, T current, J target, Path<T, J> parent) : this(assistant, current, target, parent, 0) { }

    public Path(INavigatable<T, J> assistant, T current, J target, Path<T, J> parent, float distanceToNode)
    {
        this.current = current;
        this.target = target;
        this.parent = parent;
        nav = assistant;
        previousDistance = distanceToNode;
        distance = nav.DistanceToTarget(current, target);
    }

    public IEnumerable<Path<T, J>> Advance()
    {
        T[] circumjacent = nav.GetCircumjacent(current);
        return circumjacent
            .Where(c => parent == null || !nav.IsEqual(c, parent.current))
            .Select(c => new Path<T, J>(
                nav, c, target, this, previousDistance + nav.DistanceToField(current, c)));
    }

    public INavigatable<T, J> nav;

    public T current;

    public J target;

    public Path<T, J> parent;

    public float distance;

    public float previousDistance;

    public float TotalMinimumDistance => distance + previousDistance;

    public float TotalEstimatedMinimumDistance(float accuracyFactor)
    {
        return distance + (accuracyFactor * previousDistance);
    }

    public void BuildPath(ref IList<T> result)
    {
        result.Insert(0, current);
        if (parent != null)
        {
            parent.BuildPath(ref result);
        }
    }

    public override int GetHashCode()
    {
        return (int)(distance * Mathf.Pow(2, 24) + previousDistance * Mathf.Pow(2, 16));
    }

}
