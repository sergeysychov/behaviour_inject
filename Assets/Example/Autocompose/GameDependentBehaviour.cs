using UnityEngine;
using System.Collections;
using BehaviourInject;

public class GameDependentBehaviour : MonoBehaviour {

    [Inject]
    public Game MyGame { get; set; }

    [Inject]
    public Core MyCore { get; set; }

	// Use this for initialization
	void Start () {
        if (MyGame == null)
            Debug.Log("Game is null");
        else
            Debug.Log(MyGame.ToString());
	}
}
