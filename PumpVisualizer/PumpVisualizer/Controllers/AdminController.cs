using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Web.Security;

using WebMatrix.WebData;
using System.ComponentModel.DataAnnotations;
using PumpDb;

namespace PumpVisualizer.Controllers
{
    [Authorize(Roles = "administrators")]
    public class AdminController : Controller
    {

        
        //1
        // GET: /Admin/
        private LocalDbRepository repo;
        private VisualDataRepository repo_data;
        private Logger loger = new Logger();
        private ExternalRepository repo_ext;

        public AdminController()
        {
            repo=new LocalDbRepository();
            repo_data = new VisualDataRepository(ConfigurationManager.AppSettings["dbPath"]);
            repo_ext = new ExternalRepository(ConfigurationManager.AppSettings["dbPath"]);
            //Thread.CurrentThread.CurrentCulture.
        }

        public ActionResult Settings()
        {
            return View();
        }

        
        public ActionResult DbStatus()
        {
            DbStatistic stat = this.getStat();
            ViewBag.hasPath = stat.HasAppKey;
            ViewBag.dbStatus = stat.DbStatus;
            ViewBag.dbpath = stat.DbPAth;
            return PartialView();
        }

        [HttpPost]
        public ActionResult RecreateDb()
        {
            DbStatistic stat = this.getStat();
            if (stat.HasAppKey)
            {
                Database db=new Database(stat.DbPAth);
                MethodResult res=db.CreateIfNotExistDataBase();
                if (res.isSuccess)
                {
                    if (db.CreateAllTablesIfNotExist())
                    {
                        TempData["message"] = "База данных перегенерирована! Несуществовавшие объекты созданы!";
                    }
                    else
                    {
                        TempData["message"] = "База данных перегенерирована! Но некоторые объекты не были созданы!";
                    }

                }
                else
                {
                    TempData["message"] = "При перегенерации базы возникли ошибки!\n"+res.Message; 
                }
                
            }
            else
            {
                TempData["message"] = "Ошибка! Не определен путь к базе данных в файле конфигурации";
            }
            return RedirectToAction("DbStatus");
        }

        public ActionResult OptimizeDB()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Vacuum()
        {
            Database db = new Database(ConfigurationManager.AppSettings["dbPath"]);
            if (db.IsDataBaseExist())
            {
                MethodResult res=db.Vacuum();
                if (res.isSuccess)
                {
                    return Content("<div class='text-success'>Выполнено успешно!</div>");
                }else
                {
                    return Content("<div class='text-danger'>Ошибки:</div> " + res.Message);
                }
            }else
                return Content("База данных не обнаружена " +db.GetFilePath);
        }

        [HttpPost]
        public ActionResult DeleteData(int monthAgo)
        {
            DateTime oldDate = DateTime.Now.AddMonths(-monthAgo);
            MethodResult res = repo_ext.DeleteAllByDate(DateTime.Now.AddYears(-15), oldDate);
            if (res.isSuccess)
            {
                return Content("<div class='text-success'>Выполнено успешно! Записи удалены старше " + oldDate.ToString() + "</div>");
            }
            else
            {
                return Content("<div class='text-danger'>Ошибки:</div> " + res.Message);
            }
        }
        public ActionResult CreateNewUser()
        {
            NewUser user=new NewUser();
            return PartialView(user);
        }

        [HttpPost]
        public ActionResult CreateNewUser(NewUser user)
        {
            if (ModelState.IsValid)
            {
                
                if(WebSecurity.UserExists(user.UserName))
                    ModelState.AddModelError("","Пользователь с таким именем уже существует!");
                else
                {
                    
                    try
                    {
                        WebSecurity.CreateUserAndAccount(user.UserName, user.Password, new { Description = this.Reverse(user.Password) });
                        return Content(String.Format("<h4 class='text-success'>Пользователь '{0}'создан</h4>",user.UserName));
                    }
                    catch (Exception)
                    {
                        
                        ModelState.AddModelError("", "Не удалось создать пользователя. Надо разбираться в проблеме с разработчиком"); 
                    }
                    
                }
            }
            return PartialView(user);
        }

        public ActionResult ViewUsers()
        {
            List<UserInfoMembership> userInfo = repo.GetUserInformation();
            return PartialView(userInfo);
        }

