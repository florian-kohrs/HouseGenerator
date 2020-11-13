using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class AssetPolyRef<T> : AssetReference, IAssetRefMaintainer where T : Object, IAssetRefMaintainer
{
    
    public AssetPolyRef() : base() { }

    protected AssetPolyRef(SerializationInfo info, StreamingContext context) : base(info, context) { }
    
    [SerializeField]
    private T runtimeRef;

    public T RuntimeRef
    {
        get
        {
            if (runtimeRef == null)
            {
                RestoreObject();
            }
            return runtimeRef;
        }
        set
        {
            runtimeRef = value;
        }
    }

    protected override System.Type OjectType => typeof(T);

    public IAssetInitializer GetInitializer()
    {
        return this;
    }

    public override IAssetReferencer GetReferencer()
    {
        return this;
    }

    protected override IAssetReferencer GetSaveableReferencer()
    {
        return RuntimeRef?.GetReferencer();
    }
    
    public override void InitializeAsset(Object o)
    {
        RuntimeRef = o as T;
    }

}

