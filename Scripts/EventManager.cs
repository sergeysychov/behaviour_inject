using System.Reflection;
using System.Runtime.CompilerServices;

namespace BehaviourInject.Internal
{
    public class EventManager: IEventDispatcher, IEventRegistry, IDisposable
    {
        private static readonly Dictionary<Type, EventHandlersFactory> s_KnownEventHandlersFactory = new Dictionary<Type, EventHandlersFactory>();
        private static readonly object s_StaticTarget = new NamedObject("Static Target");
        private static readonly Queue<object[]> s_argsPool = new Queue<object[]>(Enumerable.Range(0, 4).Select(i => new object[1]));

        private readonly Dictionary<Type, EventHandlers> _eventHandlers;
        private bool _isDisposed;
        private readonly Dictionary<Type, EventHandlers> _derivedInterceptors;
        private readonly Dictionary<Type, HashSet<Type>> _concreteToBaseInterceptorsMap;
        private readonly List<WeakReference<IEventDispatcher>> _attachedDispatchers;

        public EventManager()
        {
            _eventHandlers = new Dictionary<Type, EventHandlers>();
            _isDisposed = false;

            _derivedInterceptors = new Dictionary<Type, EventHandlers>();
            _concreteToBaseInterceptorsMap = new Dictionary<Type, HashSet<Type>>();
            _attachedDispatchers = new List<WeakReference<IEventDispatcher>>();
        }

        public void AttachDispatcher(IEventDispatcher dispatcher)
        {
            for (int i = 0; i < _attachedDispatchers.Count; i++)
            {
                WeakReference<IEventDispatcher> dwr = _attachedDispatchers[i];
                if (dwr.TryGetTarget(out IEventDispatcher d) && d == dispatcher) return;
            }

            _attachedDispatchers.Add(new WeakReference<IEventDispatcher>(dispatcher));
        }

        public void DetachDispatcher(IEventDispatcher dispatcher)
        {
            bool detached = false;

            for (int i = 0; i < _attachedDispatchers.Count; i++)
            {
                WeakReference<IEventDispatcher> dwr = _attachedDispatchers[i];
                detached = dwr == null;
                detached = detached || (dwr.TryGetTarget(out IEventDispatcher d) && object.ReferenceEquals(d, dispatcher));

                if (detached)
                {
                    _attachedDispatchers[i] = null;
                }
            }

            if (detached)
            {
                _attachedDispatchers.RemoveAll(wr => wr is null || !wr.TryGetTarget(out _));
            }
        }

        public static void DeclareEvent<TEvent>()
        {
            Type eventType = typeof(TEvent);

            if (s_KnownEventHandlersFactory.TryGetValue(eventType, out EventHandlersFactory factory) && factory.IsFast)
                return;

            if (factory is not null && !factory.IsFast)
            {
                factory.IsFast = true;
                factory.Factory = (argsPool) => new FastEventHandlers<TEvent>(argsPool);
                factory.DelegateType = typeof(Action<TEvent>);

                return;
            }

            s_KnownEventHandlersFactory[eventType] = new EventHandlersFactory()
            {
                IsFast = true,
                Factory = (argsPool) => new FastEventHandlers<TEvent>(argsPool),
                DelegateType = typeof(Action<TEvent>)
            };
        }

        public static Type GetDelegateType(Type eventType)
        {
            if (s_KnownEventHandlersFactory.TryGetValue(eventType, out EventHandlersFactory factory))
            {
                return factory.DelegateType;
            }

            if (Settings.Instance.EventManagerVerbosity >= EventManagerVerbosity.Warn)
            {
                string message = $"Event type: {eventType} is not declared. Constructing delegate using reflection. Consider use AOT event declaration by calling: EventManager.DeclareEvent<{eventType}>();";
#if UNITY
                UnityEngine.Debug.LogWarning();
#else
                Console.Write(message);
#endif
            }

            try
            {
                Type openGenericDelegateType = typeof(Action<>);

                Type resultEventHandlerType = openGenericDelegateType.MakeGenericType(eventType);

                s_KnownEventHandlersFactory[eventType] = new EventHandlersFactory()
                {
                    IsFast = false,
                    DelegateType = resultEventHandlerType,
                    Factory = (argsPool) => new EventHandlers(argsPool)
                };

                return resultEventHandlerType;
            }
            catch (Exception e)
            {
                ApplicationException forwardException = new ApplicationException($"Unable to create Action<{eventType}> delegate. Consider use AOT event declaration by calling: EventManager.DeclareEvent<{eventType}>();", innerException: e);
#if UNITY
                UnityEngine.Debug.LogError(forwardException);
#endif

                throw forwardException;
            }
        }

