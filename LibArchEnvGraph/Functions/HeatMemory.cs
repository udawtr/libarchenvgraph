using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 熱量 [J] (ゲート付き)
    /// </summary>
    public class HeatMemory : IGateVariable<double>
    {
        private double _prev, _current;

        private int last_t = int.MinValue;

        /// <summary>
        /// 熱量の変化量を受け取ります
        /// </summary>
        public List<IVariable<double>> HeatIn { get; set; } = new List<IVariable<double>>();

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="initialU">初期熱量[J]</param>
        public void Set(double initialU)
        {
            this._prev = initialU;
        }

        public void Commit(int t)
        {
            GetCurrent(t);
        }

        public double GetCurrent(int t)
        {
            if( last_t != t)
            {
                _current = _prev + HeatIn.Sum(x => x.Get(t));
                last_t = t;
            }
            return _current;
        }

        public double Get(int t)
        {
            return _prev;
        }
    }
}
