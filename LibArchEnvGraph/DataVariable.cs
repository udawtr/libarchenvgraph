using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public class DataVariable<T> : IVariable<T>
    {
        private T[] data;

        public DataVariable(IEnumerable<T> data)
        {
            this.data = data.ToArray();
        }

        public T Get(int t)
        {
            return data[t];
        }
    }
}
