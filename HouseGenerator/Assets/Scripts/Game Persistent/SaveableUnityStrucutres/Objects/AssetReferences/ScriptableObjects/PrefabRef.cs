using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class PrefabRef : AssetPolyRef<SaveablePrefabRoot>
{
    public PrefabRef() { }

    protected PrefabRef(SerializationInfo info, StreamingContext context) : base(info, context) { }

}
