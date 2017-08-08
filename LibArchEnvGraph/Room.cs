using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 室
    /// </summary>
    public class Room
    {
        /// <summary>
        /// 容積比熱 cρ [kJ/m^3・K]
        /// </summary>
        public double cro { get; set; }

        /// <summary>
        /// 容積[m^3]
        /// </summary>
        public double V { get; set; }

        /// <summary>
        /// 内壁面
        /// </summary>
        public List<WallSurface> Walls { get; set; }

        /// <summary>
        /// 室温 [℃] 
        /// </summary>
        public IVariable<double> RoomTemperature { get; set; }
    }
}
