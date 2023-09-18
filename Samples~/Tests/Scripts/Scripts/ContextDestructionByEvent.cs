using System.Collections;
using System.Collections.Generic;
using BehaviourInject;
using BehaviourInject.Test;
using UnityEngine;

namespace BehaviourInject.Test
{
    public class ContextDestructionByEvent : MonoBehaviour
    {
        void Start()
        {
            Context parentContext = Context.Create("base")
                .CompleteRegistration();

            Context childContext = Context.CreateChild("test", "base");
            // to reproduce crash context that is being destroyed must have some event recipitents after it.
            Context childContext2 = Context.CreateChild("default", "base")
                .CompleteRegistration();

            childContext.RegisterSingleton(new EventRecipient(childContext))
                .CompleteRegistration();

            parentContext.DispatchEvent(new DestroyEvent());
            Assert.Reached("Safely destroyed context by event.");

            parentContext.Destroy();
        }

        private class EventRecipient
        {
            private Context _context;

            public EventRecipient(Context childContext)
            {
                _context = childContext;
            }

            [InjectEvent]
            public void HandleDestroy(DestroyEvent e)
            {
                _context.Destroy();
            }
        }


        private class DestroyEvent
        {
        }
    }
}