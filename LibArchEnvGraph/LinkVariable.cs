using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 変数への参照
    /// </summary>
    public class LinkVariable<T> : IVariable<T>
    {
        /// <summary>
        /// 参照先変数
        /// </summary>
        public IVariable<T> Link { get; set; }

        public LinkVariable()
        {

        }

        public T Get(int t)
        {
            return Link.Get(t);
        }
    }
}
