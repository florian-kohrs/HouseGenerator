using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveableMonoBehaviourContainer : SaveableUnityData
{
    public SaveableMonoBehaviourContainer(Type t) : base(t) { }

    protected override Type LastSerializedType
    {
        get
        {
            return typeof(SaveableMonoBehaviour);
        }
    }

    protected override void componentAdded(ISaveableComponent c)
    {
        ((BaseSaveableMonoBehaviour)c).WasCreated = true;
    }

    protected override void componentRestored(ISaveableComponent c, IComponentAssigner assigner)
    {
        ((BaseSaveableMonoBehaviour)c).onBehaviourLoaded();
    }

}
