using UnityEngine;

namespace Gruffdev.BCS
{
	public abstract class ConfigScriptableObject : ScriptableObject
	{
		public virtual bool alwaysEnableFoldout {get;}
		
	#if UNITY_EDITOR
		[HideInInspector] public UnityEditor.AnimatedValues.AnimBool foldedOut = new UnityEditor.AnimatedValues.AnimBool
		{
			speed = 10f,
		};
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