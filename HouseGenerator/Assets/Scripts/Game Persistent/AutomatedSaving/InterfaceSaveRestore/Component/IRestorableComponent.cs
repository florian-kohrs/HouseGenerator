using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRestorableComponent
{

    /// <summary>
    /// adds the saved component to the gameObject
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    ISaveableComponent addComponent(GameObject gameObject, IComponentRecycler recycler);
    
    /// <summary>
    /// restores the component Values
    /// </summary>
    /// <returns></returns>
    ISaveableComponent restoreComponent(IComponentAssigner assigner);
    
}
