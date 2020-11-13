using System;
using UnityEngine;

public abstract class SaveableUnityComponent<T> : SaveableComponent, ITransformObject where T : Component {

    public SaveableUnityComponent() { }

    public override void saveComponentValues(GameObject gameObject, 
        IComponentAssigner assigner, PersistentGameDataController.SaveType saveType)
    {
        Component = assigner.getComponent<T>();
        saveComponent(Component, saveType);
    }

    protected abstract void saveComponent(T component, PersistentGameDataController.SaveType saveType);
    protected abstract void restoreComponent(T component);

    public bool HasComponentAttached
    {
        get
        {
            return gameObject.GetComponent<T>() != null;
        }
    }

    /// <summary>
    /// the created component
    /// </summary>
    [SerializeField]
    private T component;
    
    public T Component
    {
        get { return component; }
        set { component = value; }
    }
    
    public override Component BaseComponent
    {
        get { return component; }
        set { Component = (T)value; }
    }

    public sealed override Type getGenericType()
    {
        return typeof(T);
    }
    
    public sealed override ISaveableComponent restoreComponent(IComponentAssigner assigner)
    {
        Component = assigner.getComponent<T>();
        restoreComponent(Component);
        return this;
    }
    
    public object getTransformedValue()
    {
        return CreatedSaveableComponent;
    }

}
