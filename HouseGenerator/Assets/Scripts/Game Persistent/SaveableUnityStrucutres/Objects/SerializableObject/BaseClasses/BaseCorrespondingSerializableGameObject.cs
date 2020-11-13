using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Saves a gameobject and restores it on request
/// </summary>
[System.Serializable]
public abstract class BaseCorrespondingSerializableGameObject : 
    IRestorableGameObject, ITransformObject, IComponentRecycler, IComponentAssigner
{

    public BaseCorrespondingSerializableGameObject(GameObject gameObject)
    {
        this.gameObjectName = gameObject.name;
        this.active = gameObject.activeSelf;
        this.layer = gameObject.layer;
        this.tag = gameObject.tag;
    }

    protected abstract GameObject getSceneGameObject(Transform transform);
    
    public virtual void prepareGameObjectRestore(IList<Transform> allNonPrefabs){ }

    protected readonly bool active;

    protected readonly string tag;

    protected readonly int layer;

    protected readonly string gameObjectName;

    public List<IRestorableComponent> restoreableComponents = new List<IRestorableComponent>();

    /// <summary>
    /// stores all unrecycled components 
    /// </summary>
    [NonSerialized]
    Dictionary<Type, List<Component>> unrecycledComponents;
    
    public Dictionary<Type, List<Component>> UnrecycledComponents
    {
        get
        {
            if(unrecycledComponents == null)
            {
                unrecycledComponents = new Dictionary<Type, List<Component>>();
            }
            return unrecycledComponents;
        }
    }

    public Component getNextComponent(Type t)
    {
        Component result = null;
        ///check if components of this type were requested before
        if (UnrecycledComponents.ContainsKey(t))
        {
            List<Component> components = unrecycledComponents[t];
            ///if a component is assigned to a restorable script it is removed
            ///from the unused component list
            if (components.Count > 0)
            {
                result = components[0];
                components.RemoveAt(0);
            }
        }
        else
        {
            List<Component> components =  InGameObject.GetComponents(t).ToList();
            if (components.Count > 0)
            {
                result = components[0];
                components.RemoveAt(0);
            }
            unrecycledComponents.Add(t, components);
        }
        ///if the component could not be recycled it is created
        if (result == null)
        {
            result = InGameObject.AddComponent(t);
        }
        return result;
    }

    public void addComponent(IRestorableComponent restoreableComponent)
    {
        restoreableComponents.Add(restoreableComponent);
    }
    
    public IRestorableGameObject parent;

    public IHirachyTree<IRestorableGameObject> childTree
        = new HirachyTree<IRestorableGameObject>();

    [NonSerialized]
    private GameObject inGameObject;

    public GameObject InGameObject {
        get { return inGameObject; }
        set { inGameObject = value; }
    }

    /// <summary>
    /// transfers the saved script data per reflection to the monobehaviour.
    /// calls this function recursicly for all childs
    /// </summary>
    public void restoreComponentValues()
    {

        foreach (IRestorableComponent restoreable in restoreableComponents)
        {
            restoreable.restoreComponent(this);
        }

        foreach (IRestorableGameObject child in childTree.getValues())
        {
            child.restoreComponentValues();
        }
    }

    /// <summary>
    /// The prefabchild and the saveableNonPrefab are both restored by finding them via the 
    /// saved hirachy path. But since the saveable nonPrefab is saved from the default scene path 
    /// and the prefabchild is saved from saved scene state, the nonprefabs must be found before
    /// any sibling indecies are changed while the prefab childs must be found after setting the
    /// saved sibling indecies.
    /// </summary>
    public void prepareGameObjectReconstruction(IList<Transform> allNonPrefabs)
    {
        foreach (TreeNode<IRestorableGameObject> node in childTree.getReversedNodesWithValue())
        {
            node.Value.prepareGameObjectReconstruction(allNonPrefabs);
        }

        prepareGameObjectRestore(allNonPrefabs);
    }

    /// <summary>
    /// attaches all saved scripts to the current GameObject which arent attached already.
    /// Also deletes all saveable Scripts from the GameObject, which werent saved.
    /// Doesnt set any value to the scripts yet.
    /// </summary>
    /// <param name="saveableGameObject"></param>
    private void reconstructAttachedComponentList(BaseSaveableGameObject saveableGameObject)
    {
        ///add all saved unity components to the game object, and adds the reference to the 
        ///saveableComponent list
        ISaveableComponent savableComponent;
        foreach (IRestorableComponent component in restoreableComponents)
        {
            savableComponent = component.addComponent(saveableGameObject.gameObject, this);
        }

        ///destroy all saveable scripts, which where not created during this process
        ///(WasCreated property is false)
        ///(this only applies for the saveableBehaviours, not the UnitComponents)
        foreach (BaseSaveableMonoBehaviour saveableMonoBehaviour
            in saveableGameObject.GetComponents<BaseSaveableMonoBehaviour>())
        {
            if (!saveableMonoBehaviour.WasCreated)
            {
                UnityEngine.Object.Destroy(saveableMonoBehaviour);
            }
        }
    }
    
    /// <summary>
    /// creates the gameobject and scripts (which values are not loaded yet).
    /// this is called recursiv for all children of this game object
    /// </summary>
    public GameObject createObject(Transform parent, int siblingIndex = -1)
    {
        GameObject gameObject;

        gameObject = getSceneGameObject(parent);

        BaseSaveableGameObject saveableObject = gameObject.GetComponent<BaseSaveableGameObject>();

        inGameObject = gameObject;

        gameObject.SetActive(active);
        gameObject.tag = tag;
        gameObject.transform.parent = parent;
        gameObject.layer = layer;
        gameObject.name = gameObjectName;

        if (siblingIndex >= 0)
        {
            gameObject.transform.SetSiblingIndex(siblingIndex);
        }

        reconstructAttachedComponentList(saveableObject);

        foreach (TreeNode<IRestorableGameObject> node in childTree.getAllNodesWithValues())
        {
            node.Value.createObject(
                SaveableScene.getTransformFromPath(
                    gameObject.transform, node.getParentTreePath())
                , node.Key);
        }

        return gameObject;
    }

    public object getTransformedValue()
    {
        return inGameObject;
    }
    
    public void addChildrenWithPath(IRestorableGameObject child, Stack<int> hirachyPath)
    {
        childTree.add(child, hirachyPath);
    }

    /// <summary>
    /// since a normal "GetComponent(t)" will not work for multiple 
    /// saveable scripts of the same type, all unsaved components are 
    /// stored in this dictionary
    /// </summary>
    [NonSerialized]
    Dictionary<Type, List<Component>> alreadyRestoredComponents;

    private Dictionary<Type, List<Component>> AlreadyRestoredComponents
    {
        get
        {
            if(alreadyRestoredComponents == null)
            {
                alreadyRestoredComponents = new Dictionary<Type, List<Component>>();
            }
            return alreadyRestoredComponents;
        }
    }
    
    public T getComponent<T>() where T : Component
    {
        T result = null;
        Type t = typeof(T);
        ///check if components of this type were requested before
        if (AlreadyRestoredComponents.ContainsKey(t))
        {
            List<Component> components = alreadyRestoredComponents[t];
            ///if a component is assigned to a saveable object it is removed
            ///from the unused component list
            if (components.Count > 0)
            {
                result = (T)components[0];
                components.RemoveAt(0);
            }
        }
        else
        {
            List<Component> components = InGameObject.GetComponents(t).ToList();
            if (components.Count > 0)
            {
                result = (T)components[0];
                components.RemoveAt(0);
            }
            alreadyRestoredComponents.Add(t, components);
        }
        return result;
    }

    public void ResetAssigner()
    {
        AlreadyRestoredComponents.Clear();
    }
}
