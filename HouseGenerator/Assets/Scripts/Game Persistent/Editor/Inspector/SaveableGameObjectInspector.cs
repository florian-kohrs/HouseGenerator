using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

/// <summary>
/// used to display a custom view of the SaveableGameobject in the inspector
/// </summary>
[CustomEditor(typeof(BaseSaveableGameObject), true)]
[CanEditMultipleObjects]
public class SaveableGameObjectInspector : Editor, IElementList<Type>
{
    
    private BaseSaveableGameObject inspectorTarget;

    private void OnEnable()
    {
        inspectorTarget = (BaseSaveableGameObject)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        if (GUILayout.Button("Add Serializable Component"))
        {
            SelectSerializableComponentWindow window = new SelectSerializableComponentWindow(this);
            PopupWindow.Show(new Rect(new Vector2(0, 350), new Vector2(250, 150)), window);
        }
        serializedObject.ApplyModifiedProperties();
    }

    public void addElement(Type element)
    {
        Undo.RegisterCompleteObjectUndo(inspectorTarget, "SaveableComponent added");

        ///since an actual instance can not be used as a prefab value the type name
        ///is stored, and the instance is created at runtime.
        SaveableComponent addedComponent = (SaveableComponent)inspectorTarget.gameObject.AddComponent(element);

        if (element.BaseType.IsGenericType)
        {
            Type type = element.BaseType.GetGenericArguments()[0];
            Component component = inspectorTarget.gameObject.GetComponent(type);

            if (component == null)
            {
                // DialogWindow window = new DialogWindow(this);
                // PopupWindow.Show(new Rect(new Vector2(0, 350), new Vector2(250, 150)), window);
                component = inspectorTarget.gameObject.AddComponent(type);
            }

            addedComponent.BaseComponent = component;
        }

        ///Unity documentation disadvises to use this function in this situation,
        ///but no other solution worked for saving the new list element in the scene,
        ///without losing the reference after entering and leaving play mode.
        EditorUtility.SetDirty(inspectorTarget);
    }

    public List<Type> getElements()
    {
        List<SaveableComponent> result =
            inspectorTarget.gameObject.GetComponents<SaveableComponent>().ToList();

        return result.Select(component => component.GetType()).ToList();
    }
    
}
