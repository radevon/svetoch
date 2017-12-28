using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDb
{
    public class Marker
    {
        public int MarkerId { get; set; }

        public string Address { get; set; }
        public decimal Px { get; set; }
        public decimal Py { get; set; }
        public string Identity { get; set; }

    }
}
