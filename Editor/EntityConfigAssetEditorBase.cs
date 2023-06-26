using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public abstract class EntityConfigAssetEditorBase<T1, T2> : Editor
	where T1 : ConfigScriptableObject
	where T2 : EntityConfigAsset<T1>
{
	private const int FOLDOUT_WIDTH = 13;

	protected T2 entityConfigAsset;
	protected GUIStyle iconButtonStyle = new GUIStyle();
	protected GUIStyle componentListStyle = new GUIStyle();

	public EntityConfigEditorInstance editorInstance;

	private static int _componentEditMode = 0;
	private string[] _componentEditModeStrings = new string[] {"Edit mode", "Reorder mode"};
	private SerializedProperty _componentsProperty;

	protected virtual void OnEnable()
	{
		entityConfigAsset = (T2)target;

		iconButtonStyle.normal.background = null;
		iconButtonStyle.active.background = null;
		iconButtonStyle.hover.background = null;

		_componentsProperty = serializedObject.FindProperty("components");
		RegenerateEditors();
	}

	protected void DrawComponentList()
	{
		var previousComponentEditMode = _componentEditMode;
		_componentEditMode = GUILayout.Toolbar (_componentEditMode, _componentEditModeStrings);

		if (previousComponentEditMode != _componentEditMode)
		{
			ApplyChanges();
			RegenerateEditors();
		}

		Repaint();

		switch (_componentEditMode)
		{
			case 0:
				float oneLineHeight = EditorGUIUtility.singleLineHeight;
				int editorCount = editorInstance.editors.Length;

				entityConfigAsset.foldedOut = EditorExt.FoldoutHeader("Components", entityConfigAsset.foldedOut);

				if (entityConfigAsset.foldedOut)
				{
					EditorExt.BeginBoxGroup();
						for (int i = 0; i < editorCount; i++)
						{
							EditorExt.BeginBoxGroup();
							Editor editor = editorInstance.editors[i];

							using (var check = new EditorGUI.ChangeCheckScope())
							{
								// Header
								GUILayout.BeginHorizontal();

									int oldIndentLevel = EditorGUI.indentLevel;
									bool canBeFoldedOut = false;
									bool nullComponent = entityConfigAsset.components[i] == null;
									EditorGUI.indentLevel = 0;

									if (!nullComponent)
									{
										var iterator = editor.serializedObject.GetIterator();
										if (entityConfigAsset.components[i].alwaysEnableFoldout || iterator.CountRemaining() > 1)
										{
											canBeFoldedOut = true;

											if (!entityConfigAsset.components[i].foldedOut)
											{
												EditorGUI.BeginDisabledGroup(false);
													entityConfigAsset.components[i].foldedOut = EditorGUILayout.Toggle(entityConfigAsset.components[i].foldedOut, EditorStyles.foldout, GUILayout.Width(FOLDOUT_WIDTH));
												EditorGUI.EndDisabledGroup();
											}
											else
											{
												entityConfigAsset.components[i].foldedOut = EditorGUILayout.Toggle(entityConfigAsset.components[i].foldedOut, EditorStyles.foldout, GUILayout.Width(FOLDOUT_WIDTH));
											}
										}
									}

									if (!canBeFoldedOut)
										GUILayout.Space(FOLDOUT_WIDTH + 3);

									if (!nullComponent)
									{
										if (GUILayout.Button(entityConfigAsset.components[i].name, EditorStyles.boldLabel, GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true)))
											entityConfigAsset.components[i].foldedOut = !entityConfigAsset.components[i].foldedOut;
									}

									entityConfigAsset.components[i] = (T1)EditorGUILayout.ObjectField(entityConfigAsset.components[i], typeof(T1), false, GUILayout.ExpandWidth(true));

									if (GUILayout.Button("-", GUILayout.Width(oneLineHeight), GUILayout.Height(oneLineHeight)))
									{
										entityConfigAsset.components.RemoveAt(i);
										RegenerateEditors();
										return;
									}
									EditorGUI.indentLevel = oldIndentLevel;
									
									GUI.color = Color.white;
								GUILayout.EndHorizontal();
								
								GUILayout.Space(2);

								if (check.changed)
								{
									RegenerateEditors();
									return;
								}

								// Component Editor
								if (entityConfigAsset.components[i] != null && entityConfigAsset.components[i].foldedOut)
								{
									// EditorExt.BeginBoxGroup();
									EditorGUI.indentLevel++;
										editor.OnInspectorGUI();
									EditorGUI.indentLevel--;
									// EditorExt.EndBoxGroup();
								}

								if (i != editorCount -1)
									GUILayout.Space(5);

								if (check.changed)
									ApplyChanges();
							}
							EditorExt.EndBoxGroup();
						}

						GUILayout.Space(3);

						// Add Component area
						GUILayout.BeginHorizontal();
							GUILayout.FlexibleSpace();
								if (GUILayout.Button("Add Component", GUILayout.Width(200), GUILayout.Height(oneLineHeight + 6)))
								{
									entityConfigAsset.components.Add(null);
									ApplyChanges();
									RegenerateEditors();
								}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();

						GUILayout.Space(3);

					EditorExt.EndBoxGroup();
				}

				// Drag items from the assets window
				if (Event.current.type == EventType.DragUpdated)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Link;
					Event.current.Use();
				}
				else if (Event.current.type == EventType.DragPerform)
				{
					bool componentsUpdated = false;
					DragAndDrop.AcceptDrag();

					if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
					{
						for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
						{
							object obj = DragAndDrop.objectReferences[i];
							string path = DragAndDrop.paths[i];

							if (obj is T1 configAsset)
							{
								entityConfigAsset.components.Add(configAsset);
								ApplyChanges();
								componentsUpdated = true;
							}
						}
					}

					if (componentsUpdated)
						RegenerateEditors();
				}
			break;
			case 1:
				EditorGUILayout.PropertyField(_componentsProperty);
			break;
		}

		GUI.color = Color.white;
	}

	private void ApplyChanges()
	{
		EditorUtility.SetDirty(entityConfigAsset);
		serializedObject.ApplyModifiedProperties();
		_componentsProperty.serializedObject.Update();
	}


	private void RegenerateEditors()
	{
		editorInstance = (EntityConfigEditorInstance)ScriptableObject.CreateInstance(typeof(EntityConfigEditorInstance));

		int editorCount = entityConfigAsset.components.Count;
		editorInstance.editors = new Editor[editorCount];

		for (int i = 0; i < editorCount; i++)
		{
			if (entityConfigAsset.components[i] == null)
				continue;

			editorInstance.editors[i] = Editor.CreateEditor(entityConfigAsset.components[i]);
		}
	}
}