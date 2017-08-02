using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public class FunctionFactory
    {
        public virtual IVariable<double> Add(IVariable<double> a, IVariable<double> b)
        {
            return new Functions.Add(a, b);
        }

        public virtual IVariable<double> Subtract(IVariable<double> a, IVariable<double> b)
        {
            return new Functions.Subtract(a, b);
        }

        public virtual IVariable<double> Invert(IVariable<double> var_in)
        {
            return new Functions.Invert(var_in);
        }

        public virtual IVariable<double> StefanBolzmann(double F12, IVariable<double> T1, IVariable<double> T2, double e1 = 0.9, double e2 = 0.9)
        {
            return new Functions.StefanBolzmann
            {
                e1 = e1,
                e2 = e2,
                F12 = F12,
                T1 = T1,
                T2 = T2
            };
        }

        public virtual IVariable<double> Fourier(double dx, double rambda, double S, IVariable<double> T1, IVariable<double> T2)
        {
            return new Functions.Fourier
            {
                dx = dx,
                Rambda = rambda,
                S = S,
                T1 = T1,
                T2 = T2
            };
        }

        public virtual IVariable<double> NewtonCooling(double S, IVariable<double> Ts, IVariable<double> Tf, IVariable<double> alpha_c)
        {
            return new Functions.NewtonCooling
            {
                alpha_c = alpha_c,
                S = S,
                Ts = Ts,
                Tf = Tf
            };
        }

        public virtual IVariable<double> VentilationHeatTransfer(double V, IVariable<double> Ts, IVariable<double> Tf, double c_air = 1.007, double ro_air= 1.024)
        {
            return new Functions.VentilationHeatTransfer
            {
                c_air = c_air,
                ro_air = ro_air,
                Ts = Ts,
                Tf = Tf,
                V = V,
            };
        }

        public virtual IVariable<double> Multiply(IVariable<double> a, IVariable<double> b)
        {
            return new Functions.Multiply(a, b);
        }

        public virtual IVariable<double> DirectSolarRadiation(int tickTime, int beginDay, int days, IVariable<double> solarRadiation, IVariable<ISolarPositionData> solarPosition)
        {
            return new Functions.DirectSolarRadiation(tickTime, beginDay, days, solarRadiation, solarPosition);
        }

        public virtual IVariable<ISolarPositionData> SolarPosition(double Lat, double L, int tickTime, int beginDay, int days)
        {
            return new Functions.SolarPosition(Lat, L, tickTime, beginDay, days);
        }

        public virtual IVariable<double> Interpolate(double[] original, int scaleFactor)
        {
            return new Functions.DataInterpolator(original, scaleFactor);
        }

        public virtual IGateVariable<double> HeatMemory(List<IVariable<double>> heatIn)
        {
            return new Functions.HeatMemory
            {
                HeatIn = heatIn
            };
        }

        public virtual IVariable<double> SplitQGT(IVariable<double> QGT, double area, double solarTransmissionDistributionRate)
        {
            return new Functions.AbsorptionSolarRadiationSplitter(QGT, area, solarTransmissionDistributionRate);
        }

        public virtual IVariable<double> Temperature(double cro, double V, IVariable<double> heatIn)
        {
            return new Functions.HeatToTemp
            {
                cro = cro,
                V = V,
                U = heatIn
            };
        }
    }
}
