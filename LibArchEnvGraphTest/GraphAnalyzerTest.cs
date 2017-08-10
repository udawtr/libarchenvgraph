using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibArchEnvGraph;
using LibArchEnvGraph.Modules;

namespace LibArchEnvGraphTest
{
    [TestClass]
    public class GraphAnalyzerTest
    {
        public class DummyModule : BaseModule
        {
            public IVariable<double> Test { get; set; }

            public LinkVariable<double> LinkTest { get; set; }
        }

        [TestMethod]
        public void GetVariableListTest()
        {
            var module = new DummyModule();
            var varList = GraphAnalyzer.GetVariableList(module);

            Assert.AreEqual(2, varList.Count);
            Assert.IsTrue(varList.Any(x => x.Name == "Test"));
            Assert.IsTrue(varList.Any(x => x.Name == "LinkTest"));
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void CheckVariableLoopTest()
        {
            //自己変数参照
            var module1 = new DummyModule { Label = "自己参照モジュール1" };
            var module2 = new DummyModule { Label = "自己参照モジュール2" };
            var module3 = new DummyModule { Label = "自己参照モジュール3" };

            module1.LinkTest = new LinkVariable<double>();
            module2.LinkTest = new LinkVariable<double>();
            module3.LinkTest = new LinkVariable<double>();

            (module1.LinkTest as LinkVariable<double>).Link = module2.LinkTest;
            (module2.LinkTest as LinkVariable<double>).Link = module3.LinkTest;
            (module3.LinkTest as LinkVariable<double>).Link = module1.LinkTest;

            GraphAnalyzer.CheckVariableLoop(module1);
        }


        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void CheckVariableLoopTest2()
        {
            var module4 = new DummyModule();
            module4.LinkTest = new LinkVariable<double>();
            module4.Test = module4.LinkTest;
            GraphAnalyzer.CheckVariableLoop(module4);
        }

        [TestMethod]
        public void CheckVariableNoLoopTest()
        {
            var module1 = new DummyModule();
            module1.Test = new LinkVariable<double>();

            var module2 = new DummyModule();
            module2.Test = new Variable<double>(10);

            (module1.Test as LinkVariable<double>).Link = module2.Test;

            GraphAnalyzer.CheckVariableLoop(module1);
        }
    }
}
