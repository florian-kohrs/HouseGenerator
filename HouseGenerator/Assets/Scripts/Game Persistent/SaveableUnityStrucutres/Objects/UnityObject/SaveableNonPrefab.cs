using System.Collections.Generic;
using UnityEngine;

public class SaveableNonPrefab : BaseSaveableGameObject
{

    private Stack<int> pathFromRoot;

    public Stack<int> PathFromRoot {
        set { pathFromRoot = value; }
        get { return pathFromRoot; }
    }

    /// <summary>
    /// builds the current object path by adding all sibling counts to the
    /// stack<int> until there is no parent transform
    /// </summary>
    /// <returns></returns>
    private Stack<int> buildRootPath()
    {
        Stack<int> result = new Stack<int>();
        
        Transform current = transform;

        do
        {
            result.Push(current.GetSiblingIndex());
            current = current.parent;

        } while (current != null);
        
        return result;
    }

    protected override void Awake()
    {
        initialize();

        SaveableGame.addObjectToCurrentScene(this);

        if (SaveableGame.FirstTimeSceneLoaded)
        {
            pathFromRoot = buildRootPath();
        }

    }

    protected override IRestorableGameObject createSerializableObject()
    {
        return new SerializableNonPrefab(gameObject, pathFromRoot);
    }
}
