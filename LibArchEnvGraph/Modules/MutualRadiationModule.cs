using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 相互放射モジュール
    /// </summary>
    /// <seealso cref="RadiationHeatTransferModule"/>
    public class MutualRadiationModule : ContainerModule
    {
        private int N;

        public IVariable<double>[] TempIn { get; private set; }

        /// <summary>
        /// 相互放射収支熱量
        /// </summary>
        public LinkVariable<double>[] HeatOut { get; private set; }

        public MutualRadiationModule(int n) : base()
        {
            this.N = n;
            this.TempIn = new IVariable<double>[n];
            this.HeatOut = new LinkVariable<double>[n];

            for (int i = 0; i < n; i++)
            {
                this.HeatOut[i] = new LinkVariable<double>();
            }
        }

        public override void Init(FunctionFactory F)
        {
            if (N >= 2)
            {
                var heatOutList = new List<IVariable<double>>[N];

                for (int i = 0; i < N; i++)
                {
                    heatOutList[i] = new List<IVariable<double>>();
                }

                for (int i = 0; i < N - 1; i++)
                {
                    for (int j = i + 1; j < N; j++)
                    {
                        var R = new RadiationHeatTransferModule
                        {
                            //TODO: 放射の収支を無視した値になっているので真面目の解く
                            F12 = 1.0 / (N - 1),
                            TempIn1 = TempIn[i],
                            TempIn2 = TempIn[j]
                        };

                        heatOutList[i].Add(R.HeatOut1);
                        heatOutList[j].Add(R.HeatOut2);

                        Modules.Add(R);
                    }
                }

                for (int i = 0; i < N; i++)
                {
                    HeatOut[i].Link = F.Concat(heatOutList[i]);
                }
            }

            base.Init(F);
        }
    }
}
;