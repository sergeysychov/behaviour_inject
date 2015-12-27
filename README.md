# Behaviour Inject #

This is very simple inversion of control tool for unity MonoBehaviour.

## How to ##

You should now about only 3 entities:

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