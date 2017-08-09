using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public class DataVariable<T> : BaseVariable<T>
    {
        private T[] data;

        public DataVariable(IEnumerable<T> data)
        {
            this.data = data.ToArray();
        }

        public override T Get(int t)
        {
            return data[t];
        }
    }
}
