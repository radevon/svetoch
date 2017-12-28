using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDb
{
        /// <summary>
        ///  Класс Денису для вставки данных по приборам
        ///  
        /// </summary>
        public class ExternalRepository
        {
            private Database database;

            // конструктор по готовому объекту БД
            public ExternalRepository(Database db)
            {
                if (db != null)
                    this.database = db;
                else
                    throw new NullReferenceException("Попытка инициализовать репозиторий неинициализированной базой данных");
            }

            // конструктор по имени файла
            public ExternalRepository(string filePath)
            {
                database = new Database(filePath);
            }

            /// <summary>
            /// создание базы и таблиц если их не существует
            /// </summary>
            /// <returns>истина - значит ошибок в процессе выполнения не возникало</returns>

            public bool CreateAllIfNotExist()
            {
                return database.CreateIfNotExistDataBase().isSuccess && database.CreateAllTablesIfNotExist();
            }

            private SQLiteConnection CreateSqlConnection()
            {
                SQLiteConnection connection = new SQLiteConnection(this.database.GetDefaultConnectionString());
                return connection;
            }

            /// <summary>
            /// Вставка новой строки в таблицу
            /// </summary>
            /// <param name="recvDate">Дата снятия показания</param>
            /// <param name="identity">Идентификатор станции, с которой приходят показания - к нему в программе визуализации привязывается адресс</param>
            /// <param name="totalEnergy">Значение параметра энергопотребления</param>
            /// <param name="amperage1">Значение тока 1</param>
            /// <param name="amperage2">Значение тока 2</param>
            /// <param name="amperage3">Значение тока</param>
            /// <param name="voltage1">Значение напряжения 1</param>
            /// <param name="voltage2">Значение напряжения 2</param>
            /// <param name="voltage3">Значение напряжения 3</param>
            /// <param name="currentElectricPower">Текущая мощность</param>
            /// <param name="totalWaterRate">Общий расход воды</param>
            /// <param name="frequrency">Частота</param>
            /// <param name="presure">Давление</param>
            /// <param name="temperature">Температура</param>
            /// <param name="errors">Список ошибок</param>
            /// <returns>объект MethodResult - характеризующий успешность операции</returns>
            public MethodResult InsertNewRow(DateTime recvDate, string identity, double totalEnergy, double amperage1, double amperage2, double amperage3, double voltage1, double voltage2, double voltage3, double currentElectricPower, double totalWaterRate, double frequrency, double presure, double temperature, string errors)
            {
                try
                {
                    using (SQLiteConnection connection = CreateSqlConnection())
                    {
                        using (SQLiteCommand command = new SQLiteCommand("insert into ElectricAndWaterParams (recvDate,Identity,TotalEnergy,Amperage1,Amperage2,Amperage3,Voltage1,Voltage2,Voltage3,CurrentElectricPower,TotalWaterRate, Frequrency, Presure, Temperature, Errors) values(@recvDate,@Identity,@TotalEnergy,@Amperage1,@Amperage2,@Amperage3,@Voltage1,@Voltage2,@Voltage3,@CurrentElectricPower,@TotalWaterRate,@Frequrency, @Presure, @Temperature,@Errors)", connection))
                        {
                            command.Parameters.Add("@recvDate", System.Data.DbType.DateTime).Value = recvDate;
                            command.Parameters.Add("@Identity", System.Data.DbType.String).Value = identity;
                            command.Parameters.Add("@TotalEnergy", System.Data.DbType.Double).Value = totalEnergy;
                            command.Parameters.Add("@Amperage1", System.Data.DbType.Double).Value = amperage1;
                            command.Parameters.Add("@Amperage2", System.Data.DbType.Double).Value = amperage2;
                            command.Parameters.Add("@Amperage3", System.Data.DbType.Double).Value = amperage3;
                            command.Parameters.Add("@Voltage1", System.Data.DbType.Double).Value = voltage1;
                            command.Parameters.Add("@Voltage2", System.Data.DbType.Double).Value = voltage2;
                            command.Parameters.Add("@Voltage3", System.Data.DbType.Double).Value = voltage3;
                            command.Parameters.Add("@CurrentElectricPower", System.Data.DbType.Double).Value = currentElectricPower;
                            command.Parameters.Add("@TotalWaterRate", System.Data.DbType.Double).Value = totalWaterRate;
                            command.Parameters.Add("@Frequrency", System.Data.DbType.Double).Value = frequrency;
                            command.Parameters.Add("@Presure", System.Data.DbType.Double).Value = presure;
                            command.Parameters.Add("@Temperature", System.Data.DbType.Double).Value = temperature;
                            command.Parameters.Add("@Errors", System.Data.DbType.String).Value = errors;

                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();

                        }
                    }
                    return new MethodResult(true);
                }
                catch (Exception ex)
                {
                    return new MethodResult(false, ex.Message + "\n" + ex.StackTrace);
                }
            }

            /// <summary>
            /// Удаление данных между 2 датами по заданному объекту
            /// </summary>
            /// <param name="start">Начальная дата</param>
            /// <param name="end">Конечная дата</param>
            /// <param name="identity">Идентификатор</param>
            /// <returns>объект MethodResult - характеризующий успешность операции</returns>
            public MethodResult DeleteByDate(DateTime start, DateTime end, string identity)
            {
                try
                {
                    using (SQLiteConnection connection = CreateSqlConnection())
                    {
                        using (SQLiteCommand command = new SQLiteCommand("delete from ElectricAndWaterParams where Identity=@identity and datetime(recvDate) between @start and @end;", connection))
                        {
                            command.Parameters.Add("@identity", System.Data.DbType.String).Value = identity;
                            command.Parameters.Add("@start", System.Data.DbType.DateTime).Value = start;
                            command.Parameters.Add("@end", System.Data.DbType.DateTime).Value = end;
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();

                        }
                    }
                    return new MethodResult(true);
                }
                catch (Exception ex)
                {
                    return new MethodResult(false, ex.Message + "\n" + ex.StackTrace);
                }
            }

            /// <summary>
            /// Удаление данных между 2 датами
            /// </summary>
            /// <param name="start">Начальная дата</param>
            /// <param name="end">Конечная дата</param>
            /// <returns>объект MethodResult - характеризующий успешность операции</returns>
            public MethodResult DeleteAllByDate(DateTime start, DateTime end)
            {
                try
                {
                    using (SQLiteConnection connection = CreateSqlConnection())
                    {
                        using (SQLiteCommand command = new SQLiteCommand("delete from ElectricAndWaterParams where datetime(recvDate) between @start and @end;", connection))
                        {
                            command.Parameters.Add("@start", System.Data.DbType.DateTime).Value = start;
                            command.Parameters.Add("@end", System.Data.DbType.DateTime).Value = end;
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();

                        }
                    }
                    return new MethodResult(true);
                }
                catch (Exception ex)
                {
                    return new MethodResult(false, ex.Message + "\n" + ex.StackTrace);
                }
            }

            /// <summary>
            /// Удаление данных по объекту
            /// </summary>
            /// <param name="identity">Идентификатор</param>
            /// <returns>объект MethodResult - характеризующий успешность операции</returns>
            public MethodResult DeleteByIdentity(string identity)
            {
                try
                {
                    using (SQLiteConnection connection = CreateSqlConnection())
                    {
                        using (SQLiteCommand command = new SQLiteCommand("delete from ElectricAndWaterParams where Identity=@identity;", connection))
                        {
                            command.Parameters.Add("@identity", System.Data.DbType.String).Value = identity;
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();

                        }
                    }
                    return new MethodResult(true);
                }
                catch (Exception ex)
                {
                    return new MethodResult(false, ex.Message + "\n" + ex.StackTrace);
                }
            }


            /// <summary>
            ///  Метод удаляет таблицу с результатами подсчетов
            /// </summary>
            /// <returns>Результат удаления всех данных в таблице</returns>

            public MethodResult DeleteAll()
            {
                try
                {
                    using (SQLiteConnection connection = CreateSqlConnection())
                    {
                        using (SQLiteCommand command = new SQLiteCommand("delete from ElectricAndWaterParams;", connection))
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();

                        }
                    }
                    return new MethodResult(true);
                }
                catch (Exception ex)
                {
                    return new MethodResult(false, ex.Message + "\n" + ex.StackTrace);
                }
            }

        }

}
