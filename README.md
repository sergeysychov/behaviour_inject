# Behaviour Inject for Unity3d #

This is simple and reliable inversion of control tool for Unity3d. There are few script files with approximately 400 lines of code. You may easily handle it, support it or even extend it in the way you like. Although it provides crutial features of reflective dependency injection:
- resolving interfaces;
- injection to MonoBehaviour properties or fields;
- poco object autocomposition with constructor injection;
- using factories;

## How to ##

For most cases you will need only three entities:

* Context class
* InjectorBehaviour behaviour
* [Inject] attribute

### Initialization ###

Use any of your behaviours to settle following code. Make shure that it awakes BEFORE other behaviours, where you want to inject properties, and InjectorBehaviour.

```csharp
void Awake(){
    MyDataModel model = new MyDataModel(); //Any of your dependenies
    Context context = new Context();
    context.RegisterDependency(model);
}
```

### Injection ###

Place 'InjectorBehaviour' at first place in the GameObject, adjacently to your dependent behaviours. "Context name" field defines optional context name. Thus you can use multiple contexts simultaneously.

In your MonoBehaviour mark dependency in this way:

```csharp
public class MyBehaviour : MonoBehaviour 
{
    [Inject]
    public MyDataModel Model { get; private set; }
    
    [Inject]
    private MyDataModel _model;
}
```

Voila! MyDataModel should be there after Awake of the Injector. Note that if you want to use dependencies in Awake method, you should guarantee that InjectorBehaviour awakes before your target behaviours (but still after behaviour where context is created). In best case execution order must be like this: ContextCreator => InjectorBehaviour => InjectionTargets.

## Multiple contexts ##

If you need multiple contexts at once, you may provide context name in it's constructor 
```csharp 
new Context("my_context"); 
```
Then you should specify InjectorBehaviour to use this context by setting сorresponding name in inspector.
If no argument is passed context will be named "default".

You can not have multiple contexts with the same name at once.

It is also possible to destroy context (if it's bound to scene for example) simply by calling context.Destroy() method. 

You may create parent context that allow you to share dependencies between multiple contexts:

```charp
new Context("my_context")
	.SetParentContext("base");
```
After this any dependency that won't be found in "my_context" context will be searched in "base".

## Interfaces ##

You can specify interface injection this way:

```csharp
public class InitiatorBehavour : MonoBehaviour
{
    void Awake(){
        MockReader model = new MockReader(); //implements IReader
        Context context = new Context();
        context2.RegisterDependencyAs<MockReader, IReader>(mockReader);
    }
}

public class MyBehaviour : MonoBehaviour 
{
    [Inject]
    public IReader Reader { get; private set; }
}
```

## Autocomposition ##

BehaviourInject supports simle object hierarchy construction. Thus you may provide to the context only types of wished objects. And then during injection BehaviourInject will automatically create this objects using constructor dependency injection.

```csharp
public class InitiatorBehavour : MonoBehaviour
{
    void Awake(){
        Settings settings = new Settings("127.9.1.1");
        Context context1 = new Context();
        context1.RegisterDependency(settings);
        context1.RegisterType<Core>();
        context1.RegisterType<Connection>();
    }
}

public class MyBehaviour : MonoBehaviour 
{
    //connection is not created directly in your code. But constructed atomatically in Context;
    [Inject]
    public Connection Connector { get; set; }
}
```

Autocomposed type may have multiple constructors. If there are constructors marked by [Inject] attribute, context will use first of it. Thus make shure you have only one [Inject] for construcors. If there are no [Inject] attributes context will prefer constructor with less argument count.

```csharp
public class Connection
{
    //by default this constructor will be chosen
    //It is highly recommended to have only one constructor with [Inject] to avoid unpredictable behaviour.
    [Inject]
    public Connection(Settings settings)
    {
        ....
    }
    
    //if there are no [Inject] for any constructor this one will be preferred
    public Connection()
    {
        ...
    }
}
```

Autocomposition creates only one single object of type, keeps it and use for every appropriate injection in current context. If you need to create object for each injection use Factories described below.

## Factories ##

In case if you needed specific logic of object creation you may use factories. For example if you need to create object at some point at runtime. Or create object each time IoC resolving this type.

Factories also can be eather defined directly in code, or created by autocomposition.

```csharp
public class InitiatorBehavour : MonoBehaviour
{
    void Awake(){
        Context context1 = new Context();
        context1.RegisterType<Connection>();
        context1.RegisterFactory<Game, GameFactory>();
        //or
        var factory = new GameFactory(...);
        context1.RegisterFactory<Game>(factory);
    }
}

public class GameFactory : DependencyFactory
{
    public GameFactory(Connection connection)
    {
        ...
    }

    public object Create()
    {
        if (_connection.Connected)
            return new Game(1, "connected game");
        else
            return null;
    }
}


public class GameDependentBehaviour : MonoBehaviour {
    //created with factory!
    [Inject]
    public Game MyGame { get; set; }
}

```

## Events ##

Event model in BehaviourInject assumes that event sender resolves interface IEventDispatcher via DI and call DispatchEvent to dispatch event as object of any type except of value types.

```csharp
	[Inject]
	private IEventDispatcher _eventManager;
    
	public void CallFireEvent()
	{
		IMyEvent evt = new MyEvent();
		_eventManager.DispatchEvent(evt);
	}
```
Any targeted MonoBehaviour or object that is registered as dependency may receive that event if it have handler method. This method should contain single argument that has the same type as event object or its ancestors and signed with [InjectEvent] attribute.

```csharp
//both of this methods will be triggered on 'MyEvent' event because 'MyEvent' implements 'IMyEvent' interface

[InjectEvent]
public void ReceiveEvent(MyEvent evt)
{ ... }

[InjectEvent]
public void ReceiveEventInterface(IMyEvent evt)
{ ... }
```

This technique allows to dispatch and recieve events without taking care of subsctribing and unsubscribing.

## Watch example scene ##

There are example scenes and behaviours for simple injections, autocompositions and factories. Use it to see the action.

## Benchmark ##

On intel i5 3.2Ghz and Unity 5.3 it takes about 4 ms to make 1000 injections.
