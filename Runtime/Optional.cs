using System;
using UnityEngine;

namespace Gruffdev.BCS
{
	[Serializable]
	public struct Optional<T>
	{
		[field: SerializeField]
		public bool Enabled { get; private set; }
		[field: SerializeField]
		public T Value { get; private set; }

		public Optional(T initialValue, bool enabled = true)
		{
			Enabled = enabled;
			Value = initialValue;
		}
	}
}