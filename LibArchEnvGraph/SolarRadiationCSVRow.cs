using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Youworks.Text;

namespace LibArchEnvGraph
{
    [CSVFile(HasHeader = false, SkipRowCount = 6)]
    public class SolarRadiationCSVRow
    {
        [CSVHeader(Index = 0)]
        public string Date { get; set; }

        /// <summary>
        /// 日射量 MJ/m2/h
        /// </summary>
        [CSVHeader(Index = 1)]
        public double SolarRadiation { get; set; }
    }
}
