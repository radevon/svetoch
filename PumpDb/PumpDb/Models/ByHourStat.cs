using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDb
{
    // класс для вывода статистики почасовой за определенный день или подневной статистики за месяц
    public class ByHourStat
    {
        public DateTime HourTime { get; set; }

        public DateTime? RecvDate { get; set; }

        public double? TotalEnergy { get; set; }

        public double? TotalWaterRate { get; set; }
    }
}
