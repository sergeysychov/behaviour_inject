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
using System.Collections.Generic;

namespace BehaviourInject
{
    //Do not use this class anywhere!

    public class Context
    {
        private Dictionary<Type, object> _dependencies;
        private Dictionary<Type, DependencyFactory> _factories;
        private HashSet<Type> _autoCompositionTypes;
        private string _name;

        public Context() : this("default")
        { }


        public Context(string contextName)
        {
            _name = contextName;
            ContextRegistry.RegisterContext(contextName, this);
            _dependencies = new Dictionary<Type, object>();
            _factories = new Dictionary<Type, DependencyFactory>();
            _autoCompositionTypes = new HashSet<Type>();
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
            if (_dependencies.ContainsKey(dependencyType) || _factories.ContainsKey(dependencyType) || _autoCompositionTypes.Contains(dependencyType))
                throw new BehaviourInjectException(dependencyType.FullName + " is already registered in this context");
        }


        public void RegisterFactory<T>(DependencyFactory factory)
        {
            ThrowIfNull(factory, "factory");
            Type dependencyType = typeof(T);
            ThrowIfRegistered(dependencyType);
            _factories.Add(dependencyType, factory);
        }


        public void RegisterFactory<T, FactoryT>()
        {
            Type factoryType = typeof(FactoryT);
            Type dependencyType = typeof(T);
            ThrowIfRegistered(dependencyType);
            RegisterType<FactoryT>();
            DependencyFactory factory = (DependencyFactory)Resolve(factoryType);
            _factories.Add(dependencyType, factory);
        }


        public void RegisterType<T>()
        {
            Type dependencyType = typeof(T);
            ThrowIfRegistered(dependencyType);
            _autoCompositionTypes.Add(dependencyType);
        }


        public object Resolve(Type resolvingType)
        {
            if (_dependencies.ContainsKey(resolvingType))
                return _dependencies[resolvingType];
            else if (_factories.ContainsKey(resolvingType))
                return _factories[resolvingType].Create();
            else if (_autoCompositionTypes.Contains(resolvingType))
                return AutocomposeDependency(resolvingType);
            else
                throw new BehaviourInjectException(String.Format("Can not resolve. Type {0} not registered in this context!", resolvingType.FullName));
        }


        private object AutocomposeDependency(Type resolvingType)
        {
            ConstructorInfo constructor = FindAppropriateConstructor(resolvingType);
            ParameterInfo[] parameters = constructor.GetParameters();
            object[] arguments = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                Type argumentType = parameter.ParameterType;

                if (argumentType == resolvingType)
                    throw new BehaviourInjectException("Cyclic dependency occured: " + argumentType.FullName);

                arguments[i] = Resolve(argumentType);
            }

            object result = constructor.Invoke(arguments);
            _dependencies.Add(resolvingType, result);

            return result;
        }


        private ConstructorInfo FindAppropriateConstructor(Type resolvingType)
        {
            ConstructorInfo[] constructors = resolvingType.GetConstructors();

            ConstructorInfo constructorWithLeastArguments = null;
            int leastParameters = Int32.MaxValue;

            for (int i = 0; i < constructors.Length; i++)
            {
                ConstructorInfo constructor = constructors[i];

                if (HasAttribute(constructor))
                    return constructor;

                int parametersLength = constructor.GetParameters().Length;
                if (parametersLength < leastParameters)
                {
                    constructorWithLeastArguments = constructor;
                    leastParameters = parametersLength;
                }
            }

            return constructorWithLeastArguments;
        }


        private bool HasAttribute(MemberInfo constructor)
        {
            object[] attributes = constructor.GetCustomAttributes(typeof(InjectAttribute), true);
            return attributes.Length > 0;
        }

        
        public void Destroy()
        {
            ContextRegistry.UnregisterContext(_name);
        }
    }
}
