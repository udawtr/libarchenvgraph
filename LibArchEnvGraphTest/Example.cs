using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibArchEnvGraph;
using LibArchEnvGraph.Modules;
using static LibArchEnvGraph.Utility;

namespace LibArchEnvGraphTest
{
    [TestClass]
    public class Example
    {
        [TestMethod]
        public void Example_6_2()
        {
            var F = new FunctionFactory();

            var q = F.Oppenheim(F.Variable(20 + 273.15), F.Variable(15 + 273.15), 1.0, 0.9, 0.9).Get(0);

            Assert.AreEqual(22.8, q, 0.1);
        }

        [TestMethod]
        public void Example_6_3()
        {
            var F = new FunctionFactory();

            var alpha_i = 9.0;  //[W/m2K]
            var alpha_o = 23.0; //[W/m2K]
            var materials = new[]
            {
                new { Name = "木材", depth=15.0, lambda = 0.179},
                new { Name = "グラスウール", depth=30.0, lambda = 0.044},
                new { Name = "コンクリート", depth=120.0, lambda = 1.637},
                new { Name = "モルタル", depth=30.0, lambda = 1.087},
            };
            var r_air = 0.083;  //中空層

            var R = 1.0 / alpha_i + r_air + 1.0 / alpha_o;
            foreach (var m in materials)
            {
                R += m.depth / 1000 / m.lambda;
            }
            var K = 1.0 / R;

            Assert.AreEqual(1.10, R, 0.01);
            Assert.AreEqual(0.91, K, 0.01);


            //F.HeatTransmission
            var q = F.OverallHeatTransmission(F.Variable(20), F.Variable(0), 0.91, 6);
            Assert.AreEqual(109.2, q.Get(0), 0.1);

            //HeatTransmissionModule
            var wall = new OverallHeatTransmissionModule
            {
                K = 0.91,
                S = 6,
                TempIn = new IVariable<double>[] {
                    F.Variable(20),
                    F.Variable(0)
                }
            };
            wall.Init(F);
            Assert.AreEqual(-109.2, wall.HeatOut[0].Get(0), 0.1);
            Assert.AreEqual(109.2, wall.HeatOut[1].Get(0), 0.1);
        }

        [TestMethod]
        public void Example_6_5()
        {
            var Ti = 20.0;
            var To = 0.0;
            var K = 0.91;
            var alpha_i = 9.0;  //[W/m2K]
            var alpha_o = 23.0; //[W/m2K]

            var Tsi = Ti - K / alpha_i * (Ti - To);
            var Tso = To + K / alpha_o * (Ti - To);

            Assert.AreEqual(18.0, Tsi, 0.1);
            Assert.AreEqual(0.8, Tso, 0.1);
        }

        [TestMethod]
        public void Example_6_7()
        {
            var F = new FunctionFactory();

            var a_s = 0.7;      //日射吸収率 [-]
            var K = 2.0;        //外壁の熱貫流率 [W/m2K]
            var alpha_o = 23;   //外側熱伝達率 [W/m2K]
            var eps = 0.9;      //長波長放射率 [-]

            var Te = F.SAT(F.Variable(0), F.Variable(600), F.Variable(100), a_s, eps, alpha_o);
            var SAT = F.SAT(F.Variable(32), F.Variable(600), F.Variable(100), a_s, eps, alpha_o);
            var q = F.OverallHeatTransmission(SAT, F.Variable(25), 2.0, 1.0);

            Assert.AreEqual(14.3, Te.Get(0), 0.1);
            Assert.AreEqual(46.3, SAT.Get(0), 0.1);
            Assert.AreEqual(42.6, q.Get(0), 0.1);
        }

        [TestMethod]
        public void Example_6_8()
        {
            var tei12 = GetTotalTransmittance(0.86, 0.12, 0.08, 0.51);
            Assert.AreEqual(0.11, tei12, 0.01);

            var a_bar = GetTotalAbsorptivity(0.06, 0.37, 0.86, 0.12, 0.08, 0.51);
            Assert.AreEqual(0.09, a_bar[0], 0.01);
            Assert.AreEqual(0.33, a_bar[1], 0.01);
        }

        [TestMethod]
        public void Example_6_10()
        {
            var KS = GetTotalHeatTransmissionRate(
                new[] { 2.0, 2.0, 2.0, 2.0, 2.0, 2.0 },
                new[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 },
                1.0/3600
            );

            Assert.AreEqual(12.34, KS, 0.01);
        }

        [TestMethod]
        public void Example_6_11()
        {
            var KS = GetTotalHeatTransmissionRate(
                new[] {
                    //開口部
                    6.31,
                    6.31,
                    6.31,
                    6.31,
                    //屋根
                    1.5,
                    //外壁
                    2.0,
                    2.0,
                    2.0,
                    2.0,
                    //床
                    1.7
                },
                new[] {
                    //開口部
                    1.2*3.6,
                    1.2*5.4,
                    1.2*3.6,
                    1.2*5.4,
                    //屋根
                    5.0 * 10.0,
                    //外壁
                    5.0 * 4.0 - 1.2 * 3.6,
                    10.0 * 4.0 - 1.2 * 5.4,
                    5.0 * 4.0 - 1.2 * 3.6,
                    10.0 * 4.0 - 1.2 * 5.4,
                    //床
                    5.0 * 10.0
                },
                1.5 * 5.0 * 10.0 * 4.0 / 3600
            );

            Assert.AreEqual(593.6, KS, 0.01);
        }
    }
}
