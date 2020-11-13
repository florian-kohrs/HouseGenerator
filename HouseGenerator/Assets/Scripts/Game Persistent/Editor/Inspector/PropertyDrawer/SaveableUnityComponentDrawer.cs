using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

//[CustomPropertyDrawer(typeof(SaveableComponent),true)]
public class SaveableUnityComponentDrawer : PropertyDrawer
{
    public SaveableComponent component;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var targetObject = property.serializedObject.targetObject;
        var targetObjectClassType = targetObject.GetType();
        var field = targetObjectClassType.GetField(property.propertyPath);
        if (field != null)
        {
            object value = field.GetValue(targetObject);

            string fieldName = firstToUpper(field.Name);

            component = (SaveableComponent)value;
            Object choosenObject = EditorGUI.ObjectField(position, fieldName, component.BaseComponent, component.getGenericType(), true);
            if (component.BaseComponent != choosenObject)
            {
                Undo.RegisterCompleteObjectUndo(targetObject, "changed " + fieldName);

                Component choosenComponent = (choosenObject as Component);

                component.BaseComponent = choosenComponent;

                ///Unity documentation disadvises to use this function in this situation,
                ///but no other solution worked for saving the new list element in the scene,
                ///without losing the reference after entering and leaving play mode
                EditorUtility.SetDirty(targetObject);
            }
        }
    }

    private string firstToUpper(string text)
    {
        string result = "";

        if (text != null) {

            result = text[0] + "";
            result = result.ToUpper();

            for(int i = 1; i < text.Length; i++)
            {
                result += text[i];
            }
        }

        return result;
    }
    
}
