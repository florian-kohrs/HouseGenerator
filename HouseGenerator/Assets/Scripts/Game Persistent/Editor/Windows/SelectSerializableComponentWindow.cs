using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

/// <summary>
/// used to show a popup window to select a Serializable Unity Component to add to a SaveableGameobject
/// </summary>
[CanEditMultipleObjects]
public class SelectSerializableComponentWindow : PopupWindowContent, IComparer<Type>
{
    public SelectSerializableComponentWindow(IElementList<Type> elementList)
    {
        this.elementList = elementList;
        classTypes = getListOfType<SaveableComponent>();
    }

    private IElementList<Type> elementList;

    private List<Type> classTypes;

    public override Vector2 GetWindowSize()
    {
        return new Vector2(225, 150);
    }

    /// <summary>
    /// since the popup box is recreated every time "OnGui" is called,
    /// the selected item has to be stored outside the function
    /// </summary>
    private int selected = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rect"></param>
    public override void OnGUI(Rect rect)
    {
        bool empty = false;
        
        List<string> classNames = classTypes.Select(current => current.Name).ToList();
        if (classNames.Count == 0)
        {
            empty = true;
            classNames.Add("empty");
            EditorGUILayout.LabelField("No Scripts found deriving from the");
            EditorGUILayout.LabelField("\"SerializeableComponent\" class.");
        }

        string[] options = classNames.ToArray();

        selected = EditorGUILayout.Popup(/*"Select Type:", */selected, options);

        if (!empty)
        {
            if (GUILayout.Button("Add Component"))
            {
                elementList.addElement(classTypes[selected]);
                OnGUI(rect);
            }
        }
        if (GUILayout.Button("Back"))
        {
            editorWindow.Close();
        }
    }

    /// <summary>
    /// returns a list of all Types which derives from  T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public List<Type> getListOfType<T>() where T : class
    {
        List<Type> result = new List<Type>();
        List<Type> existingTypes = elementList.getElements()
            .Where(current => current != null).ToList();

        foreach (Type type in
            Assembly.GetAssembly(typeof(T)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract
            && myType.IsSubclassOf(typeof(T)) && !myType.IsGenericType
            && !existingTypes.Contains(myType)))
        {
            result.Add(type);
        }
        result.Sort(this);
        result.Reverse();
        return result;
    }

    public int Compare(Type x, Type y)
    {
        return string.Compare(y.GetType().Name, x.GetType().Name);
    }
    
}
