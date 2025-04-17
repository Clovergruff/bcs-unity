using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using UnityEditor.Search;
using Gruffdev.BCS;
using System.Text;

namespace Gruffdev.BCSEditor
{
	public abstract class ActorConfigAssetEditorBase<T1, T2> : Editor
		where T1 : ConfigScriptableObject
		where T2 : ActorConfigAsset<T1>
	{
		private readonly Color HEADER_EVEN_ID_COLOR = new Color(0, 0, 0, 0.1f);
		private readonly Color HEADER_ODD_ID_COLOR = new Color(0, 0, 0, 0.05f);
		private readonly Color HEADER_HOVER_COLOR = new Color(1, 1, 1, 0.05f);
		private readonly Color HEADER_SELECTED_COLOR = new Color(0, 0.4f, 1, 0.25f);
		private readonly Color DARK_ICON_COLOR = new Color(0.9f, 0.9f, 0.9f, 1);
		private readonly Color STATE_COMPONENT_DOT_COLOR = new Color(0.5f, 0.5f, 0.5f, 0.85f);
		private readonly Color LIGHT_MODE_DARK_ICON_COLOR = new Color(0.25f, 0.25f, 0.25f, 1);
		private readonly Color DARK_MODE_DARK_ICON_COLOR = new Color(0.75f, 0.75f, 0.75f, 1);
		
		private readonly string[] COMPONENT_EDIT_MODE_STRINGS = new string[] {"Edit mode", "Reorder mode"};
		private readonly float HEADER_HEIGHT = 18;
		private readonly int FOLDOUT_WIDTH = 13;

		protected T2 actorConfigAsset;
		protected GUIStyle iconButtonStyle = new GUIStyle();
		protected GUIStyle componentListStyle = new GUIStyle();

		public ActorConfigEditorInstance EditorInstance { get; private set; }

		private static int _componentEditMode = 0;
		private SerializedProperty _componentsProperty;
		private Rect[] _headerRects;
		private Rect[] _componentRects;
		private int _hoveringComponentId = -1;
		private bool[] _selectedComponents;

		private Vector2 _oldMousePosition;

		protected virtual void OnEnable()
		{
			actorConfigAsset = (T2)target;

			iconButtonStyle.normal.background = null;
			iconButtonStyle.active.background = null;
			iconButtonStyle.hover.background = null;

			_componentsProperty = serializedObject.FindProperty("components");
			RegenerateEditors();
		}

		private void OnGUI()
		{
			Event e = Event.current;
			if (_hoveringComponentId == -1 && e.type == EventType.MouseDown)
			{
				for (int i = 0; i < _selectedComponents.Length; i++)
					_selectedComponents[i] = false;
			}
		}

		public void DrawComponentList()
		{
			bool wasHoveringOnComponent = false;
			var previousComponentEditMode = _componentEditMode;
			_componentEditMode = GUILayout.Toolbar (_componentEditMode, COMPONENT_EDIT_MODE_STRINGS);

			if (previousComponentEditMode != _componentEditMode)
			{
				ApplyChanges();
				RegenerateEditors();
			}

			switch (_componentEditMode)
			{
				case 0:
					float oneLineHeight = EditorGUIUtility.singleLineHeight;
					int editorCount = EditorInstance.editors.Length;

					actorConfigAsset.foldedOut = EditorExt.FoldoutHeader("Components", actorConfigAsset.foldedOut);

					if (actorConfigAsset.foldedOut)
					{
						EditorExt.BeginBoxGroup();
						for (int i = 0; i < editorCount; i++)
						{
							// EditorExt.BeginBoxGroup();
							EditorGUI.indentLevel++;
							Editor editor = EditorInstance.editors[i];

							using (var check = new EditorGUI.ChangeCheckScope())
							{
								// Header
								Rect headerRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(HEADER_HEIGHT));
								Rect headerSelectRect = headerRect;
								headerSelectRect.x -= 3;
								headerSelectRect.y -= 1;
								headerSelectRect.width += 6;
								headerSelectRect.height += 2;

								if (Event.current.type == EventType.Repaint)
								{
									_componentRects[i].x -= 3;
									// _componentRects[i].y -= 1;
									_componentRects[i].width += 6;
									// _componentRects[i].height += 2;
								}

								// EditorGUI.DrawRect(_componentRects[i], new Color(1, 0, 0, 0.5f));

								if (_selectedComponents[i])
									EditorGUI.DrawRect(_componentRects[i], HEADER_SELECTED_COLOR);
								else if (_hoveringComponentId == i)
									EditorGUI.DrawRect(headerSelectRect, HEADER_HOVER_COLOR);
								else if (i % 2 == 0)
									EditorGUI.DrawRect(headerSelectRect, HEADER_EVEN_ID_COLOR);
								else
									EditorGUI.DrawRect(headerSelectRect, HEADER_ODD_ID_COLOR);
									
								if (Event.current.type == EventType.Repaint)
								{
									_headerRects[i] = headerRect;
									_componentRects[i] = headerRect;	
								}

								GUILayout.BeginArea(_headerRects[i]);
								GUILayout.BeginHorizontal();

								int oldIndentLevel = EditorGUI.indentLevel;
								bool canBeFoldedOut = false;
								bool nullComponent = actorConfigAsset.components[i] == null;
								float leftPadding = FOLDOUT_WIDTH + 3;
								EditorGUI.indentLevel = 0;

								if (!nullComponent)
								{
									EditorGUI.BeginDisabledGroup(true);
									var iterator = editor.serializedObject.GetIterator();
									if (actorConfigAsset.components[i].EditorAlwaysEnableFoldout || iterator.CountRemaining() > 1)
									{
										canBeFoldedOut = true;
										EditorGUILayout.Toggle(actorConfigAsset.components[i].EditorFoldedOut, EditorStyles.foldout, GUILayout.Width(FOLDOUT_WIDTH), GUILayout.Height(HEADER_HEIGHT));
									}
									else
									{
										EditorExt.LabelFieldColor(STATE_COMPONENT_DOT_COLOR, EditorGUIUtility.IconContent("DotFill"), GUILayout.Width(FOLDOUT_WIDTH), GUILayout.Height(HEADER_HEIGHT));
									}
									EditorGUI.EndDisabledGroup();
								}
								else
								{
									GUILayout.Space(leftPadding);
								}

								if (nullComponent)
								{
									DrawMonochromeEditorIcon("console.warnicon.inactive.sml@2x", GUILayout.Width(18), GUILayout.ExpandHeight(true));
								}
								else
								{
									DrawMonochromeEditorIcon("ScriptableObject On Icon", GUILayout.Width(18), GUILayout.Height(HEADER_HEIGHT));
									// EditorExt.DrawUnityEditorIcon("console.warnicon.sml", GUILayout.Width(18), GUILayout.Height(_headerHeight));
									GUILayout.Label(actorConfigAsset.components[i].name, EditorStyles.boldLabel, GUILayout.MaxWidth(250), GUILayout.Height(HEADER_HEIGHT));
								}

								var buttonRect = GUILayoutUtility.GetLastRect();
								var xMax = buttonRect.xMax;
								buttonRect.x = 0;
								buttonRect.xMax = xMax;
								buttonRect.y -= 1;
								buttonRect.height += 2;

								Event current = Event.current;
								var eventType = current.type;

								if (buttonRect.Contains(current.mousePosition))
								{
									wasHoveringOnComponent = true;
									_hoveringComponentId = i;

									if (eventType == EventType.MouseDown && current.button == 0)
									{
										if (current.control)
										{
											_selectedComponents[i] = !_selectedComponents[i];
										}
										else
										{
											if (canBeFoldedOut && !nullComponent)
											{
												if (current.clickCount > 1 || current.mousePosition.x < buttonRect.x + leftPadding)
													ToggleComponentFoldout(i);
												else
													HighlightSpecificComponent(i);
											}
											else
												HighlightSpecificComponent(i);
										}
									}
									else if (eventType == EventType.ContextClick)
									{
										_selectedComponents[i] = true;

										GenericMenu menu = new GenericMenu();
										int selectedComponentCount = 0;
										foreach (var sel in _selectedComponents)
											if (sel) selectedComponentCount++;

										if (i == 0 || selectedComponentCount > 1)
											menu.AddDisabledItem(new GUIContent("Move Up"), false);
										else
											menu.AddItem(new GUIContent("Move Up"), false, OnMenuMoveComponentUp, i);

										if (i == editorCount - 1 || selectedComponentCount > 1)
											menu.AddDisabledItem(new GUIContent("Move Down"), false);
										else
											menu.AddItem(new GUIContent("Move Down"), false, OnMenuMoveComponentDown, i);

										menu.AddSeparator("");
										menu.AddItem(
											new GUIContent(selectedComponentCount > 1 ? "Remove Components" : "Remove Component"),
											false,
											OnMenuRemoveSelectedComponents
										);

										bool hasNestedComponents = false;
										bool hasAssetComponents = false;

										for (int si = 0; si < _selectedComponents.Length; si++)
										{
											if (!_selectedComponents[si])
												continue;

											bool isNested = AssetDatabase.IsSubAsset(actorConfigAsset.components[si].GetInstanceID());

											if (isNested)
												hasNestedComponents = true;
											else
												hasAssetComponents = true;
										}

										if (hasNestedComponents)
										{
											menu.AddItem(
												new GUIContent(selectedComponentCount > 1 ? "Extract Components" : "Extract Component"),
												false,
												OnMenuExtractSelectedComponents
											);
										}

										if (hasAssetComponents)
										{
											menu.AddItem(
												new GUIContent(selectedComponentCount > 1 ? "Embed Components" : "Embed Component"),
												false,
												OnMenuNestSelectedComponents
											);
										}

										menu.ShowAsContext();

										current.Use(); 
									}
								}

								T1 comp = actorConfigAsset.components[i];
								if (comp != null && AssetDatabase.IsSubAsset(comp.GetInstanceID()))
								{
									EditorGUILayout.Space();
								}
								else
								{
									actorConfigAsset.components[i] = (T1)EditorGUILayout.ObjectField(actorConfigAsset.components[i], typeof(T1), false, GUILayout.ExpandWidth(true), GUILayout.Height(HEADER_HEIGHT));
								}

								// if (GUILayout.Button("-", GUILayout.Width(oneLineHeight), GUILayout.ExpandHeight(true)))
								GUILayout.Label("", GUILayout.MaxWidth(10), GUILayout.Height(HEADER_HEIGHT));
								Rect closeButtonRect = GUILayoutUtility.GetLastRect();
#if UNITY_6000_0_OR_NEWER
								var closeImage = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_close" : "close").image;
#else
								var closeImage = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_winbtn_win_close" : "winbtn_win_close").image;
#endif
								closeButtonRect.x -= 4;
								closeButtonRect.y = HEADER_HEIGHT * 0.5f - closeImage.height * 0.5f;
								closeButtonRect.width = closeImage.width;
								closeButtonRect.height = closeImage.height;

								GUI.DrawTexture(closeButtonRect, closeImage);
								// GUI.Label(closeButtonRect, EditorGUIUtility.IconContent("d_winbtn_win_close_a@2x"));
								// if (GUILayout.Button("EditorGUIUtility.IconContent("d_winbtn_win_close_a@2x")", GUIStyle.none, GUILayout.Width(oneLineHeight), GUILayout.ExpandHeight(true)))
								if (GUI.Button(closeButtonRect, "", GUIStyle.none))
								{
									RemoveComponentAt(i);
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
								if (actorConfigAsset.components[i] != null && actorConfigAsset.components[i].EditorFoldedOut)
								{
									EditorGUI.indentLevel++;
										editor.OnInspectorGUI();
									EditorGUI.indentLevel--;
								}

								if (check.changed)
									ApplyChanges();
							}

							if (Event.current.type == EventType.Repaint)
								_componentRects[i].yMax = GUILayoutUtility.GetLastRect().yMax;

							EditorGUI.indentLevel--;
						}

						GUILayout.Space(3);

						// Add Component area
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("Add Component", GUILayout.Width(200), GUILayout.Height(oneLineHeight + 6)))
						{
							string[] paths = GetAllScriptableObjectPaths();

							GenericMenu menu = new GenericMenu();

							menu.AddItem(new GUIContent("Empty"), false, OnMenuAddEmptyComponentField);
							
							foreach (var path in paths)
							{
								string filename = Path.GetFileNameWithoutExtension(path);
								List<string> words = SearchUtils.SplitCamelCase(filename).ToList();
								
								if (words.Count > 1)
								{
									var category = words[0];
									words.RemoveAt(0);
									filename = $"{category}/{String.Join("", words)}";
								}

								menu.AddItem(new GUIContent($"Project/{filename}"), false, OnMenuAddExistingComponent, path);
							}

							menu.AddSeparator("");

							Type[] derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
								.SelectMany(domainAssembly => domainAssembly.GetTypes())
								.Where(type => typeof(T1).IsAssignableFrom(type)
								).ToArray();

							foreach (Type comp in derivedTypes)
							{
								bool componentAlreadyExists = false;
								foreach (T1 existingComponent in actorConfigAsset.components)
								{
									if (existingComponent.GetType() == comp)
									{
										componentAlreadyExists = true;
										break;
									}
								}

								string compName = GetShortComponentName(comp);

								if (compName == "Component")
									continue;

								if (componentAlreadyExists)
									menu.AddDisabledItem(new GUIContent(compName));
								else
									menu.AddItem(new GUIContent(compName), false, OnMenuCreateNestedComponent, comp);
							}

							menu.ShowAsContext();
							Event.current.Use(); 
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();

						GUILayout.Space(3);

						EditorExt.EndBoxGroup();
					}

					if (_hoveringComponentId == -1 && Event.current.type == EventType.MouseDown)
					{
						for (int i = 0; i < _selectedComponents.Length; i++)
							_selectedComponents[i] = false;
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
									actorConfigAsset.components.Add(configAsset);
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
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(_componentsProperty);
					EditorGUI.indentLevel--;
				break;
			}

			GUI.color = Color.white;

			if (!wasHoveringOnComponent && Event.current.type == EventType.Repaint)
				_hoveringComponentId = -1;

			if (Event.current.type == EventType.MouseDrag ||
				Event.current.type == EventType.MouseMove ||
				Event.current.type == EventType.MouseDown ||
				Event.current.type == EventType.MouseUp ||
				Event.current.type == EventType.MouseEnterWindow ||
				Event.current.type == EventType.MouseLeaveWindow ||
				Event.current.type == EventType.Repaint ||
				Event.current.mousePosition != _oldMousePosition)
			{
				Repaint();
			}

			_oldMousePosition = Event.current.mousePosition;
		}

		private void RemoveComponentAt(int index) => RemoveComponent(actorConfigAsset.components[index]);

		private void RemoveComponent(T1 component)
		{
			actorConfigAsset.components.Remove(component);

			if (AssetDatabase.IsSubAsset(component.GetInstanceID()))
			{
				// Undo.DestroyObjectImmediate(component);
				DestroyImmediate(component, true);
				AssetDatabase.SaveAssets();
			}
			
			RegenerateEditors();
		}

		private string GetShortComponentName(Type comp)
		{
			string compName = comp.ToString();
			string actorTypeName = typeof(T1).ToString();
			actorTypeName = actorTypeName.Remove(actorTypeName.Length - 15, 15);
			compName = compName.Remove(compName.Length - 6, 6).Remove(0, actorTypeName.Length);
			compName = UnPascalCase(compName);
			return compName;
		}

		public string UnPascalCase(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return "";

			var newText = new StringBuilder(text.Length * 2);
			newText.Append(text[0]);

			for (int i = 1; i < text.Length; i++)
			{
				var currentUpper = char.IsUpper(text[i]);
				var prevUpper = char.IsUpper(text[i - 1]);
				var nextUpper = (text.Length > i + 1) ? char.IsUpper(text[i + 1]) || char.IsWhiteSpace(text[i + 1]): prevUpper;
				var spaceExists = char.IsWhiteSpace(text[i - 1]);
				if (currentUpper && !spaceExists && (!nextUpper || !prevUpper))
						newText.Append(' ');
				newText.Append(text[i]);
			}
			return newText.ToString();
		}

		private string[] GetAllScriptableObjectPaths()
		{
			var SOs = AssetDatabase.FindAssets($"t: {typeof(T1).Name}").ToList();

			string[] paths = new string[SOs.Count];
			for (int i = 0; i < SOs.Count; i++)
			{
				paths[i] = AssetDatabase.GUIDToAssetPath(SOs[i]);
			}

			return paths;
		}

		private void HighlightSpecificComponent(int index)
		{
			for (int i = 0; i < _selectedComponents.Length; i++)
				_selectedComponents[i] = index == i;
		}

		private void ToggleComponentFoldout(int i) => actorConfigAsset.components[i].EditorFoldedOut = !actorConfigAsset.components[i].EditorFoldedOut;

		private void OnMenuMoveComponentDown(object userData)
		{
			var index = (int)userData;
			if (index == actorConfigAsset.components.Count - 1)
				return;

			MoveListItem(ref actorConfigAsset.components, index, index + 1);
			for (int i = 0; i < _selectedComponents.Length; i++)
				_selectedComponents[i] = i == index + 1; 

			ApplyChanges();
			RegenerateEditors();
		}

		private void OnMenuMoveComponentUp(object userData)
		{
			var index = (int)userData;
			if (index == 0)
				return;

			MoveListItem(ref actorConfigAsset.components, index, index - 1);
			for (int i = 0; i < _selectedComponents.Length; i++)
				_selectedComponents[i] = i == index - 1;

			ApplyChanges();
			RegenerateEditors();
		}

		private void OnMenuRemoveSelectedComponents()
		{
			// var index = (int)userData;
			// actorConfigAsset.components.RemoveAt(index);
			// for (int i = 0; i < _selectedComponents.Length; i++)
			// {
			// 	if (_selectedComponents[i])
			// 		actorConfigAsset.components.RemoveAt(i);
			// }
			for (int i = 0; i < actorConfigAsset.components.Count; i++)
			{
				if (_selectedComponents[i])
				{
					_selectedComponents[i] = false;
					actorConfigAsset.components.RemoveAt(i);
					i--;
				}
			}

			ApplyChanges();
			RegenerateEditors();
		}

		private void OnMenuNestSelectedComponents()
		{
			for (int i = 0; i < actorConfigAsset.components.Count; i++)
			{
				if (_selectedComponents[i])
				{
					T1 component = actorConfigAsset.components[i];
					int assetInstanceId = component.GetInstanceID();

					if (AssetDatabase.IsSubAsset(assetInstanceId))
						continue;

					T1 nestedComponent = Instantiate(component);
					nestedComponent.name = component.name;

					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assetInstanceId));
					AssetDatabase.AddObjectToAsset(nestedComponent, actorConfigAsset);
					AssetDatabase.SaveAssets();


					actorConfigAsset.components[i] = nestedComponent;
				}
			}

			AssetDatabase.Refresh();

			ApplyChanges();
			RegenerateEditors();
		}

