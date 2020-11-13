using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The SaveableComponent is a component that can save and restore itself.
/// This Structure is a HybridComponent even tho its using a saveableContainer for its data,
/// since the script values need to be loaded in the original component after
/// this instance is restored
/// </summary>
public abstract class SaveableComponent : BaseSaveableObject, IHybridComponent
{
    public abstract void saveComponentValues(GameObject gameObject, IComponentAssigner assigner, PersistentGameDataController.SaveType saveType);

    /// <summary>
    /// only used for editor display -> use property "Component" instead
    /// </summary>
    public abstract Component BaseComponent {get;set;}

    ///can be simplified by using reflection!
    public abstract Type getGenericType();
    
    public IRestorableComponentContainer CreatedSaveableComponent { get; private set; }
    
    public override IRestorableComponent createRestoreableComponent()
    {
        CreatedSaveableComponent = new SaveableUnityComponentContainer(GetType());
        return CreatedSaveableComponent;
    }

    public override IRestorableComponent saveComponent(GameObject gameObject, 
        IComponentAssigner assigner, PersistentGameDataController.SaveType saveType)
    {
        saveComponentValues(gameObject, assigner, saveType);
        AutomatedScriptTransfer.transferComponentSaving
            (this, CreatedSaveableComponent.DataContainer);
        return CreatedSaveableComponent;
    }

    /// <summary>
    /// returns the component T from the given gameObject, null if its not attached
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    protected T getBehaviour<T>(GameObject gameObject) where T : Component
    {
        T result = gameObject.GetComponent<T>();
        if (result == null)
        {
            throw new ComponentNotFoundException("Error while searching a component! The SerializeableComponent " + this.GetType().Name +
                " expected to find the component " + typeof(T).Name + " on the GameObject " + gameObject.name + " but component is null!");
        }
        return result;
    }

    public abstract ISaveableComponent restoreComponent(IComponentAssigner assigner);

    public ISaveableComponent addComponent(GameObject gameObject, IComponentRecycler recycler)
    {
        ISaveableComponent result = (ISaveableComponent)recycler.getNextComponent(getGenericType());
        return result;
    }
}
