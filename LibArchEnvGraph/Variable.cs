using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public class Variable<T> : BaseVariable<T>
    {
        private Func<int, T> func;

        public Variable(Func<int,T> func)
        {
            this.func = func;
        }

        public Variable(T constValue)
        {
            this.func = (t) => constValue;
        }

        public override T Update(int t)
        {
            return func(t);
        }
    }
}
