using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBoxCollider : SerializeableCollider<BoxCollider>
{

    private Serializable3DVector center;

    private Serializable3DVector size;

    protected override void saveComponent(BoxCollider component, PersistentGameDataController.SaveType saveType)
    {
        base.saveComponent(component, saveType);
        center = new Serializable3DVector(component.center);
        size = new Serializable3DVector(component.size);
    }

    protected override void restoreComponent(BoxCollider component)
    {
        base.restoreComponent(component);
        component.center = center.v;
        component.size = size.v;
    }

}
