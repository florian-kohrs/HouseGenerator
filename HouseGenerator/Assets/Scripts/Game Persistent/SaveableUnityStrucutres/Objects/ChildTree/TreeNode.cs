
using System;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public class TreeNode<T> : ITreeNode<T>
{

    public TreeNode(TreeNode<T> previous, int key, T value = default(T))
    {
        this.previous = previous;
        this.value = value;
        this.key = key;
        nodes = new SortedList<int,ITreeNode<T>>();
    }

    private T value;

    public T Value
    {
        get { return value; }
        set { this.value = value; }
    }

    private readonly int key;

    public int Key
    {
        get { return key; }
    }

    private TreeNode<T> previous;
    
    public SortedList<int, ITreeNode<T>> nodes;
    
    public ITreeNode<T> Previous
    {
        get { return previous; }
    }

    /// <summary>
    /// adds a new Node to the nodes list if the new node didnt exist already.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ITreeNode<T> buildNode(int key, T value = default(T))
    {
        ITreeNode<T> result = getNode(key);
        
        if (result == null)
        {
            result = new TreeNode<T>(this,key, value);
            nodes.Add(key,result);
        }
        
        return result;
    }

    public bool removeNode(T node, bool searchChilds = false)
    {
        bool result = nodes
            .Select(n => n.Value.Value)
            .Where(v => object.ReferenceEquals(v, node)).FirstOrDefault() != null;

        foreach(KeyValuePair<int,ITreeNode<T>> current in nodes)
        {
            result = current.Value.removeNode(node, searchChilds);
        }

        return result;
    }

    public bool removeNodeAt(int key)
    {
        bool result = nodes.ContainsKey(key);
        if (result)
        {
            nodes.Remove(key);
        }
        return result;
    }
    
    public ITreeNode<T> getNode(int key)
    {
        ITreeNode<T> result;
        nodes.TryGetValue(key,out result);
        return result;
    }

    public bool hasNode(int node)
    {
        return getNode(node) != null;
    }
    
    public void getTreePath(Stack<int> list)
    {
        ///normaly I`m against this implementation where "return" is called early
        ///in the function, but it was neccesary to make the function end-recursive
        if (Previous == null)
        {
            return;
        }
        list.Push(Key);
        previous.getTreePath(list);
    }

    public Stack<int> getFullTreePath()
    {
        Stack<int> result = new Stack<int>();
        result.Push(Key);
        if (Previous != null)
        {
            previous.getTreePath(result);
        }
        return result;
    }

    public Stack<int> getParentTreePath()
    {
        Stack<int> result = new Stack<int>();
        if (Previous != null)
        {
            previous.getTreePath(result);
        }
        return result;
    }

    public void getAllNodesWithValues(IList<ITreeNode<T>> result)
    {

        if(value != null)
        {
            result.Add(this);
        }

        foreach (ITreeNode<T> node in nodes.Values)
        {
            node.getAllNodesWithValues(result);
        }
    }

    public void getValues(IList<T> values)
    {

        if(Value != null)
        {
            values.Add(Value);
        }
        
        foreach(ITreeNode<T> node in nodes.Values)
        {
            node.getValues(values);
        }
    }

    public void clearNodes()
    {
        nodes.Clear();
    }

    public void getReversedNodesWithValue(Stack<ITreeNode<T>> result)
    {
        if (value != null)
        {
            result.Push(this);
        }

        foreach (ITreeNode<T> node in nodes.Values)
        {
            node.getReversedNodesWithValue(result);
        }
    }

    public void addBranch(ITreeNode<T> branch)
    {
        addBranch(branch, findFreeIndex());
    }

    private int currentFreeIndex = int.MaxValue;
      
    private int findFreeIndex()
    {
        while (nodes.ContainsKey(currentFreeIndex))
        {
            currentFreeIndex--;
        }
        return currentFreeIndex;
    }

    public void addBranch(ITreeNode<T> branch, int index)
    {
        nodes.Add(index, branch);
    }


}