        [HttpPost]
        public ActionResult DeleteAccount(string UserName)
        {
            try
            {
                if (!WebSecurity.UserExists(UserName))
                    return Content(String.Format("<h4 class='text-danger'>Не найден пользователь '{0}'</h4>", UserName));

                string[] userRoles = Roles.GetRolesForUser(UserName);
                if (userRoles.Count() > 0)
                {
                    Roles.RemoveUserFromRoles(UserName, userRoles);
                }
                SimpleMembershipProvider provider = Membership.Provider as SimpleMembershipProvider;
                bool deleteResult = provider.DeleteAccount(UserName);
                deleteResult = deleteResult&&provider.DeleteUser(UserName, true);
                if (deleteResult)
                {
                    return Content(String.Format("<h4 class='text-success'>Аккаунт пользователя '{0}' успешно удален</h4>", UserName));
                }
                else
                {
                    return Content(String.Format("<h4 class='text-danger'>Аккаунта пользователя '{0}' возможно не удалось удалить полностью</h4>", UserName));
                }
            }
            catch (Exception)
            {
                return Content(String.Format("<h4 class='text-danger'>Возникли ошибки в процессе удаления аккаунта пользователя '{0}'</h4>", UserName));
            }
            
        }

        public ActionResult ViewLog(int CurrentPage=0)
        {
            LogInfoVM log=new LogInfoVM();
            LogDbClass db=new LogDbClass();
            
            try
            {
                if (ConfigurationManager.AppSettings["logFilePath"] != null)
                {
                    log.LogFilePath = ConfigurationManager.AppSettings["logFilePath"];
                }
                else
                {
                    log.LogFilePath = "не задан";
                }
                string dbPath=ConfigurationManager.AppSettings["dbPath"];
                log.CurrentPage = CurrentPage;
                log.TotalCount = db.GetLogDataCount(dbPath);
                log.AllDbLogs = new List<LogMessage>();
                IEnumerable<LogRecord> records=db.GetAllLogData(dbPath, CurrentPage, log.PageSize);
                foreach (LogRecord l in records)
                {
                    log.AllDbLogs.Add(new LogMessage() { Id = l.Id, MessageDate = l.MessageDate, MessageText = l.MessageText, MessageType = l.MessageType, UserName = l.UserName });
                }
            
                return PartialView(log);
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

                return Content(String.Format("<h4 class='text-danger'>Возникли ошибки. Данные записаны в лог</h4>"));
            }
        }


        public ActionResult DbEditor()
        {
            return PartialView();
        }

        

        public ActionResult ChangeAdminPassword()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult ChangeAdminPassword(string oldPassword,string newPassword)
        {
            if (String.IsNullOrEmpty(oldPassword) || String.IsNullOrEmpty(newPassword))
                return View("ChangeAdminPassword");

            try
            {
                if (WebSecurity.ChangePassword("admin", oldPassword, newPassword))
                {
                    return Content("<h4 class='text-success'>Пароль пользователя успешно изменен!</h4>");
                }
                else
                {
                    return Content("<h4 class='text-danger'>Не удалось изменить пароль пользователя! Проверьте правильность введенных данных.</h4>");
                }
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

                return Content("<h4 class='text-danger'>Не удалось изменить пароль. Возникло исключение. Данные записаны в лог.</h4>");
            }
            
        }



       

        public ActionResult GenerateAdreses(int delete=0)
        {
            if(delete!=0)
                repo_data.DeleteAllMarkers();

            //TestDataGenerator gen = new TestDataGenerator();
            //gen.GenerateAdreses(3800,4600);
            return Content("Сгенерировано успешно");
        }

        public ActionResult GenerateUnikom()
        {
            DataGen gen=new DataGen();
            gen.GenerateData("345",DateTime.Now.AddDays(-1),2.5,3.6,600);
            return Content("Сгенерировано успешно");
        }

        public ActionResult DeleteAllAdreses()
        {
           
            repo_data.DeleteAllMarkers();
            return Content("Адреса удалены");
        }

        private DbStatistic getStat()
        {
            DbStatistic stat=new DbStatistic();
            stat.HasAppKey = ConfigurationManager.AppSettings.AllKeys.Contains("dbPath");
            stat.DbPAth = stat.HasAppKey ? ConfigurationManager.AppSettings["dbPath"] : "";
            if (stat.HasAppKey)
            {
                Database db=new Database(ConfigurationManager.AppSettings["dbPath"]);
                stat.DbStatus = db.IsDataBaseExist();
            }
            else
            {
                stat.DbStatus = false;
            }
            return stat;
        }

        private class DbStatistic
        {
             internal string DbPAth { get; set; }
             internal bool DbStatus { get; set; }
             internal bool HasAppKey { get; set; }
        }

        private string Reverse(string text)
        {
            if (text == null) return null;

            // this was posted by petebob as well 
            char[] array = text.ToCharArray();
            Array.Reverse(array);
            return new String(array);
        }

    }



    
}
