using System.Collections.Generic;

public interface IElementList<T> {

    void addElement(T element);

    List<T> getElements();

}
