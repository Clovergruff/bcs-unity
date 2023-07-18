using System;
using UnityEngine;

namespace Gruffdev.BCS
{
	[Serializable]
	public struct Optional<T>
	{
		[SerializeField] public bool enabled;
		[SerializeField] public T value;

		public Optional(T initialValue, bool enabled = true)
		{
			this.enabled = enabled;
			value = initialValue;
		}
	}
}