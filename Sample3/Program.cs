using LibArchEnvGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample3
{
    class Program
    {
        static void Main(string[] args)
        {
            var F = new FunctionFactory();

            var a = new Variable<double>(5);
            var b = new Variable<double>(4);
            var x = new DataVariable<double>(new double[] { 1, 2, 3, 4, 5 });

            var y = F.Add(F.Multiply(a, x), b);

            for (int t = 0; t < 5; t++)
            {
                Console.WriteLine($"{t}\t{y.Get(t)}");
            }
        }
    }
}
