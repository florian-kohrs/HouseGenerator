using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ExtensionEnumerable
{

    public static T GrabOne<T>(this IList<T> ts)
    {
        return ts[Random.Range(0, ts.Count)];
    }

}

