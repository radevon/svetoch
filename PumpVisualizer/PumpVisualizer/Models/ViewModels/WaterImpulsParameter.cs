using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PumpVisualizer
{
    public class WaterImpulsParameter
    {
        [Required]
        public string Identity { get; set; }
        [Required(ErrorMessage = "Поле должно содержать значение")]
        public double WaterKoef { get; set; }
        
        [Required(ErrorMessage = "Поле должно содержать значение")]
        [Range(0,Double.MaxValue,ErrorMessage="Значение должно быть >=0")]
        public double WaterVolume { get; set; }
        [Required(ErrorMessage = "Поле должно содержать значение")]
        public double CurrentWaterImpulse { get; set; }


        // Рассчитываю нач значение имп счетчика для вычисления текущего расхода воды
        public double StartWaterImpulse { get { return WaterVolume-CurrentWaterImpulse*WaterKoef; } }



    }
}