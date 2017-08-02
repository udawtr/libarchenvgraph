using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public interface IGateVariable<T> : IVariable<T>
    {
        void Set(T newValue);

        T GetCurrent(int t);

        void Commit(int t);
    }
}
