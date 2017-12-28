using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDb
{
    public class PumpNominalParam
    {
        [Required]
        public string Identity { get; set; }

        [Range(0.0, 100.0, ErrorMessage = "Коэффициент потери мощности в режиме сухого хода должен быть в пределах от 1% до 100%")]
        [Required(ErrorMessage = "Поле max коэффициент потери мощности в режиме сухого хода должно содержать значение")]
        
        public double KoefUndo { get; set; }

        [Required(ErrorMessage = "Поле коэффициент перегрузки должно содержать значение")]
        [Range(0.0,100.0,ErrorMessage="Коэффициент перегрузки должен быть в пределах от 1% до 100%")]
        
        public double KoefOver { get; set; }

        [Required(ErrorMessage = "Поле номинальная мощность должно содержать значение")]
        [Range(0.0, 1000.0, ErrorMessage = "Номинальная мощность должна быть в пределах от 1 до 1000 кВт")]
        
        public double NominalPower { get; set; }
    }
}
