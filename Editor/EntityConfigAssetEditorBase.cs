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
	private readonly Color COMPONENT_EVEN_ID_COLOR = new Color(0.8f, 0.8f, 0.8f, 1);

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
							if (i % 2 == 0)
								GUI.backgroundColor = COMPONENT_EVEN_ID_COLOR;

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
													entityConfigAsset.components[i].foldedOut = EditorGUILayout.Toggle(entityConfigAsset.components[i].foldedOut, EditorStyles.foldout, GUILayout.Width(FOLDOUT_WIDTH), GUILayout.ExpandHeight(true));
												EditorGUI.EndDisabledGroup();
											}
											else
											{
												entityConfigAsset.components[i].foldedOut = EditorGUILayout.Toggle(entityConfigAsset.components[i].foldedOut, EditorStyles.foldout, GUILayout.Width(FOLDOUT_WIDTH), GUILayout.ExpandHeight(true));
											}
										}
									}

									if (!canBeFoldedOut)
										GUILayout.Space(FOLDOUT_WIDTH + 3);

									if (!nullComponent)
									{
										GUILayout.Label(entityConfigAsset.components[i].name, EditorStyles.boldLabel, GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

										// if (GUILayout.Button(entityConfigAsset.components[i].name, EditorStyles.boldLabel, GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true)))
										// 	entityConfigAsset.components[i].foldedOut = !entityConfigAsset.components[i].foldedOut;

										Rect buttonRect = GUILayoutUtility.GetLastRect();
										Event current = Event.current;

										if (buttonRect.Contains(current.mousePosition))
										{
											if (current.type == EventType.MouseDown && current.button == 0)
											{
												if (canBeFoldedOut)
													entityConfigAsset.components[i].foldedOut = !entityConfigAsset.components[i].foldedOut;
											}
											else if (current.type == EventType.ContextClick)
											{
												GenericMenu menu = new GenericMenu();

												if (i == 0)
													menu.AddDisabledItem(new GUIContent("Move Up"), false);
												else
													menu.AddItem(new GUIContent("Move Up"), false, OnMenuMoveComponentUp, i);

												if (i == editorCount - 1)
													menu.AddDisabledItem(new GUIContent("Move Down"), false);
												else
													menu.AddItem(new GUIContent("Move Down"), false, OnMenuMoveComponentDown, i);

												// menu.AddSeparator("");
												// menu.AddItem(new GUIContent("Select Asset"), false, OnMenuSelectComponentAsset, i);
												menu.AddSeparator("");
												menu.AddItem(new GUIContent("Remove Component"), false, OnMenuRemoveComponent, i);
												menu.ShowAsContext();

												current.Use(); 
											}
										}
									}

									entityConfigAsset.components[i] = (T1)EditorGUILayout.ObjectField(entityConfigAsset.components[i], typeof(T1), false, GUILayout.ExpandWidth(true), GUILayout.Height(oneLineHeight));

									if (GUILayout.Button("-", GUILayout.Width(oneLineHeight), GUILayout.ExpandHeight(true)))
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

								if (check.changed)
									ApplyChanges();
							}
							EditorExt.EndBoxGroup();

							GUI.backgroundColor = Color.white;
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

	private void OnMenuMoveComponentDown(object userData)
	{
		var index = (int)userData;
		if (index == entityConfigAsset.components.Count - 1)
			return;

		MoveListItem(ref entityConfigAsset.components, index, index + 1);
		RegenerateEditors();
	}

	private void OnMenuMoveComponentUp(object userData)
	{
		var index = (int)userData;
		if (index == 0)
			return;

		MoveListItem(ref entityConfigAsset.components, index, index - 1);
		RegenerateEditors();
	}

	private void OnMenuRemoveComponent(object userData)
	{
		var index = (int)userData;
		entityConfigAsset.components.RemoveAt(index);
		RegenerateEditors();
	}
	
	private void OnMenuSelectComponentAsset(object userData)
	{
		var index = (int)userData;
		Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GetAssetPath(entityConfigAsset.components[index]));
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

	public void MoveListItem<T>(ref List<T> list, int oldIndex, int newIndex)
	{
		T item = list[oldIndex];
		list.RemoveAt(oldIndex);
		list.Insert(newIndex, item);
	}
}