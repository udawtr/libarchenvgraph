using LibArchEnvGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //基本演算の例
            BasicCalculation.Run();

            //エンタルピーを求める例(ラムダ式)
            EnthalpyLambda.Run();

            //エンタルピーを求める例(クラスを用いる例)
            EnthalpyClass.Run();
        }
    }
}
