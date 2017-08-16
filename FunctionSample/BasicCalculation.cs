using LibArchEnvGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionSample
{
    public static class BasicCalculation
    {
        public static void Run()
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Basic calculation");
            Console.WriteLine("----------------------------------");


            var F = new FunctionFactory();

            //定数
            var a = F.Constant(5);
            var b = F.Constant(4);
            var x = F.Constant(1, 2, 3, 4, 5);

            var y = F.Add(F.Multiply(a, x), b);

            for (int t = 0; t < 5; t++)
            {
                Console.WriteLine($"{a.Get(t)} * {x.Get(t)} + {b.Get(t)} => {y.Get(t)}");
            }

            Console.WriteLine("");
        }
    }
}
