using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveableGame
{

    public SaveableGame(PersistentGameDataController gameDataController)
    {
        GameDataController = gameDataController;
    }

    #region static properties for global accessibility

    /// <summary>
    /// is true, when the loaded scene was never saved before or the game is not in loading phase.
    /// if this is true none objects in the loaded scene are deleted during loading phase.
    /// -> default scene is loaded.
    /// also "onAwake" is called for the objects, event tho "isLoading" is true. 
    /// (normaly "onBehaviourLoaded" will be called for them")
    /// </summary>
    public static bool FirstTimeSceneLoaded
    {
        get
        {
            return !PersistentGameDataController.IsLoading
                || SaveableGame.getCurrentGame().keepExistingObjects;
        }
        set
        {
            SaveableGame.getCurrentGame().keepExistingObjects = value;
        }
    }

    /// <summary>
    /// slighty differs from "KeepExistingObjects". "KeepObjects" is also true during the process
    /// of creating saved gameobjects. This will delay their "onAwake" method after "onLoaded"
    /// was called
    /// </summary>
    public static bool KeepObjects
    {
        get
        {
            return FirstTimeSceneLoaded
                || SaveableGame.getCurrentGame().IsInstantiating;
        }
    }

    #endregion

    #region not serialized fields and its properties

    /// <summary>
    /// referenecs to the PersistentGameDataController, so the game can load its scenes,
    /// and access other useful properties
    /// </summary>
    [System.NonSerialized]
    private static PersistentGameDataController gameDataController = null;

    private static PersistentGameDataController GameDataController
    {
        get { return gameDataController; }
        set { gameDataController = value; }
    }

    /// <summary>
    /// all references to scenes are not serialized, because scenes are 
    /// saved in a different file
    /// </summary>
    [System.NonSerialized]
    private List<SaveableScene> allScenes = new List<SaveableScene>();

    /// <summary>
    /// Contains all scenes, which where loaded (entered) during the current play session
    /// </summary>
    public List<SaveableScene> AllScenes
    {
        get { return allScenes; }
        private set { allScenes = value; }
    }

    [System.NonSerialized]
    private SaveableScene currentScene;

    public SaveableScene CurrentScene
    {
        get { return currentScene; }
        set
        {
            CurrentSceneName = value.SceneName;
            currentScene = value;
        }
    }

    [System.NonSerialized]
    private bool isInstantiating;

    /// <summary>
    /// is true during the duration of instantiating all saved objects
    /// during load phase
    /// </summary>
    private bool IsInstantiating { set { isInstantiating = value; } get { return isInstantiating; } }

    [System.NonSerialized]
    private bool keepExistingObjects;

    #endregion

    #region serialized fields and its properties (+getter and setter)

    /// <summary>
    /// used to save data, that is not bound to a gameobject
    /// </summary>
    private List<object> staticGameData = new List<object>();

    /// <summary>
    /// adds any type which is serializable 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    private void addStaticGameData<T>(T data)
    {
        if (typeof(T).IsSerializable/* .IsDefined(typeof(ISerializable), true)*/)
        {
            staticGameData.Add(data);
        }
        else
        {
            throw new Exception("Tried to add type " + typeof(T).Name + 
                " to static data list, but type " + typeof(T).Name + 
                " is not marked as serializable!");
        }
    }

    /// <summary>
    /// adds any type which is marked as serializable to 
    /// a list of objects,which are saved and loaded.
    /// This can be used to store data, wich is not assigned to
    /// gameobjects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    public static void addGameData<T>(T data)
    {
        getCurrentGame().addStaticGameData(data);
    }

    private string gameName;

    public string GameName {
        get { return gameName; }
        set { gameName = value; }
    }

    public string CurrentSceneName { get; private set; }

    public DateTime SaveTime { get; set; }
    
    public void setCurrentScene(string sceneName)
    {
        CurrentScene = getScene(sceneName);
    }

    #endregion
    
    public void loadGame(PersistentGameDataController gameDataController)
    {
        SaveableGame.GameDataController = gameDataController;

        SaveableScene loadedScene = GameDataController.LoadSaveable<SaveableScene>
                (FolderSystem.getSceneSavePath(GameName,
                CurrentSceneName));

        ///reset scene list so no changes made before loading are interfering
        AllScenes = new List<SaveableScene>();

        addScene(loadedScene);
        CurrentScene = loadedScene;
        CurrentScene.initiateLoadedScene();

        ///load new scene
        SceneManager.LoadScene(CurrentSceneName);
    }

    /// <summary>
    /// calls "saveScene" for the current scene
    /// </summary>
    /// <param name="saveType"></param>
    public void saveCurrentScene(PersistentGameDataController.SaveType saveType)
    {
        currentScene.saveScene(saveType);
    }

    /// <summary>
    /// calls "saveScene" for the current scene
    /// </summary>
    /// <param name="saveType"></param>
    public List<IRestorableGameObject> saveCurrentScene(
        PersistentGameDataController.SaveType saveType, List<ISaveableGameObject> ignoreForSave)
    {
        return currentScene.saveScene(saveType, ignoreForSave);
    }

    /// <summary>
    /// creates the new scene if it doesnt exist already, in this case "keepObjects" will be true, 
    /// so the scene wont be empty when loaded.
    /// </summary>
    /// <param name="sceneName"></param>
    public SaveableScene prepareNextScene(string sceneName)
    {
        return prepareNextScene(sceneName, new List<IRestorableGameObject>());
    }

    public SaveableScene prepareNextScene(string sceneName, List<IRestorableGameObject> transferToNextScene, bool loadScene = true)
    {
        SaveableScene result = getScene(sceneName);
        FirstTimeSceneLoaded = !loadScene;
        ///check if the scene is loaded already
        if (result == null)
        {
            ///if the scene was not loaded, check if it is even existing as a file in the game folder
            if (sceneExists(sceneName))
            {
                ///if the scene exists in a file load it
                result = GameDataController.LoadSaveable<SaveableScene>(FolderSystem.getSceneSavePath(GameName, sceneName));
                /////create new Scene list. Since its marked as "NonSerialized" its null after loading
                //AllScenes = new List<SaveableScene>();
                addScene(result);
            }
            else
            {
                ///if the scene doesnt exist in file, create a new scene
                result = addNewScene(sceneName);
                ///since the scene was not existing already, "KeepExistingObjects"
                ///is set to true, as the scene objects
                ///should not be deleted when first entering a scene
                FirstTimeSceneLoaded = true;
            }
        }
        CurrentScene = result;
        CurrentScene.initiateLoadedScene();
        
        CurrentScene.TransferedObjectTree = transferToNextScene;
        return result;
    }

    public void restoreCurrentSceneObjects(PersistentGameDataController.GameLoadInitiated gameLoadEvent, bool restoreData)
    {
        IsInstantiating = true;
        CurrentScene.restoreScene(gameLoadEvent, restoreData);
        IsInstantiating = false;
    }

    /// <summary>
    /// will add the scene
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="current">scene will be set as the current scene of the game</param>
    /// <returns>return true when the scene was added</returns>
    public void addScene(SaveableScene scene)
    {
        allScenes.Add(scene);
    }

    /// <summary>
    /// will create a new scene and add it to scene list
    /// </summary>
    /// <param name="sceneName"></param>
    public SaveableScene addNewScene(string sceneName)
    {
        SaveableScene result = new SaveableScene(sceneName);
        addScene(result);
        return result;
    }

    /// <summary>
    /// returns a list of all scenen names which are in the scene folder of
    /// the current game, but are not in the list of all scenes
    /// </summary>
    /// <returns></returns>
    public List<string> getAllNotLoadedScenePaths()
    {
        List<string> result = new List<string>();

        string defaultScenePath = FolderSystem.getDefaulScenePath(GameName);

        ///read all scenes of current game
        foreach (string s in Directory.GetFiles(defaultScenePath))
        {
            string currentFileName = Path.GetFileNameWithoutExtension(s);
            ///every not loaded scene is added to list
            if (!isSceneLoaded(currentFileName))
            {
                result.Add(s);
            }
        }

        return result;
    }

    /// <summary>
    /// returns true if a scene exists in the loadedScene list with a name equals to the given name
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public bool isSceneLoaded(string sceneName)
    {
        return getScene(sceneName) != null;
    }

    /// <summary>
    /// returns true when the requested scene is either in the scenelist or saved to a file
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public bool sceneExists(string sceneName)
    {
        return isSceneLoaded(sceneName) 
            || FolderSystem.sceneExists(GameName, sceneName);
    }
    
    /// <summary>
    /// returns the scene with the given scene name out of all scenes loaded during the current play session
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public SaveableScene getScene(string sceneName)
    {
        return allScenes.Where(scene => scene.SceneName == sceneName).FirstOrDefault();
    }

    /// <summary>
    /// adds the given slot to the garabe heap, which will get the object removed
    /// after the saved objects are restored
    /// </summary>
    /// <param name="gameObject"></param>
    public static void addObjectToGarbageHeap(GameObject gameObject)
    {
        SaveableScene currentScene = getCurrentGame().CurrentScene;
        currentScene.GarbageHeap.Push(gameObject);
    }

    public static void addObjectsToCurrentScene(params ISaveableGameObject[] gameObjects)
    {
        foreach (ISaveableGameObject s in gameObjects)
        {
            addObjectToCurrentScene(s);
        }
    }

    public static void addObjectToCurrentScene(ISaveableGameObject gameObject)
    {
        SaveableScene currentScene = getCurrentGame().CurrentScene;
        currentScene.AllInGameObjects.Add(gameObject);
    }

    public static void removeObjectsFromSavedList(params ISaveableGameObject[] gameObjects)
    {
        foreach (ISaveableGameObject s in gameObjects)
        {
            removeObjectFromSavedList(s);
        }
    }

    public static void removeObjectFromSavedList(ISaveableGameObject gameObject)
    {
        SaveableScene currentScene = getCurrentGame().CurrentScene;
        currentScene.AllInGameObjects.Remove(gameObject);
    }

    /// <summary>
    /// returns the currently active game 
    /// (if there is none, the current scene is used to create a new Game)
    /// </summary>
    /// <returns></returns>
    private static SaveableGame getCurrentGame()
    {
        if(GameDataController == null)
        {
            Debug.LogWarning("Current Game was requested before the game was loaded. " +
                "This should be avoided by loading an existing game or starting a new one out of a menue" +
                " with no saveable objects in its scene.");

            PersistentGameDataController.EnterRunningGame();
        }

        return GameDataController.GetCurrentGame();
    }

}
