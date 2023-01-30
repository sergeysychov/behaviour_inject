using UnityEngine;
using System.Collections;
using BehaviourInject;

public class InitiatorBehaviour : MonoBehaviour {
    private Context _context1;
    
	// Use this for initialization
	void Awake () {
		
        Network networker = new Network();
		_context1 = Context.Create()
			.RegisterDependencyAs<Network, IReader>(networker);
	}


    void OnDestroy()
    {
		_context1.Destroy(); //this may be local context of this scene
    }
}
