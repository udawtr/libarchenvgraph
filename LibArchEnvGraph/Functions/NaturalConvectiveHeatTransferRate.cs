﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 自然対流熱伝達率 [-] を求める
    /// </summary>
    /// <seealso cref="NewtonCooling"/>
    public class NaturalConvectiveHeatTransferRate : BaseVariable<double>
    {
        /// <summary>
        /// 固体(壁体)/流体(空気)の表面温度 [K]
        /// </summary>
        public IVariable<double>[] TempIn { get; set; }

        /// <summary>
        /// c値(自然対流作用の程度)
        /// </summary>
        public double cValue { get; set; }

        /// <summary>
        /// c値: 暖房時の天井面または冷房時の床表面(自然対流作用大)
        /// </summary>
        public const double cValueHeatingCeilOrCoolingFloor = 2.67;

        /// <summary>
        /// c値: 暖房時の床表面または冷房時の天井面(自然対流作用小)
        /// </summary>
        public const double cValueHeatingFloorOrCoolingCeil = 0.755;

        /// <summary>
        /// c値: 垂直壁表面
        /// </summary>
        public const double cValueVerticalWallSurface = 1.98;

        public override double Update(int t)
        {
            var dT = Math.Abs(TempIn[0].Get(t) - TempIn[1].Get(t));
            var alpha_c = cValue * Math.Pow(dT, 0.25);

            System.Diagnostics.Debug.Assert(!Double.IsNaN(alpha_c));

            return alpha_c;
        }
    }
}
