using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDb
{
    public class Database
    {
        private String FilePath;

        // базовый sql для создания таблицы маркеров
        private string MarkerTableSql = @"CREATE TABLE IF NOT EXISTS 'db_object_marker' (
	'markerId'	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	'address'	TEXT NOT NULL,
	'px'	REAL NOT NULL,
	'py'	REAL NOT NULL,
	'identity'	TEXT NOT NULL UNIQUE
);";

        private string HeatTableSql = @"CREATE TABLE IF NOT EXISTS 'ElectricAndWaterParams' (
	'Id'	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	'Identity'	TEXT NOT NULL,
	'TotalEnergy'	REAL NOT NULL DEFAULT 0,
	'Amperage1'	REAL NOT NULL DEFAULT 0,
    'Amperage2'	REAL NOT NULL DEFAULT 0,
    'Amperage3'	REAL NOT NULL DEFAULT 0,
	'Voltage1'	REAL NOT NULL DEFAULT 0,
    'Voltage2'	REAL NOT NULL DEFAULT 0,
    'Voltage3'	REAL NOT NULL DEFAULT 0,
	'CurrentElectricPower'	REAL NOT NULL DEFAULT 0,
	'TotalWaterRate'	REAL NOT NULL DEFAULT 0,
    'Frequrency' REAL NOT NULL DEFAULT 0,
    'Presure' REAL NOT NULL DEFAULT 0,
    'Temperature' REAL NOT NULL DEFAULT 0,
	'RecvDate'	TEXT NOT NULL,
	'Errors'	TEXT
);";

        private string WaterKoefTable = @"CREATE TABLE IF NOT EXISTS 'WaterKoefs' (
	'Identity'	TEXT NOT NULL UNIQUE,
	'WaterKoef'	real NOT NULL DEFAULT 1,
	'WaterStartValue'	REAL NOT NULL DEFAULT 0
);";

        private string PumpNominalParamTable = @"CREATE TABLE IF NOT EXISTS 'PumpNominalParam' (
	'Identity'	TEXT NOT NULL UNIQUE,
	'KoefUndo'	real NOT NULL DEFAULT 30,
	'KoefOver'	REAL NOT NULL DEFAULT 10,
    'NominalPower' real not null default 0
);";

      private string LogTable = @"CREATE TABLE if not exists 'loging' (
	'Id'	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
	'MessageDate'	TEXT NOT NULL,
	'UserName'	TEXT,
	'MessageType'	TEXT,
	'MessageText'	TEXT
);";



        public Database(string filePath)
        {
            this.FilePath = filePath;
        }

        // получить, установить путь к файлу БД
        public string SetFilePath
        {
            get { return this.FilePath; }
            set { this.FilePath = value; }
        }

        public string GetFilePath
        {
            get
            {
                return this.FilePath;
            }
        }

        // существует ли база данных
        public bool IsDataBaseExist()
        {
            return File.Exists(this.FilePath);
        }


        // создание базы если её не существует
        public MethodResult CreateIfNotExistDataBase()
        {

            if (!IsDataBaseExist())
            {
                try
                {
                    SQLiteConnection.CreateFile(this.FilePath);

                }
                catch (Exception ex)
                {
                    return new MethodResult(false, ex.Message + "\n" + ex.StackTrace);
                }
            }
            return new MethodResult(true);
        }

        public bool CreateAllTablesIfNotExist()
        {
            bool allSuccess = this.ExecuteSqlCreateTable(this.MarkerTableSql).isSuccess
                              && this.ExecuteSqlCreateTable(this.HeatTableSql).isSuccess
                              && this.ExecuteSqlCreateTable(this.WaterKoefTable).isSuccess
                              && this.ExecuteSqlCreateTable(this.PumpNominalParamTable).isSuccess
                              && this.ExecuteSqlCreateTable(this.LogTable).isSuccess;
            return allSuccess;
        }



        public string GetDefaultConnectionString()
        {
            return String.Format("Data Source={0};Version=3;journal_mode=WAL;auto_vacuum = FULL;", FilePath);
        }

        // вызов скрипта создания таблиц
        private MethodResult ExecuteSqlCreateTable(string sql)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(this.GetDefaultConnectionString()))
                {
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                        return new MethodResult(true);
                    }

                }
            }
            catch (Exception ex)
            {
                return new MethodResult(false, ex.Message + "\n" + ex.StackTrace);
            }

        }

        public MethodResult Vacuum()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(this.GetDefaultConnectionString()))
                {
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        connection.Open();
                        command.CommandText = "vacuum;";
                        command.ExecuteNonQuery();
                        connection.Close();
                        return new MethodResult(true);
                    }

                }
            }
            catch (Exception ex)
            {
                return new MethodResult(false, ex.Message + "\n" + ex.StackTrace);
            }
        }

    }

    /// <summary>
    /// Структура, возвращаемая из методов работы с БД - информация о успешности операции
    /// </summary>
    public struct MethodResult
    {

        public MethodResult(bool success)
        {
            this.isSuccess = success;
            Message = String.Empty;
        }

        public MethodResult(bool success, string message)
        {
            this.isSuccess = success;
            this.Message = message;
        }

        // как отработал метод (с ошибками или без)
        public bool isSuccess;
        // сообщение об ошибке если isSuccess=false
        public string Message;
    }


}
