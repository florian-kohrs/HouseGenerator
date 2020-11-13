using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SerializableNonPrefab : BaseCorrespondingSerializableGameObject
{
    
    public SerializableNonPrefab(GameObject gameObject, Stack<int> rootPath) : base(gameObject)
    {
        this.pathFromRoot = rootPath;
    }

    private Stack<int> pathFromRoot = new Stack<int>();


    protected override GameObject getSceneGameObject(Transform parent)
    {
        return InGameObject;
    }

    public override void prepareGameObjectRestore(IList<Transform> nonPrefabs)
    {
        InGameObject = SaveableScene.getTransformFromPath(pathFromRoot).gameObject;
        SaveableNonPrefab saveableNonPrefab = InGameObject.GetComponent<SaveableNonPrefab>();
        saveableNonPrefab.PathFromRoot = pathFromRoot;
        nonPrefabs.Add(InGameObject.transform);
    }

}
