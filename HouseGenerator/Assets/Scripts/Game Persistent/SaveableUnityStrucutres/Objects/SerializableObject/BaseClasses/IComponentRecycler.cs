using System;
using UnityEngine;

public interface IComponentRecycler
{
    Component getNextComponent(Type t);
}