        private void DispatchEvent<TEvent>(ref TEvent @event, Type eventType, Type actualEventType, EventHandlers handlers)
        {
            if (actualEventType == eventType && (handlers is IFastEventHandlers<TEvent> fast))
            {
                fast.Notify(@event);

                return;
            }
            else if (handlers is IFastObjectEventHandlers fastObject)
            {
                fastObject.Notify(@event);

                return;
            }

            if (handlers is ISlowEventHandlers slow)
            {
                slow.Notify(@event);
            }
        }

        public void DispatchEvent<TEvent>(TEvent @event)
        {
            if (_isDisposed) return;

            if (@event == null)
                throw new BehaviourInjectException("Dispatched event can not be null");

            Type eventType = typeof(TEvent);
            Type actualEventType = eventType;
            if (!eventType.IsValueType)
            {
                actualEventType = @event.GetType();
            }

            if (_concreteToBaseInterceptorsMap.TryGetValue(actualEventType, out HashSet<Type> interceptorTypes))
            {
                foreach (Type interceptorType in interceptorTypes)
                { 
                    DispatchEvent(ref @event, eventType, actualEventType, _derivedInterceptors[interceptorType]);
                }
            }

            if (_eventHandlers.TryGetValue(actualEventType, out EventHandlers handlers))
            {
                DispatchEvent(ref @event, eventType, actualEventType, handlers);
            }

            for (int i = 0; i < _attachedDispatchers.Count; i++)
            {
                WeakReference<IEventDispatcher> dwr = _attachedDispatchers[i];
                if (dwr is not null && dwr.TryGetTarget(out IEventDispatcher dispatcher))
                {
                    dispatcher.DispatchEvent(@event);
                }
            }
        }

        #region IEventBus

        private EventHandlers CreateEventHandlers(Type eventType)
        {
            if (s_KnownEventHandlersFactory.TryGetValue(eventType, out EventHandlersFactory factory))
            {
                return factory.Factory(s_argsPool);
            }
            else
            {
                return new EventHandlers(s_argsPool);
            }
        }

        private EventHandlers EnsureEventHandlers(Type eventType)
        {
            if (!_eventHandlers.TryGetValue(eventType, out EventHandlers handlers))
            {
                _eventHandlers[eventType] = handlers = CreateEventHandlers(eventType);
            }

            return handlers;
        }
        #region ValidateHandler
        private bool ValidationMessage(string message, string paramName)
        {
            if (Settings.Instance.ThrowOnSubscriptionFailure)
            {
                throw new ArgumentException(message: message, paramName: paramName);
            }
            if (Settings.Instance.EventManagerVerbosity > EventManagerVerbosity.None)
            {
#if UNITY
                UnityEngine.Debug.LogError(message);
#else
                Console.WriteLine(message);
#endif
            }

            return false;
        }

        private bool ValidateHandler(Type eventType, Delegate handler)
        {
            if (handler is null)
                return ValidationMessage(message: "Handler must be specified", paramName: nameof(handler));

            if (eventType is null)
                return ValidationMessage(message: "Event type must be specified", paramName: nameof(eventType));

            MethodInfo method = handler.GetMethodInfo();
            if (method.ReturnType != typeof(void))
                return ValidationMessage(message: "Handler method must return void", paramName: nameof(handler));

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 1)
                return ValidationMessage(message: "Handler method must have only one parameter", paramName: nameof(handler));

            Type parameterType = parameters[0].ParameterType;
            if (!parameterType.IsAssignableFrom(eventType))
                return ValidationMessage(message: $"Handler method parameter must be assignable from event type. Event type: {eventType}; parameter type: {parameterType}", paramName: nameof(handler));

            if (!parameterType.IsValueType && eventType.IsValueType)
                return ValidationMessage(message: $"Event type is ValueType, handler parameter type is not ValueType. Event type: {eventType}; parameter type: {parameterType}", paramName: nameof(handler));

            if (eventType.IsValueType && !s_KnownEventHandlersFactory.ContainsKey(eventType))
            {
                if (Settings.Instance.EventManagerVerbosity > EventManagerVerbosity.None)
                {
                    string message = $"Possible excessive BOXING warning! You are subscribing to value type event. Consider declare event by calling EventManager.DeclareEvent<{eventType}>() to avoid boxing";
#if UNITY
                    UnityEngine.Debug.LogWarning(message);
#else
                    Console.WriteLine(message);
#endif
                }
            }


