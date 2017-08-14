using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// モジュールの基本クラス
    /// </summary>
    public abstract class BaseModule : ICalculationGraph
    {
        /// <summary>
        /// モジュールのラベル
        /// </summary>
        public string Label { get; set; }

        public virtual void Commit(int t)
        {
        }

        public virtual void Init(FunctionFactory F)
        {
        }

        public override string ToString()
        {
            if (Label != null) return Label;
            return $"{this.GetType().ToString()}";
        }
    }
}
