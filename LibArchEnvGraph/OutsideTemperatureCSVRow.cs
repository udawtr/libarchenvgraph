using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Youworks.Text;

namespace LibArchEnvGraph
{
    [CSVFile(HasHeader = false, SkipRowCount = 6)]
    public class OutsideTemperatureCSVRow
    {
        [CSVHeader(Index = 0)]
        public string Date { get; set; }

        /// <summary>
        /// 外気温 [℃]
        /// </summary>
        [CSVHeader(Index = 1)]
        public double Temp { get; set; }
    }
}
