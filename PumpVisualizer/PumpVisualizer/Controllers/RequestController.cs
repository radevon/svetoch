using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PumpDb;

namespace PumpVisualizer.Controllers
{
    [Authorize]
    public class RequestController : Controller
    {
        //
        // GET: /Request/
        private VisualDataRepository repo_data;
        private Logger loger = new Logger();

        public RequestController()
        {
            repo_data = new VisualDataRepository(ConfigurationManager.AppSettings["dbPath"]);
        }

        public ActionResult RequestParam()
        {
            List<Marker> markers = repo_data.GetAllMarkers().OrderBy(x => x.Address).ToList();
            return View("Request", markers);
        }

    }

}
