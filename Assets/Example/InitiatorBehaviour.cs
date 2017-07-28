using UnityEngine;
using System.Collections;
using BehaviourInject;

public class InitiatorBehaviour : MonoBehaviour {

	private const string BASE = "base";
	private const string TEST = "test";

    private Context _context1;
    private Context _context2;
    
	// Use this for initialization
	void Awake () {

        DataModel dataModel = new DataModel("dataOne");
        Network networker = new Network();

		Context.Create(BASE)
			.RegisterDependency(dataModel);

		_context1 = Context.Create()
			.SetParentContext(BASE)
			.RegisterDependency(dataModel)
			.RegisterDependencyAs<Network, IReader>(networker);

        DataModel mockData = new DataModel("this is mock data");
        MockReader mockReader = new MockReader();

		_context2 = Context.Create(TEST)
			.SetParentContext(BASE)
			.RegisterDependencyAs<MockReader, IReader>(mockReader);
	}


    void OnDestroy()
    {
        _context2.Destroy(); //this may be local context of this scene
    }
}
