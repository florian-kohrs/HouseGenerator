using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

public static class AutomatedScriptTransfer
{

    public static void transferScriptsSaving(object source, Dictionary<string, object> target, PersistentGameDataController.SaveType transferState)
    {
        Type sourceType = source.GetType();

        Type savedAttribute = typeof(SaveAttribute);

        foreach (FieldInfo f in getFieldsFromType(sourceType, typeof(SaveableMonoBehaviour)))
        {
            if (f.IsDefined(savedAttribute, true))
            {
                SaveAttribute currentSaveAttribute = (SaveAttribute)
                    (f.GetCustomAttributes(savedAttribute, false)[0]);

                if ((transferState == PersistentGameDataController.SaveType.Game && currentSaveAttribute.saveForScene)
                    || (transferState == PersistentGameDataController.SaveType.Scene && currentSaveAttribute.saveForGame))
                {
                    object value = getValue(f.GetValue(source));
                    target.Add(f.Name, value);
                }
            }
        }
    }

    public static void restoreData(Dictionary<string, object> source,
        ISaveableComponent target, Type lastSerializedType)
    {
        FieldInfo[] fields = getFieldsFromType(target.GetType(), lastSerializedType);

        foreach (KeyValuePair<string, object> entry in source)
        {
            fields.Where(field => field.Name == entry.Key).First().
                setValue(target, getValue(entry.Value));
        }
    }

    private static FieldInfo[] getFieldsFromType(Type target, Type last, bool lastInclusive = true)
    {
        List<FieldInfo> fields = new List<FieldInfo>();

        ///add all public and private fields of current type
        fields.AddRange(target.GetFields((BindingFlags.Public
            | BindingFlags.NonPublic | BindingFlags.Instance)));

        ///add all private fields of parent types. Stops after type "BaseSaveableMonoBehaviour" 
        Type parentType = target;
        while (parentType != last && (lastInclusive || parentType.BaseType != last))
        {
            parentType = parentType.BaseType;
            fields.AddRange(parentType.GetFields((
                BindingFlags.NonPublic | BindingFlags.Instance)).Where(f => !f.IsFamily));
        }

        return fields.ToArray();
    }

    /// <summary>
    /// returns the value of an object, unless its an ITransformObject. Then 
    /// the getTransformedValue will get called and returned.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private static object getValue(object source)
    {
        object result = source;

        if (result is ITransformObject)
        {
            ITransformObject transformObject = (ITransformObject)result;
            result = transformObject.getTransformedValue();
        }
        //else
        //{
        //    Type t = source.GetType();
        //    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>))
        //    {
        //        IList a = source as IList; 
        //        Type itemType = t.GetGenericArguments()[0];
        //        if (itemType == typeof(IRestoreObject))
        //        {
        //            return source as 
        //        }
        //    }
        //    //    else if (value is IEnumerable<KeyValuePair<object, object>>)
        //    //    {
        //    //        //foreach (object o in (value as IEnumerable<K>)
        //    //    }
        //    //    else
        //    //    {
        //    //        field.SetValue(target, value);
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    field.SetValue(target, value);
        //    //}
        //}

        return result;
    }

    /// <summary>
    /// will set the value into the field unless value is IRestoreObject.
    /// Then the value returned by value.restoreObject is set into the field
    /// </summary>
    /// <param name="field"></param>
    /// <param name="target"></param>
    /// <param name="value"></param>
    private static void setValue(this FieldInfo field, object target, object value)
    {
        if (value != null)
        {
            if (value is IRestoreObject)
            {
                field.SetValue(target, ((IRestoreObject)value).restoreObject());
            }
            else
            {
                field.SetValue(target, value);
            }
            //else
            //{
            //    Type t = value.GetType();
            //    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>))
            //    {
            //        Type itemType = t.GetGenericArguments()[0];
            //        if (itemType == typeof(IRestoreObject))
            //        {
            //            field.SetValue(target, (value as IList<IRestoreObject>).Select(v => v.restoreObject()).ToList());
            //        }
            //        else if (value is IEnumerable<KeyValuePair<object, object>>)
            //        {
            //            //foreach (object o in (value as IEnumerable<K>)
            //        }
            //        else
            //        {
            //            field.SetValue(target, value);
            //        }
            //    }
            //    else
            //    {
            //        field.SetValue(target, value);
            //    }
            //}
        }
    }

    /// <summary>
    /// stores all fieldValues which are Serializable into the serialization info
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public static void transferComponentSaving(SaveableComponent source, Dictionary<string, object> target)
    {
        FieldInfo[] targetFields = getFieldsFromSaveableComponent(source.GetType());

        foreach (FieldInfo f in targetFields)
        {
            target.Add(f.Name, getValue(f.GetValue(source)));
        }

    }

    /// <summary>
    /// returns all field infos from the given type until the SerializableUnityComponent<?> type
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private static FieldInfo[] getFieldsFromSaveableComponent(Type target)
    {
        List<FieldInfo> fields = new List<FieldInfo>();

        ///add all public and private fields of current type
        fields.AddRange(target.GetFields((BindingFlags.Public
            | BindingFlags.NonPublic | BindingFlags.Instance)));

        ///add all private fields of parent types. Stops after type "BaseSaveableMonoBehaviour" 
        Type parentType = target;
        while (parentType.BaseType.BaseType != typeof(SaveableComponent))
        {
            parentType = parentType.BaseType;
            fields.AddRange(parentType.GetFields((
                BindingFlags.NonPublic | BindingFlags.Instance)).Where(f => !f.IsFamily));
        }

        return fields.ToArray();
    }

}
