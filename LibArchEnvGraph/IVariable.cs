using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public interface IVariable<T>
    {
        T Get(int t);
    }
}
