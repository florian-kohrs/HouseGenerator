using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRestorableGameObject
{

    /// <summary>
    /// restores the gameobject as child of the given parent with the given 
    /// sibling index. last children if index is -1
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="siblingIndex"></param>
    /// <returns></returns>
    GameObject createObject(Transform parent, int siblingIndex = -1);
    
    void restoreComponentValues();

    void addComponent(IRestorableComponent component);
    
    void addChildrenWithPath(IRestorableGameObject child, Stack<int> hirachyPath);

    void prepareGameObjectReconstruction(IList<Transform> allNonPrefabs);

}
