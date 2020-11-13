using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SaveableUnityData : IRestorableComponentContainer, ITransformObject
{

    public SaveableUnityData(Type t)
    {
        this.savedComponentType = t;
    }
    
    protected readonly Type savedComponentType;

    [System.NonSerialized]
    private ISaveableComponent createdComponent;

    /// <summary>
    /// saves for all serialized fields their names paired with their value
    /// </summary>
    private Dictionary<string, object> values = new Dictionary<string, object>();

    public Dictionary<string, object> DataContainer
    {
        get
        {
            return values;
        }
    }

    public ISaveableComponent addComponent(GameObject gameObject, IComponentRecycler recycler)
    {
        createdComponent = (ISaveableComponent)recycler.getNextComponent(savedComponentType);
        componentAdded(createdComponent);
        return createdComponent;
    }

    protected virtual void componentAdded(ISaveableComponent c) { }

    protected abstract Type LastSerializedType { get; }

    public ISaveableComponent restoreComponent(IComponentAssigner assigner)
    {
        AutomatedScriptTransfer.restoreData(values, createdComponent, LastSerializedType);
        componentRestored(createdComponent, assigner);
        return createdComponent;
    }

    protected virtual void componentRestored(ISaveableComponent c, IComponentAssigner assigner) { }
    
    public object getTransformedValue()
    {
        return createdComponent;
    }

}
