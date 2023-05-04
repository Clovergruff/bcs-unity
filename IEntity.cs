public interface IEntity
{
}

public interface IEntityUpdate
{
	public void OnUpdate();
}

public interface IEntityLateUpdate
{
	public void OnLateUpdate();
}

public interface IEntityFixedUpdate
{
	public void OnFixedUpdate();
}