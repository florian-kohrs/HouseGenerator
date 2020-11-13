using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveRefToAssetTest : SaveableMonoBehaviour
{

    [Save]
    public List<ScriptableObjectRef> srcs;

    [Save]
    public ScriptableObjectRef scr;

    [Save]
    public int x = 5;
    
    public int y = 5;
}
