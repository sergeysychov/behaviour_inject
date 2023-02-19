using BehaviourInject;
using BehaviourInject.Test;
using UnityEngine;

namespace BehaviourInject.Test
{
    public class TestFactoryMethod : MonoBehaviour
    {
        private void Start()
        {
            ArgumentDependency dependency = new ArgumentDependency();
            ResultDependency resultDependency = new ResultDependency();
            dependency.PropertyDependency = resultDependency;
            
            Context context = Context.Create("default")
                .RegisterDependency(dependency)
                .RegisterAsFunction<ArgumentDependency, ResultDependency>(a => a.PropertyDependency);
            
            Assert.Equals(context.Resolve<ResultDependency>(), resultDependency, "Factory method dependency");
            
            context.Destroy();
        }


        private class ArgumentDependency
        {
            public ResultDependency PropertyDependency;
        }
        
        
        private class ResultDependency
        {
            
        }
    }
}