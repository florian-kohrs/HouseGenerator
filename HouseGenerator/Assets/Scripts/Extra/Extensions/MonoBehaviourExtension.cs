using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoBehaviourExtension
{

    /// <summary>
    /// delay the execution of a function
    /// </summary>
    /// <param name="source"></param>
    /// <param name="delay"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerator DoDelayed(this MonoBehaviour source, float delay, System.Action action)
    {
        IEnumerator result = DoStuff(delay,action);
        source.StartCoroutine(result);
        return result;
    }

    /// <summary>
    /// same as "DoDelayed" but wont throw an Exception if action is null
    /// </summary>
    /// <param name="source"></param>
    /// <param name="delay"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerator DoDelayedSave(this MonoBehaviour source, float delay, System.Action action)
    {
        if(action != null)
        {
            DoDelayed(source, delay, action);
        }
        else
        {
            yield return null;
        }
    }

    private static IEnumerator DoStuff(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }

}
