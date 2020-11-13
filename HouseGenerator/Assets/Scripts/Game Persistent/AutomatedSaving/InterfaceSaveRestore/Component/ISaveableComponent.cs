using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveableComponent
{

    IRestorableComponent createRestoreableComponent();

    IRestorableComponent saveComponent(GameObject gameObject, 
        IComponentAssigner assigner, PersistentGameDataController.SaveType saveType);
    
}
