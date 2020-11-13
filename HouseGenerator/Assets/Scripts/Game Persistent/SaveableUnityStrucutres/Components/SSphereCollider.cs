using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSphereCollider : SerializeableCollider<SphereCollider>
{
    protected override void restoreComponent(SphereCollider component)
    {
        base.restoreComponent(component);
    }

    protected override void saveComponent(SphereCollider component, PersistentGameDataController.SaveType saveType)
    {
        base.saveComponent(component, saveType);
    }

}
