using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCapsuleCollider : SerializeableCollider<CapsuleCollider>
{
    protected override void restoreComponent(CapsuleCollider component)
    {
        base.restoreComponent(component);
    }

    protected override void saveComponent(CapsuleCollider component, PersistentGameDataController.SaveType saveType)
    {
        base.saveComponent(component, saveType);
    }

}
