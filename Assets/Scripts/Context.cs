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

namespace BehaviourInject
{
    //Do not use this class anywhere!

    public class Context
    {
        private Dictionary<Type, object> _dependencies;

        public Context() : this("default")
        { }


        public Context(string contextName)
        {
            ContextRegistry.RegisterContext(contextName, this);
            _dependencies = new Dictionary<Type, object>();
        }

        public void RegisterDependency<T>(T dependency) {
            RegisterDependencyAs<T, T>(dependency);
        }


        public void RegisterDependencyAs<T, IT>(T dependency) where T : IT
        {
            ThrowIfNull(dependency, "dependency");
            Type dependencyType = typeof(IT);
            ThrowIfRegistered(dependencyType);
            _dependencies.Add(dependencyType, dependency);
        }


        private void ThrowIfNull(object target, string argName)
        {
            if (target == null)
                throw new BehaviourInjectException(argName + " is null");
        }


        private void ThrowIfRegistered(Type dependencyType)
        {
            if (_dependencies.ContainsKey(dependencyType))
                throw new BehaviourInjectException(dependencyType.FullName + " is already registered in this context");
        }


        public object Resolve(Type resolvingType)
        {
            if (!_dependencies.ContainsKey(resolvingType))
                throw new BehaviourInjectException(String.Format("Type {0} not registered in this context!", resolvingType.FullName));

            return _dependencies[resolvingType];
        }
    }
}
