using UnityEngine;
using System.Collections;
using BehaviourInject;

public class Initiator : MonoBehaviour {
    
	void Awake () {
        Settings settings = new Settings("127.9.1.1");
        Context context1 = new Context();
        context1.RegisterDependency(settings);
        context1.RegisterType<Core>();
        context1.RegisterType<Connection>();
        context1.RegisterFactory<Game, GameFactory>();
    }
}
