using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentGameDataController
{

    #region define events

    ///event when the game loading is finished
    public delegate void GameLoadFinish();
    public event GameLoadFinish onGameLoadFinish;

    /// <summary>
    /// event when all objects and scripts were created but the scripts values werent
    /// restored yet -> pretty usefull to subscribe to events if needed
    /// </summary>
    public delegate void GameLoadInitiated();
    public event GameLoadInitiated onGameLoadInitiated;

    #endregion

    public enum SaveType
    {
        /// <summary>
        /// a game is saved either by checkpoints, 
        /// or when the users chooses to do so
        /// </summary>
        Game,
        /// <summary>
        /// if the current scene is left and a new one entered, 
        /// the old scene is saved before getting disposed
        /// </summary>
        Scene
    }

    /// <summary>
    /// when a new game starts this scene will be loaded
    /// as long as no other one was specified
    /// </summary>
    private string defaultSceneName = "startscene";

    /// <summary>
    /// object used as lock to provide starting multiple loading processes
    /// </summary>
    private Object SyncRoot = new Object();

    public string DefaultSceneName
    {
        get { return defaultSceneName; }
        set { defaultSceneName = value; }
    }

    /// <summary>
    /// is true during the loading phase
    /// </summary>
    public static bool IsLoading
    {
        get { return GetInstance().isLoading; }
        private set { GetInstance().isLoading = value; }
    }

    /// <summary>
    /// is true while loading a game or starting a new one
    /// </summary>
    private bool isLoading;

    /// <summary>
    /// behaves kinda simular to "isLoading" but is set to false in a later loading 
    /// state
    /// </summary>
    private bool internIsLoading = false;

    private static PersistentGameDataController instance;

    public static Timer timer = new Timer();

    private PersistentGameDataController()
    {
        Debug.Log(FolderSystem.getDefaultSaveSlotPath());

        initializeSavePath();

        CheckForExistingSettings();

        LoadSaveSlots();

        initializeEvents();
    }

    /// <summary>
    /// this list is the only thing that will get saved
    /// </summary>
    private List<SaveableGame> allSavedGames = new List<SaveableGame>();

    public int getAllSavedGamesCount()
    {
        return allSavedGames.Count;
    }

    public static Settings Settings
    {
        get
        {
            return GetInstance().settings;
        }
    }

    private Settings settings;

    private SaveableGame currentGame;

    public SaveableGame GetCurrentGame()
    {
        return currentGame;
    }

    private bool IsDataRestored { get; set; }

    /// <summary>
    /// saves the current scene and loads the new one afterwards. The parameters
    /// "saveCurrent" and "loadNext" can be used to not save the current scene,
    /// and not loading the next scene. The intention for this is, that the menue
    /// probalby should not be saved
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="saveCurrent">should the current scene be saved?</param>
    /// <param name="loadNext">should the next scene be loaded?</param>
    /// <param name="transferObjects">theese objects will be taken to the next scene<</param>
    public void EnterScene(string sceneName, bool saveCurrent = true,
        bool loadNext = true, params ISaveableGameObject[] transferObjects)
    {
        lock (SyncRoot)
        {
            IsDataRestored = loadNext;

            List<IRestorableGameObject> transferedHirachy = null;
            if (saveCurrent)
            {
                transferedHirachy = SaveCurrentSceneBeforeEnteringNew(transferObjects.ToList());
            }

            ///creates next scene if it doesnt exist

            currentGame.prepareNextScene(sceneName, transferedHirachy, loadNext);

            LoadScene(sceneName);
        }
    }


    public void EnterScene(string sceneName)
    {
        EnterScene(sceneName, true, true);
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void SaveCurrentSceneBeforeEnteringNew()
    {
        ///if the scene-change is not for loading purpose, the current scene objects will be
        ///saved before being destroyed 
        if (!IsLoading)
        {
            IsLoading = true;

            SaveScene();

            ///clear ingame object list, since the objects wont exist in the next scene
            currentGame.CurrentScene.clearCurrentSceneObjects();
        }
    }

    private List<IRestorableGameObject> SaveCurrentSceneBeforeEnteringNew(List<ISaveableGameObject> ignoreForSave)
    {
        List<IRestorableGameObject> result = null;

        ///if the scene-change is not for loading purpose, the current scene objects will be
        ///saved before being destroyed 
        if (!IsLoading)
        {
            IsLoading = true;

            result = saveScene(ignoreForSave);

            ///clear ingame object list, since the objects wont exist in the next scene
            currentGame.CurrentScene.clearCurrentSceneObjects();
        }

        return result;
    }

    private static PersistentGameDataController GetInstance()
    {
        if (instance == null)
        {
            instance = new PersistentGameDataController();
        }
        return instance;
    }

    /// <summary>
    /// will load the existing settings or create them, if they didnt exist before.
    /// </summary>
    private void CheckForExistingSettings()
    {
        string settingsSavepath = FolderSystem.getSettingsPath();
        ///initialize and save if not exisiting
        if (!File.Exists(settingsSavepath))
        {
            settings = new Settings();
            Save(settingsSavepath, settings);
        }
        else
        {
            ///save, if settings file exists
            settings = LoadSaveable<Settings>(settingsSavepath);
        }
    }

    public static void SaveSettings()
    {
        GetInstance().SaveSettings_();;
    }

    private void SaveSettings_()
    {
        Save(FolderSystem.getSettingsPath(), settings);
    }

    private void initializeEvents()
    {
        SceneManager.activeSceneChanged += delegate
        {
            if (IsLoading)
            {
                timer.addCheckPoint("new Scene loaded");
                currentGame.restoreCurrentSceneObjects(onGameLoadInitiated, IsDataRestored);
                IsDataRestored = false;
                timer.addCheckPoint("scene restored");
                IsLoading = false;
            }
        };

        SceneManager.sceneLoaded += delegate
        {
            if (internIsLoading)
            {
                if (onGameLoadFinish != null)
                {
                    onGameLoadFinish();
                }
                internIsLoading = false;

                ///clear all registered objects so the references wont be left after
                ///the objects were destroyed
                onGameLoadFinish = null;
                timer.finish("finished loading");
            }
        };
    }

    private void initializeSavePath()
    {
        FolderSystem.createDefaultFolderSystem();
    }

    public static SceneSwitcher GetSceneSwitcher()
    {
        return GetInstance().GetSceneSwitcher_();
    }

    private SceneSwitcher GetSceneSwitcher_()
    {
        return new SceneSwitcher(GetInstance());
    }

    /// <summary>
    /// starts a new game and opens a scene named equals to the 
    /// "DefaultSceneName" property (="startscene")
    /// </summary>
    public static void NewGame()
    {
        GetInstance().NewGame_();
    }

    private void NewGame_()
    {
        NewGame_(DefaultSceneName);
    }
    public static void NewGame(string startScene)
    {
        GetInstance().NewGame_(startScene);
    }

    /// <summary>
    /// starts a new game and opens a scene with the same name than the given name
    /// </summary>
    /// <param name="startScene"></param>
    private void NewGame_(string startScene)
    {
        IsLoading = true;
        instance.currentGame = new SaveableGame(this);
        EnterScene(startScene, false, true);
    }

    /// <summary>
    /// If the game starts without creating a new saveable game or loading an existing game
    /// a new game will be instantly created
    /// </summary>
    public static void EnterRunningGame()
    {
        GetInstance().EnterRunningGame_();
    }

    private void EnterRunningGame_()
    {
        lock (SyncRoot)
        {
            IsLoading = true;
            currentGame = new SaveableGame(this);
            string currentSceneName = SceneManager.GetActiveScene().name;

            currentGame.prepareNextScene(currentSceneName, null, false);
        }
    }

    /// <summary>
    /// saves the scene temporarily -> isnt saved to file
    /// </summary>
    private void SaveScene()
    {
        currentGame.saveCurrentScene(PersistentGameDataController.SaveType.Scene);
    }


    private List<IRestorableGameObject>
        saveScene(List<ISaveableGameObject> ignoreForSave)
    {
        return currentGame.saveCurrentScene(
            PersistentGameDataController.SaveType.Scene, ignoreForSave);
    }

    public static bool SaveGame(string saveName)
    {
        return GetInstance().SaveGame_(saveName);
    }

    private bool SaveGame_(string saveName)
    {
        string s;
        return SaveGame_(saveName, out s);
    }

    /// <summary>
    /// saves the game, by either overwriting an existing game slot, or
    /// creating a new one
    /// </summary>
    /// <param name="saveName"></param>
    /// <param name="resultMessage">returns either "Game saved." or the 
    /// occured exception message</param>
    /// <returns>return true when no exception was thrown</returns>
    public static bool SaveGame(string saveName, out string resultMessage)
    {
        return GetInstance().SaveGame_(saveName, out resultMessage);
    }

    private bool SaveGame_(string saveName, out string resultMessage)
    {
        timer.start("Save Game");
        bool result = true;
        // try
        // {
        SaveableGame overwriteThis =
            allSavedGames.Where(game => game.GameName == saveName).FirstOrDefault();

        ///if a game is overwritten, and its not the current game, 
        ///or a new save slot is created the current
        ///game has to be reloaded from file, to avoid two games
        ///in the game list have the same reference
        bool overwriteCurrent = saveName == currentGame.GameName;

        ///the current game index is used, to destroy the reference between the 
        ///original loaded game and the new save slot.
        int currentGameIndex = -1;

        ///this is only needed, when a different save slot than the current
        ///is used or no save slot is overwritten (new one or other)
        if (!overwriteCurrent || overwriteThis == null)
        {
            currentGameIndex = allSavedGames.IndexOf(currentGame);
        }

        string oldGameName = currentGame.GameName;

        ///converts all objects into saveable objects
        currentGame.saveCurrentScene(PersistentGameDataController.SaveType.Game);

        timer.addCheckPoint("current scene saved");

        currentGame.GameName = saveName;

        if (overwriteThis != null && !overwriteCurrent)
        {
            allSavedGames.Remove(overwriteThis);
            FolderSystem.DeleteGame(overwriteThis);
            overwriteThis = null;
        }

        ///store game and loaded scenes in file
        SaveGameAndScenesToFile(saveName);

        ///if a new save slot is created, all not loaded scenes must be copied
        ///in the new director, so all data is transfered
        if (overwriteThis == null)
        {
            ///only copy if the current game exists in the savedGameList
            if (currentGameIndex >= 0)
            {
                CopyAllNotLoadedScenesToDirectory
                    (FolderSystem.getDefaulScenePath(saveName));
            }
            allSavedGames.Add(currentGame);
        }

        ///if the index is bigger equals zero the old game should/will be reloaded
        ///to avoid two saved games reference to the same saveableGame
        if (currentGameIndex >= 0)
        {
            ///load old game and set it in the savedgame list
            allSavedGames[currentGameIndex] = LoadSaveable<SaveableGame>
                (FolderSystem.getGameSavePath(oldGameName));
        }

        resultMessage = "Game saved.";
        result = true;
        /*  }
          catch (System.Exception ex)
          {
              ///if an error occured, the first attempt to fix it, is to 
              ///reload the existing games out of the files
              loadSaveSlots();
              resultMessage = "An error occured while saving the game: " + 
                  ex.ToString() + ", StackTrace:" + ex.StackTrace;
              result = false;
              Debug.LogWarning(resultMessage);
          }*/
        timer.finish("finished saving");
        return result;
    }

    /// <summary>
    /// copies all not loaded scenes to new directory
    /// </summary>
    /// <param name="copyTo"></param>
    private void CopyAllNotLoadedScenesToDirectory(string copyTo)
    {
        foreach (string s in currentGame.getAllNotLoadedScenePaths())
        {
            string copyToFile = copyTo + "/" + Path.GetFileName(s);
            File.Copy(s, copyToFile);
        }
    }

    private void SaveGameAndScenesToFile(string saveName)
    {
        FolderSystem.createNewSaveSlotDirectory(saveName);

        ///save game
        Save(FolderSystem.getGameSavePath(currentGame.GameName), currentGame);

        ///create or overwrite all changed scenes
        foreach (SaveableScene s in currentGame.AllScenes)
        {
            ///if the scene exists, but was not changed since the last save, it doesnt need to be saved again
            if (s.DirtyData)
            {
                Save(FolderSystem.getSceneSavePath(currentGame, s.SceneName), s);
                s.DirtyData = false;
            }
        }
    }

    /// <summary>
    /// loads all saved games slots, to show the load choices
    /// </summary>
    public void LoadSaveSlots()
    {
        foreach (string s in FolderSystem.getAllSaveSlotNames())
        {
            SaveableGame game = LoadSaveable<SaveableGame>
                (FolderSystem.getGameSavePath(Path.GetFileName(s)));
            allSavedGames.Add(game);
        }
    }


    public static SaveableGame[] GetAllSaveSlots()
    {
        return instance.allSavedGames.ToArray();
    }

    /// <summary>
    /// load the game with the given name
    /// </summary>
    /// <param name="index"></param>
    public static void LoadGame(string name)
    {
        GetInstance().LoadGame_(name);
    }

    private void LoadGame_(string name)
    {
        LoadGame_(allSavedGames.IndexOf
            (allSavedGames.Where(game => game.GameName == name).First()));
    }

    public static bool IsValidSavedGame(string gameName)
    {
        return GetInstance().IsValidSavedGame_(gameName);
    }

    private bool IsValidSavedGame_(string gameName)
    {
        return allSavedGames.Where(game => game.GameName == gameName).FirstOrDefault() != null;
    }

    /// <summary>
    /// load the game with the given index
    /// </summary>
    /// <param name="index"></param>
    public static void LoadGame(int index)
    {
        GetInstance().LoadGame_(index);
    }

    /// <summary>
    /// exits the game without saving and enters the menue
    /// </summary>
    /// <param name="menueSceneName"></param>
    public static void ExitGame(string menueSceneName)
    {
        GetInstance().ExitGame_(menueSceneName);
    }

    private void ExitGame_(string menueSceneName)
    {
        currentGame = null;
        LoadScene(menueSceneName);
    }

    public void LoadGame_(int index)
    {
        timer.start("started loading");
        lock (SyncRoot)
        {
            IsDataRestored = true;

            IsLoading = true;

            internIsLoading = true;

            ///reload game out of file (in case a reference changed something)
            allSavedGames[index] = LoadSaveable<SaveableGame>
                (FolderSystem.getGameSavePath(allSavedGames[index].GameName));

            timer.addCheckPoint("read data out of file");

            ///clones the save slot, so it can be edited without altering the save slot
            currentGame = allSavedGames[index];

            currentGame.loadGame(this);

        }
    }

    private void Clear()
    {
        throw new System.NotImplementedException();
    }

    private void Save(string path, object saveThis)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(path))
        {
            bf.Serialize(file, saveThis);
        }
    }

    /// <summary>
    /// load the object from the given path and cast it to the given type T
    /// </summary>
    /// <typeparam name="T">The Type to load</typeparam>
    /// <param name="path">path to load the object from</param>
    /// <returns></returns>
    public T LoadSaveable<T>(string path)
    {
        T result = default(T);
        //Debug.Log("loadthis." + path);
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.OpenRead(path))
            {
                result = (T)bf.Deserialize(file);
            }
        }
        else
        {
            throw new DirectoryNotFoundException("Directory not found: " + path);
        }
        return result;
    }
}