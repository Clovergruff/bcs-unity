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
	private readonly Color HEADER_EVEN_ID_COLOR = new Color(0, 0, 0, 0.1f);
	private readonly Color HEADER_HOVER_COLOR = new Color(1, 1, 1, 0.05f);
	private readonly Color HEADER_SELECTED_COLOR = new Color(0, 0.4f, 1, 0.25f);

	protected T2 entityConfigAsset;
	protected GUIStyle iconButtonStyle = new GUIStyle();
	protected GUIStyle componentListStyle = new GUIStyle();

	public EntityConfigEditorInstance editorInstance;

	private static int _componentEditMode = 0;
	private string[] _componentEditModeStrings = new string[] {"Edit mode", "Reorder mode"};
	private SerializedProperty _componentsProperty;
	private float _headerHeight = 20;
	private Rect[] _headerRects;
	private int _hoveringComponentId = -1;
	private int _selectedComponentId = -1;

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
		bool wasHoveringOnComponent = false;
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
							// EditorExt.BeginBoxGroup();
							EditorGUI.indentLevel++;
							Editor editor = editorInstance.editors[i];

							using (var check = new EditorGUI.ChangeCheckScope())
							{
								// Header
								Rect headerRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(_headerHeight));
								Rect headerSelectRect = headerRect;
								headerSelectRect.x -= 3;
								// headerSelectRect.y -= 2;
								headerSelectRect.width += 6;
								// headerSelectRect.height += 6;

								if (Event.current.type == EventType.Repaint)
        							_headerRects[i] = headerRect;

								if (_selectedComponentId == i)
									EditorGUI.DrawRect(headerSelectRect, HEADER_SELECTED_COLOR);
								else if (_hoveringComponentId == i)
									EditorGUI.DrawRect(headerSelectRect, HEADER_HOVER_COLOR);
								else if (i % 2 == 0)
									EditorGUI.DrawRect(headerSelectRect, HEADER_EVEN_ID_COLOR);

								GUILayout.BeginArea(_headerRects[i]);
								GUILayout.BeginHorizontal();

									int oldIndentLevel = EditorGUI.indentLevel;
									bool canBeFoldedOut = false;
									bool nullComponent = entityConfigAsset.components[i] == null;
									EditorGUI.indentLevel = 0;

									if (!nullComponent)
									{
										EditorGUI.BeginDisabledGroup(true);
										var iterator = editor.serializedObject.GetIterator();
										if (entityConfigAsset.components[i].alwaysEnableFoldout || iterator.CountRemaining() > 1)
										{
											canBeFoldedOut = true;
											EditorGUILayout.Toggle(entityConfigAsset.components[i].foldedOut, EditorStyles.foldout, GUILayout.Width(FOLDOUT_WIDTH), GUILayout.ExpandHeight(true));
										}
										EditorGUI.EndDisabledGroup();
									}

									float leftPadding = FOLDOUT_WIDTH + 3;

									if (!canBeFoldedOut)
										GUILayout.Space(leftPadding);

									if (!nullComponent)
									{
										GUILayout.Label(entityConfigAsset.components[i].name, EditorStyles.boldLabel, GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

										// if (GUILayout.Button(entityConfigAsset.components[i].name, EditorStyles.boldLabel, GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true)))
										// 	entityConfigAsset.components[i].foldedOut = !entityConfigAsset.components[i].foldedOut;

										Rect buttonRect = GUILayoutUtility.GetLastRect();
										buttonRect.x -= leftPadding;
										buttonRect.width += leftPadding;
										Event current = Event.current;
										var eventType = current.type;

										if (buttonRect.Contains(current.mousePosition))
										{
											wasHoveringOnComponent = true;
											_hoveringComponentId = i;
											
											if (eventType == EventType.MouseDown && current.button == 0)
											{
												if (canBeFoldedOut)
													entityConfigAsset.components[i].foldedOut = !entityConfigAsset.components[i].foldedOut;

												_selectedComponentId = i;
											}
											else if (eventType == EventType.ContextClick)
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

									entityConfigAsset.components[i] = (T1)EditorGUILayout.ObjectField(entityConfigAsset.components[i], typeof(T1), false, GUILayout.ExpandWidth(true), GUILayout.Height(_headerHeight));

									if (GUILayout.Button("-", GUILayout.Width(oneLineHeight), GUILayout.ExpandHeight(true)))
									{
										entityConfigAsset.components.RemoveAt(i);
										RegenerateEditors();
										return;
									}
									EditorGUI.indentLevel = oldIndentLevel;
									
									GUI.color = Color.white;
								GUILayout.EndHorizontal();
								GUILayout.EndArea();
								
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

							EditorGUI.indentLevel--;
							// EditorExt.EndBoxGroup();
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

				if (_hoveringComponentId == -1 && Event.current.type == EventType.MouseDown)
				{
					_selectedComponentId = -1;
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

		if (!wasHoveringOnComponent && Event.current.type == EventType.Repaint)
			_hoveringComponentId = -1;

		Repaint();
	}

	private void OnMenuMoveComponentDown(object userData)
	{
		var index = (int)userData;
		if (index == entityConfigAsset.components.Count - 1)
			return;

		MoveListItem(ref entityConfigAsset.components, index, index + 1);
		_selectedComponentId = index + 1;

		RegenerateEditors();
	}

	private void OnMenuMoveComponentUp(object userData)
	{
		var index = (int)userData;
		if (index == 0)
			return;

		MoveListItem(ref entityConfigAsset.components, index, index - 1);
		_selectedComponentId = index - 1;

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
		_headerRects = new Rect[editorCount];

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