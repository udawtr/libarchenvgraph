using LibArchEnvGraph.Functions;
using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 住宅を表すクラス
    /// </summary>
    public class House
    {
        /// <summary>
        /// 室一覧
        /// </summary>
        public List<Room> Rooms { get; set; } = new List<Room>();

        /// <summary>
        /// 壁体一覧
        /// </summary>
        public List<Wall> Walls { get; set; } = new List<Wall>();

        /// <summary>
        /// 外壁面一覧
        /// </summary>
        public List<WallSurface> OuterSurfaces { get; set; }
    }
}
