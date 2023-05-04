using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityConfigAsset<T> : ScriptableObject
{
	public bool foldedOut {get; set;} = true;

	public List<T> components = new List<T>();
}
