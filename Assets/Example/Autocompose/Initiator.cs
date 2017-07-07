using UnityEngine;
using System.Collections;
using BehaviourInject;

public class Initiator : MonoBehaviour {
    
	void Awake () {
        Settings settings = new Settings("127.9.1.1");
        Context context1 = new Context()
			.RegisterDependency(settings)
			.RegisterType<Core>()
			.RegisterType<Connection>()
			.RegisterType<MyData>()
			.RegisterFactory<Game, GameFactory>()
			.CreateAll();
    }
}
