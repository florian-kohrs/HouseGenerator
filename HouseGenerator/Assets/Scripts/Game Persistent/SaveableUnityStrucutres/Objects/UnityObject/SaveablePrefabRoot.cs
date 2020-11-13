using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Transferable]
public class SaveablePrefabRoot : BaseSaveableGameObject, IAssetRefMaintainer/*, IAssetInitializer*/
{
    
    [HideInInspector]
    public PrefabRef assetRef;

    public IAssetReferencer GetReferencer()
    {
        return assetRef;
    }

    protected override void Awake()
    {
        base.Awake();
        if (!SaveableGame.KeepObjects)
        { 
            ///if the object is not gonna be kept, it is added to the garbage heap so it can be deleted later
            SaveableGame.addObjectToGarbageHeap(gameObject);
        }
    }

    public new void prepareSceneTransition()
    {
        base.prepareSceneTransition();
    }

    protected override IRestorableGameObject createSerializableObject()
    {
        return new SerializablePrefabRoot(gameObject, this);
    }

    public IAssetInitializer GetInitializer()
    {
        return assetRef;
    }

    //public void InitializeAsset(UnityEngine.Object assetRef)
    //{
    //    assetRef.GeIn
    //}
}
