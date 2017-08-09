using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 変数への参照
    /// </summary>
    public class LinkVariable<T> : BaseVariable<T>
    {
        /// <summary>
        /// 参照先変数
        /// </summary>
        public IVariable<T> Link { get; set; }

        public LinkVariable(string label = null) : base(label)
        {
        }

        public override T Update(int t)
        {
            return Link.Get(t);
        }
    }
}