            return true;
        }
#endregion

        public void Subscribe(Type eventType, Delegate handler, bool handleAllDerived) => Subscribe(eventType, handler, handleAllDerived, handleDerived: null);

        private void SubscribeRefTypeEventHandler(Type eventType, Delegate handler, bool handleAllDerived, Type[] handleDerived)
        {
            if (handleAllDerived)
            {
                if (!_derivedInterceptors.TryGetValue(eventType, out EventHandlers derivedHandlers))
                {
                    derivedHandlers = CreateEventHandlers(eventType);

                    _derivedInterceptors[eventType] = derivedHandlers;
                }
                derivedHandlers.AddListener(handler);

                foreach (var eventHandlersKp in _eventHandlers)
                {
                    if (!eventHandlersKp.Key.IsValueType && eventType.IsAssignableFrom(eventHandlersKp.Key))
                    {
                        eventHandlersKp.Value.Unsubscribe(handler);

                        RegisterInterceptors(eventHandlersKp.Key);
                    }
                }

                return;
            }
            else
            {
                if (_derivedInterceptors.TryGetValue(eventType, out EventHandlers derivedHandlers)
                    && derivedHandlers.HasListener(handler))
                    return;

                EventHandlers handlers = EnsureEventHandlers(eventType);
                handlers.AddListener(handler);
            }

            RegisterInterceptors(eventType);

            if (handleAllDerived || handleDerived is null) return;

            foreach (Type filterType in handleDerived)
            {
                Subscribe(filterType, handler, false, handleDerived: null);
            }
        }

        private void RegisterInterceptors(Type eventType)
        {
            foreach (var derivedInterceptorKp in _derivedInterceptors)
            {
                if (derivedInterceptorKp.Key != eventType)
                {
                    Type interceptorType = derivedInterceptorKp.Key;
                    if (interceptorType.IsAssignableFrom(eventType))
                    {
                        if (!_concreteToBaseInterceptorsMap.TryGetValue(eventType, out HashSet<Type> interceptorTypes))
                        {
                            interceptorTypes = _concreteToBaseInterceptorsMap[eventType] = new HashSet<Type>();
                        }
                        interceptorTypes.Add(interceptorType);
                    }
                }
            }
        }

        private void SubscribeValueTypeEventHandler(Type eventType, Delegate handler)
        {
            EventHandlers handlers = EnsureEventHandlers(eventType);
            handlers.AddListener(handler);
        }

        public void Subscribe(Type eventType, Delegate handler, bool handleAllDerived, Type[] handleDerived)
        {
            if (_isDisposed) return;

            if (!ValidateHandler(eventType, handler)) return;

            if (eventType.IsValueType)
            {
                SubscribeValueTypeEventHandler(eventType, handler);
            }
            else
            {
                SubscribeRefTypeEventHandler(eventType, handler, handleAllDerived, handleDerived);
            }
        }

        public void Unsubscribe(Type eventType, Delegate handler) => Unsubscribe(eventType, handler, filter: null);
        

        public void Unsubscribe(Type eventType, Delegate handler,Type[] filter)
        {
            if (_eventHandlers.TryGetValue(eventType, out EventHandlers handlers))
            {
                handlers.Unsubscribe(handler);
            }

            if (_derivedInterceptors.TryGetValue(eventType, out handlers))
            {
                handlers.Unsubscribe(handler);
            }


            if (filter is null) return;
            foreach (Type filterType in filter)
            {
                if (_eventHandlers.TryGetValue(filterType, out handlers))
                {
                    handlers.Unsubscribe(handler);
                }
            }
        }

