public interface IEntity
{
}

public interface IEntityUpdate
{
	public abstract void OnUpdate();
}

public interface IEntityLateUpdate
{
	public abstract void OnLateUpdate();
}

public interface IEntityFixedUpdate
{
	public abstract void OnFixedUpdate();
}