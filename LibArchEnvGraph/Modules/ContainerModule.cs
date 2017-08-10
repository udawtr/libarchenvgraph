using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// コンテナモジュール
    /// 
    /// 他のモジュールを格納するモジュール。
    /// 一括で初期化やコミットを行うメソッドを提供します。
    /// </summary>
    public class ContainerModule : BaseModule
    {
        public List<ICalculationGraph> Modules { get; set; } = new List<ICalculationGraph>();

        public override void Init(FunctionFactory F)
        {
            foreach (var m in Modules)
            {
                m.Init(F);
            }
        }

        public override void Commit(int t)
        {
            foreach (var m in Modules)
            {
                m.Commit(t);
            }
        }
    }
}
