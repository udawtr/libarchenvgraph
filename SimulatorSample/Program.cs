using LibArchEnvGraph;
using LibArchEnvGraph.Modules;
using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Youworks.Text;
using System.IO;

namespace SimulatorSample
{

    class Program
    {
        static void Main(string[] args)
        {
            //基本的な住宅の計算例
            SimpleSimulator.Run();

            //対流を無視した住宅の計算例
            NoConvectiveSimulator.Run();
        }
    }
}
