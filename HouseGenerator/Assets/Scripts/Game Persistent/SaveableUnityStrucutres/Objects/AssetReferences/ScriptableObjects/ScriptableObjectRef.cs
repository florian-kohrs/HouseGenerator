using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class ScriptableObjectRef : AssetPolyRef<SaveableScriptableObject>
{
    public ScriptableObjectRef() { }

    protected ScriptableObjectRef(SerializationInfo info, StreamingContext context) : base(info, context) { }
}