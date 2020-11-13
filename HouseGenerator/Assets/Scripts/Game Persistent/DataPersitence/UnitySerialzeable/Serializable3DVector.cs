using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;


[Serializable]
public class Serializable3DVector : ISerializable
{

    public override bool Equals(object obj)
    {
        if (obj != null && obj is Serializable3DVector)
        {
            return v.Equals(((Serializable3DVector)obj).v);
        }
        else
        {
            return v.Equals(obj);
        }
    }

    public override int GetHashCode()
    {
        return v.GetHashCode();
    }

    protected Serializable3DVector(SerializationInfo info, StreamingContext context)
    {
        v.x = info.GetSingle("x");
        v.y = info.GetSingle("y");
        v.z = info.GetSingle("z");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("x", v.x);
        info.AddValue("y", v.y);
        info.AddValue("z", v.z);
    }

    public Vector3 v;


    public static implicit operator Vector3(Serializable3DVector vec)
    {
        return vec.v;
    }

    public static implicit operator Serializable3DVector(Vector3 vec)
    {
        return new Serializable3DVector(vec);
    }

    public Serializable3DVector(float x, float y, float z) : this(new Vector3(x,y,z)) { }

    public Serializable3DVector(Vector3 vector)
    {
        this.v = vector;
    }
    
}
