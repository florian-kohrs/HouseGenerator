
using System.Collections.Generic;

public interface ITreeNode<T>
{
    void getAllNodesWithValues(IList<ITreeNode<T>> tree);

    void getReversedNodesWithValue(Stack<ITreeNode<T>> tree);

    void getValues(IList<T> values);

    Stack<int> getFullTreePath();

    Stack<int> getParentTreePath();

    ITreeNode<T> Previous { get; }

    T Value { get; set; }

    int Key { get; }

    /// <summary>
    /// will return the node or add it if it wasnt existing already
    /// </summary>
    /// <param name="branchIndex"></param>
    /// <param name="value">value of the node</param>
    /// <returns></returns>
    ITreeNode<T> buildNode(int branchIndex, T value = default(T));

    void addBranch(ITreeNode<T> node);

    void addBranch(ITreeNode<T> node, int key);

    ITreeNode<T> getNode(int key);

    void clearNodes();

    /// <summary>
    /// will remove the node as child
    /// </summary>
    /// <param name="node"></param>
    /// <param name="searchChilds">will search recursivly for the node</param>
    /// <returns>true when the node was removed</returns>
    bool removeNode(T node, bool searchChilds = false);
    
    bool removeNodeAt(int key);

}
