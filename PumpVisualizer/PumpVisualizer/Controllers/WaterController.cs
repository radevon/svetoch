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
    public class WaterController : Controller
    {
        private VisualDataRepository repo_;
        private Logger loger;

        public WaterController()
        {
            repo_ = new VisualDataRepository(ConfigurationManager.AppSettings["dbPath"]);
            loger=new Logger();
            CultureInfo cult = CultureInfo.CreateSpecificCulture("ru-RU");
            cult.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = cult;

        }


        //
        // GET: /Water/

        public ActionResult ViewWaterParams(string identity,string ReturnUrl)
        {
            try
            {
                WaterKoefs koef = repo_.GetKoefsByIdentity(identity);
                ViewBag.ReturnUrl = ReturnUrl;
                return View(koef);
            }
            catch (Exception ex)
            {
                LogMessage message = new LogMessage()
                {
                    Id = -1,
                    MessageDate = DateTime.Now,
                    UserName = User.Identity.Name,
                    MessageType = "error",
                    MessageText = ex.Message + ex.StackTrace
                };

                loger.LogToFile(message);
                loger.LogToDatabase(message);

                return Content("Ошибка при получении параметров счетчика воды с идентификатором: " + identity);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveWaterParams(WaterKoefs koefs, string ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = ReturnUrl;
                TempData["message"] = "Проверьте правильность заполнения полей!";
                return View("ViewWaterParams", koefs);
            }
            try
            {
                int res = repo_.AddOrUpdateKoef(koefs);
                if (res > 0)
                {
                    TempData["message"] = "Значения успешно сохранены!";
                }
                else
                {
                    TempData["message"] = "Не удалось изменить параметры счетчика!";
                }
                return RedirectToAction("ViewWaterParams", "Water", new { identity = koefs.Identity, ReturnUrl = ReturnUrl });
            }
            catch (Exception ex)
            {
                 LogMessage message = new LogMessage()
                {
                    Id = -1,
                    MessageDate = DateTime.Now,
                    UserName = User.Identity.Name,
                    MessageType = "error",
                    MessageText = ex.Message + ex.StackTrace
                };

                loger.LogToFile(message);
                loger.LogToDatabase(message);

                return Content("Ошибка при обновлении параметров счетчика воды с идентификатором: " + koefs.Identity);
            }
            
        }


        public ActionResult ViewPumpParams(string identity, string ReturnUrl)
        {       
            try
            {
                PumpNominalParam param = repo_.GetNominalParamsByIdentity(identity);
                if (param == null)
                {
                    param = new PumpNominalParam() { Identity=identity};
                }
                ViewBag.ReturnUrl = ReturnUrl;
                return View(param);
            }
            catch (Exception ex)
            {
                LogMessage message = new LogMessage()
                {
                    Id = -1,
                    MessageDate = DateTime.Now,
                    UserName = User.Identity.Name,
                    MessageType = "error",
                    MessageText = ex.Message + ex.StackTrace
                };

                loger.LogToFile(message);
                loger.LogToDatabase(message);

                return Content("Ошибка при получении номинальных параметров работы насоса: " + identity);
            }
            
        } 

        [HttpPost]
        public ActionResult SavePumpParams(PumpNominalParam param, string ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = ReturnUrl;
                TempData["message"] = "Проверьте правильность заполнения полей!";
                return View("ViewPumpParams", param);
            }
            try
            {
                int res = repo_.AddOrUpdateNomParams(param);
                if (res > 0)
                {
                    TempData["message"] = "Значения успешно сохранены!";
                }
                else
                {
                    TempData["message"] = "Не удалось изменить параметры счетчика!";
                }
                return RedirectToAction("ViewPumpParams", "Water", new { identity = param.Identity, ReturnUrl = ReturnUrl });
            }
            catch (Exception ex)
            {
                LogMessage message = new LogMessage()
                {
                    Id = -1,
                    MessageDate = DateTime.Now,
                    UserName = User.Identity.Name,
                    MessageType = "error",
                    MessageText = ex.Message + ex.StackTrace
                };

                loger.LogToFile(message);
                loger.LogToDatabase(message);

                return Content("Ошибка при обновлении номинальных параметров насоса с идентификатором: " + param.Identity);
            }
        }

        [HttpPost]
        public JsonResult PumpNominalParameters(string identity)
        {
                PumpNominalParam param = repo_.GetNominalParamsByIdentity(identity);
                if (param == null)
                    param = new PumpNominalParam()
                    {
                        Identity = identity,
                        KoefOver = 0,
                        KoefUndo = 0,
                        NominalPower = 0
                    };
                return Json(param);
        }
       
    }
}
