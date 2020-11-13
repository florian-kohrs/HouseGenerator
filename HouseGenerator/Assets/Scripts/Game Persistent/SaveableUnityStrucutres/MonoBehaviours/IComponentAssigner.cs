using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this interface is used to display an assigner who holds for all
/// types t that get getComponent<T> != getComponent<T> (unless both requests return null)
/// This means "getComponent" is a non deterministic component Request
/// </summary>
public interface IComponentAssigner
{

    T getComponent<T>() where T : Component;

    void ResetAssigner();

}
