using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace PumpDb
{

    // Параметры станции (электрич счетчика + водяного)
    public class ElectricAndWaterParams
    {
        public int Id { get; set; }
        // идентификатор объекта
       [JsonProperty("ident")]
        public string Identity { get; set; }
        // энергопотребление 
        [JsonProperty("enrg")]
        public double TotalEnergy { get; set; }
        // ток
        [JsonProperty("a1")]
        public double Amperage1 { get; set; }

        [JsonProperty("a2")]
        public double Amperage2 { get; set; }

        [JsonProperty("a3")]
        public double Amperage3 { get; set; }
        // Напряжение
        [JsonProperty("u1")]
        public double Voltage1 { get; set; }
        [JsonProperty("u2")]
        public double Voltage2 { get; set; }
        [JsonProperty("u3")]
        public double Voltage3 { get; set; }

        // Текущая мощность
        [JsonProperty("pw")]
        public double CurrentElectricPower { get; set; }

        // Общий расход воды в импульсах с прибора
        [JsonProperty("wtr")]
        public double TotalWaterRate { get; set; }

        // Дата снятия показаний
        [JsonProperty("dt")]
        public DateTime RecvDate { get; set; }

        // Ошибки
        [JsonProperty("err")]
        public string Errors { get; set; }
        // частота
        [JsonProperty("freq")]
        public double Frequrency { get; set; }

        // давление
        [JsonProperty("pres")]
        public double Presure { get; set; }

        // температура IGBT
        [JsonProperty("temp")]
        public double Temperature { get; set; }

    }
}
