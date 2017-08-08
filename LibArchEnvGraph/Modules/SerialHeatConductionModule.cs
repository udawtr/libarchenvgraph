using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    public class SerialHeatConductionModule : ContainerModule
    {
        public double depth { get; set; }

        /// <summary>
        /// 容積比熱 cρ [kJ/m^3・K]
        /// </summary>
        public double cro { get; set; }

        /// <summary>
        /// 分割数
        /// </summary>
        public int n_slice { get; set; } = 2;

        public double S { get; set; }

        /// <summary>
        /// 熱伝導率 [W/mK]
        /// </summary>
        public double Rambda { get; set; }

        /// <summary>
        /// 単位時間 [s]
        /// </summary>
        public double dt { get; set; }

        private double dx;

        private List<HeatCapacityModule> heatCapacityModuleList;

        private List<ConductiveHeatTransferModule> conductiveModuleList;

        public List<IVariable<double>> HeatIn1 { get; private set; }

        /// <summary>
        /// 出力温度 [K]
        /// </summary>
        public LinkVariable<double> TempOut1 { get; private set; }

        public List<IVariable<double>> HeatIn2 { get; private set; }

        /// <summary>
        /// 出力温度 [K]
        /// </summary>
        public LinkVariable<double> TempOut2 { get; private set; }

        public SerialHeatConductionModule()
        {
            HeatIn1 = new List<IVariable<double>>();
            HeatIn2 = new List<IVariable<double>>();
            TempOut1 = new LinkVariable<double>();
            TempOut2 = new LinkVariable<double>();

            heatCapacityModuleList = new List<HeatCapacityModule>();
            conductiveModuleList = new List<ConductiveHeatTransferModule>();
        }

        public override void Init(FunctionFactory F)
        {
            dx = depth / n_slice;

            for (int i = 0; i < n_slice; i++)
            {
                heatCapacityModuleList.Add(new HeatCapacityModule
                {
                    cro = cro,
                    V = dx * S,
                });
            }

            for (int i = 0; i < n_slice - 1; i++)
            {
                conductiveModuleList.Add(new ConductiveHeatTransferModule
                {
                    dx = dx,
                    Rambda = Rambda,
                    S = S,
                    dt = dt
                });
            }

            for (int i = 0; i < n_slice - 1; i++)
            {
                heatCapacityModuleList[i].HeatIn.Add(conductiveModuleList[i].HeatOut1);
                heatCapacityModuleList[i+1].HeatIn.Add(conductiveModuleList[i].HeatOut2);

                conductiveModuleList[i].TempIn1 = heatCapacityModuleList[i].TempOut;
                conductiveModuleList[i].TempIn2 = heatCapacityModuleList[i + 1].TempOut;
            }

            heatCapacityModuleList[0].HeatIn.Add(F.Multiply(dt, F.Concat(HeatIn1)));
            heatCapacityModuleList[n_slice - 1].HeatIn.Add(F.Multiply(dt, F.Concat(HeatIn2)));

            TempOut1.Link = heatCapacityModuleList[0].TempOut;
            TempOut2.Link = heatCapacityModuleList[n_slice - 1].TempOut;

            for (int i = 0; i < n_slice; i++)
            {
                heatCapacityModuleList[i].Init(F);
            }
            for (int i = 0; i < n_slice  -1; i++)
            {
                conductiveModuleList[i].Init(F);
            }

            Modules.AddRange(heatCapacityModuleList);
            Modules.AddRange(conductiveModuleList);
        }
    }
}
