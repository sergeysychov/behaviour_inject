using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using BehaviourInject;
using Debug = UnityEngine.Debug;

public class EventBenchmark : MonoBehaviour {

	[SerializeField]
	private GameObject _prefab;
	[SerializeField]
	private int _recievers = 1000;

	[Inject]
	public IEventDispatcher EventManager { get; private set; }

	IEnumerator Start()
	{
		for (int i = 0; i < _recievers; i++)
		{
			Instantiate(_prefab);
		}

		yield return new WaitForSeconds(1f);
		var watch = new Stopwatch();
		watch.Start();
		EventManager.DispatchEvent(new MyEvent { _index = 1 });
		watch.Stop();
		Debug.Log(_recievers + " event receivers took " + watch.ElapsedMilliseconds + " ms");
	}
}
