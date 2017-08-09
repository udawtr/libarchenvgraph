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

        string Label { get; set; }
    }

    public abstract class BaseVariable<T> : IVariable<T>
    {
        protected int last_t = int.MinValue;

        protected T Data;

        public BaseVariable(string label = null)
        {
            this.Label = label;
        }

        public virtual T Get(int t)
        {
            if( last_t != t )
            {
                Data = Update(t);
                last_t = t;
            }
            return Data;
        }

        public virtual T Update(int t)
        {
            throw new NotImplementedException();
        }

        public string Label { get; set; }
    }
}
