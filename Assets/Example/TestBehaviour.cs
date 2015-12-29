using UnityEngine;
using System.Collections;
using BehaviourInject;

public class TestBehaviour : MonoBehaviour {

    [Inject]
    public DataModel Model { get; set; }

	// Use this for initialization
	void Start ()
    {
        Debug.Log(gameObject.name + " TB1");
        Debug.Log(Model.Data);
    }
}
