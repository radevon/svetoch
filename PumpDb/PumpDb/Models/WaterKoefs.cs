using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDb
{
    public class WaterKoefs
    {
        [Required]
        public string Identity { get;  set; }
        [Required(ErrorMessage = "Поле должно содержать значение")]
        public double WaterKoef { get; set; }
        [Required(ErrorMessage = "Поле должно содержать значение")]
        public double WaterStartValue { get; set; }
    }
}
