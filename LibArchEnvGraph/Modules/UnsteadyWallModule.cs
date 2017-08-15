using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 非定常1次元壁体モジュール
    /// 
    ///               +----------+
    ///               |          |
    ///    HeatIn1 -->+          +--> HeatOut1
    ///               |          |
    ///    HeatIn2 -->+  非定常  +--> HeatOut2
    ///               |  一次元  |
    ///    TempIn1 -->+  壁体M   +--> TempOut1
    ///               |          |
    ///    TempIn2 -->+          +--> TempOut2
    ///               |          |
    ///               +----+-----+
    ///                    |
    ///            depth --+
    ///              cro --+
    ///          n_slice --+
    ///                S --+
    ///           Rambda --+
    ///                c --+
    ///               dt --+
    /// 
    /// 入力:
    /// - 奥行 depth [m]
    /// - 容積比熱 cro [kJ/m^3・K]
    /// - 分割数 n_slice [個]
    /// - 表面積 S [m2]
    /// - 熱伝導率 Rambda [W/mK]
    /// - 室外側／室内側入力温度 TempIn [K]
    /// - 室外側／室内側入力熱量 HeatIn [W]
    /// - 単位時間 dt [s]
    /// - c値 c [-]
    /// 
    /// 出力:
    /// - 室外側／室内側出力温度 TempOut [K]
    /// - 出力対流熱移動量 HeatOut [W]
    /// </summary>
    public class UnsteadyWallModule : ContainerModule, IWallModule
    {
        /// <summary>
        /// 奥行 [m]
        /// </summary>
        public double depth { get; set; }

        /// <summary>
        /// 容積比熱 cρ [kJ/m^3・K]
        /// </summary>
        public double cro { get; set; }

        /// <summary>
        /// 分割数
        /// </summary>
        public int n_slice { get; set; } = 2;

        /// <summary>
        /// 表面積 [m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// 熱伝導率 [W/mK]
        /// </summary>
        public double Rambda { get; set; }

        /// <summary>
        /// 単位時間 [s]
        /// </summary>
        public double dt { get; set; }

        /// <summary>
        /// 対流熱伝達率 [W/m2K]
        /// </summary>
        public double[] a { get; set; } = new double[] { 23, 9};

        private double dx;

        private ConvectiveHeatTransferModule[] nv;

        private List<HeatCapacityModule> heatCapacityModuleList;

        private List<ConductiveHeatTransferModule> conductiveModuleList;

        /// <summary>
        /// 入力流体温度 [K]
        /// </summary>

        public IVariable<double>[] TempIn { get; set; } = new IVariable<double>[2];

        /// <summary>
        /// 出力壁体温度 [K]
        /// </summary>
        public IVariable<double>[] TempOut { get; private set; } = new[]
        {
            new LinkVariable<double>("非定常1次元壁体 - 表面温度1 [K]"),
            new LinkVariable<double>("非定常1次元壁体 - 表面温度2 [K]")
        };

        /// <summary>
        /// 入力熱流 [W]
        /// </summary>
        public IList<IVariable<double>>[] HeatIn { get; set; } = new[]
        {
            new List<IVariable<double>>(),
            new List<IVariable<double>>()
        };

        /// <summary>
        /// 出力熱流 [W]
        /// </summary>
        public IVariable<double>[] HeatOut { get; private set; } = new[]
        {
            new LinkVariable<double>("非定常1次元壁体 - 表面熱流1 [J]"),
            new LinkVariable<double>("非定常1次元壁体 - 表面熱流2 [J]"),
        };

        public UnsteadyWallModule()
        {
            Label = "非定常1次元壁体M";

            heatCapacityModuleList = new List<HeatCapacityModule>();
            conductiveModuleList = new List<ConductiveHeatTransferModule>();
            nv = new[]
            {
                new ConvectiveHeatTransferModule(),
                new ConvectiveHeatTransferModule(),
            };
        }

        public override void Init(FunctionFactory F)
        {
            dx = depth / n_slice;

            //層壁体の作成
            for (int i = 0; i < n_slice; i++)
            {
                heatCapacityModuleList.Add(new HeatCapacityModule
                {
                    cro = cro,          //容積比熱
                    V = dx * S,         //容積
                    dt = dt,
                    Label = $"層壁体{i+1}({Label})"
                });
            }

            //層壁体間の熱伝導の作成
            for (int i = 0; i < n_slice - 1; i++)
            {
                conductiveModuleList.Add(new ConductiveHeatTransferModule
                {
                    dx = dx,            //層の中心間の距離 [m]
                    Lambda = Rambda,    //熱伝導率
                    S = S,              //表面積[m2]
                    Label = $"層壁体{i+1}<=>{i+2}間の熱伝導 ({Label})"
                });

                heatCapacityModuleList[i].HeatIn.Add(conductiveModuleList[i].HeatOut[0]);
                heatCapacityModuleList[i+1].HeatIn.Add(conductiveModuleList[i].HeatOut[1]);

                conductiveModuleList[i].TempIn[0] = heatCapacityModuleList[i].TempOut;
                conductiveModuleList[i].TempIn[1] = heatCapacityModuleList[i + 1].TempOut;
            }

            //室外側・室内側層壁体への外部からの熱移動
            heatCapacityModuleList[0].HeatIn.Add(F.Concat(HeatIn[0]));
            heatCapacityModuleList[n_slice - 1].HeatIn.Add(F.Concat(HeatIn[1]));


            //自然換気設定
            nv[0].Label = $"表面対流熱移動1 ({Label})";
            nv[0].alpha_c = F.Variable(a[0]);
            nv[0].S = S;
            nv[0].TempIn = new[]
            {
                this.TempIn[0],  //室外温度
                heatCapacityModuleList[0].TempOut,  //室外側表面温度(室外側層壁体温度)
            };
            heatCapacityModuleList[0].HeatIn.Add(nv[0].HeatOut[1]);

            nv[1].Label = $"表面対流熱移動2 ({Label})";
            nv[1].alpha_c = F.Variable(a[1]);
            nv[1].S = S;
            nv[1].TempIn = new[]
            {
                heatCapacityModuleList[n_slice - 1].TempOut,    //室内側表面温度(室内側層壁体温度)
                this.TempIn[1]  //室内温度
            };
            heatCapacityModuleList[n_slice - 1].HeatIn.Add(nv[1].HeatOut[0]);

            //出力変数の登録
            (TempOut[0] as LinkVariable<double>).Link = heatCapacityModuleList[0].TempOut;  //室外側表面温度
            (TempOut[1] as LinkVariable<double>).Link = heatCapacityModuleList[n_slice - 1].TempOut;    //室内側表面温度
            (HeatOut[0] as LinkVariable<double>).Link = nv[0].HeatOut[0];   //室外への熱移動
            (HeatOut[1] as LinkVariable<double>).Link = nv[1].HeatOut[1];   //室内への熱移動

            TempOut[0].Label = $"{Label} - 表面温度1[K]";
            TempOut[1].Label = $"{Label} - 表面温度2[K]";
            HeatOut[0].Label = $"{Label} - 表面熱流1[J]";
            HeatOut[1].Label = $"{Label} - 表面熱流2[J]";

            //内部モジュールの登録
            Modules.AddRange(heatCapacityModuleList);
            Modules.AddRange(conductiveModuleList);
            Modules.AddRange(nv);

            //初期化
            base.Init(F);
        }
    }
}
