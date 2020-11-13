using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;


[Serializable]
public class Serializable2DVector : ISerializable
{

    public override bool Equals(object obj)
    {
        if(obj != null && obj is Serializable2DVector)
        {
            return v.Equals(((Serializable2DVector)obj).v);
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

    public float x => v.x;

    public float y => v.y;

    private Serializable2DVector(SerializationInfo info, StreamingContext context)
    {
        v.x = info.GetSingle("x");
        v.y = info.GetSingle("y");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("x", v.x);
        info.AddValue("y", v.y);
    }

    public Vector2 v;
    
    public Serializable2DVector(float x, float y) : this(new Vector2(x,y)) { }

    public Serializable2DVector(Vector2 vector)
    {
        v = vector;
    }

    public static implicit operator Vector2(Serializable2DVector vec)
    {
        return vec.v;
    }

    public static implicit operator Serializable2DVector(Vector2 vec)
    {
        return new Serializable2DVector(vec);
    }

}
