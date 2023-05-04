using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConfigScriptableObject : ScriptableObject
{
	public virtual bool alwaysEnableFoldout {get;}
	public bool foldedOut {get; set;}

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
