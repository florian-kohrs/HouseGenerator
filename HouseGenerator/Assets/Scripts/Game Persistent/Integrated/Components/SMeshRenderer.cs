using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMeshRenderer : SaveableUnityComponent<MeshRenderer>
{

    private bool isEnabled;

    protected override void restoreComponent(MeshRenderer component)
    {
        component.enabled = isEnabled;
    }

    protected override void saveComponent(MeshRenderer component, PersistentGameDataController.SaveType saveType)
    {
        isEnabled = component.enabled;
    }
}
