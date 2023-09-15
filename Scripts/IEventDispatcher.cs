namespace BehaviourInject
{
	public interface IEventDispatcher
	{
		void DispatchEvent<TEvent>(TEvent @event);
    }
}
