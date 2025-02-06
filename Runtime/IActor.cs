namespace Gruffdev.BCS
{
	public interface IActor
	{
	}

	public interface IActorUpdate
	{
		public abstract void OnUpdate();
	}

	public interface IActorLateUpdate
	{
		public abstract void OnLateUpdate();
	}

	public interface IActorFixedUpdate
	{
		public abstract void OnFixedUpdate();
	}
}