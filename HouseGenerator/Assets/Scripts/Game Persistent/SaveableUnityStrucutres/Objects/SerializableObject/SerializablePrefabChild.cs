using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializablePrefabChild : BaseCorrespondingSerializableGameObject
{

    private int siblingIndex;

    public int SiblingIndex { get { return siblingIndex; } set { siblingIndex = value; } }

    public SerializablePrefabChild(GameObject gameObject, int siblingIndex) : base(gameObject)
    {
        this.siblingIndex = siblingIndex;
    }

    private GameObject getSceneGameObject(Transform parent, int siblingIndex)
    {
        return parent.GetChild(siblingIndex).gameObject;
    }

    protected override GameObject getSceneGameObject(Transform parent)
    {
        return getSceneGameObject(parent, SiblingIndex);
    }

}
