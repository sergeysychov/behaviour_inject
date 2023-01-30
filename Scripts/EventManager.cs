using System;
using System.Collections.Generic;

namespace BehaviourInject.Internal
{
	public class EventManager : IEventDispatcher, EventTransmitter
	{
		//public event Action<object> EventInjectors;

		private EventManager _parent;
		private List<EventTransmitter> _eventHandlers;
		private List<EventTransmitter> _executionBuffer;

		public EventManager()
		{
			_eventHandlers = new List<EventTransmitter>(32);
			_executionBuffer = new List<EventTransmitter>(_eventHandlers.Capacity);
		}

		public void DispatchEvent(object evnt)
		{
			if (evnt == null)
				throw new BehaviourInjectException("Dispatched event can not be null");
			Type eventType = evnt.GetType();

			if(eventType.IsValueType)
				throw new BehaviourInjectException("Dispatched event can not be value type");

			// _eventHandlers list might be changed during event execution.
			_executionBuffer.Clear();
			_executionBuffer.AddRange(_eventHandlers);
			int count = _eventHandlers.Count;
			for (int i = 0; i < count; i++)
			{
				_executionBuffer[i].TransmitEvent(evnt);
			}
		}


		public void SetParent(EventManager manager)
		{
			_parent = manager;
			_parent.AddTransmitter(this);
		}


		public void ClearParent()
		{
			if (_parent != null)
				_parent.RemoveTransmitter(this);
			_parent = null;
		}


		public void AddTransmitter(EventTransmitter transmitter)
		{
			if (transmitter == null)
				throw new ArgumentNullException("transmitter");
			_eventHandlers.Add(transmitter);
		}


		public void RemoveTransmitter(EventTransmitter transmitter)
		{
			_eventHandlers.Remove(transmitter);
		}


		public void TransmitEvent(object evt)
		{
			DispatchEvent(evt);
		}
	}


	public interface EventTransmitter
	{
		void TransmitEvent(object evt);
	}
}
