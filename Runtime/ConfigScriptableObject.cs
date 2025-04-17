using System;
using UnityEngine;

namespace Gruffdev.BCS
{
	public abstract class ConfigScriptableObject : ScriptableObject
	{
#if UNITY_EDITOR
		private bool _editorFoldedOut;
		public bool EditorFoldedOut
		{
			get => _editorFoldedOut;
			set
			{
				if (_editorFoldedOut == value)
					return;

				_editorFoldedOut = value;
				OnEditorFoldout?.Invoke(value);
			}
		}

		public virtual bool EditorAlwaysEnableFoldout { get; }

		public event Action<bool> OnEditorFoldout;
#endif

		public static bool IsComponentNull(ScriptableObject stackObject, ConfigScriptableObject configScriptableObject)
		{
			if (configScriptableObject == null)
			{
				Debug.LogWarning($"{stackObject.name} has a null item in it's config list. Please consider a cleanup.");
				return true;
			}

			return false;
		}
	}
}