using UnityEngine;
using System.Collections;
using BehaviourInject;

public class InitiatorBehaviour : MonoBehaviour {
    
	// Use this for initialization
	void Awake () {

        DataModel dataModel = new DataModel("dataOne");
        Network networker = new Network();

        Context context1 = new Context();
        context1.RegisterDependency(dataModel);
        context1.RegisterDependencyAs<Network, IReader>(networker);

        DataModel mockData = new DataModel("this is mock data");
        MockReader mockReader = new MockReader();

        Context context2 = new Context("test");
        context2.RegisterDependency(mockData);
        context2.RegisterDependencyAs<MockReader, IReader>(mockReader);
	}
}
