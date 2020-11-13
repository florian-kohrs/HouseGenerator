using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveableScene
{

    public SaveableScene(string name)
    {
        this.sceneName = name;
        DirtyData = true;
    }

    /// <summary>
    /// if this is true, the scene will be saved to file upon next game save
    /// </summary>
    [System.NonSerialized]
    private bool dirtyData;

    public bool DirtyData { get { return dirtyData; } set { dirtyData = value; } }

    private readonly string sceneName;

    public string SceneName { get { return sceneName; } }

    private HirachyTree<IRestorableGameObject> restoreableObjectTree = new HirachyTree<IRestorableGameObject>();

    private List<IRestorableGameObject> transferedObjects;

    public List<IRestorableGameObject> TransferedObjectTree
    {
        get { return transferedObjects; }
        set { transferedObjects = value; }
    }

    public HirachyTree<IRestorableGameObject> RestoreableObjectTree
    {
        get { return restoreableObjectTree; }
    }

    [System.NonSerialized]
    private List<ISaveableGameObject> allInGameObjects = new List<ISaveableGameObject>();

    /// <summary>
    /// stores all gameobjects which are a SaveableRootPrefab and 
    /// were existing in the loaded (loaded from save slot) scene
    /// </summary>
    [System.NonSerialized]
    private Stack<GameObject> garbageHeap = new Stack<GameObject>();

    public Stack<GameObject> GarbageHeap
    {
        get { return garbageHeap; }
        private set { garbageHeap = value; }
    }

    public List<ISaveableGameObject> AllInGameObjects
    {

        get
        {
            if (allInGameObjects == null)
            {
                AllInGameObjects = new List<ISaveableGameObject>();
            }
            return allInGameObjects;
        }

        private set { allInGameObjects = value; }
    }

    /// <summary>
    /// will iterate the list of registered inGameISaveableGameObject and have them create a 
    /// SaveableObject which are saved within this list or referenced by other SaveableObjects.
    /// However this function just stores the data temporary and does not write it into the scene file!
    /// "DirtyData" is set to true, so this scene will be saved to fíle upon next game save.
    /// </summary>
    /// <param name="saveState"></param>
    public void saveScene(PersistentGameDataController.SaveType saveState)
    {
        List<ISaveableGameObject> empty = new List<ISaveableGameObject>();
        saveScene(saveState, empty);
    }

    /// <summary>
    /// will iterate the list of registered inGameISaveableGameObject and have them create a 
    /// SaveableObject which are saved within this list or referenced by other SaveableObjects.
    /// However this function just stores the data temporary and does not write it into the scene file!
    /// "DirtyData" is set to true, so this scene will be saved to fíle upon next game save.
    /// </summary>
    /// <param name="saveState"></param>
    /// <param name="ignoreForSave">hand any gameobject over that shall not be saved. 
    /// instead their theirs trees of restoreableGameobejcts are returned
    /// usefull to take gameobject to the next scene</param>
    /// <returns>this functions returns the unsaved objects, since its very
    /// likely that they shall be transfered to another scene</returns>
    public List<IRestorableGameObject> saveScene(PersistentGameDataController.SaveType saveState,
        List<ISaveableGameObject> ignoreForSave)
    {
        List<IRestorableGameObject> result = new List<IRestorableGameObject>();

        ///clear current list, and fill it with current objects
        RestoreableObjectTree.clear();

        HirachyTree<ISaveableGameObject> saveableObjectTree = new HirachyTree<ISaveableGameObject>();

        Stack<int> currentHirachy;

        ///reset the hirachy tree before saving again
        foreach (ISaveableGameObject gameObject in AllInGameObjects)
        {
            gameObject.ResetChildNodes();
        }

        ///set parent and children of all objects
        ///also adds all objects with no saveable parent to root list
        foreach (ISaveableGameObject gameObject in AllInGameObjects)
        {
            ///if the objects should not be saved in the current scene they
            ///are just ignored for now. since the objects dont use this tree to find 
            ///their parent, the hirachy of the non saved objects are not lost
            if (!ignoreForSave.Contains(gameObject))
            {
                if (!gameObject.findAndSetParent(out currentHirachy))
                {
                    saveableObjectTree.add(gameObject, currentHirachy);
                }
            }
        }

        ///all the gameObjects with no saveableParent are returning their saveable object, 
        ///which are added to the list. childrens are recursivly added aswell
        foreach (ITreeNode<ISaveableGameObject> gameObject in saveableObjectTree.getAllNodesWithValues())
        {
            RestoreableObjectTree.add
                (gameObject.Value.saveObjectAndPrepareScripts(), gameObject.getFullTreePath());
        }

        ///even though the object is not saved in this scene the restoreable objects are
        ///still created and returned as result so they can be used in other scenes
        foreach (ISaveableGameObject gameObject in ignoreForSave)
        {
            result.Add(gameObject.saveObjectAndPrepareScripts());
        }

        IList<ISaveableGameObject> savedObjects = saveableObjectTree.getValues();
        ///after all object with their scripts were initiated the scripts values are saved
        foreach (ISaveableGameObject gameObject in savedObjects)
        {
            gameObject.saveAllBehaviours(saveState);
        }

        ///all not saved objects are saved aswell. however their restoreable references 
        ///will not be associated with the current scene. Instead they are returned
        ///so they can be used in other scenes
        foreach (ISaveableGameObject gameObject in ignoreForSave)
        {
            gameObject.saveAllBehaviours(saveState);
        }

        ///deleting all children for next save
        saveableObjectTree.clear();

        ///since the scene is saved, it needs to be stored to file on the 
        ///next game save
        DirtyData = true;

        return result;
    }

    /// <summary>
    /// clears list of all inGameObjects
    /// </summary>
    public void clearCurrentSceneObjects()
    {
        AllInGameObjects.Clear();
    }

    /// <summary>
    /// restores the whole saved scene
    /// </summary>
    /// <param name="onSetDataInitiated"></param>
    public void restoreScene(PersistentGameDataController.GameLoadInitiated onSetDataInitiated, bool restoreData)
    {
        if (restoreData)
        {
            recreateObjectsAndScripts();
        }

        createTransferedObjects(transferedObjects);

        ///call all event-subscriber
        if (onSetDataInitiated != null)
        {
            onSetDataInitiated();
        }

        if (restoreData)
        {
            RestoreSavedComponentValues();
        }

        RestoreTransferedValues();
    }

    /// <summary>
    /// destroys all gameobjects in the "garbageHeap" list
    /// </summary>
    private void cleanGarbageHeap()
    {
        if (GarbageHeap != null)
        {
            while (GarbageHeap.Count > 0)
            {
                GameObject.DestroyImmediate(GarbageHeap.Pop());
            }
        }
    }

    /// <summary>
    /// creates the game objects with all its saved data and creates all saved scripts for
    /// each gameobject without saved values
    /// </summary>
    private void recreateObjectsAndScripts()
    {
        ///as the parent of the nonPrefabs is going to change, the process must be reversed so that all nonPrefabs
        ///can find their inGameObject without their hirachy sibling index path changed
        IEnumerable<ITreeNode<IRestorableGameObject>> treeNodes = RestoreableObjectTree.getReversedNodesWithValue();

        ///when instantiating the objects, it is important that the process is in normal order, so the 
        ///Enumerable is reversed again and saved in this list.
        List<ITreeNode<IRestorableGameObject>> nodes = new List<ITreeNode<IRestorableGameObject>>();

        ///save all Transforms from nonSaveablePrefabs 
        IList<Transform> allSavedNonPrefabs = new List<Transform>();

        foreach (ITreeNode<IRestorableGameObject> treeNode in treeNodes)
        {
            ///get all objects the possibility to find themselve in the default scene state
            ///before the sibling indicies are restored (only the NonPrefabObjects uses it)
            treeNode.Value.prepareGameObjectReconstruction(allSavedNonPrefabs);
            nodes.Insert(0, treeNode);
        }

        ///set all parent to null so they wont be a child of a saved prefab, since all prefabs will
        ///be destroyed and so would the nonPrefabs as the childs of destroyed objects are 
        ///destroyed aswell
        foreach (Transform t in allSavedNonPrefabs)
        {
            t.SetParent(null);
            ///the order of the objects is one of the most important things in the process of restoring the 
            ///saved scene. all prefabs must be set as last prefabs so the parent hirachy of the root elements 
            ///is not adulterated
            t.SetAsLastSibling();
        }

        ///delete all obsolete objects from the default scene so the saved hirachy path adds up with the
        ///scene
        cleanGarbageHeap();

        foreach (ITreeNode<IRestorableGameObject> treeNode in nodes)
        {
            ///create the saved objects and also hands its parent over, determined by the objects
            ///hirachy path
            treeNode.Value.createObject(
            getTransformFromPath(
                treeNode.getParentTreePath()
            )
            , treeNode.Key);
        }

    }

    private void createTransferedObjects(List<IRestorableGameObject> transferedObjects)
    {
        ///to avoid a bug from Unity, where in Standalone the order of scene root objects
        ///is not deterministic and the sibling index of root objects is always null,
        ///there will be a single main root object, all objects must be childs of
        Transform parent = SceneRootTransform;
        if (this.transferedObjects != null)
        {
            foreach (IRestorableGameObject restorable in transferedObjects)
            {
                ///create the object without a parent as root element
                restorable.createObject(parent);
            }
        }
    }

    public void RestoreTransferedValues()
    {
        if (transferedObjects != null)
        {
            foreach (IRestorableGameObject restorableGameObject in transferedObjects)
            {
                restorableGameObject.restoreComponentValues();
            }
            transferedObjects.Clear();
        }
    }

    /// <summary>
    /// sets the saved values to the created scripts
    /// </summary>
    public void RestoreSavedComponentValues()
    {
        foreach (IRestorableGameObject restorableGameObject in RestoreableObjectTree.getValues())
        {
            restorableGameObject.restoreComponentValues();
        }

    }

    public static Transform getTransformFromPath(Transform start, Stack<int> path)
    {
        int i;
        return getTransformFromPath(start, path, out i);
    }

    /// <summary>
    /// returns the scene object from the given path. also intializes "lastInt" with the last
    /// value in the stack ( equals siblingIndex)
    /// </summary>
    /// <param name="start"></param>
    /// <param name="path"></param>
    /// <param name="lastInt"></param>
    /// <returns></returns>
    public static Transform getTransformFromPath(Transform start, Stack<int> path, out int lastInt)
    {
        Transform result = start;

        lastInt = 0;
        int count = 0;
        foreach (int i in path)
        {
            lastInt = i;
            result = result.GetChild(lastInt);
            count++;
        }
        return result;
    }

    public static Transform getTransformFromPath(Stack<int> path)
    {
        int i;
        return getTransformFromPath(path, out i);
    }

    public static Transform getTransformFromPath(Stack<int> path, out int lastInt)
    {
        Transform start = null;

        if (path.Count > 0)
        {
            int index = path.Pop();

            start = SceneManager.GetActiveScene().GetRootGameObjects()[index].transform;

            start = getTransformFromPath(start, path, out lastInt);

            path.Push(index);
        }
        else
        {
            lastInt = -1;
        }

        return start;
    }

    internal void initiateLoadedScene()
    {
        GarbageHeap = new Stack<GameObject>();
    }

    public static Transform SceneRootTransform
    {
        get
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()[0].transform;
        }
    }

    public static void SetAsRootTransform(Transform t)
    {
        t.parent = SceneRootTransform;
        t.SetAsLastSibling();
    }

}