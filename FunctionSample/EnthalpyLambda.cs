using LibArchEnvGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionSample
{
    public static class EnthalpyLambda
    {
        public static void Run()
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Enthalpy using lambda expression");
            Console.WriteLine("----------------------------------");

            var F = new FunctionFactory();

            //湿り空気温度 [C]
            var theta = F.Constant(20);

            //湿り空気の絶対湿度 [kg/kg']
            var x = F.Constant(0.001, 0.005, 0.01, 0.05);

            //エンタルピーを求める式
            var h = F.Function(t => 0.24 * theta.Get(t) + x.Get(t) * (587.5 + 0.441 * theta.Get(t)));

            for (int t= 0; t < 4; t++)
            {
                Console.WriteLine($"theta:{theta.Get(t)}, x:{x.Get(t)} => {h.Get(t)} [kcal/kg']");
            }

            Console.WriteLine("");
        }
    }
}
