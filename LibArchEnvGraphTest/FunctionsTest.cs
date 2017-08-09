using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibArchEnvGraph.Functions;
using LibArchEnvGraph;

namespace LibArchEnvGraphTest
{
    [TestClass]
    public class FunctionsTest
    {
        [TestMethod]
        public void AddTest()
        {
            Add target = new Add(new Variable<double>(1), new Variable<double>(2));
            Assert.AreEqual(3.0, target.Get(0));
        }

        [TestMethod]
        public void ConcatTest()
        {
            Concat target1 = new Concat(new[] { new Variable<double>(1), new Variable<double>(2) });
            Assert.AreEqual(3.0, target1.Get(0));

            Concat target2 = new Concat(new Variable<double>(1), new Variable<double>(2));
            Assert.AreEqual(3.0, target2.Get(0));
        }

        [TestMethod]
        public void SubtractTest()
        {
            Subtract target = new Subtract(new Variable<double>(10), new Variable<double>(2));
            Assert.AreEqual(8.0, target.Get(0));
        }

        [TestMethod]
        public void MultiplyTest()
        {
            Multiply target = new Multiply(new Variable<double>(10), new Variable<double>(2));
            Assert.AreEqual(20.0, target.Get(0));
        }

        [TestMethod]
        public void InvertTest()
        {
            Invert target = new Invert(new Variable<double>(10));
            Assert.AreEqual(-10.0, target.Get(0));
        }

        [TestMethod]
        public void DataInterploatorTest()
        {
            var target = new DataInterpolator(new double[] { 0.0, 8.0}, 4);

            Assert.AreEqual(0.0, target.Get(0));
            Assert.AreEqual(2.0, target.Get(1));
            Assert.AreEqual(4.0, target.Get(2));
            Assert.AreEqual(6.0, target.Get(3));
            Assert.AreEqual(8.0, target.Get(4));
            Assert.AreEqual(6.0, target.Get(5));
            Assert.AreEqual(4.0, target.Get(6));
            Assert.AreEqual(2.0, target.Get(7));
        }

        [TestMethod]
        public void NewtonCoolingTest()
        {
            var target = new NewtonCooling();
            target.alpha_c = new Variable<double>(1.23);
            target.S = 4.0;
            target.Ts = new Variable<double>(20.0);
            target.Tf = new Variable<double>(10.0);

            var dT = 20.0 - 10.0;
            var q = 1.23 * dT * 4.0;

            Assert.AreEqual(q, target.Get(0));
        }

        [TestMethod]
        public void StefanBolzmannTest()
        {
            var target = new StefanBolzmann();
            target.e1 = 0.85;
            target.e2 = 0.90;
            target.F12 = 0.2;
            target.T1 = new Variable<double>(20.0 + 273.15);
            target.T2 = new Variable<double>(10.0 + 273.15);

            var dU = 0.85 * 0.90 * 0.2 * 5.67 * (Math.Pow((20.0 + 273.15) / 100, 4.0) - Math.Pow((10.0 + 273.15) / 100, 4.0));

            Assert.AreEqual(dU, target.Get(0));
        }

        [TestMethod]
        public void FourierTest()
        {
            var target = new Fourier();
            target.Rambda = 0.1;
            target.dx = 0.2;
            target.S = 10.0;
            target.T1 = new Variable<double>(20.0);
            target.T2 = new Variable<double>(10.0);

            var dU = -1.0 * 0.1 * 10.0 * (20.0 - 10.0) / 0.2;

            Assert.AreEqual(dU, target.Get(0));
        }

        [TestMethod]
        public void MemoryTest()
        {
            Memory m = new Memory();
            m.DataIn = new Variable<double>(t => t + 1);
            m.Set(10);

            //初期値確認
            Assert.AreEqual(10.0, m.Get(0));

            m.Commit(0);
            Assert.AreEqual(0.0, m.Get(0));

            m.Commit(1);
            Assert.AreEqual(1.0, m.Get(1));

            m.Commit(2);
            Assert.AreEqual(2.0, m.Get(2));

            m.Commit(3);
            Assert.AreEqual(3.0, m.Get(3));
        }

        [TestMethod]
        public void HeatToTempTest()
        {
            HeatToTemp T = new HeatToTemp();
            T.cro = 1234;   // kJ/m3K
            T.V = 567;      //m3
            T.Heat = new Variable<double>(100);

            var expected = (100.0 / (1234 * 1000 * 567));
            Assert.AreEqual(expected, T.Get(0));
        }
    }
}
