using UnityEditor;
using UnityEngine;

public class CheckPrefabs : MonoBehaviour
{

    [MenuItem("SaveableAssets/Validate all Prefabs")]
    static void checkAllPrefabs()
    {
        AssetPathFixer.checkAllPrefabsForSaveableEnvironment();
    }

    [MenuItem("SaveableAssets/Validate unchecked Prefabs")]
    static void checkUncheckedPrefabs()
    {
        AssetPathFixer.checkUncheckedPrefabsForSaveableEnvironment();
    }

    [MenuItem("SaveableAssets/Validate all Assets")]
    static void checkAllAssets()
    {
        AssetPathFixer.RevalidateAllSaveableObjects();
    }

}
