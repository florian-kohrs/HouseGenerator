using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// T is value, V is branch
/// </summary>
/// <typeparam name="T">value</typeparam>
/// <typeparam name="V">key</typeparam>
[System.Serializable]
public class HirachyTree<T> : IHirachyTree<T> {

    public HirachyTree()
    {
        root = new TreeNode<T>(null,0);
    }
    
    private ITreeNode<T> root;
    
    public IList<T> getValues()
    {
        IList<T> result = new List<T>();

        root.getValues(result);

        return result;
    }

    public IList<ITreeNode<T>> getAllNodesWithValues()
    {
        IList<ITreeNode<T>> result = new List<ITreeNode<T>>();
        root.getAllNodesWithValues(result);
        return result;
    }
    
    public void clear()
    {
        this.root.clearNodes();
    }

    public void addBranch(ITreeNode<T> branch, Stack<int> path)
    {
        ITreeNode<T> current = root;

        while (path.Count > 1)
        {
            current = current.buildNode(path.Pop());
        }

        if(path.Count == 1)
        {
            current.addBranch(branch, path.Pop());
        }
        else //Count == 0
        {
            current.addBranch(branch);
        }

    }

    public void add(T node, Stack<int> path)
    {
        ITreeNode<T> current = root;

        while (path.Count > 0)
        {
            current = current.buildNode(path.Pop());
        }

        ///set the value of the last node of 
        ///the path to the given gameobject
        current.Value = node;
    }

    /// <summary>
    /// removes the node at the given children path
    /// </summary>
    /// <param name="path"></param>
    public void removeNodeAt(Stack<int> path)
    {
        ITreeNode<T> current = root;

        while (path.Count > 1)
        {
            current = current.getNode(path.Pop());
        }

        root.removeNodeAt(path.Pop());
    }

    public IEnumerable<ITreeNode<T>> getReversedNodesWithValue()
    {
        Stack<ITreeNode<T>> result = new Stack<ITreeNode<T>>();
        root.getReversedNodesWithValue(result);
        return result;
    }

    public bool remove(T node, bool searchChilds = false)
    {
        return root.removeNode(node, searchChilds);
    }

}
