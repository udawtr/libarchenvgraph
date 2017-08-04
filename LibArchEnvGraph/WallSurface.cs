using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 壁面
    /// </summary>
    public class WallSurface
    {
        /// <summary>
        /// 壁体
        /// </summary>
        public Wall Wall { get; set; }

        /// <summary>
        /// 面No = 1 or 2
        /// </summary>
        public int SurfaceNo { get; set; }

        /// <summary>
        /// 表面温度 [℃]
        /// </summary>
        public IVariable<double> Temperature { get; set; }
    }
}
