using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gruffdev.BCS
{
	public abstract class EntityConfigAsset<T> : ScriptableObject
	{
	#if UNITY_EDITOR
		[System.NonSerialized] public UnityEditor.AnimatedValues.AnimBool foldedOut = new UnityEditor.AnimatedValues.AnimBool
		{
			value = true,
			target = true,
			speed = 10f,
		};
	#endif

		public List<T> components = new List<T>();
	}
}