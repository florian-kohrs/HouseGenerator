using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a hybrid component can be saved and restored
/// </summary>
public interface IHybridComponent : IRestorableComponent, ISaveableComponent {


}
