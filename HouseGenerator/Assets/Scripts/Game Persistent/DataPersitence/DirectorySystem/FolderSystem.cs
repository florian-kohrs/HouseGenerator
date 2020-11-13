using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FolderSystem {

    #region store settings
    ///in this region, the structur of the stored data is set.
    ///current try: each scene is safed in an own file. a gameslot is defined by an order
    ///containing multiple files of different scenes
    ///this is done to minimize the loading time by loading few small files
    ///instead if one big, as its complexity is O^2
    public static readonly string saveDirName = "Stored";

    public static readonly string sceneFolderName = "Scenes";
    
    public static readonly string settingsFileName = "settings";

    public static readonly string fileType = "Bodo";

    #endregion

    /// <summary>
    /// creates the given relative path as standart directory.
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="startHere">start path that is guaranteed to exist</param>
    public static void createPath(params string[] pathParts)
    {
        string currentPath = combine(pathParts);

        if (!Directory.Exists(currentPath))
        {
            Directory.CreateDirectory(currentPath);
        }
    }

    /// <summary>
    /// creates the given relative path as asset directory.
    /// </summary>
    /// <param name="pathParts"></param>
    public static void createAssetPath(params string[] pathParts)
    {
#if UNITY_EDITOR
        if (pathParts != null && pathParts.Length > 0)
        {
            string relativeBuildPath = pathParts[0];
            Array.ForEach(
                pathParts.Skip(1).ToArray(),
                (path) =>
                {
                    string newPath = relativeBuildPath + "/" + path;
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(relativeBuildPath, path);
                    }
                    relativeBuildPath = newPath;
                }
               );
        }
#endif
    }

    /// <summary>
    /// combines the pathParts by putting an '/' between each string if the string doesnt already end with an '/'
    /// </summary>
    /// <param name="pathParts"></param>
    /// <returns></returns>
    public static string combine(params string[] pathParts)
    {
        string result = "";
        Array.ForEach(pathParts, (p) => {
            if (result.Length > 0 && result[result.Length - 1] != '/')
            {
                result += "/";
            }
            result += p;
        });
        return result;
    }
    
    public static string[] getAllSaveSlotNames()
    {
        return Directory.GetDirectories(getDefaultSaveSlotPath());
    }

    public static void createDefaultFolderSystem()
    {
        createPath(Application.persistentDataPath, saveDirName);
    }

    public static void DeleteGame(SaveableGame game)
    {
        string path = getGameDirectory(game.GameName);
        Directory.Delete(path, true);
    }

    /// <summary>
    /// creates the foldersystem for the given game name
    /// </summary>
    /// <param name="gameName"></param>
    /// <returns>return the full path for the scene folder</returns>
    public static string createNewSaveSlotDirectory(string gameName)
    {
        string defaultPath = getDefaultSaveSlotPath();
        string path = getDefaulScenePath(gameName);
        createPath(defaultPath, gameName, sceneFolderName);
        return path;
    }

    public static string getGameDirectory(string gameName)
    {
        return getDefaultSaveSlotPath() + "/" + gameName;
    }

    public static string getDefaulScenePath(string gameName)
    {
        return getDefaultSaveSlotPath() + "/" + gameName + "/" + sceneFolderName;
    }

    public static string getSettingsPath()
    {
        return getDefaultSaveSlotPath() + "/" + settingsFileName + "." + settingsFileName;
    }

    public static string getDefaultSettingsPath()
    {
        return getDefaultSaveSlotPath();
    }

    public static string getDefaultSaveSlotPath()
    {
        return Application.persistentDataPath + "/" + saveDirName;
    }

    public static string getGameSavePath(SaveableGame game)
    {
        return getGameSavePath(game.GameName);
    }

    public static string getGameSavePath(string gameName)
    {
        string result = getDefaultSaveSlotPath() + "/" + gameName + "/" + 
            gameName+ "." + fileType;
        return result;
    }
    
    public static bool sceneExists(string gameSlotName, string sceneName)
    {
        return File.Exists(getSceneSavePath(gameSlotName, sceneName));
    }
    
    public static string getSceneSavePath(SaveableGame game, string sceneName)
    {
        return getSceneSavePath(game.GameName, sceneName);
    }

    public static string getSceneSavePath(string gameName, string sceneName)
    {
        string result = Application.persistentDataPath + "/" +
            saveDirName + "/" + gameName + "/" + sceneFolderName + "/" +
            sceneName + "." + fileType;
        return result;
    }

}
