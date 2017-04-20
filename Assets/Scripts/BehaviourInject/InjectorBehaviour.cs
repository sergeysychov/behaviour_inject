/*
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
using System.Reflection;
using UnityEngine;
using BehaviourInject.Internal;

namespace BehaviourInject
{
    public class InjectorBehaviour : MonoBehaviour
    {
        [SerializeField]
        private string _contextName = "default";

        private Context _context;

        void Awake()
        {
            _context = ContextRegistry.GetContext(_contextName);
            
            FindAndResolveDependencies();
        }


        public void FindAndResolveDependencies()
        {
            MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
            
            for (int i = 0; i < components.Length; i++)
            {
                MonoBehaviour behaviour = components[i];

                if (behaviour == this)
                    continue;

                InjectToBehaviour(behaviour);
            }
        }


        public void InjectToBehaviour(MonoBehaviour behaviour)
        {
			Type componentType = behaviour.GetType();

			IBehaviourInjection[] injections = ReflectionCache.GetInjections(componentType);

			foreach (IBehaviourInjection injection in injections)
			{
				object dependency = _context.Resolve(injection.DependencyType);
				injection.Inject(behaviour, dependency);
			}
		}
    }


    public static class InjectorMonobehaviourExtensions {
        public static void ForceInject(this MonoBehaviour behaviour)
        {
            behaviour.SendMessage("InjectToBehaviour", behaviour, SendMessageOptions.DontRequireReceiver);
        }
    }
}
