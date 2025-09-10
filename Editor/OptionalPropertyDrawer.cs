using UnityEditor;
using UnityEngine;
using Gruffdev.BCS;

namespace Gruffdev.BCSEditor
{
	[CustomPropertyDrawer(typeof(Optional<>))]
	public class OptionalPropertyDrawer : PropertyDrawer
	{
		private const float CHECKBOX_WIDTH = 20;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var valueProperty = property.FindPropertyRelative("<Value>k__BackingField");
			return EditorGUI.GetPropertyHeight(valueProperty);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label )
		{
			var valueProperty = property.FindPropertyRelative("<Value>k__BackingField");
			var toggleProperty = property.FindPropertyRelative("<Enabled>k__BackingField");
			float originalWidth = position.width;

			// Value
			EditorGUI.BeginProperty(position, label, property);
			position.width -= CHECKBOX_WIDTH;
			
			EditorGUI.BeginDisabledGroup(!toggleProperty.boolValue);
			EditorGUI.PropertyField(position, valueProperty, label, true);
			EditorGUI.EndDisabledGroup();

			// Toggle checkbox
			int oldIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			position.x += originalWidth - CHECKBOX_WIDTH + 3; // Wtf?
			position.width = CHECKBOX_WIDTH;
			position.height = EditorGUI.GetPropertyHeight(toggleProperty);

			EditorGUI.PropertyField(position, toggleProperty, GUIContent.none);
			EditorGUI.indentLevel = oldIndentLevel;
			EditorGUI.EndProperty();
		}
	}
}