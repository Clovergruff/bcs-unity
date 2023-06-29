using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;

public abstract class EntityConfigAsset<T> : ScriptableObject
{
	[System.NonSerialized] public AnimBool foldedOut = new AnimBool
	{
		value = true,
		target = true,
		speed = 10f,
	};

	public List<T> components = new List<T>();
}
