using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveableUnityComponentContainer : SaveableUnityData
{ 

    public SaveableUnityComponentContainer(Type t) : base(t) { }

    protected override Type LastSerializedType
    {
        get
        {
            return typeof(SaveableComponent);
        }
    }

    protected override void componentRestored(ISaveableComponent c, IComponentAssigner assigner)
    {
        ((IHybridComponent)c).restoreComponent(assigner);
    }

}