        public void UnsubscribeTarget(object target)
        {
            target = target ?? s_StaticTarget;

            foreach (var handlersKp in _eventHandlers)
            { 
                handlersKp.Value.UnsubscribeTarget(target);
            }

            foreach (var handlersKp in _derivedInterceptors)
            {
                handlersKp.Value.UnsubscribeTarget(target);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _eventHandlers.Clear();
        }
#endregion

        #region EventHandlers
        private interface ISlowEventHandlers
        {
            void Notify(object @event);
        }

        private interface IFastObjectEventHandlers
        {
            void Notify(object @event);
        }

        private interface IFastEventHandlers<TEvent>
        {
            void Notify(TEvent @event);
        }

        private interface ICaller<TEvent>
        { 
            void Notify(TEvent @event);
        }

        private class EventHandlersFactory
        {
            public bool IsFast;
            public Func<Queue<object[]>, EventHandlers> Factory;
            public Type DelegateType;
        }

        private class EventHandlers : ISlowEventHandlers
        {
            protected List<Listener> _listeners;

            private readonly ConditionalWeakTable<object, object> _weakSubscribers;
            private readonly Queue<object[]> _argsPool;
            protected int _usage;

            public EventHandlers(Queue<object[]> argsPool)
            {
                _listeners = new List<Listener>(64);
                _weakSubscribers = new ConditionalWeakTable<object, object>();
                _argsPool = argsPool;
                _usage = 0;
            }

            public void AddListener(Delegate handler)
            {
                bool isSingle = true;
                object target = handler.Target;
                if (target is null)
                {
                    isSingle = false;
                    target = s_StaticTarget;
                }

                if (!_weakSubscribers.TryGetValue(target, out object handlers))
                {
                    _weakSubscribers.Add(target, handler);
                    _listeners.Add(new Listener(target, handler));
                }
                else
                {
                    HashSet<Delegate> list = handlers as HashSet<Delegate>;
                    if (list is null)
                    {
                        Delegate oldHandler = handlers as Delegate;

                        if (isSingle && oldHandler == handler) return;

                        list = new HashSet<Delegate>() { oldHandler, handler};
                        _weakSubscribers.Remove(target);
                        _weakSubscribers.Add(target, list);
                        _listeners.Add(new Listener(target, handler));
                    }
                    else
                    {
                        if (isSingle && list.Contains(handler)) return;

                        list.Add(handler);
                        _listeners.Add(new Listener(target, handler));
                    }
                }
            }

            public bool HasListener(Delegate handler)
            {
                object target = handler.Target ?? s_StaticTarget;

                if (!_weakSubscribers.TryGetValue(target, out object handlers)) return false;

                HashSet<Delegate> list = handlers as HashSet<Delegate>;
                if (list is not null)
                    return list.Contains(handler);

                return handler == (handlers as Delegate);
            }

            private void NotifyList(List<Listener> list, object @event)
            {
                int count = list.Count;
                bool hasDead = false;
                Interlocked.Increment(ref _usage);
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        Listener listener = list[i];
                        if (!listener.TryGetHandler(out Delegate handler))
                        {
                            list[i] = default;
                            hasDead = true;
                            continue;
                        }
                        
                        if (!_argsPool.TryDequeue(out object[] invokeArgs))
                        {
                            invokeArgs = new object[1];
                        }

                        invokeArgs[0] = @event;
                        try
                        {
                            handler.DynamicInvoke(invokeArgs);
                        }
                        catch(Exception e)
                        {
                            string message = $"Invoke handler: {e.Message}\r\n{e.StackTrace}";
#if UNITY
                            UnityEngine.Debug.LogError(message);
#else
                            Console.WriteLine(message);
#endif
                        }
                        finally
                        {
                            invokeArgs[0] = null;
                            _argsPool.Enqueue(invokeArgs);
                        }

                    }
                }
                finally
                {
                    if (Interlocked.Decrement(ref _usage) == 0 && hasDead)
                    {
                        Cleanup();
                    }
                }
            }

            protected void Cleanup()
            {
                if (_listeners.Count > 0)
                {
                    int lastFreeSlot = -1;
                    for (int i = 0; i < _listeners.Count; i++)
                    {
                        Listener listener = _listeners[i];
                        if (listener.IsDead())
                        {
                            if (lastFreeSlot == -1)
                            {
                                lastFreeSlot = i;
                            }
                            continue;
                        }
                        else
                        {
                            if (lastFreeSlot != -1)
                            {
                                _listeners[lastFreeSlot++] = listener;
                            }
                        }
                    }

                    if (lastFreeSlot != -1)
                    {
                        _listeners.RemoveRange(lastFreeSlot, _listeners.Count - lastFreeSlot);
                    }
                }
            }

            public void Notify(object @event) => NotifyList(_listeners, @event);

            private void RemoveListenerReference(object target, Delegate handler)
            {
                object value;
                if (_weakSubscribers.TryGetValue(target, out value))
                {
                    HashSet<Delegate> list = value as HashSet<Delegate>;
                    if (list is null)
                    {
                        _weakSubscribers.Remove(target);
                    }
                    else
                    {
                        list.Remove(handler);

                        if (list.Count == 0)
                        {
                            _weakSubscribers.Remove(target);
                        }
                    }
                }
            }

