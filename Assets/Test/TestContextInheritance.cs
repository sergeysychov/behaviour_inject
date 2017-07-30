using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestContextInheritance : MonoBehaviour
	{
		void Start()
		{
			InheritedDependency inherited = new InheritedDependency();
			OverridenDependency parentDep = new OverridenDependency();
			var parentContext = Context.Create(Context.DEFAULT)
				.RegisterDependency(inherited)
				.RegisterDependency(parentDep);
			
			OverridenDependency siblingOverrideDep = new OverridenDependency();
			ChildDependency siblingChildDep = new ChildDependency();
			var siblingContext = Context.Create("green")
				.SetParentContext(Context.DEFAULT)
				.RegisterDependency(siblingOverrideDep)
				.RegisterDependency(siblingChildDep);

			OverridenDependency secondOverrideDep = new OverridenDependency();
			ChildDependency childDep = new ChildDependency();
			var secondContext = Context.Create("test")
				.SetParentContext(Context.DEFAULT)
				.RegisterDependency(secondOverrideDep)
				.RegisterDependency(childDep);
			
			var thirdContext = Context.Create("base")
				.SetParentContext("test");

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
			bool isReached = false;
			bool isReachedThird = false;
			bool isReachedParent = false;
			parentContext.EventManager.EventInjectors += WrongHandler;
			secondContext.EventManager.EventInjectors += (e) => { isReached = true; };
			secondContext.TestResolve<IEventDispatcher>().DispatchEvent(evt);
			Assert.True(isReached, "inherit event reached child");
			isReached = false;
			
			parentContext.EventManager.EventInjectors -= WrongHandler;
			parentContext.EventManager.EventInjectors += (e) => { isReachedParent = true; };
			thirdContext.EventManager.EventInjectors += (e) => { isReachedThird = true; };
			parentContext.TestResolve<IEventDispatcher>().DispatchEvent(evt);
			Assert.True(isReached && isReachedParent && isReachedThird, "inherit event reached all");

			//destruction

			thirdContext.Destroy();
			Assert.False(secondContext.IsDestroyed || parentContext.IsDestroyed, "context child no inversive destruction");
			parentContext.Destroy();
			Assert.True(siblingContext.IsDestroyed && secondContext.IsDestroyed, "context child inherited destruction");
		}


		private void WrongHandler(object evt)
		{
			Assert.NotReached("parent context catched event from child");
		}

		private class InheritedDependency
		{ }
		private class OverridenDependency
		{ }
		private class ChildDependency
		{ }
		private class Event
		{ }
	}
}
