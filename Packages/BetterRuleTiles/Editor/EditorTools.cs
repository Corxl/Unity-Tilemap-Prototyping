using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class EditorTools<T> where T: UnityEngine.Object
{

    public static T[] ArrayField(string label, ref bool foldOut, T[] array, Type type) => ArrayField(new GUIContent(label), ref foldOut, array, type);
    public static T[] ArrayField(GUIContent label, ref bool foldOut, T[] array, Type type)
    {
        //display a foldout
        foldOut = EditorGUILayout.Foldout(foldOut, label);

        //if the foldout is closed just ignore the method
        if (!foldOut) return array;

        //display and change size
        int newSize = EditorGUILayout.DelayedIntField(new GUIContent("    Size", "Size of the array. Press enter to apply changes."), array.Length);
        if (newSize != array.Length) Array.Resize(ref array, newSize);

        //set elements
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = (T)EditorGUILayout.ObjectField(new GUIContent($"    Element {i}"), array[i], type, false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }

        return array;
    }
}