            public void Unsubscribe(Delegate handler)
            {
                object target = handler.Target ?? s_StaticTarget;
                int count = _listeners.Count;
                bool hasDead = false;
                Interlocked.Increment(ref _usage);
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        Listener listener = _listeners[i];
                        if (listener.IsDead()
                            || (listener.IsMatch(target, handler)))
                        {
                            _listeners[i] = default;
                            hasDead = true;
                            RemoveListenerReference(target, handler);

                            continue;
                        }
                    }
                }
                finally
                {
                    if (Interlocked.Decrement(ref _usage) == 0 && hasDead)
                    {
                        Cleanup();
                    }
                }
            }

            public void UnsubscribeTarget(object target)
            {
                target = target ?? s_StaticTarget;
                int count = _listeners.Count;
                bool hasDead = false;
                
                _weakSubscribers.Remove(target);
                Interlocked.Increment(ref _usage);
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        Listener listener = _listeners[i];
                        if (listener.IsDead() || listener.Target == target)
                        {
                            _listeners[i] = default;
                            hasDead = true;

                            continue;
                        }
                    }
                }
                finally
                {
                    if (Interlocked.Decrement(ref _usage) == 0 && hasDead)
                    {
                        Cleanup();
                    }
                }
            }
        }

        private sealed class FastEventHandlers<TEvent> : EventHandlers,
            IFastEventHandlers<TEvent>,
            IFastObjectEventHandlers
        {
            public FastEventHandlers(Queue<object[]> argsPool)
                : base(argsPool) { }

            private void NotifyList(List<Listener> list, ref TEvent @event)
            {
                int count = list.Count;
                bool hasDead = false;
                Interlocked.Increment(ref _usage);
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        Listener listener = list[i];
                        if (!listener.TryGetHandler(out Delegate handler))
                        {
                            list[i] = default;
                            hasDead = true;
                            continue;
                        }

                        Action<TEvent> actualHandler = handler as Action<TEvent>;
                        if (actualHandler is null)
                        {
                            list[i] = default;
                            hasDead = true;
                            continue;
                        }

                        try
                        {
                            actualHandler(@event);
                        }
                        catch (Exception e)
                        {
                            string message = $"Invoke fast handler: {e.Message}\r\n{e.StackTrace}";
#if UNITY
                            UnityEngine.Debug.LogError(message);
#else
                            Console.WriteLine(message);
#endif
                        }
                    }
                }
                finally
                {
                    if (Interlocked.Decrement(ref _usage) == 0 && hasDead)
                    {
                        Cleanup();
                    }
                }
            }

            public void Notify(TEvent @event) => NotifyList(_listeners, ref @event);
            void IFastObjectEventHandlers.Notify(object @event)
            {
                TEvent theEvent = (TEvent) @event;
                NotifyList(_listeners, ref theEvent);
            }
        }

        private struct Listener
        {
            private WeakReference<object> _target;
            private WeakReference<Delegate> _handler;
            public Listener(object target, Delegate handler)
            {
                _target = new WeakReference<object>(target);
                _handler = new WeakReference<Delegate>(handler);
            }

            private T GetWeakTarget<T>(WeakReference<T> wr)
                where T : class
            {
                if (wr is null) return null;

                if (wr.TryGetTarget(out T result))
                    return result;

                return null;
            }

            public object Target => GetWeakTarget(_target);
            public Delegate Handler => GetWeakTarget(_handler);

            public bool TryGetHandler(out Delegate handler)
            {
                handler = Handler;

                return !IsDead() && handler is not null;
            }

            private bool IsDead<T>(WeakReference<T> wr)
                where T : class
            {
                if (wr is null) return true;

                if (!wr.TryGetTarget(out T target)) return true;

                if (object.ReferenceEquals(target, null)) return true;
#if UNITY
                if (target is UnityEngine.Object unityObject) 
                    return unityObject == null;
#endif

                return false;
            }
            public bool IsDead()
            {
                return
                    IsDead(_handler)
                    || IsDead(_target);
            }

            public bool IsMatch(object target, Delegate handler)
            {
                return
                    Object.ReferenceEquals(target, Target) 
                    && Object.Equals(handler, Handler);
            }
        }
#endregion
        #region Utility
        private class NamedObject
        {
            private string _name;
            public NamedObject(string name)
            {
                _name = name;
            }

            public override string ToString() => _name;
        }
        #endregion
    }
}
