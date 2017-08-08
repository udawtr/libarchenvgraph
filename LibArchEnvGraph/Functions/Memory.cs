using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// メモリゲート
    /// </summary>
    public class Memory : IGateVariable<double>
    {
        private double _prev, _current;

        private int last_t = int.MinValue;

        /// <summary>
        /// Commit実行時に保持する値
        /// </summary>
        public IVariable<double> DataIn { get; set; } 

        public Memory()
        {
            _prev = _current = 0.0;
        }

        public void Set(double newValue)
        {
            _prev = newValue;
        }

        public void Commit(int t)
        {
            GetCurrent(t);
        }

        public double GetCurrent(int t)
        {
            if( last_t != t)
            {
                _prev = _current;
                _current = DataIn.Get(t);
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
