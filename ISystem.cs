public interface IEntitySystem
{
	void LateSetup();
}

public interface ISystemUpdate
{
	public void OnUpdate();
}

public interface ISystemLateUpdate
{
	public void OnLateUpdate();
}

public interface ISystemFixedUpdate
{
	public void OnFixedUpdate();
}