using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BehaviourInject.Internal;

namespace BehaviourInject.Test
{
	public class TestContextInheritance : MonoBehaviour
	{
		void Start()
		{
			InheritedDependency inherited = new InheritedDependency();
			OverridenDependency parentDep = new OverridenDependency();
			var parentContext = Context.Create(Context.DEFAULT)
				.RegisterSingleton(inherited)
				.RegisterSingleton(parentDep);
			
			OverridenDependency siblingOverrideDep = new OverridenDependency();
			ChildDependency siblingChildDep = new ChildDependency();
			var siblingContext = Context.CreateChild("green")
				.RegisterSingleton(siblingOverrideDep)
				.RegisterSingleton(siblingChildDep)
                .CompleteRegistration();

			OverridenDependency secondOverrideDep = new OverridenDependency();
			ChildDependency childDep = new ChildDependency();
			var secondContext = Context.CreateChild("test", Context.DEFAULT, overrideEventManager: true)
				.RegisterSingleton(secondOverrideDep)
				.RegisterSingleton(childDep);
			
			var thirdContext = Context.CreateChild("base", "test")
                .CompleteRegistration();

			Assert.Equals(inherited, secondContext.TestResolve<InheritedDependency>(), "inherit inherited 1gen dependency");
			Assert.Equals(inherited, siblingContext.TestResolve<InheritedDependency>(), "inherit inherited 11gen dependency");
			Assert.Equals(inherited, thirdContext.TestResolve<InheritedDependency>(), "inherit inherited 2gen dependency");

			try
			{
				parentContext.TestResolve<ChildDependency>();
				Assert.NotReached("parent context resolved inappropriate dependency or haven't thrown an exception");
			}
			catch (BehaviourInjectException e)
			{
				Assert.Exception(e, "preventing reversed inheritanse");
			}

			Assert.NotEquals(parentDep, 
				secondContext.TestResolve<OverridenDependency>(), "inherited override dep");
			Assert.Equals(secondOverrideDep, 
				secondContext.TestResolve<OverridenDependency>(), "inherit resolver overriden");
			Assert.NotEquals( siblingContext.TestResolve<OverridenDependency>(),
				secondContext.TestResolve<OverridenDependency>(),  "inherited no sibling interference");

			Assert.Equals(secondOverrideDep, 
				thirdContext.TestResolve<OverridenDependency>(), "inherit inherited 2gen from 1gen dependency");

			//events

			Event evt = new Event();
            
			var wrong = new WrongTransmitter();
			var parentContextTransmitter = new FlaggedTransmitter();
			var secondContextTransmitter = new FlaggedTransmitter();
			var thirdContextTransmitter = new FlaggedTransmitter();

			parentContext.EventManager.AttachDispatcher(wrong);
			secondContext.EventManager.AttachDispatcher(secondContextTransmitter);
			secondContext.TestResolve<IEventDispatcher>().DispatchEvent(evt);

			Assert.True(secondContextTransmitter.IsReached, "inherit event reached child");
			secondContextTransmitter.IsReached = false;

			parentContext.EventManager.DetachDispatcher(wrong);
			parentContext.EventManager.AttachDispatcher(parentContextTransmitter);
			thirdContext.EventManager.AttachDispatcher(thirdContextTransmitter);
			
			parentContext.TestResolve<IEventDispatcher>().DispatchEvent(evt);

			Assert.True(
				parentContextTransmitter.IsReached && 
				secondContextTransmitter.IsReached && 
				thirdContextTransmitter.IsReached, "inherit event reached all");

			//destruction
            thirdContext.Destroy();
			Assert.False(secondContext.IsDestroyed || parentContext.IsDestroyed, "context child no inversive destruction");
			parentContext.Destroy();
			Assert.True(siblingContext.IsDestroyed && secondContext.IsDestroyed, "context child inherited destruction");
		}

		private class InheritedDependency
		{ }
		private class OverridenDependency
		{ }
		private class ChildDependency
		{ }
		private class Event
		{ }


		private class WrongTransmitter : IEventDispatcher
        {
            public void DispatchEvent<TEvent>(TEvent evnt)
            {
                Assert.NotReached("parent context catched event from child");
            }
		}

		private class FlaggedTransmitter: IEventDispatcher
		{
			public bool IsReached { get; set; }
			public void DispatchEvent<TEvent>(TEvent evnt)
			{
                IsReached = true;
            }
		}
	}
}
