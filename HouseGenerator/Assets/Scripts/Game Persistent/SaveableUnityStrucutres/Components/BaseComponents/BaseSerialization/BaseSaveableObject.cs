using System;
using UnityEngine;

/// <summary>
/// Base class for all classes, that derive from MonoBehaviour and are 
/// serializable.
/// (this class is used so Unity gameobjects can find all saveable Scripts with
/// getComponents<BaseSaveableObject>)
/// </summary>
public abstract class BaseSaveableObject : MonoBehaviour, ISaveableComponent
{
    public abstract IRestorableComponent createRestoreableComponent();

    public abstract IRestorableComponent saveComponent(GameObject gameObject, 
        IComponentAssigner assigner, PersistentGameDataController.SaveType saveType);
    
}

