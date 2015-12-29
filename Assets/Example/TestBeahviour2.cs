using UnityEngine;
using System.Collections;
using BehaviourInject;

public class TestBeahviour2 : MonoBehaviour {

    [Inject]
    public DataModel Model { get; set; }

    [Inject]
    public IReader Reader { get; set; }


	void Start () {
        Debug.Log(gameObject.name + " TB2");
        Debug.Log(Model.Data);
        Debug.Log(Reader.Read());
	}
}
