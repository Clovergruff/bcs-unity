using UnityEditor;
using UnityEngine;
using Gruffdev.BCS;

namespace Gruffdev.BCSEditor
{
	public class ActorSystemEditorBase<T> : Editor
		where T : MonoBehaviour
	{
		protected T system;

		protected virtual void OnEnable()
		{
			if (target != null)
				system = (T)target;
		}

		public override void OnInspectorGUI()
		{
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				this.DrawDefaultInspectorWithoutScriptField();
			}
		}
	}
}