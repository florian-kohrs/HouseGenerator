using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// will move all prefabs who are currently not in a resource folder into a resource folder
/// </summary>
public class AssetPathFixer
{
    public const string RESOURCE_FOLDER_NAME = "Resources";

    public const string PREFAB_FOLDER_NAME = "Prefabs";

    public const string ASSET_FOLDER_NAME = "Assets";

    public static readonly string absoluteAssetFolderPath;

    private static string RelativePrefabPath
    {
        get
        {
            return RESOURCE_FOLDER_NAME + "/" + PREFAB_FOLDER_NAME;
        }
    }

    private static string RelativePrefabPathFromAssetFolder
    {
        get
        {
            return ASSET_FOLDER_NAME + "/" + RelativePrefabPath;
        }
    }

    private static string RelativeResourceFolderPath
    {
        get
        {
            return ASSET_FOLDER_NAME + "/" + RESOURCE_FOLDER_NAME;
        }
    }

    static AssetPathFixer()
    {
        absoluteAssetFolderPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/") + 1);
    }

    public static void checkUncheckedPrefabsForSaveableEnvironment()
    {
        checkPrefabsForSaveableEnvirenment(true);
    }

    public static void checkAllPrefabsForSaveableEnvironment()
    {
        checkPrefabsForSaveableEnvirenment(false);
    }

    private static void checkPrefabsForSaveableEnvirenment(bool onlyValidateCurrentlyUnchecked = true)
    {
        foreach (string path in getAllPrefabPathes())
        {
            checkPrefabForSaveableEnvironment(path, onlyValidateCurrentlyUnchecked);
        }
    }

    private static void checkPrefabForSaveableEnvironment(string s, bool onlyValidateCurrentlyUnchecked)
    {
        GameObject current = getPrefabFromPath(s);
        SaveablePrefabRoot saveBehaviour = current.GetComponent<SaveablePrefabRoot>();
        if (saveBehaviour != null)
        {
            checkPrefab(s, saveBehaviour, onlyValidateCurrentlyUnchecked);
        }
        PrefabUtility.UnloadPrefabContents(current);
    }

    private static void checkPrefab(string path, SaveablePrefabRoot saveablePrefab, bool onlyValidateCurrentlyUnchecked = true)
    {
        string relativePrefabPath = path;

        ///only move the prefab when its not already located in a "Resources" folder
        if (!saveablePrefab.GetReferencer().WasAlreadyValidated || !onlyValidateCurrentlyUnchecked)
        {
            checkAsset(path, saveablePrefab,
                p => p.gameObject.name,
                (p,s) =>
                {
                    PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(saveablePrefab.gameObject);
                    if (prefabStage != null)
                    {
                        EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                    }
                    PrefabUtility.SaveAsPrefabAsset(saveablePrefab.gameObject, s);
                });
        }
    }

    private static void checkAsset<T>(string currentPath, T assetRef, System.Func<T,string> getName, System.Action<T, string> onCheckDone = null) where T : Object, IAssetRefMaintainer
    { 
        string newRelativeAssetPath;
        assetRef.GetInitializer().InitializeAsset(assetRef);
        IAssetReferencer r = assetRef.GetReferencer();
        r.AssetExtension = Path.GetExtension(currentPath).Remove(0,1);
        ///check if the prefab is already in the correct directory
        if (currentPath.IndexOf(RelativeResourceFolderPath) != 0)
        {
            newRelativeAssetPath = currentPath.Insert
            (currentPath.IndexOf("/"), "/" + RESOURCE_FOLDER_NAME + "/" + r.AssetExtension);
        }
        else
        {
            newRelativeAssetPath = currentPath;
        }

        string newRelativePrefabDirectory = newRelativeAssetPath.Substring(0, newRelativeAssetPath.LastIndexOf('/'));
        string assetMoveErrorMessage = null;

        ///only move the asset when its not already located in the resourceFolder
        if (currentPath.IndexOf(RelativeResourceFolderPath) != 0)
        {
            FolderSystem.createAssetPath(newRelativePrefabDirectory.Split('/'));
            assetMoveErrorMessage = AssetDatabase.MoveAsset(currentPath, newRelativeAssetPath);
        }

        if (string.IsNullOrEmpty(assetMoveErrorMessage))
        {
            r.AssetName = getName(assetRef);
            string pathFromAssetFolder = newRelativePrefabDirectory.Remove(0, RelativeResourceFolderPath.Length + 1);
            r.WasAlreadyValidated = true;
            r.RelativePathFromResource = pathFromAssetFolder;
            onCheckDone?.Invoke(assetRef, newRelativeAssetPath);
        }
        else
        {
            Debug.LogError("Asset couldnt be moved to path: " + newRelativePrefabDirectory + ". Error: " + assetMoveErrorMessage);
        }
    }

    private static List<string> getAllPrefabPathes()
    {
        List<string> result = new List<string>();
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string s in allAssetPaths)
        {
            if (s.Contains(".prefab"))
            {
                result.Add(s);
            }
        }
        return result;
    }
    
    
    public static void RevalidateAllSaveableObjects()
    {
        foreach (string s in AssetDatabase.GetAllAssetPaths())
        {
            if (s.IndexOf("Assets") == 0)
            {
                string extension = Path.GetExtension(s);
                if (extension != string.Empty)
                {
                    extension = extension.Remove(0, 1);
                    switch (extension)
                    {
                        case ("prefab"):
                            {
                                checkPrefabForSaveableEnvironment(s, false);
                                break;
                            }
                        case ("asset"):
                            {
                                SaveableScriptableObject scr = AssetDatabase.LoadAssetAtPath<SaveableScriptableObject>(s);
                                if (scr != null)
                                {
                                    checkAsset(s, scr, obj => obj.name, (o, _) => EditorUtility.SetDirty(o));
                                }
                                    break;
                            }
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
    }


    private static ScriptableObject getScriptableObjectFromPath(string path)
    {
        return AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
    }

    /// <summary>
    /// returns the prefab at the given path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static GameObject getPrefabFromPath(string path)
    {
        return PrefabUtility.LoadPrefabContents(path);
    }
}
