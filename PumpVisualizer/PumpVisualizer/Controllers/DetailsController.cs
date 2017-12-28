using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using PumpDb;
using System.Reflection;
using System.Globalization;
using PumpDb.Models;

namespace PumpVisualizer.Controllers
{
    [Authorize]
    public class DetailsController : Controller
    {
        //
        // GET: /Details/

        private VisualDataRepository repo_;
        private Logger loger;

        public DetailsController()
        {
            repo_ = new VisualDataRepository(ConfigurationManager.AppSettings["dbPath"]);
            loger=new Logger();
        }

        
        public ActionResult ViewParameters(int id)
        {
            Marker obj = repo_.GetMarkerById(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            
            return View(obj);
        }

        

        public ActionResult GetDataBySmallPeriod(string identity, string parameterGraph)
        {
            DateTime end = DateTime.Now;
            double interval = 0.5;
            int interval_table = 30;

            
            try
            {
                interval = Convert.ToDouble(ConfigurationManager.AppSettings["DataVisualInterval"], CultureInfo.GetCultureInfo("en-US").NumberFormat);
                interval_table = Convert.ToInt32(ConfigurationManager.AppSettings["DataTableInterval"], CultureInfo.GetCultureInfo("en-US").NumberFormat);
            }
            catch (Exception ex)
            {
                LogMessage message = new LogMessage()
                {
                    MessageDate = DateTime.Now,
                    MessageType = "error",
                    MessageText = ex.Message + ex.StackTrace
                };

                loger.LogToFile(message);
                loger.LogToDatabase(message);
                interval = 0.5;
                interval_table = 30;
            }
            
            
            DateTime start = end.AddHours(-interval);
            IEnumerable<ElectricAndWaterParamsExtended> data = repo_.GetExtendedPumpParamsByIdentityAndDate(identity, start, end);
            
            EWdata jsonData = new EWdata();
            jsonData.StartDate = start;
            jsonData.EndDate = end;
            IEnumerable<ElectricAndWaterParamsExtended> temp=data.OrderByDescending(x => x.RecvDate);
            
            jsonData.DataTable = temp.Where(x=>x.RecvDate>end.AddMinutes(-interval_table)).ToList();
            
            PropertyInfo infoprop = (typeof(ElectricAndWaterParamsExtended)).GetProperty(parameterGraph); 
            jsonData.DataGraph = temp.Select(x=>new DataForVisual(){RecvDate=x.RecvDate,Value=(double)infoprop.GetValue(x)}).ToList();
            
            return Content(JsonConvert.SerializeObject(jsonData),"application/json");
        }


        public JsonResult GetDataTest(string identity)
        {
            DateTime end = DateTime.Now;
            double interval = 0.5;
            int interval_table = 30;
            

            DateTime start = end.AddHours(-interval);
            IEnumerable<ElectricAndWaterParamsExtended> data = repo_.GetExtendedPumpParamsByIdentityAndDate(identity, start, end);
            
            return Json(data.ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}
