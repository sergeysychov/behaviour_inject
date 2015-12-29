using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject
{
    public interface DependencyFactory
    {
        object Create();
    }
}
