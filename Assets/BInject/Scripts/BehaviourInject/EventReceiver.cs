using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject
{
	public interface IReceiver
	{
		void Receive(object t);
	}


	public class EventReceiver<T> : IReceiver
	{
		public event Action<T> OnEvent;

		public void Receive(object t)
		{
			OnEvent((T)t);
		}
	}
}
