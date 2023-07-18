using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gruffdev.BCS
{
	public abstract class EntityManager<T> : MonoBehaviour where T : IEntity
	{
		public List<T> allEntities;
		public List<T> activeEntities = new List<T>();
		public List<T> inactiveEntities = new List<T>();
		public List<IEntityUpdate> activeUpdateEntities = new List<IEntityUpdate>();
		public List<IEntityLateUpdate> activeLateUpdateEntities = new List<IEntityLateUpdate>();
		public List<IEntityFixedUpdate> activeFixedUpdateEntities = new List<IEntityFixedUpdate>();

		public static EntityManager<T> I;

		protected virtual void Awake()
		{
			I = this;
		}

		protected virtual void Update()
		{
			for (int i = 0; i < activeUpdateEntities.Count; i++)
				activeUpdateEntities[i].OnUpdate();
		}

		protected virtual void LateUpdate()
		{
			for (int i = 0; i < activeLateUpdateEntities.Count; i++)
				activeLateUpdateEntities[i].OnLateUpdate();
		}


		protected virtual void FixedUpdate()
		{
			for (int i = 0; i < activeFixedUpdateEntities.Count; i++)
				activeFixedUpdateEntities[i].OnFixedUpdate();
		}

		public virtual void DisableEntity(T entity, bool disableForPooling = true)
		{
			if (disableForPooling && !inactiveEntities.Contains(entity))
				inactiveEntities.Add(entity);

			activeEntities.Remove(entity);

			if (entity is IEntityUpdate entityUpdate) // && activeUpdateEntities.Contains(entityUpdate))
				activeUpdateEntities.Remove(entityUpdate);

			if (entity is IEntityLateUpdate entityLateUpdate) // && activeLateUpdateEntities.Contains(entityLateUpdate))
				activeLateUpdateEntities.Remove(entityLateUpdate);

			if (entity is IEntityFixedUpdate entityFixedUpdate) // && activeFixedUpdateEntities.Contains(entityFixedUpdate))
				activeFixedUpdateEntities.Remove(entityFixedUpdate);
		}

		public virtual void RemoveEntity(T entity)
		{
			DisableEntity(entity, false);
			allEntities.Remove(entity);
			inactiveEntities.Remove(entity);
		}

		public virtual void AddEntity(T entity)
		{
			if (!allEntities.Contains(entity))
				allEntities.Add(entity);
		}

		public virtual void EnableEntity(T entity)
		{
			if (!activeEntities.Contains(entity))
				activeEntities.Add(entity);

			inactiveEntities.Remove(entity);

			if (entity is IEntityUpdate entityUpdate && !activeUpdateEntities.Contains(entityUpdate))
				activeUpdateEntities.Add(entityUpdate);

			if (entity is IEntityLateUpdate entityLateUpdate && !activeLateUpdateEntities.Contains(entityLateUpdate))
				activeLateUpdateEntities.Add(entityLateUpdate);

			if (entity is IEntityFixedUpdate entityFixedUpdate && !activeFixedUpdateEntities.Contains(entityFixedUpdate))
				activeFixedUpdateEntities.Add(entityFixedUpdate);
		}
	}
}