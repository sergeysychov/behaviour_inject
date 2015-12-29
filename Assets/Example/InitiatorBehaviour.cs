using UnityEngine;
using System.Collections;
using BehaviourInject;

public class InitiatorBehaviour : MonoBehaviour {

    private Context _context1;
    private Context _context2;
    
	// Use this for initialization
	void Awake () {

        DataModel dataModel = new DataModel("dataOne");
        Network networker = new Network();

        _context1 = new Context();
        _context1.RegisterDependency(dataModel);
        _context1.RegisterDependencyAs<Network, IReader>(networker);

        DataModel mockData = new DataModel("this is mock data");
        MockReader mockReader = new MockReader();

        _context2 = new Context("test");
        _context2.RegisterDependency(mockData);
        _context2.RegisterDependencyAs<MockReader, IReader>(mockReader);
	}


    void OnDestroy()
    {
        _context2.Destroy(); //this may be local context of this scene
    }
}
