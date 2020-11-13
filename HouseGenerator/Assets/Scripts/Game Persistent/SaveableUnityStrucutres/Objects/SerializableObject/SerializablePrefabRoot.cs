using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SerializablePrefabRoot : BaseCorrespondingSerializableGameObject
{
    public SerializablePrefabRoot
        (GameObject gameObject, IAssetRefHolder refHolder) : base(gameObject)
    {
        prefabRef = refHolder.GetReferencer();
    }

    private IAssetReferencer prefabRef;
    
    protected override GameObject getSceneGameObject(Transform parent)
    {
        return instantiateSaveableGameObject(getPrefabFromName(prefabRef));
    }

    /// <summary>
    /// calls the default instantiate method 
    /// (Could be left out -> may be done in the future)
    ///</summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    private GameObject instantiateSaveableGameObject(GameObject gameObject)
    {
        GameObject result = GameObject.Instantiate(gameObject);
        return result;
    }

    /// <summary>
    /// returns the prefab in the resource folder with the given name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private GameObject getPrefabFromName(IAssetReferencer prefabRef)
    {
        string prefabPath = prefabRef.RelativePathFromResource;
        string name = prefabRef.AssetName;
        if (prefabPath != "" && prefabPath[prefabPath.Length - 1] != '/')
        {
            prefabPath += "/";
        }

        GameObject result = Resources.Load<GameObject>(prefabPath + name);

        return result;
    }

}
