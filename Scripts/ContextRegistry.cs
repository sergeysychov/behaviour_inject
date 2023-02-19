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
using System.Collections.Generic;
#if BINJECT_DIAGNOSTICS
using BehaviourInject.Diagnostics;
#endif

namespace BehaviourInject.Internal
{
    //Do not use this class or it's methods anywhere!

    internal class ContextRegistry
    {
        private static Dictionary<string, Context> _contextRegistry = new Dictionary<string, Context>();

        public static void RegisterContext(string name, Context context)
		{
			if (ContextNameDoesntExist(name))
				throw new ContextCreationException(String.Format("Context name \"{0}\" has to be enlisted in settings.", name));

            if (context == null)
				throw new ContextCreationException("You tried to register null context. Wait... why the heck are you even using this method???");

            if (_contextRegistry.ContainsKey(name))
				throw new ContextCreationException(String.Format("Context with name \"{0}\" already exists!", name));

            _contextRegistry.Add(name, context);
#if BINJECT_DIAGNOSTICS
			BinjectDiagnostics.ContextCount++;
#endif
        }


		private static bool ContextNameDoesntExist(string name)
		{
			string[] names = Settings.GetContextNames();
			for (int i = 0; i < names.Length; i++)
				if (names[i] == name)
					return false;
			return true;
		}


        public static void UnregisterContext(string name)
        {
            if(! _contextRegistry.ContainsKey(name))
                throw new BehaviourInjectException(String.Format("Context \"{0}\" already removed or never existed!", name));

            _contextRegistry.Remove(name);
#if BINJECT_DIAGNOSTICS
			BinjectDiagnostics.ContextCount--;
#endif
		}


		public static Context GetContext(string name)
		{
			if (!_contextRegistry.ContainsKey(name))
                throw new BehaviourInjectException(String.Format("Context with name \"{0}\" does not exist!", name));

            return _contextRegistry[name];
        }

        public static Context GetContext(int index)
		{
			string[] names = Settings.GetContextNames();
			if (index >= names.Length)
				throw new BehaviourInjectException("Context index out of bounds " + index);

			return GetContext(names[index]);
        }


        public static bool Contains(string name)
        {
	        return _contextRegistry.ContainsKey(name);
        }
    }
}
