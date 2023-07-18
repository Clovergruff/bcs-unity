using UnityEditor;
using Gruffdev.BCS;

namespace Gruffdev.BCSEditor
{
	public class EntityComponentEditorBase<T> : Editor
		where T : ConfigScriptableObject
	{
		protected T config;

		protected virtual void OnEnable()
		{
			if (target != null)
				config = (T)target;
		}

		public override void OnInspectorGUI()
		{
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				this.DrawDefaultInspectorWithoutScriptField();

				if (check.changed)
				{
					EditorUtility.SetDirty(config);
					serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}