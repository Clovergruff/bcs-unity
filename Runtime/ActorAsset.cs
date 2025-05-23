using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gruffdev.BCS
{
	public abstract class ActorConfigAsset<T> : ScriptableObject
	{
	#if UNITY_EDITOR
		[System.NonSerialized] public bool foldedOut = true;
	#endif

		public List<T> components = new();
	}
}