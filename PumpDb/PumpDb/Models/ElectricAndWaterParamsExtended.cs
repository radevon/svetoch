using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PumpDb.Models
{
    public class ElectricAndWaterParamsExtended:ElectricAndWaterParams
    {
        // коэффициент пересчета импульсов в расход воды
        [JsonProperty("koef")]
        public double WaterKoef { get; set; }

        // нач значение расхода воды
        [JsonProperty("wtr0")]
        public double WaterStartValue { get; set; }

        // расход воды на основе расчета по коэффициенту
        [JsonProperty("wtrc")]
        public double WaterCalculated
        {
            get
            {
                return WaterStartValue + WaterKoef * TotalWaterRate;
            }
        }

        // Отношение показаний электроэнергии к воде  кв.ч/м3
        [JsonProperty("en2wt")]
        public double WaterEnergy
        {
            get { return this.WaterCalculated != 0 ? this.TotalEnergy / this.WaterCalculated : 0; }
        }
    }
}
