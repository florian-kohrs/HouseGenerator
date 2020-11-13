using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveableScriptableObject : ScriptableObject, ITransformObject, IAssetRefMaintainer, IAssetInitializer
{

    [HideInInspector]
    public ScriptableObjectRef assetRef;

    public IAssetInitializer GetInitializer()
    {
        return this;
    }
    
    public IAssetReferencer GetReferencer()
    {
        return assetRef?.GetReferencer();
    }

    public object getTransformedValue()
    {
        return assetRef;
    }

    public void InitializeAsset(Object asset)
    {
        assetRef.RuntimeRef = asset as SaveableScriptableObject;
        assetRef.GetInitializer().InitializeAsset(asset);
    }
}
