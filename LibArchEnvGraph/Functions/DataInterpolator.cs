﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    public class DataInterpolator : IVariable<double>
    {
        private double[] data;

        public string Label { get; set; }

        public double Get(int n)
        {
            return data[n];
        }

        public DataInterpolator(double[] original, int scaleFactor)
        {
            Init(original, scaleFactor);
        }

        /// <summary>
        /// 気象データの補完
        /// </summary>
        /// <param name="meteorologicalData">気象データ</param>
        /// <returns>補完された気象データ</returns>
        public void Init(double[] original, int scaleFactor)
        {
            this.data = new double[original.Length * scaleFactor];

            int off = 0;
            for (int i = 0; i < original.Length; i++)
            {
                var dataNext = i >= original.Length - 1 ? original[0] : original[i + 1];
                var data = original[i];

                for (int j = 0; j < scaleFactor; j++, off++)
                {
                    double alpha =  ((double)j) / scaleFactor;
                    this.data[off] = (1.0 - alpha) * (data) + alpha * (dataNext);
                }
            }
        }
    }
}
