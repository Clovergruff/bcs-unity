using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gruffdev.BCS
{
	public class GameEntity : MonoBehaviour, IEntity
	{
		protected virtual void Awake() => GameEntityManager.I.AddEntity(this);
		protected virtual void OnEnable() => GameEntityManager.I.EnableEntity(this);
		protected virtual void OnDisable() => GameEntityManager.I.DisableEntity(this);
		protected virtual void OnDestroy() => GameEntityManager.I.RemoveEntity(this);

		public virtual void OnUpdate() { }
		public virtual void OnLateUpdate() { }
		public virtual void OnFixedUpdate() { }

	}
}