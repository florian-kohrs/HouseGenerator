using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IHirachyTree<T>
{

    IList<T> getValues();

    IList<ITreeNode<T>> getAllNodesWithValues();

    IEnumerable<ITreeNode<T>> getReversedNodesWithValue();
    
    void clear();

    void add(T gameObject, Stack<int> path);

    bool remove(T node, bool searchChilds = false);

    void removeNodeAt(Stack<int> path);

}

