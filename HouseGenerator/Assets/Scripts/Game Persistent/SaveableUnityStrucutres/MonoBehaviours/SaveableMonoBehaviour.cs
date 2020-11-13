using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T">the type which stores the data for this saveable monobehaviour</typeparam>
public abstract class SaveableMonoBehaviour : BaseSaveableMonoBehaviour
{
   
    /// <summary>
    /// the delegate is used, to unsubscribe from all events, when the script (gameobject) is destroyed,
    /// to avoid the event source reference destroyed objects
    /// </summary>
    private delegate void OnScriptDestroy();
    private event OnScriptDestroy onDestroyEvent;
    
    [Save]
    private bool isEnabled;

    /// <summary>
    /// if the existence of a script should be saved, but no data behind it this can be set to "true".
    /// </summary>
    [Save]
    public bool dontSaveScriptValues;
    
    /// <summary>
    /// since the field "enabled" in the base class doesn`t have the "Save"-attribute
    /// the variable is kinda copied here with the "Save"-Attribute.
    /// The value is set from its GameObject when the scene is going to be saved
    /// </summary>
    public bool IsEnabled
    {
        get { return isEnabled; }
        set { isEnabled = value; }
    }
    
    protected virtual void setDataBeforeSaving(PersistentGameDataController.SaveType saveType) { }
    
    public sealed override void setSaveData(PersistentGameDataController.SaveType saveType)
    {
        IsEnabled = enabled;
        setDataBeforeSaving(saveType);
    }

    /// <summary>
    /// use this method to subscribe to events, so all registered events will be unsubscribed
    /// when the object is being destroyed
    /// An example of usage: "subscribeEvent(delegate { inventoryController.onItemEquiped += onItemEquipEvent; },
    /// delegate { inventoryController.onItemEquiped -= onItemEquipEvent; });"
    /// </summary>
    /// <param name="subscribe"></param>
    /// <param name="unsubscribe"></param>
    public void subscribeEvent(Action subscribe, Action unsubscribe)
    {
        subscribe();

        onDestroyEvent += () =>
        {
            unsubscribe();
        };
    }

    /// <summary>
    /// is called when the game was loaded, and the script is fully 
    /// initiated 
    /// </summary>
    public sealed override void onBehaviourLoaded()
    {
        enabled = isEnabled;
        BehaviourLoaded();
        OnAwake();
    }

    /// <summary>
    /// this method will only get called the first time an object is instantiated,
    /// and not when it was loaded later in game.
    /// </summary>
    protected virtual void OnFirstTimeBehaviourAwakend() { }

    /// <summary>
    /// will get called instead on "onAwake" when the game was loaded
    /// </summary>
    protected virtual void BehaviourLoaded() { }
 
    protected virtual void onDestroy() { }
    
    /// <summary>
    /// do not override this method by any mean with the "new" keyword, as Unity
    /// wont call this method anymore, which is essential for saving this script.
    /// Use the virutal function "onAwake" instead!! Be aware tho that "onAwake" is 
    /// not called, when the game is loaded. In this case the method "behaviourLoaded" will
    /// be called.
    /// </summary>
    protected void Awake()
    {
        ///only call virutal "onAwake" method when the object was not loaded.
        if (SaveableGame.FirstTimeSceneLoaded)
        {
            OnAwake();
            OnFirstTimeBehaviourAwakend();
        }
    }

    protected void OnDestroy()
    {
        onDestroy();
        if (onDestroyEvent != null)
        {
            onDestroyEvent();
        }
    }
    
    
}
