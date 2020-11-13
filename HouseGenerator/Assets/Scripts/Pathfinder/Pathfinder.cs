using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathAccuracy { Perfect, VeryGood, Good, Decent, NotSoGoodAnymore, ITakeAnyThing }

public class Pathfinder<T, J>
{

    private float AccuracyFactor(PathAccuracy acc)
    {
        float result;
        switch (acc)
        {
            case PathAccuracy.Perfect:
                {
                    result = 1f;
                    break;
                }
            case PathAccuracy.VeryGood:
                {
                    result = 0.95f;
                    break;
                }
            case PathAccuracy.Good:
                {
                    result = 0.8f;
                    break;
                }
            case PathAccuracy.Decent:
                {
                    result = 0.5f;
                    break;
                }
            case PathAccuracy.NotSoGoodAnymore:
                {
                    result = 0.25f;
                    break;
                }
            case PathAccuracy.ITakeAnyThing:
                {
                    result = 0.05f;
                    break;
                }
            default:
                {
                    throw new System.ArgumentException("Unexpected Accuracy: " + acc);
                }
        }
        return result;
    }


    public static IList<T> FindPath(INavigatable<T, J> assistant, T start, J target, PathAccuracy accuracy)
    {
        return new Pathfinder<T, J>(assistant, start, target, accuracy).GetPath();
    }

    private Pathfinder(INavigatable<T, J> assistant, T start, J target, PathAccuracy accuracy)
    {
        this.start = start;
        this.target = target;
        this.accuracy = accuracy;
        pathAccuracy = AccuracyFactor(accuracy);
        nav = assistant;
    }

    INavigatable<T, J> nav;

    PathAccuracy accuracy;

    float pathAccuracy;

    protected J target;

    protected T start;

    protected IList<T> GetPath()
    {
        count = 0;
        AddTail(new Path<T, J>(nav, start, target, null));
        return BuildPath();
    }

    protected IList<T> BuildPath()
    {
        while (!ReachedTarget && HasTail)
        {
            AdvanceClosest();
        }
        IList<T> result = new List<T>();

        UncheckedClosestField.BuildPath(ref result);

        return result;
    }

    public void AdvanceClosest()
    {
        Path<T, J> closest;
        if (TryGetClosestField(out closest))
        {
            usedFields.Add(closest.distance, null);
            IEnumerable<Path<T, J>> newPaths = closest.Advance();

            RemoveClosest();

            foreach (Path<T, J> p in newPaths)
            {
                AddTail(p);
            }
        }
    }

    public bool ReachedTarget => nav.ReachedTarget(UncheckedClosestField.current, target);

    public SortedList<float, List<Path<T, J>>> pathTails = new SortedList<float, List<Path<T, J>>>();

    public SortedDictionary<float, int?> usedFields = new SortedDictionary<float, int?>();

    public bool Contains(T t)
    {
        bool found = false;

        for (int i = 0; i < pathTails.Count && !found; i++)
        {
            for (int x = 0; x < pathTails[i].Count && !found; x++)
            {
                found = nav.IsEqual(pathTails[i][x].current, t);
            }
        }

        return found;
    }

    public void RemoveClosest()
    {
        pathTails.Values[0].RemoveAt(0);
        if (pathTails.Values[0].Count == 0)
        {
            pathTails.RemoveAt(0);
        }
    }

    protected static int count = 0;
    public void AddTail(Path<T, J> p)
    {
        if (!usedFields.ContainsKey(p.distance))
        {
            List<Path<T, J>> r;

            if (!pathTails.TryGetValue(p.TotalEstimatedMinimumDistance(pathAccuracy), out r))
            {
                r = new List<Path<T, J>>();
                pathTails.Add(p.TotalEstimatedMinimumDistance(pathAccuracy), r);
            }

            r.Add(p);
        }
        else
        {
            count++;
        }
    }

    protected bool TryGetClosestField(out Path<T,J> path)
    {
        bool isEmpty = pathTails.Count <= 0;

        path = null;

        while (path == null && !isEmpty)
        {
            if (usedFields.ContainsKey(UncheckedClosestField.distance))
            {
                count += pathTails.Values[0].Count;
                pathTails.RemoveAt(0);
            }
            else
            {
                path = UncheckedClosestField;
            }
            isEmpty = pathTails.Count > 0;
        }

        return path != null;

    }

    protected bool HasTail => pathTails.Count > 0;

    public Path<T, J> UncheckedClosestField
    {
        get
        {
            return pathTails.Values[0][0];
        }
    }



}
