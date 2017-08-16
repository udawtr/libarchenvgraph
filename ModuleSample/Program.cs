using LibArchEnvGraph;
using LibArchEnvGraph.Modules;
using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //自然対流伝熱モジュールの利用例
            NaturalConvectiveHeatTransferSample.Run();
        }
    }
}
