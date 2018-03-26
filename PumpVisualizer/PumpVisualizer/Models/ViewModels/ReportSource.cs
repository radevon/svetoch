using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PumpVisualizer
{
    public class ReportSource
    {
        public ReportSource()
        {
            DateParam = DateTime.Now;
        }
     

        [DataType(DataType.Date,ErrorMessage="Значение поля должно быть эквивалентно дате в формате 'дд.мм.гггг'")]
        [DisplayFormat(DataFormatString = "{0:dd'.'MM'.'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name="Отчетная дата")]
        [Required]
        public DateTime DateParam { get; set; }

        [Display(Name = "Адрес объекта")]
        [Required]
        public string Identity { get; set; }

        public List<SelectListItem> Adresses { get; set; }
    }
}