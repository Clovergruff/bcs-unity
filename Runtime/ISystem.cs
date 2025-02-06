namespace Gruffdev.BCS
{
	public interface IActorSystem
	{
		void LateSetup();
		void ReusedSetup();
	}

	public interface IUpdate
	{
		public void OnUpdate();
	}

	public interface ILateUpdate
	{
		public void OnLateUpdate();
	}

	public interface IFixedUpdate
	{
		public void OnFixedUpdate();
	}
}