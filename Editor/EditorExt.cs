using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorExt
{
	private const int INDENTATION_WIDTH = 16;

	private static List<int> indentations = new List<int>();

	public static bool DrawDefaultInspectorWithoutScriptField(this Editor inspector)
	{
		EditorGUI.BeginChangeCheck();

		inspector.serializedObject.Update();
		
		SerializedProperty iterator = inspector.serializedObject.GetIterator();
		iterator.NextVisible(true);

		while (iterator.NextVisible(false))
			EditorGUILayout.PropertyField(iterator, true);

		inspector.serializedObject.ApplyModifiedProperties();

		return EditorGUI.EndChangeCheck();
	}

    public static void BeginBoxGroup() => BeginBoxGroup(EditorStyles.helpBox);
    public static void BeginBoxGroup(GUIStyle style)
	{
		var indentLevel = EditorGUI.indentLevel;

		GUILayout.BeginHorizontal();
		GUILayout.Space(indentLevel * INDENTATION_WIDTH);
		GUILayout.BeginVertical(style);

		indentations.Add(indentLevel);
		EditorGUI.indentLevel = 0;
	}

	public static void EndBoxGroup()
	{
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		var lastIndex = indentations.Count - 1;
		EditorGUI.indentLevel = indentations[lastIndex];
		indentations.RemoveAt(lastIndex);
	}

	public static bool FoldoutHeader(string label, bool foldout)
	{
		if (foldout)
		{
			foldout = GUILayout.Toggle(foldout, label, EditorStyles.foldoutHeader);
		}
		else
		{
			EditorGUI.BeginDisabledGroup(false);
				foldout = GUILayout.Toggle(foldout, label, EditorStyles.foldoutHeader);
			EditorGUI.EndDisabledGroup();
		}

		return foldout;
	}

	public static T FoldoutObject<T>(string label, ref bool foldout, Object obj, Editor objEditor, int indentAmount = 1) where T : Object
	{
		BeginBoxGroup();

		EditorGUILayout.BeginHorizontal();
			foldout = FoldoutHeader(label, foldout);
			obj = (T)EditorGUILayout.ObjectField(obj, typeof(T), allowSceneObjects: false);
		EditorGUILayout.EndHorizontal();

		if (foldout)
		{
			if (obj == null)
			{
				EditorGUILayout.HelpBox("Null Object.", MessageType.Warning);
			}
			else
			{
				if (objEditor == null)
				{
					EditorGUILayout.HelpBox("Editor Not Initialized!", MessageType.Error);
				}
				else
				{
					EditorGUI.indentLevel += indentAmount;
						objEditor.OnInspectorGUI();
					EditorGUI.indentLevel -= indentAmount;
				}
			}
		}
		EndBoxGroup();

		return (T)obj;
	}
}
