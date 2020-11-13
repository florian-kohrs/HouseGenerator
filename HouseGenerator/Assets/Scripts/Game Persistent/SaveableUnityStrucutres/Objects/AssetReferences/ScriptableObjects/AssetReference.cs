using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public abstract class AssetReference : IAssetReferencer, IAssetInitializer, IAssetRefHolder, ISerializable
{

    public AssetReference() { }
    
    protected AssetReference(SerializationInfo info, StreamingContext context)
    {
        assetName = (string)info.GetValue(nameof(assetName), typeof(string));
        relativePathFromResource = (string)info.GetValue(nameof(relativePathFromResource), typeof(string));
        assetExtension = (string)info.GetValue(nameof(assetExtension), typeof(string));
        RestoreObject();
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        IAssetReferencer referencer = GetSaveableReferencer();
        assetName = referencer?.AssetName;
        relativePathFromResource = referencer?.RelativePathFromResource;
        assetExtension = referencer?.AssetExtension;
        info.AddValue(nameof(assetName), assetName);
        info.AddValue(nameof(relativePathFromResource), relativePathFromResource);
        info.AddValue(nameof(assetExtension), assetExtension);
    }

    public override bool Equals(object obj)
    {
        if (obj != null && obj is AssetReference)
        {
            return ((AssetReference)obj).AssetIdentifier.Equals(AssetIdentifier);
        }
        else
        {
            return base.Equals(obj);
        }
    }

    public override int GetHashCode()
    {
#if UNITY_EDITOR
        if(AssetIdentifier == "")
        {
            Debug.LogError("Saveable asset has not been initialized yet! To fix this click in the Menue \"SaveableAssets -> Validate all Assets\".");
        }
#endif
        return AssetIdentifier.GetHashCode();
    }

    public string AssetIdentifier
    {
        get
        {
            IAssetReferencer r = GetSaveableReferencer();
            if(r == null)
            {
                return RelativePathFromResource + AssetName + AssetExtension;
            }
            else
            {
                return r?.RelativePathFromResource + r?.AssetName + r?.AssetExtension;
            }
        }
    }

    public abstract void InitializeAsset(Object assetRef);

    public abstract IAssetReferencer GetReferencer();

    protected abstract IAssetReferencer GetSaveableReferencer();

    public System.Type assetType;

    [HideInInspector]
    [SerializeField]
    private string relativePathFromResource;

    public string RelativePathFromResource
    {
        get { return relativePathFromResource; }
        set { relativePathFromResource = value; }
    }

    [HideInInspector]
    [SerializeField]
    private string assetName;

    public string AssetName
    {
        get { return assetName; }
        set { assetName = value; }
    }

    [HideInInspector]
    [SerializeField]
    private string assetExtension;

    public string AssetExtension
    {
        get { return assetExtension; }
        set { assetExtension = value; }
    }

    [HideInInspector]
    [SerializeField]
    private bool wasAlreadyValidated;

    public bool WasAlreadyValidated
    {
        get { return wasAlreadyValidated; }
        set { wasAlreadyValidated = value; }
    }

    protected abstract System.Type OjectType
    {
        get;
    }

    public Object RestoreObject()
    {
        Object result;
        //if (!refRestoreTable.TryGetValue(this, out result))
        {
            result = Resources.Load(RelativePathFromResource + "/" + AssetName, OjectType);
            if (result != null)
            {
                //refRestoreTable.Add(this, result);
            }
        }
        InitializeAsset(result);
        return result;
    }

    /// <summary>
    /// this dictionary will prevent the same asset references from 
    /// being searched in the resource folder multiple times
    /// -> currently ends in an endless loop
    /// </summary>
    private static Dictionary<string, Object> refRestoreTable =
        new Dictionary<string, Object>();

}
