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
        public DateTime HourTime { get; set; }  // время часа суток

        public DateTime? RecvDateEnergy { get; set; }  // фактическое время получения данных по электроэнергии

        public double? TotalEnergy { get; set; }   // значение электроэнергии на дату RecvDateEnergy

        public DateTime? RecvDateWater { get; set; }  // фактическое время получения данных по воде

        public double? TotalWaterRate { get; set; }   // значение воды на дату RecvDateWater
    }
}
