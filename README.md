# Behaviour Inject #

This is very simple inversion of control tool for unity MonoBehaviour.

## How to ##

For most cases you will need only three entities:

* InjectorBehaviour
* Context
* [Inject] attribute

## Initialization ##

Use any of your behavuours that starts BEFORE other behaviours, where you want the injection, to settle this code:

```
#!c#

void Awake(){
    MyDataModel model = new MyDataModel(); //Any of your dependenies
    Context context = new Context();
    context.RegisterDependency(model);
}
```

## Injection ##

Place 'InjectorBehaviour' at first place in the GameObject, adjacently to your dependent behaviours. "Context name" field defines optional context name. Thus you can use multiple contexts simultaneously.

In your MonoBehaviour mark dependency in this way:

```
#!c#

MyBehaviour : MonoBehaviour 
{
    [Inject]
    public MyDataModel Model { get; private set; }
}
```

Voila! MyDataModel should be there after Awake of the Injector.

## Multiple contexts ##

If you need multiple contexts at once, you may provide context name in it's constructor ( new Context("test_context"); ).
If no argument is passed context is named "default".

! Warning ! You can not have multiple contexts with the same name.

## Interfaces ##

You can specify interface injection this way:

```
#!c#
class InitiatorBehavour : MonoBehaviour
{
    void Awake(){
        MockReader model = new MockReader(); //Any of your dependenies
        Context context = new Context();
        context2.RegisterDependencyAs<MockReader, IReader>(mockReader);
    }
}

MyBehaviour : MonoBehaviour 
{
    [Inject]
    public MyDataModel Model { get; private set; }
}
```

## Watch example scene ##

Mechanism is showed in example scene. Use it to see how it works.

## Benchmark ##

On intel i5 3.2Ghz and Unity 5.3 it takes about 50 ms to make 1000 injections.