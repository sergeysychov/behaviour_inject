using UnityEngine;
using System.Collections;
using BehaviourInject;

public class TestBeahviour2 : MonoBehaviour {

	public DataModel _model;
	public IReader _reader;

	public EventReceiver<MyEvent> eventReceiver { get; private set; }
	
	void Start () {
        Debug.Log(gameObject.name + " TB2");
        Debug.Log(_model.Data);
        Debug.Log(_reader.Read());
		eventReceiver = new EventReceiver<MyEvent>();

		eventReceiver.OnEvent += HandleEvent;
	}


	[Inject]
	public void Init(DataModel model, IReader reader)
	{
		_model = model;
		_reader = reader;
	}
	

	public void HandleEvent(MyEvent evnt)
	{
		Debug.Log("/// catch event " + evnt._index);
	}

	[InjectEvent]
	private void HandleEvent2(IMyEvent evnt)
	{
		Debug.Log("catch IMyEvent " + evnt.Num());
	}

	[InjectEvent]
	public void HandleEvent3(InternEvent evnt)
	{
		Debug.Log("catch event3 ");
	}

	public class InternEvent {
	}
}
