using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibArchEnvGraph.Modules;
using LibArchEnvGraph;
using LibArchEnvGraph.Functions;

namespace LibArchEnvGraphTest
{
    [TestClass]
    public class ModulesTest
    {
        [TestMethod]
        public void HeatCapacityModuleTest()
        {
            var F = new FunctionFactory();

            //容積比熱 1000 kJ/m3K, 容積 0.1m3
            var target = new HeatCapacityModule
            {
                cro = 1000.0,
                V = 0.1,
            };

            //毎時 100000J 加熱する
            target.HeatIn.Add(new Variable<double>(100000));

            target.Init(F);

            var results = new double[20];
            for (int i = 0; i < 20; i++)
            {
                target.Commit(i);
                results[i] = target.TempOut.Get(i);
            }

            // 計算上は1 [K]ずつ加熱される
            // 100000J = 100 kJ
            // 100 [kJ] / (1000[kJ/m3K]*0.1[m3]) = 100 [kJ] / 100 [kJ/K] = 1 [K]

            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual((double)i, results[i]);
            }
        }

        [TestMethod]
        public void ConductiveHeatTransferModuleTest()
        {
            var F = new FunctionFactory();


            var target = new ConductiveHeatTransferModule
            {
                
            };
        }

        [TestMethod]
        public void ConvectiveHeatTransferModuleTest()
        {
            var F = new FunctionFactory();

            //外気 10度, 壁 0度
            var target = new ConvectiveHeatTransferModule
            {
                alpha_c = F.Variable(2),
                S = 2.0,    //2.0m2
                TempIn = new[]
                {
                    F.Function(t => 0),
                    F.Function(t => 10),
                }
            };

            target.Init(F);

            //熱移動量の取得
            var q_f_to_s = target.HeatOut[0].Get(0);
            var q_s_to_f = target.HeatOut[1].Get(0);

            //外気から壁への熱流は40W
            // 2.0 * 2.0 * (10 - 0) = 40 W
            Assert.AreEqual(40.0, q_f_to_s);
            Assert.AreEqual(-40.0, q_s_to_f);
        }

        /// <summary>
        /// 自然風による熱流の計算
        /// </summary>
        [TestMethod]
        public void NaturalConvectiveHeatTransferModuleTest1()
        {
            var F = new FunctionFactory();

            var target = new NaturalConvectiveHeatTransferModule
            {
                cValue = 1.5,
                S = 2.0,    //2.0m2
                TempIn = new IVariable<double>[]
                {
                    F.Function(t => 0),
                    F.Function(t => 10),
                }
            };

            target.Init(F);

            //熱流の取得
            var q_f_to_s = target.HeatOut[0].Get(0);
            var q_s_to_f = target.HeatOut[1].Get(0);

            //外気から壁への熱流は40W
            // a = 1.5 * (10 - 0) ^ 0.25 = 2.667419
            // 2.667419 * 2.0 * (10 - 0) = 53.348382 W
            Assert.AreEqual(53.348382, q_f_to_s, 0.000001);
            Assert.AreEqual(-53.348382, q_s_to_f, 0.000001);
        }

        /// <summary>
        /// 薄い壁が外気のみと接している場合
        /// </summary>
        [TestMethod]
        public void NaturalConvectiveHeatTransferModuleTest2()
        {
            var F = new FunctionFactory();

            var wall = new HeatCapacityModule
            {
                V = 0.1,
                cro = 1000,
                dt = 60 * 60,
            };

            var wind = new NaturalConvectiveHeatTransferModule
            {
                cValue = 1.5,
                S = 2.0,    //2.0m2
                TempIn = new IVariable<double>[] {
                    wall.TempOut,
                    F.Function(t => 15),
                }
            };

            wall.HeatIn.Add(wind.HeatOut[0]);

            wall.Init(F);
            wind.Init(F);

            var results = new double[100];
            for (int i = 0; i < 100; i++)
            {
                wall.Commit(i);
                wind.Commit(i);

                results[i] = wall.TempOut.Get(i);
            }

            //最終的には 15[K] に収束するはず。
            Assert.AreEqual(15.0, results[99], 0.1);
        }

        [TestMethod]
        public void UnsteadyWall1DModuleTest()
        {
            var F = new FunctionFactory();

            var target = new UnsteadyWallModule
            {
                cro = 1000.0,
                S = 2.0,    //2m2
                depth = 0.05,   //5cm
                n_slice = 5,    //スライス数=5
                Rambda = 0.2,    //熱伝導率 0.2 W/mK
                dt = 1,    //10秒
            };

            //壁体表面温度と流体温度を同じにする
            //※F.Memoryを挟まないと自己循環参照に陥るので注意
            target.TempIn = new [] {
                F.Memory(target.TempOut[0]),
                F.Memory(target.TempOut[1]),
            };

            //1000秒間だけ 1000J/s = 1000W I面加熱する
            target.HeatIn[0].Add(new Variable<double>(t => t < 1000 ? 1000 : 0));

            //1000秒間だけ 500J/s = 500W II面加熱する
            target.HeatIn[1].Add(new Variable<double>(t => t < 1000 ? 500 : 0));

            target.Init(F);

            var results = new double[2, 10000];
            for (int i = 0; i < 10000; i++)
            {
                target.Commit(i);
                foreach(var mem in target.TempIn.OfType<Memory>())
                {
                    mem.Commit(i);
                }

                results[0,i] = target.TempOut[0].Get(i);
                results[1,i] = target.TempOut[1].Get(i);
            }

            //最終的には 15[K] に収束するはず。
            Assert.AreEqual(15.0, results[0, 9999], 0.1);
            Assert.AreEqual(15.0, results[1, 9999], 0.1);
        }
    }
}
