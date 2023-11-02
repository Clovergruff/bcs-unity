using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Gruffdev.BCSEditor
{
	public static class EditorExt
	{
		public static int INDENTATION_WIDTH = 16;
		private static GUIStyle OBJECT_FIELD_PREVIEW_STYLE = new GUIStyle(GUI.skin.button)
			{
				padding = new RectOffset(2, 2, 2, 2)
			};

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

		public static AnimBool FoldoutHeader(string label, AnimBool foldout)
		{
			if (foldout.target)
			{
				foldout.target = GUILayout.Toggle(foldout.target, label, EditorStyles.foldoutHeader);
			}
			else
			{
				EditorGUI.BeginDisabledGroup(false);
					foldout.target = GUILayout.Toggle(foldout.target, label, EditorStyles.foldoutHeader);
				EditorGUI.EndDisabledGroup();
			}

			return foldout;
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

		public static T FoldoutObject<T>(string label, ref AnimBool foldout, Object obj, Editor objEditor, int indentAmount = 1) where T : Object
		{
			EditorGUILayout.BeginHorizontal();
				GUILayout.Space(EditorGUI.indentLevel * INDENTATION_WIDTH - INDENTATION_WIDTH);
				foldout = FoldoutHeader(label, foldout);
				obj = (T)EditorGUILayout.ObjectField(obj, typeof(T), allowSceneObjects: false);
			EditorGUILayout.EndHorizontal();

			if (foldout)
				DrawFoldoutObjectContents(obj, objEditor, indentAmount);
			EditorGUILayout.EndFadeGroup();

			return (T)obj;
		}

		public static T FoldoutObject<T>(string label, ref bool foldout, Object obj, Editor objEditor, int indentAmount = 1) where T : Object
		{
			EditorGUILayout.BeginHorizontal();
				GUILayout.Space(EditorGUI.indentLevel * INDENTATION_WIDTH - INDENTATION_WIDTH);
				foldout = FoldoutHeader(label, foldout);
				obj = (T)EditorGUILayout.ObjectField(obj, typeof(T), allowSceneObjects: false);
			EditorGUILayout.EndHorizontal();

			if (foldout)
				DrawFoldoutObjectContents(obj, objEditor, indentAmount);
			EditorGUILayout.EndFadeGroup();

			return (T)obj;
		}

		private static void DrawFoldoutObjectContents(Object obj, Editor objEditor, int indentAmount)
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
					BeginBoxGroup();
					EditorGUI.indentLevel += indentAmount;
					objEditor.OnInspectorGUI();
					EditorGUI.indentLevel -= indentAmount;
					EndBoxGroup();
				}
			}
		}

		public static T ObjectFieldWithPreview<T>(T obj, float previewSize) where T : UnityEngine.Object
		{
			if (obj != null)
			{
				GameObject go = (GameObject)obj.GetType().GetProperty("gameObject").GetValue(obj, null);
				return ObjectFieldWithPreview<T>(obj, AssetPreview.GetAssetPreview(go), previewSize); 
			}
			return ObjectFieldWithPreview<T>(obj, null, previewSize); 
		}

		public static T ObjectFieldWithPreview<T>(T obj, Texture2D previewTexture, float previewSize) where T : UnityEngine.Object
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(EditorGUI.indentLevel * INDENTATION_WIDTH);

			int oldIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			if (previewTexture != null)
			{
				if (GUILayout.Button(previewTexture, OBJECT_FIELD_PREVIEW_STYLE, GUILayout.Width(previewSize), GUILayout.Height(previewSize)))
				{
					EditorGUIUtility.PingObject(obj);
				}
			}

			obj = (T)EditorGUILayout.ObjectField(obj, typeof(T), false, GUILayout.Height(previewSize));

			EditorGUI.indentLevel = oldIndentLevel;

			GUILayout.EndHorizontal();

			return obj;
		}

		public static void LabelFieldColor(Color color, GUIContent label, params GUILayoutOption[] options)
		{
			var oldColor = GUI.color;
			GUI.color = color;
			EditorGUILayout.LabelField(label, options);
			GUI.color = oldColor;
		}

		public static void DrawUnityEditorIcon(string iconName, params GUILayoutOption[] options)
		{
			GUILayout.Label(EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? $"d_{iconName}" : iconName), options);
		}
	}
}