		private void OnMenuExtractSelectedComponents()
		{
			string mainAssetPath = AssetDatabase.GetAssetPath(actorConfigAsset.GetInstanceID());

			for (int i = 0; i < actorConfigAsset.components.Count; i++)
			{
				if (_selectedComponents[i])
				{
					T1 component = actorConfigAsset.components[i];

					if (!AssetDatabase.IsSubAsset(component.GetInstanceID()))
						continue;

					AssetDatabase.RemoveObjectFromAsset(component);
					AssetDatabase.SaveAssets();
					AssetDatabase.CreateAsset(component,
						$"{Path.GetDirectoryName(mainAssetPath)}/{component.name}.asset");
					AssetDatabase.SaveAssets();
				}
			}
			
			AssetDatabase.Refresh();

			ApplyChanges();
			RegenerateEditors();
		}
		
		private void OnMenuSelectComponentAsset(object userData)
		{
			var index = (int)userData;
			Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GetAssetPath(actorConfigAsset.components[index]));
		}

		private void OnMenuAddEmptyComponentField()
		{
			actorConfigAsset.components.Add(null);
			ApplyChanges();
			RegenerateEditors();
		}

		private void OnMenuAddExistingComponent(object userData)
		{
			var path = (string)userData;
			T1 asset = (T1)AssetDatabase.LoadAssetAtPath(path, typeof(T1));

			if (AssetDatabase.IsSubAsset(asset.GetInstanceID()))
				return;

			actorConfigAsset.components.Add(asset);

			ApplyChanges();
			RegenerateEditors();
		}

		private void OnMenuCreateNestedComponent(object userData)
		{
			Type componentType = (Type)userData;
			T1 asset = (T1)CreateInstance((Type)userData);
			asset.name = GetShortComponentName(componentType);
			// asset.editorAssetIsNested = true;

			AssetDatabase.AddObjectToAsset(asset, actorConfigAsset);
			AssetDatabase.SaveAssets();

			actorConfigAsset.components.Add(asset);

			ApplyChanges();
			RegenerateEditors();
		}

		private void ApplyChanges()
		{
			EditorUtility.SetDirty(actorConfigAsset);
			serializedObject.ApplyModifiedProperties();
			_componentsProperty.serializedObject.Update();

		}

		private void RegenerateEditors()
		{
			int editorCount = actorConfigAsset.components.Count;

			if (EditorInstance)
			{
				for (int i = 0; i < EditorInstance.editors.Length; i++)
					DestroyImmediate(EditorInstance.editors[i]);

				DestroyImmediate(EditorInstance);
			}

			EditorInstance = (ActorConfigEditorInstance)ScriptableObject.CreateInstance(typeof(ActorConfigEditorInstance));

			EditorInstance.editors = new Editor[editorCount];
			_headerRects = new Rect[editorCount];
			_componentRects = new Rect[editorCount];
			_selectedComponents = new bool[editorCount];

			for (int i = 0; i < editorCount; i++)
			{
				if (actorConfigAsset.components[i] == null)
					continue;

				EditorInstance.editors[i] = Editor.CreateEditor(actorConfigAsset.components[i]);
			}
		}

		public void MoveListItem<T>(ref List<T> list, int oldIndex, int newIndex)
		{
			T item = list[oldIndex];
			list.RemoveAt(oldIndex);
			list.Insert(newIndex, item);
		}

		private void DrawMonochromeEditorIcon(string iconName, params GUILayoutOption[] options)
		{
			EditorExt.LabelFieldColor(
				EditorGUIUtility.isProSkin ? DARK_MODE_DARK_ICON_COLOR : LIGHT_MODE_DARK_ICON_COLOR,
				EditorGUIUtility.IconContent(iconName),
				options);
		}
	}
}