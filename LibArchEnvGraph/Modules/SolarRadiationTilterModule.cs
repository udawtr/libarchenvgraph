using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 傾斜面日射量
    /// </summary>
    public class SolarRadiationTilterModule : BaseModule
    {
        const double toRad = Math.PI / 180.0;
        private readonly double shapeFactorToSky;
        private readonly double groundReturnRate;
        private readonly IVariable<ISolarPositionData> solarPositionSource;

        private readonly IVariable<double> directSolarRadiation;
        private readonly IVariable<double> diffusedSolarRadiation;

        /// <summary>
        /// 入射角の方向余弦
        /// </summary>
        private IVariable<double> directionCosine;

        /// <summary>
        /// 傾斜面直達日射量
        /// </summary>
        public LinkVariable<double> ID { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 傾斜面拡散日射量
        /// </summary>
        public LinkVariable<double> Id { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 傾斜面全天日射量
        /// </summary>
        public LinkVariable<double> Iw { get; private set; } = new LinkVariable<double>();

        private bool Inited = false;

        public SolarRadiationTilterModule(double shapeFactorToSky, double groundReturnRate, IVariable<double> directSolarRadiation, IVariable<double> diffusedSolarRadiation, IVariable<ISolarPositionData> solarPosition, IVariable<double> directionCosine)
        {
            this.shapeFactorToSky = shapeFactorToSky;
            this.groundReturnRate = groundReturnRate;
            this.directSolarRadiation = directSolarRadiation;
            this.diffusedSolarRadiation = diffusedSolarRadiation;
            this.solarPositionSource = solarPosition;
            this.directionCosine = directionCosine;
        }

        public override void Init(FunctionFactory F)
        {
            //傾斜面直達日射量
            ID.Link = F.Multiply(directionCosine, directSolarRadiation);

            //傾斜面拡散日射量
            Id.Link = new TiltDiffusedSolarRadiation
            {
                ShapeFactorToSky = shapeFactorToSky,
                GroundReturnRate = groundReturnRate,
                SolarPosition = solarPositionSource,
                DirectSolarRadiation = directSolarRadiation,
                DiffusedSolarRadiation = diffusedSolarRadiation
            };

            //傾斜面全天日射量
            Iw.Link = F.Add(ID, Id);

            Inited = true;
        }
    }
}
