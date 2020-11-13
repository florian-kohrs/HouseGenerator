using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// creates a saveable copy on request
/// </summary>
[DisallowMultipleComponent]
public abstract class BaseSaveableGameObject : MonoBehaviour, 
    ISaveableGameObject, ITransformObject, IComponentAssigner
{

    protected abstract IRestorableGameObject createSerializableObject();

    protected void initialize()
    {
        if (childrenWithPath == null)
        {
            childrenWithPath = new HirachyTree<ISaveableGameObject>();
        }
    }

    /// <summary>
    /// stores all components, that are going to be saved
    /// </summary>
    public ISaveableComponent[] saveableComponents;

    /// <summary>
    /// saves for each SaveableGameObject child the list of the gameObject parent Names, so the hirachy can be 
    /// rebuild when the game is loaded
    /// </summary>
    private HirachyTree<ISaveableGameObject> childrenWithPath =
        new HirachyTree<ISaveableGameObject>();

    /// <summary>
    /// the next parent in the scene hirachy, which got the "SaveableGameobject" component attached
    /// </summary>
    private ISaveableGameObject parent;

    /// <summary>
    /// the serialized object created to save the information of this object. also used to save references to this object.
    /// </summary>
    public IRestorableGameObject CreatedSerializableGameObject { get; set; }

    /// <summary>
    /// instantiates all restoreableBehaviours for either the 
    /// "saveableUnityComponents" and the "saveableBehaviours" lists
    /// </summary>
    private void instantiateSaveableComponents()
    {
        IRestorableComponent restorableComponent;

        ///save all registered components and add reference to restoreable list
        foreach (ISaveableComponent component in saveableComponents)
        {
            restorableComponent = component.createRestoreableComponent();
            CreatedSerializableGameObject.addComponent(restorableComponent);
        }
    }

    /// <summary>
    /// saves all script values by calling the "saveComponent" method for all saveableComponents.
    /// Also calls this method rekursivly for its children
    /// </summary>
    /// <param name="saveState"></param>
    public void saveAllBehaviours(PersistentGameDataController.SaveType saveState)
    {
        foreach (ISaveableComponent b in saveableComponents)
        {
            b.saveComponent(gameObject, this, saveState);
        }

        foreach (ISaveableGameObject children in childrenWithPath.getValues())
        {
            children.saveAllBehaviours(saveState);
        }
        ResetAssigner();
    }

    protected virtual void Awake()
    {
        initialize();

        ///if the object was not created during loading phase or the 
        ///load phase is creating all saved objects, or the scene is loaded in default state, add this object to saving list
        ///otherwise destroy it
        if (SaveableGame.KeepObjects)
        {
            SaveableGame.addObjectToCurrentScene(this);
        }
    }

    /// <summary>
    /// will add the children to the dictionary
    /// </summary>
    /// <param name="child">element to add</param>
    /// <param name="childpath">children path of gameObjects names to real parent</param>
    public void addChildren(ISaveableGameObject child, Stack<int> childpath)
    {
        childrenWithPath.add(child, childpath);
    }
    
    private bool IsRootObject
    {
        get { return this.parent == null; }
    }
    
    private void OnDestroy()
    {
        ///if the object was destroyed while the game was not in loading phase,
        ///its removed from the object saving list
        if (!PersistentGameDataController.IsLoading)
        {
            SaveableGame.removeObjectFromSavedList(this);
        }
    }
    
    public bool findAndSetParent()
    {
        Stack<int> _ = new Stack<int>();
        return findAndSetParent(out _);
    }

    /// <summary>
    /// will set the parent (and add this to the parents children).
    /// return true when a parent was set.
    /// </summary>
    /// <returns>return true when a parent was set</returns>
    public bool findAndSetParent(out Stack<int> hirachyPath)
    {
        hirachyPath = setParent();
        return !IsRootObject;
    }

    /// <summary>
    /// will set the current Parent aswell as adding this object to the parents children.
    /// Returns the hirachy child index as list starting from the parent object
    /// </summary>
    protected virtual Stack<int> setParent()
    {
        Stack<int> result = new Stack<int>();

        result.Push(transform.GetSiblingIndex());
        Transform current = transform.parent;
        parent = null;

        if (current != null)
        {
            do
            {
                ///this will not work for scene root objects in standalone build!!!  
                ///scenes must be setup with only one root element only!!
                int currentSiblingIndex = current.GetSiblingIndex();
                
                ///add current non saved object sibling index to parent stack trace
                result.Push(currentSiblingIndex);
                this.parent = current.GetComponent<BaseSaveableGameObject>();
                current = current.parent;
            } while (current != null && parent == null);
        }

        if (parent != null)
        {
            ///if the parent is not null, the sibling index of the parent is removed from the stack
            ///as its not needed to find its own child
            //Debug.Log("Transform has parent so the last index is deleted");
            result.Pop();
            parent.addChildren(this, result);
        }
        
        return result;
    }
    
    /// <summary>
    /// creates a serializable gamobject and sets values of it. Also creates all restoreablescripts
    /// which are going to be saved, however their values are not set in this method.
    /// will also call this method for each of its children adding them to its children list
    /// </summary>
    /// <returns></returns>
    public IRestorableGameObject saveObjectAndPrepareScripts()
    {
        CreatedSerializableGameObject = createSerializableObject();

        saveableComponents = findAllSaveableComponents();

        instantiateSaveableComponents();

        ///sets children hirachy of created serializable gameobject
        foreach (ITreeNode<ISaveableGameObject> childrenWithPath in
            this.childrenWithPath.getAllNodesWithValues())
        {
            CreatedSerializableGameObject.addChildrenWithPath(
                childrenWithPath.Value.saveObjectAndPrepareScripts(),
                childrenWithPath.getFullTreePath()
            );
        }

        return CreatedSerializableGameObject;
    }


    [NonSerialized]
    private Type transferableObjectType = typeof(TransferableAttribute);

    public bool IsTransferable
    {
        get { return GetType().IsDefined(transferableObjectType, true); }
    }

    /// <summary>
    /// will 
    /// </summary>
    protected void prepareSceneTransition()
    {
        foreach (BaseSaveableGameObject g in findAllSaveableTransferableChilds(transform)){
            ///abstract the saving algorithm from saveablescene for no code redundance
        }
    }


    private List<BaseSaveableGameObject> findAllSaveableTransferableChilds(Transform t)
    {
        List<BaseSaveableGameObject> result = new List<BaseSaveableGameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            BaseSaveableGameObject saveableObject = child.GetComponent<BaseSaveableGameObject>();
            if (saveableObject != null)
            {
                if (saveableObject.IsTransferable)
                {
                    result.Add(saveableObject);
                }
            }
            else
            {
                result.AddRange(findAllSaveableTransferableChilds(child));
            }
        }
        return result;
    }

    /// <summary>
    /// finds all Components deriving from BaseSaveableObject
    /// </summary>
    public ISaveableComponent[] findAllSaveableComponents()
    {
        return GetComponents<BaseSaveableObject>();
    }

    public object getTransformedValue()
    {
        return CreatedSerializableGameObject;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int getSiblingIndex()
    {
        return transform.GetSiblingIndex();
    }

    /// <summary>
    /// since a normal "GetComponent(t)" will not work for multiple 
    /// saveable scripts of the same type, all unsaved components are 
    /// stored in this dictionary
    /// </summary>
    [NonSerialized]
    Dictionary<Type,List<Component>> alreadySavedComponents = new Dictionary<Type, List<Component>>();
    
    public T getComponent<T>() where T : Component
    {
        T result = null;
        Type t = typeof(T);
        ///check if components of this type were requested before
        if (alreadySavedComponents.ContainsKey(t))
        {
            List<Component> components = alreadySavedComponents[t];
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
            List<Component> components = GetComponents(t).ToList();
            if (components.Count > 0)
            {
                result = (T)components[0];
                components.RemoveAt(0);
            }
            alreadySavedComponents.Add(t, components);
        }
        return result;
    }

    public void ResetAssigner()
    {
        alreadySavedComponents.Clear();
    }

    public void ResetChildNodes()
    {
        childrenWithPath =
        new HirachyTree<ISaveableGameObject>();
    }
}
