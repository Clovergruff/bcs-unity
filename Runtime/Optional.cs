using System;
using UnityEngine;

namespace Gruffdev.BCS
{
	[Serializable]
	public struct Optional<T>
	{
		[SerializeField]
		public bool Enabled { get; private set; }
		[SerializeField]
		public T Value { get; private set; }

		public Optional(T initialValue, bool enabled = true)
		{
			Enabled = enabled;
			Value = initialValue;
		}
	}
}