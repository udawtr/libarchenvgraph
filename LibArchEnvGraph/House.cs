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

        /// <summary>
        /// 外気温 [度]
        /// </summary>
        public IVariable<double> OutsideTemperature { get; set; }

        /// <summary>
        /// 日射量 [W]
        /// </summary>
        public IVariable<double> SolarRadiation { get; set; }


        private void ResolveReference(WallSurface wallSurface)
        {
            if (wallSurface.Wall == null && !String.IsNullOrEmpty(wallSurface.Name))
            {
                wallSurface.Wall = GetWall(wallSurface.Name);
            }
        }

        public void ResolveReference()
        {
            foreach (var room in Rooms)
            {
                foreach (var wallSurface in room.Walls)
                {
                    ResolveReference(wallSurface);
                }
            }

            foreach (var wallSurface in OuterSurfaces)
            {
                ResolveReference(wallSurface);
            }
        }

        public Wall GetWall(string name)
        {
            return Walls.SingleOrDefault(x => x.Name == name);
        }
    }
}
