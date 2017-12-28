using System.Globalization;
using System.Reflection;
using PumpDb;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PumpDb.Models;
using Newtonsoft.Json;

namespace PumpVisualizer.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {

        private VisualDataRepository repo_;
        private Logger loger;
        //
        // GET: /History/

        public HistoryController()
        {
            repo_ = new VisualDataRepository(ConfigurationManager.AppSettings["dbPath"]);
            loger = new Logger();
        }

        public ActionResult History(int id)
        {
            Marker obj = repo_.GetMarkerById(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            return View(obj);
        }


        public ActionResult GetByPeriod(string identity, DateTime start_, string parameterGraph = "Amperage1")
        {
            DateTime end=start_.AddHours(1);
            IEnumerable<ElectricAndWaterParamsExtended> data = repo_.GetExtendedPumpParamsByIdentityAndDate(identity, start_, end);
            EWdata jsonData = new EWdata();
            jsonData.StartDate = start_;
            jsonData.EndDate = end;
            IEnumerable<ElectricAndWaterParamsExtended> temp = data.OrderBy(x => x.RecvDate);
            jsonData.DataTable = temp.ToList();
            PropertyInfo infoprop = (typeof(ElectricAndWaterParamsExtended)).GetProperty(parameterGraph);
            jsonData.DataGraph = temp.Select(x => new DataForVisual() { RecvDate = x.RecvDate, Value = (double)infoprop.GetValue(x) }).ToList();
            return Content(JsonConvert.SerializeObject(jsonData), "application/json");
        }


       
    }
}
