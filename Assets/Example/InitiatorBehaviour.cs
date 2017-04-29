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

		Context baseContest = new Context(BASE);
		baseContest.RegisterDependency(dataModel);

		_context1 = new Context()
			.SetParentContext(BASE)
			.RegisterDependency(dataModel)
			.RegisterDependencyAs<Network, IReader>(networker);

        DataModel mockData = new DataModel("this is mock data");
        MockReader mockReader = new MockReader();

        _context2 = new Context(TEST)
			.SetParentContext(BASE)
			.RegisterDependency(mockData)
			.RegisterDependencyAs<MockReader, IReader>(mockReader);
	}


    void OnDestroy()
    {
        _context2.Destroy(); //this may be local context of this scene
    }
}
