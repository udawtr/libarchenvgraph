using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibArchEnvGraph;
using LibArchEnvGraph.Modules;
using System.Collections.Generic;

namespace LibArchEnvGraphTest
{
    [TestClass]
    public class GraphAnalyzerTest
    {
        public class DummyModule : BaseModule
        {
            public IVariable<double> TestIn { get; set; }

            public LinkVariable<double> LinkTestOut { get; set; }

            public IVariable<double>[] ArrayTestIn { get; set; }

            public LinkVariable<double>[] ArrayLinkTestOut { get; set; }

            public IList<IVariable<double>> ListTestIn { get; set; }

            public IList<LinkVariable<double>> ListLinkTestOut { get; set; }
        }

        [TestMethod]
        public void GetVariableListTest()
        {
            var module = new DummyModule();
            var varList = GraphAnalyzer.GetVariableList(module);

            Assert.AreEqual(6, varList.Count);
            Assert.IsTrue(varList.Any(x => x.Name == "TestIn"));
            Assert.IsTrue(varList.Any(x => x.Name == "LinkTestOut"));
            Assert.IsTrue(varList.Any(x => x.Name == "ArrayTestIn"));
            Assert.IsTrue(varList.Any(x => x.Name == "ArrayLinkTestOut"));
            Assert.IsTrue(varList.Any(x => x.Name == "ListTestIn"));
            Assert.IsTrue(varList.Any(x => x.Name == "ListLinkTestOut"));
        }

        /// <summary>
        /// 間接参照ループ
        /// 
        ///              +---------+                                   +---------+                                         +---------+
        ///              |         |                                   |         |                                         |         |
        /// +--TestIn -->+ Module1 +-->LinkTestOut-----ListTestIn[0]-->+ Module2 +-->ArrayLinkTestOut[1]--ArrayTestIn[0]-->+ Module3 +-->ListLinkTestOut[0]---+
        /// |            |         |                                   |         |                                         |         |                        |
        /// |            +---------+                                   +---------+                                         +---------+                        |
        /// |                                                                                                                                                 |
        /// +-------------------------------------------------------------------------------------------------------------------------------------------------+
        /// </summary>
        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void CheckVariableLoopTest()
        {
            //自己変数参照
            var module1 = new DummyModule();
            var module2 = new DummyModule();
            var module3 = new DummyModule();

            module1.LinkTestOut = new LinkVariable<double>();
            module2.ArrayLinkTestOut = new LinkVariable<double>[2]
            {
                new LinkVariable<double>(),
                new LinkVariable<double>()
            };
            module3.ListLinkTestOut = new List<LinkVariable<double>>
            {
                new LinkVariable<double>()
            };

            module1.TestIn = module3.ListLinkTestOut[0];
            module2.ListTestIn = new List<IVariable<double>>
            {
                module1.LinkTestOut
            };
            module3.ArrayTestIn = new IVariable<double>[]
            {
                module2.ArrayLinkTestOut[1]
            };

            //内部結合
            (module1.LinkTestOut as LinkVariable<double>).Link = module1.TestIn;
            (module2.ArrayLinkTestOut[1] as LinkVariable<double>).Link = module2.ListTestIn[0];
            (module3.ListLinkTestOut[0] as LinkVariable<double>).Link = module3.ArrayTestIn[0];

            //名前辞書
            module1.Label = "M1";
            module2.Label = "M2";
            module3.Label = "M3";

            module1.LinkTestOut.Label = "M1.LinkTestOut";
            module2.ArrayLinkTestOut[0].Label = "M2.ArrayLinkTestOut[0]";
            module2.ArrayLinkTestOut[1].Label = "M2.ArrayLinkTestOut[1]";
            module3.ListLinkTestOut[0].Label = "M3.ListLinkTestOut[0]";

            GraphAnalyzer.CheckVariableLoop(module1);
        }

        /// <summary>
        /// 自己参照ループ(出力の参照が入力に設定されている;入力設定誤り)
        /// </summary>

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void CheckVariableLoopTest2()
        {
            var module4 = new DummyModule();
            module4.LinkTestOut = new LinkVariable<double>();
            module4.TestIn = module4.LinkTestOut;
            GraphAnalyzer.CheckVariableLoop(module4);
        }

        /// <summary>
        /// ループなし
        /// </summary>
        [TestMethod]
        public void CheckVariableNoLoopTest()
        {
            var module1 = new DummyModule() { Label = "M1" };
            var module2 = new DummyModule() { Label = "M2" };

            module1.LinkTestOut = new LinkVariable<double>();
            module2.LinkTestOut = new LinkVariable<double>();

            module1.TestIn = module2.LinkTestOut;

            GraphAnalyzer.CheckVariableLoop(module1);
        }
    }
}
