using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SRigidBody : SaveableUnityComponent<Rigidbody>
{
    
    private Serializable3DVector vel;
    private Serializable3DVector angVel;
    private bool isKinematic;

    protected override void restoreComponent(Rigidbody component)
    {
        component.isKinematic = isKinematic;
        component.velocity = vel.v;
        component.angularVelocity = angVel.v;
    }

    protected override void saveComponent(Rigidbody component, PersistentGameDataController.SaveType saveType)
    {
        isKinematic = component.isKinematic;
        vel = new Serializable3DVector(component.velocity);
        angVel = new Serializable3DVector(component.angularVelocity);
    }
}
