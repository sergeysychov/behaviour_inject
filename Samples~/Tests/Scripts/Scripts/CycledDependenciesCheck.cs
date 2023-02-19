using UnityEngine;

namespace BehaviourInject.Test
{
    public class CycledDependenciesCheck : MonoBehaviour 
    {
        private void Start()
        {
            Context context = Context.Create();

            context
                .RegisterType<CycledA>()
                .RegisterType<CycledB>()
                .RegisterType<CycledC>();

            try
            {
                context.Resolve<CycledA>();
            }
            catch (BehaviourInjectException e)
            {
                Assert.Exception(e, "Catached dependency cycle");
            }
            
            context.Destroy();
        }
        
        
        private class CycledA
        {
            private int _counter;
            
            public CycledA(CycledB b)
            {
                _counter++;
                if (_counter > 1)
                {
                    Assert.NotReached("Cycled dependency not catched");
                    throw new BehaviourInjectException("FAILED TO CATCH");
                }
            }
        }
        
        
        private class CycledB
        {
            // public CycledB(CycledC C)
            // {
            //     
            // }

            [Inject]
            public void Init(CycledC c)
            {
                
            }
        }
        
        
        private class CycledC
        {
            public CycledC(CycledA a)
            {
                
            }
        }
    }
}