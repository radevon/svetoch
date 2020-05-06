using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using PumpDb.Models;

namespace PumpDb
{
    public class VisualDataRepository
    {
        private Database db_;


        public VisualDataRepository(string filePath)
        {
            db_ = new Database(filePath);
        }

        public VisualDataRepository(Database db)
        {
            if (db == null)
                throw new NullReferenceException("Попытка инициализовать репозиторий неинициализированной базой данных");
            db_ = db;
        }

        // объекты
        #region Table db_object_marker

        public IEnumerable<Marker> GetAllMarkers()
        {
            IEnumerable<Marker> markers = Enumerable.Empty<Marker>();

            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                markers = conn.Query<Marker>("select markerId, address, px, py, identity from db_object_marker");
            }

            return markers;
        }

        public Marker GetMarkerById(int id)
        {
            Marker marker = null;

            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                marker = conn.QuerySingleOrDefault<Marker>("select markerId, address, px, py, identity from db_object_marker where markerId=@id_", new { id_ = id });
            }
            //if (marker == null) throw new Exception("Не найден объект в базе по id: " + id.ToString());
            return marker;
        }

        public Marker GetMarkerByIdentity(string identity)
        {
            Marker marker = null;
            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                marker = conn.QuerySingleOrDefault<Marker>("select markerId, address, px, py, identity from db_object_marker where identity=@identity_", new { identity_ = identity });
            }

            return marker;
        }


        public int InsertMarker(Marker marker)
        {

            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                return conn.ExecuteScalar<int>("insert into db_object_marker (address, px, py, identity) values(@address,@px,@py,@identity);SELECT last_insert_rowid();", marker);
            }

        }

        public int UpdateMarker(Marker marker)
        {

            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                return conn.Execute("update db_object_marker set address=@address, px=@px, py=@py, identity=@identity where markerId=@markerId", marker);
            }

        }

        public int DeleteMarkerById(int id)
        {
            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                return conn.Execute("delete from db_object_marker where markerId=@id", new { id = id });
            }

        }

        public void DeleteAllMarkers()
        {
            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                conn.Execute("delete from db_object_marker");
            }
        }

        #endregion


        #region Table ElectricAndWaterParams

        // параметры по Id - 1 запись или null
        public ElectricAndWaterParams GetPumpParamsById(int id)
        {
            ElectricAndWaterParams parameter = null;

            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                parameter = conn.QuerySingleOrDefault<ElectricAndWaterParams>("select Id, Identity, TotalEnergy, Amperage1, Amperage2, Amperage3, Voltage1, Voltage2, Voltage3, CurrentElectricPower, TotalWaterRate, RecvDate, Frequrency, Presure, Temperature, Errors, EngineAmp from ElectricAndWaterParams where Id=@id_", new { id_ = id });
            }
            return parameter;
        }

        public IEnumerable<ElectricAndWaterParams> GetPumpParamsByIdentityAndDate(string identity, DateTime from, DateTime to)
        {
            IEnumerable<ElectricAndWaterParams> parameters = Enumerable.Empty<ElectricAndWaterParams>();

            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                parameters = conn.Query<ElectricAndWaterParams>("select Id, Identity, TotalEnergy, Amperage1, Amperage2, Amperage3, Voltage1, Voltage2, Voltage3, CurrentElectricPower, TotalWaterRate, RecvDate, Frequrency, Presure, Temperature, Errors, EngineAmp from ElectricAndWaterParams where Identity=@identity_ and datetime(recvDate) between @start and @end;", new { identity_ = identity, start = from, end = to });
            }
            return parameters;
        }

         public ElectricAndWaterParams GetPumpParamsByIdentityLast(string identity)
        {
            ElectricAndWaterParams parameters = null;

            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                parameters = conn.Query<ElectricAndWaterParams>("select Id, Identity, TotalEnergy, Amperage1, Amperage2, Amperage3, Voltage1, Voltage2, Voltage3, CurrentElectricPower, TotalWaterRate, RecvDate, Frequrency, Presure, Temperature, Errors, EngineAmp from ElectricAndWaterParams where Identity=@identity_ order by recvDate desc limit 1;", new { identity_ = identity }).SingleOrDefault();
            }
            return parameters;
        }

        // расширеный метод для получения расхода воды с учетом коэффиц и нач значения
        public IEnumerable<ElectricAndWaterParamsExtended> GetExtendedPumpParamsByIdentityAndDate(string identity, DateTime from, DateTime to)
        {
            IEnumerable<ElectricAndWaterParamsExtended> parameters = Enumerable.Empty<ElectricAndWaterParamsExtended>();

            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                parameters = conn.Query<ElectricAndWaterParamsExtended>("select param.Id, param.Identity, param.TotalEnergy, param.Amperage1, param.Amperage2, param.Amperage3, param.Voltage1, param.Voltage2, param.Voltage3, param.CurrentElectricPower, param.TotalWaterRate, param.RecvDate, param.Frequrency, param.Presure, param.Temperature, param.Errors, param.EngineAmp, ifnull(k.WaterKoef,1) as WaterKoef, ifnull(k.WaterStartValue,0) as WaterStartValue  from ElectricAndWaterParams param left join WaterKoefs k on param.Identity=k.Identity where param.Identity=@identity_ and datetime(param.recvDate) between @start and @end;", new { identity_ = identity, start = from, end = to });
            }
            return parameters;
        }

        
        #endregion

        // таблица начальных параметров воды
        #region Waterkoefs

        // получить параметры
        public WaterKoefs GetKoefsByIdentity(string identity_)
        {
            WaterKoefs koefs = null;
            using (IDbConnection conn=new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                koefs = conn.QuerySingleOrDefault<WaterKoefs>(
                        "select Identity, WaterKoef, WaterStartValue from WaterKoefs where Identity=@identity_",
                        new {identity_ = identity_});
            }
            return koefs;
        }

        // удалить параметры
        public int RemoveKoefsByIdentity(string identity_)
        {
            int result = 0;
            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                result = conn.Execute("delete from WaterKoefs where Identity=@identity_",
                        new {identity_ = identity_});
            }
            return result;
        }


        public int AddOrUpdateKoef(WaterKoefs new_)
        {
            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                return conn.Execute("insert  or replace into WaterKoefs (identity, WaterKoef, WaterStartValue) values(@identity,@WaterKoef,@WaterStartValue)", new_);
            }
        }




        #endregion

        #region PumpNominalParam

        public PumpNominalParam GetNominalParamsByIdentity(string identity_)
        {
            PumpNominalParam param = null;
            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                param = conn.QuerySingleOrDefault<PumpNominalParam>(
                        "select Identity, KoefUndo, KoefOver, NominalPower from PumpNominalParam where Identity=@identity_",
                        new { identity_ = identity_ });
            }
            return param;
        }

        public int AddOrUpdateNomParams(PumpNominalParam new_)
        {
            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                return conn.Execute("insert  or replace into PumpNominalParam (identity, KoefUndo, KoefOver,NominalPower) values(@identity,@KoefUndo,@KoefOver,@NominalPower)", new_);
            }
        }

        #endregion

        #region Statistic

        /*
        public IEnumerable<ByHourStat> GetStatByHour(DateTime day, string identity)
        {
            IEnumerable<ByHourStat> parameters = Enumerable.Empty<ByHourStat>();
            string sqlstring = @"WITH RECURSIVE dates(date) AS (
  VALUES(@day_)
  UNION ALL
  SELECT datetime(date, '+1 hour')
  FROM dates
  WHERE date < datetime(@day_,'+1 day')
) select dates.date as HourTime, params.recvDate, params.TotalEnergy, ifnull((select waterStartValue from waterKoefs where identity=@identity_),0)+ params.TotalWaterRate*ifnull((select waterKoef from waterKoefs where identity=@identity_),1) as TotalWaterRate  from dates
left join 
(select par.recvDate, par.TotalEnergy, par.TotalWaterRate from electricandwaterparams par where par.identity=@identity_) params on params.recvDate=(select min(e.recvDate) from electricandwaterparams e where e.identity=@identity_ and e.recvDate>=dates.date and e.recvDate<datetime(dates.date,'+1 hour') and (e.TotalEnergy>0 or e.TotalWaterRate>0)) order by dates.date";

            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                parameters = conn.Query<ByHourStat>(sqlstring, new { identity_ = identity, day_ = day.ToString("yyyy-MM-dd HH:mm:ss") });
            }
            return parameters;
        }
        */

        public IEnumerable<ByHourStat> GetStatByHour(DateTime day, string identity)
        {
            IEnumerable<ByHourStat> parameters = Enumerable.Empty<ByHourStat>();
            string sqlstring = @"WITH RECURSIVE dates(date) AS (
  VALUES(@day_)
  UNION ALL
  SELECT datetime(date, '+1 hour')
  FROM dates
  WHERE date < datetime(@day_,'+1 day')
) 
select dates.date as HourTime,
	   energy.recvDate as recvDateEnergy,
	   energy.TotalEnergy,
	   water.recvDate as recvDateWater,
	   ifnull((select waterStartValue from waterKoefs where identity=@identity_),0)+ water.TotalWaterRate*ifnull((select waterKoef from waterKoefs where identity=@identity_),1) as TotalWaterRate
	   
	   from dates
	   left join 
            (select par.recvDate, par.TotalEnergy from electricandwaterparams par where par.identity=@identity_ and par.TotalEnergy>0) energy 
	   on energy.recvDate=(select min(e.recvDate) from electricandwaterparams e where e.identity=@identity_ and e.recvDate>=dates.date and e.recvDate<datetime(dates.date,'+1 hour') and e.TotalEnergy>0) 
	   
	   left join 
            (select par.recvDate, par.TotalWaterRate from electricandwaterparams par where par.identity=@identity_ and par.TotalWaterRate>0) water 
	   on water.recvDate=(select min(e.recvDate) from electricandwaterparams e where e.identity=@identity_ and e.recvDate>=dates.date and e.recvDate<datetime(dates.date,'+1 hour') and e.TotalWaterRate>0) 
	   
	   order by dates.date";

            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                parameters = conn.Query<ByHourStat>(sqlstring, new { identity_ = identity, day_ = day.ToString("yyyy-MM-dd HH:mm:ss") });
            }
            return parameters;
        }


        /*
         * (select datetime(date(@day_)||' '||time('00:00:00')) as hourTime union select  datetime(date(@day_)||' '||time('01:00:00')) union select  datetime(date(@day_)||' '||time('02:00:00')) union select  datetime(date(@day_)||' '||time('03:00:00')) union select  datetime(date(@day_)||' '||time('04:00:00')) union select  datetime(date(@day_)||' '||time('05:00:00')) union select  datetime(date(@day_)||' '||time('06:00:00')) union select  datetime(date(@day_)||' '||time('07:00:00')) union select  datetime(date(@day_)||' '||time('08:00:00')) union select  datetime(date(@day_)||' '||time('09:00:00')) union select  datetime(date(@day_)||' '||time('10:00:00')) union select  datetime(date(@day_)||' '||time('11:00:00')) union select  datetime(date(@day_)||' '||time('12:00:00')) 
union select  datetime(date(@day_)||' '||time('13:00:00')) union select  datetime(date(@day_)||' '||time('14:00:00')) union select  datetime(date(@day_)||' '||time('15:00:00')) union select  datetime(date(@day_)||' '||time('16:00:00')) union select  datetime(date(@day_)||' '||time('17:00:00')) union select  datetime(date(@day_)||' '||time('18:00:00')) union select  datetime(date(@day_)||' '||time('19:00:00')) union select  datetime(date(@day_)||' '||time('20:00:00')) union select  datetime(date(@day_)||' '||time('21:00:00')) union select  datetime(date(@day_)||' '||time('22:00:00')) union select  datetime(date(@day_)||' '||time('23:00:00')) union select datetime(date(@day_,'+1 days')||' '||time('00:00:00'))
) dates 
         * 
         * */


        /*
        public IEnumerable<ByHourStat> GetStatByDays(DateTime month, string identity)
        {
            IEnumerable<ByHourStat> parameters = Enumerable.Empty<ByHourStat>();
            DateTime startMonth = new DateTime(month.Year, month.Month, 1);
            string sqlstring = @"WITH RECURSIVE dates(date) AS (
  VALUES(@day_)
  UNION ALL
  SELECT date(date, '+1 day')
  FROM dates
  WHERE date < date(@day_,'+1 month')
)
SELECT dates.date as HourTime,  params.recvDate, params.TotalEnergy, ifnull((select waterStartValue from waterKoefs where identity=@identity_),0)+ params.TotalWaterRate*ifnull((select waterKoef from waterKoefs where identity=@identity_),1) as TotalWaterRate  FROM dates
left join 
(select par.recvDate, par.TotalEnergy, par.TotalWaterRate from electricandwaterparams par where par.identity=@identity_) params on params.recvDate=(select min(e.recvDate) from electricandwaterparams e where e.identity=@identity_ and e.recvDate>=dates.date and e.recvDate<datetime(dates.date,'+1 day') and (e.TotalEnergy>0 or e.TotalWaterRate>0))";
            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                parameters = conn.Query<ByHourStat>(sqlstring, new { identity_ = identity, day_ = startMonth.ToString("yyyy-MM-dd") });
            }
            return parameters;
        }
        */

        public IEnumerable<ByHourStat> GetStatByDays(DateTime month, string identity)
        {
            IEnumerable<ByHourStat> parameters = Enumerable.Empty<ByHourStat>();
            DateTime startMonth = new DateTime(month.Year, month.Month, 1);
            string sqlstring = @"WITH RECURSIVE dates(date) AS (
  VALUES(@day_)
  UNION ALL
  SELECT datetime(date, '+1 day')
  FROM dates
  WHERE date < datetime(@day_,'+1 month')
) 
select dates.date as HourTime,
	   energy.recvDate as recvDateEnergy,
	   energy.TotalEnergy,
	   water.recvDate as recvDateWater,
	   ifnull((select waterStartValue from waterKoefs where identity=@identity_),0)+ water.TotalWaterRate*ifnull((select waterKoef from waterKoefs where identity=@identity_),1) as TotalWaterRate
	   
	   from dates
	   left join 
            (select par.recvDate, par.TotalEnergy from electricandwaterparams par where par.identity=@identity_ and par.TotalEnergy>0) energy 
	   on energy.recvDate=(select min(e.recvDate) from electricandwaterparams e where e.identity=@identity_ and e.recvDate>=dates.date and e.recvDate<datetime(dates.date,'+1 day') and e.TotalEnergy>0) 
	   
	   left join 
            (select par.recvDate, par.TotalWaterRate from electricandwaterparams par where par.identity=@identity_ and par.TotalWaterRate>0) water 
	   on water.recvDate=(select min(e.recvDate) from electricandwaterparams e where e.identity=@identity_ and e.recvDate>=dates.date and e.recvDate<datetime(dates.date,'+1 day') and e.TotalWaterRate>0) 
	   
	   order by dates.date";
            using (IDbConnection conn = new SQLiteConnection(this.db_.GetDefaultConnectionString()))
            {
                parameters = conn.Query<ByHourStat>(sqlstring, new { identity_ = identity, day_ = startMonth.ToString("yyyy-MM-dd") });
            }
            return parameters;
        }

        #endregion

    }


}
