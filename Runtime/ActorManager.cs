using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gruffdev.BCS
{
	public abstract class ActorManager<T> : MonoBehaviour where T : IActor
	{
		public static ActorManager<T> Instance { get; private set; }

		public List<T> AllEntities { get; private set; } = new();
		public List<T> ActiveEntities { get; private set; } = new();
		public List<T> InactiveEntities { get; private set; } = new();
		public List<IActorUpdate> ActiveUpdateEntities { get; private set; } = new();
		public List<IActorLateUpdate> ActiveLateUpdateEntities { get; private set; } = new();
		public List<IActorFixedUpdate> ActiveFixedUpdateEntities { get; private set; } = new();

		protected virtual void Awake()
		{
			Instance = this;
		}

		protected virtual void Update()
		{
			for (int i = 0; i < ActiveUpdateEntities.Count; i++)
				ActiveUpdateEntities[i].OnUpdate();
		}

		protected virtual void LateUpdate()
		{
			for (int i = 0; i < ActiveLateUpdateEntities.Count; i++)
				ActiveLateUpdateEntities[i].OnLateUpdate();
		}

		protected virtual void FixedUpdate()
		{
			for (int i = 0; i < ActiveFixedUpdateEntities.Count; i++)
				ActiveFixedUpdateEntities[i].OnFixedUpdate();
		}

		public virtual void Enable(T actor)
		{
			if (!ActiveEntities.Contains(actor))
				ActiveEntities.Add(actor);

			InactiveEntities.Remove(actor);

			if (actor is IActorUpdate actorUpdate && !ActiveUpdateEntities.Contains(actorUpdate))
				ActiveUpdateEntities.Add(actorUpdate);

			if (actor is IActorLateUpdate actorLateUpdate && !ActiveLateUpdateEntities.Contains(actorLateUpdate))
				ActiveLateUpdateEntities.Add(actorLateUpdate);

			if (actor is IActorFixedUpdate actorFixedUpdate && !ActiveFixedUpdateEntities.Contains(actorFixedUpdate))
				ActiveFixedUpdateEntities.Add(actorFixedUpdate);
		}

		public virtual void Disable(T actor, bool disableForPooling = true)
		{
			if (disableForPooling && !InactiveEntities.Contains(actor))
				InactiveEntities.Add(actor);

			ActiveEntities.Remove(actor);

			if (actor is IActorUpdate actorUpdate)
				ActiveUpdateEntities.Remove(actorUpdate);

			if (actor is IActorLateUpdate actorLateUpdate)
				ActiveLateUpdateEntities.Remove(actorLateUpdate);

			if (actor is IActorFixedUpdate actorFixedUpdate)
				ActiveFixedUpdateEntities.Remove(actorFixedUpdate);
		}

		public virtual void Add(T actor)
		{
			if (!AllEntities.Contains(actor))
				AllEntities.Add(actor);
		}

		public virtual void Remove(T actor)
		{
			Disable(actor, false);
			AllEntities.Remove(actor);
			InactiveEntities.Remove(actor);
		}
	}
}