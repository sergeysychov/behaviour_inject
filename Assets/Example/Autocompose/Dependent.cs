using UnityEngine;
using System.Collections;
using BehaviourInject;

public class Dependent : MonoBehaviour {

    public GameObject _prefab;

    [Inject]
    public Core _myCore;

    [Inject]
    public Connection Connector { get; set; }

	void Start () {
        Debug.Log("Autocomposed core data: " + _myCore.GetData());
	}


    public void Connect()
    {
        Connector.Connected = true;
    }


    public void CreateGameDependent()
    {
        Instantiate(_prefab);
    }
}
