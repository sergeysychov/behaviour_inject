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

namespace BehaviourInject.Internal
{
    //Do not use this class or it's methods anywhere!

    public class ContextRegistry
    {
        private static Dictionary<string, Context> _contextRegistry = new Dictionary<string, Context>();

        public static void RegisterContext(string name, Context context)
        {
            if (context == null)
                throw new BehaviourInjectException("You tried to register null context. Wait... why the heck are you even using this method???");

            if (_contextRegistry.ContainsKey(name))
                throw new BehaviourInjectException(String.Format("Context with name \"{0}\" already exists!", name));

            _contextRegistry.Add(name, context);
        }


        public static void UnregisterContext(string name)
        {
            if(! _contextRegistry.ContainsKey(name))
                throw new BehaviourInjectException(String.Format("Context \"{0}\" already removed or never existed!", name));

            _contextRegistry.Remove(name);
        }


        public static Context GetContext(string name)
        {
            if (!_contextRegistry.ContainsKey(name))
                throw new BehaviourInjectException(String.Format("Context with name \"{0}\" does not exist!", name));

            return _contextRegistry[name];
        }
    }
}
