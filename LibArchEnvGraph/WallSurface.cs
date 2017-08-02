using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public class WallSurface
    {
        public Wall Wall { get; set; }

        public int SurfaceNo { get; set; }

        public IVariable<double> Temperature { get; set; }
    }
}
