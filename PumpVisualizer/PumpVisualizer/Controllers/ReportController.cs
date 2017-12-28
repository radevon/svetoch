using PumpDb;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace PumpVisualizer.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {

        private VisualDataRepository repo_;
        private Logger loger;

        public ReportController()
        {
            repo_ = new VisualDataRepository(ConfigurationManager.AppSettings["dbPath"]);
            loger = new Logger();
            CultureInfo cult = CultureInfo.CreateSpecificCulture("ru-RU");
            cult.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = cult;
        }
        //
        // GET: /Report/

        public ActionResult Index()
        {
            List<Marker> objects = repo_.GetAllMarkers().OrderBy(x => x.Address).ToList();
            ReportSource rc = new ReportSource();
            rc.Adresses = objects.Select(x => new SelectListItem() { Text = x.Address, Value = x.Identity }).ToList();

            return View("ReportIndex", rc);
        }

        [HttpPost]
        public ActionResult GenerateReport(ReportSource rc, String ReportType)
        {
            if (!ModelState.IsValid)
            {
                return View("ReportIndex", rc);
            }


            Marker m = repo_.GetMarkerByIdentity(rc.Identity);
            if (m != null)
            {
                ViewBag.ObjectName = m.Address;
            }
            else
            {
                ViewBag.ObjectName = "-";
            }

            if (ReportType.IndexOf("Месячный", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                List<ByHourStat> values = repo_.GetStatByDays(rc.DateParam, rc.Identity).ToList();
                return View("StatByMonth", values);
            }
            else
            {
                List<ByHourStat> values = repo_.GetStatByHour(rc.DateParam, rc.Identity).ToList();
                return View("StatByDay", values);
            }


        }

    }
}
