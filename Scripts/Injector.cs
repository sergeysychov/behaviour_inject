﻿/*
The MIT License (MIT)

Copyright (c) 2015 Sergey Sychov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using BehaviourInject.Internal;
#if BINJECT_DIAGNOSTICS
using BehaviourInject.Diagnostics;
#endif

namespace BehaviourInject
{
	[DisallowMultipleComponent]
	public class Injector : MonoBehaviour
    {
		[SerializeField]
        private int _contextIndex = 0;
		[SerializeField]
        private bool _useHierarchy = false;

        private Context _context;
		private MonoBehaviour[] _componentsCache;

        void Awake()
		{
			if (_useHierarchy)
			{
				_context = GetContextFromHierarchy();
			}
			else
			{
				_context = ContextRegistry.GetContext(_contextIndex);
			}

			_context.OnContextDestroyed += HandleContextDestroyed;

			FindAndResolveDependencies();

#if BINJECT_DIAGNOSTICS
			BinjectDiagnostics.InjectorsCount++;
#endif
		}


		private Context GetContextFromHierarchy()
		{
			Transform ancestor = this.gameObject.transform;

			while(ancestor != null)
			{
				HierarchyContext[] components = ancestor.GetComponents<HierarchyContext>();
				if (components.Length > 0)
					return components[0].GetContext();
				ancestor = ancestor.parent;
			}

			throw new BehaviourInjectException("No ancestor context found");
		}


		private void HandleContextDestroyed()
		{
			Destroy(gameObject);
		}


        public void FindAndResolveDependencies()
        {
            MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();

#if BINJECT_DIAGNOSTICS
			BinjectDiagnostics.RecipientCount += components.Length;
#endif

			_componentsCache = components;

			foreach (MonoBehaviour component in components)
			{
				if (component == null || this == component)
					continue;

				InjectToBehaviour(component);
			}
        }

        public void DelayedInject(MonoBehaviour behaviour)
        {
	        _componentsCache = null;
	        InjectToBehaviour(behaviour);
        }
        
        private void InjectToBehaviour(MonoBehaviour behaviour)
        {
			Type componentType = behaviour.GetType();

			IMemberInjection[] injections = ReflectionCache.GetInjections(componentType);

			foreach (IMemberInjection injection in injections)
			{
				injection.Inject(behaviour, _context);
			}
			_context.AutoSubscribeToEvents(behaviour);
		}

		void OnDestroy()
		{
            //_context might be not initialized in case of exception in Awake (e.g. context not found)
            if (_context is not null)
			{
                _context.OnContextDestroyed -= HandleContextDestroyed;

                MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
				foreach (MonoBehaviour component in components)
				{
					_context.UnsubscribeTarget(component);
				}
			}

            _context = null;

#if BINJECT_DIAGNOSTICS
			BinjectDiagnostics.InjectorsCount--;
			BinjectDiagnostics.RecipientCount -= _componentsCache.Length;
#endif
		}
	}


    public static class InjectorMonobehaviourExtensions {
        public static void ForceInject(this MonoBehaviour behaviour)
        {
            behaviour.SendMessage("DelayedInject", behaviour, SendMessageOptions.DontRequireReceiver);
		}
    }
}
