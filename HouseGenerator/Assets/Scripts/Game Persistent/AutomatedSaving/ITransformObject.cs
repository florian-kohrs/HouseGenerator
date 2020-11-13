using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// can be used to define an alternative behaviour for objects when being saved, instead of the
/// default value transfer via the "Save"-attribute
/// </summary>
public interface ITransformObject
{

    object getTransformedValue();

}
