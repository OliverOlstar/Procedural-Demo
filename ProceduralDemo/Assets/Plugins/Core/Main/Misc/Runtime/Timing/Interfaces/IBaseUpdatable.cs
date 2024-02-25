public interface IBaseUpdatable
{
	void OnRegistered();
	void OnDeregistered();
	double DeltaTime { get; }
}
